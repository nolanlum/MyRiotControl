using System;

using System.Data.Common;

namespace RiotControl
{
	class Reader
	{
		int Index;
		DbDataReader DataReader;

		public Reader(DbDataReader reader)
		{
			Index = 0;
			DataReader = reader;
		}

		public object Get()
		{
			object output = DataReader[Index];
			Index++;
			return output;
		}

		public int Integer()
		{
			object value = Get();
			//Hack for aggregates
			if (value.GetType() == typeof(long))
				return (int) (long) value;
			else if (value.GetType() == typeof(decimal))
				return (int) (decimal) value;
			else
				return (int)value;
		}

		public int? MaybeInteger()
		{
			object value = Get();
			if (value.GetType() == typeof(DBNull))
				return null;
			else
				return (int)value;
		}

		public string String()
		{
			object value = Get();
			if (value.GetType() == typeof(DBNull))
				return null;
			else
				return (string)value;
		}

		public bool Boolean()
		{
			object tryBool = Get();
			bool? tryTwo = tryBool as bool?;

			if (tryTwo.HasValue) return tryTwo.Value;

			return (ulong) tryBool == 1;
		}

		public double Double()
		{
			return (double)Get();
		}

		public DateTime Time()
		{
			return (DateTime)Get();
		}

		public void SanityCheck(string[] fields)
		{
			if (fields.Length != Index)
				throw new Exception("Data reader field count mismatch");
		}
	}
}
