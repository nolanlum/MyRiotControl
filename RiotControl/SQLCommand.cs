using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;

namespace RiotControl
{
	class SQLCommand
	{
		static Regex cast_replace = new Regex("cast\\((.*?) as .*?\\)");

		public string Query;
		public DbCommand Command;

		List<string>.Enumerator Enumerator;
		Profiler CommandProfiler;

		SqlTypes SqlType;

		public SQLCommand(string query, DbConnection connection, Profiler profiler = null, params object[] arguments)
		{
			CommandProfiler = profiler;
			Query = string.Format(query, arguments);

			// All logic assumes MySQL is the "edge case", and the fallback is postgre.
			if (connection is MySqlConnection)
			{
				Query = Query.Replace(":", "?").Replace("at time zone 'UTC'", "");
				Query = cast_replace.Replace(Query, "$1");

				SqlType = new SqlTypes(MySqlDbType.Int32, MySqlDbType.Text, MySqlDbType.Double, MySqlDbType.Bit, MySqlDbType.VarChar);
			}
			else
			{
				SqlType = new SqlTypes(NpgsqlDbType.Integer, NpgsqlDbType.Text, NpgsqlDbType.Double, NpgsqlDbType.Boolean, NpgsqlDbType.Varchar);
			}

			Command = DatabaseConnectionProvider.GetCommand(Query, connection);
		}

		public void SetFieldNames(List<string> fields)
		{
			Enumerator = fields.GetEnumerator();
		}

		void Add(string name, NpgsqlDbType type)
		{
			Command.Parameters.Add(new NpgsqlParameter(name, type));
		}

		void Add(string name, MySqlDbType type)
		{
			Command.Parameters.Add(new MySqlParameter(name, type));
		}

		void Set(string name, NpgsqlDbType type, object value)
		{
			if(!Command.Parameters.Contains(name))
				Command.Parameters.Add(new NpgsqlParameter(name, type));
			Command.Parameters[name].Value = value;
		}

		void Set(string name, MySqlDbType type, object value)
		{
			if (!Command.Parameters.Contains(name))
				Command.Parameters.Add(new MySqlParameter(name, type));
			Command.Parameters[name].Value = value;
		}

		void Set(NpgsqlDbType type, object value)
		{
			Enumerator.MoveNext();
			Command.Parameters.Add(new NpgsqlParameter(Enumerator.Current, type));
			Command.Parameters[Command.Parameters.Count - 1].Value = value;
		}

		void Set(MySqlDbType type, object value)
		{
			Enumerator.MoveNext();
			Command.Parameters.Add(new MySqlParameter(Enumerator.Current, type));
			Command.Parameters[Command.Parameters.Count - 1].Value = value;
		}

		public void Set(string name, int value)
		{
			if (SqlType.Integer is MySqlDbType)
				Set(name, (MySqlDbType) SqlType.Integer, value);
			else
				Set(name, (NpgsqlDbType) SqlType.Integer, value);
		}

		public void Set(string name, int? value)
		{
			object argument;
			if (value.HasValue)
				argument = value.Value;
			else
				argument = null;

			if (SqlType.Integer is MySqlDbType)
				Set(name, (MySqlDbType) SqlType.Integer, argument);
			else
				Set(name, (NpgsqlDbType) SqlType.Integer, argument);
		}

		public void Set(string name, bool value)
		{
			if (SqlType.Boolean is MySqlDbType)
				Set(name, (MySqlDbType) SqlType.Boolean, value);
			else
				Set(name, (NpgsqlDbType) SqlType.Boolean, value);
		}

		public void Set(string name, string value)
		{
			if (SqlType.Text is MySqlDbType)
				Set(name, (MySqlDbType) SqlType.Text, value);
			else
				Set(name, (NpgsqlDbType) SqlType.Text, value);
		}

		public void SetEnum(string name, string value)
		{
			if (SqlType.Varchar is MySqlDbType)
				Set(name, (MySqlDbType) SqlType.Varchar, value);
			else
				Set(name, (NpgsqlDbType) SqlType.Varchar, value);
		}

		public void Set(int value)
		{
			if (SqlType.Integer is MySqlDbType)
				Set((MySqlDbType) SqlType.Integer, value);
			else
				Set((NpgsqlDbType) SqlType.Integer, value);
		}

		public void Set(int? value)
		{
			object argument;
			if (value.HasValue)
				argument = value.Value;
			else
				argument = null;

			if (SqlType.Integer is MySqlDbType)
				Set((MySqlDbType) SqlType.Integer, argument);
			else
				Set((NpgsqlDbType) SqlType.Integer, argument);
		}

		public void Set(int[] value)
		{
			if (SqlType.Integer is MySqlDbType)
			{
				// MySQL doesn't support arrays, so pack the array into a blob.
				byte[] b = new byte[value.Length * 4];

				for (int i = 0; i < value.Length; i++)
					Array.Copy(BitConverter.GetBytes(value[i]), 0, b, i * 4, 4);

				Set(MySqlDbType.Blob, b);
			}
			else
				Set(NpgsqlDbType.Array | NpgsqlDbType.Integer, value);
		}

		public void Set(string value)
		{
			if (SqlType.Text is MySqlDbType)
				Set((MySqlDbType) SqlType.Text, value);
			else
				Set((NpgsqlDbType) SqlType.Text, value);
		}

		public void Set(double value)
		{
			if (SqlType.Double is MySqlDbType)
				Set((MySqlDbType) SqlType.Double, value);
			else
				Set((NpgsqlDbType) SqlType.Double, value);
		}

		public void Set(bool value)
		{
			if (SqlType.Boolean is MySqlDbType)
				Set((MySqlDbType) SqlType.Boolean, value);
			else
				Set((NpgsqlDbType) SqlType.Boolean, value);
		}

		public void SetEnum(string value)
		{
			if (SqlType.Varchar is MySqlDbType)
				Set((MySqlDbType) SqlType.Varchar, value);
			else
				Set((NpgsqlDbType) SqlType.Varchar, value);
		}

		void Start()
		{
			if (CommandProfiler != null)
				CommandProfiler.Start(Query);
		}

		void Stop()
		{
			if (CommandProfiler != null)
				CommandProfiler.Stop();
		}

		public int Execute()
		{
			Start();
			int rowsAffected = Command.ExecuteNonQuery();
			Stop();
			return rowsAffected;
		}

		public DbDataReader ExecuteReader()
		{
			Start();
			DbDataReader reader = Command.ExecuteReader();
			Stop();
			return reader;
		}

		public object[] ExecuteSingleRow()
		{
			Start();

			using (DbDataReader reader = Command.ExecuteReader())
			{
				if (!reader.Read())
				{
					Stop();
					return null;
				}

				object[] data = new object[reader.FieldCount];
				reader.GetValues(data);

				Stop();
				return data;
			}
		}

		public object ExecuteScalar()
		{
			Start();
			object output = Command.ExecuteScalar();
			Stop();
			return output;
		}

		public void CopyParameters(SQLCommand command)
		{
			foreach(object o in command.Command.Parameters)
				Command.Parameters.Add(o);
		}

		private class SqlTypes
		{
			public object Integer;
			public object Text;
			public object Double;
			public object Boolean;
			public object Varchar;

			public SqlTypes(object i, object t, object d, object b, object v)
			{
				Integer = i;
				Text = t;
				Double = d;
				Boolean = b;
				Varchar = v;
			}
		}
	}

	public static class Extensions {
		public static bool TryAsBool(this object o)
		{
			if (o is bool)
				return (bool) o;
			else if (o is ulong)
				return (ulong) o == 1;
			else if (o is uint)
				return (uint) o == 1;
			else if (o is byte)
				return (byte) o == 1;
			else return false;
		}
	}
}
