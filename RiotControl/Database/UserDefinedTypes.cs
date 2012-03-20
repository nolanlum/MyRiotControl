﻿using System;
using System.Collections.Generic;

namespace RiotControl
{
	enum RegionType
	{
		NorthAmerica,
		EuropeWest,
		EuropeNordicEast,
	}

	enum MapType
	{
		TwistedTreeline,
		SummonersRift,
		Dominion,
	}

	enum GameModeType
	{
		Custom,
		Bot,
		Normal,
		Solo,
		Premade,
	}

	static class UserDefinedTypes
	{
		static Dictionary<string, RegionType> RegionTypeDictionary = new Dictionary<string, RegionType>()
		{
			{"north_america", RegionType.NorthAmerica},
			{"europe_west", RegionType.EuropeWest},
			{"europe_nordic_east", RegionType.EuropeNordicEast},
		};

		static Dictionary<string, MapType> MapTypeDictionary = new Dictionary<string, MapType>()
		{
			{"twisted_treeline", MapType.TwistedTreeline},
			{"summoners_rift", MapType.SummonersRift},
			{"dominion", MapType.Dominion},
		};

		static Dictionary<string, GameModeType> GameModeTypeDictionary = new Dictionary<string, GameModeType>()
		{
			{"custom", GameModeType.Custom},
			{"normal", GameModeType.Normal},
			{"bot", GameModeType.Bot},
			{"solo", GameModeType.Solo},
			{"premade", GameModeType.Premade},
		};

		static Dictionary<MapType, string> MapTypeStringDictionary = new Dictionary<MapType, string>()
		{
			{MapType.TwistedTreeline, "Twisted Treeline"},
			{MapType.SummonersRift, "Summoner's Rift"},
			{MapType.Dominion, "Dominion"},
		};

		static Dictionary<GameModeType, string> GameModeTypeStringDictionary = new Dictionary<GameModeType, string>()
		{
			{GameModeType.Custom, "Custom"},
			{GameModeType.Bot, "Co-op vs. AI"},
			{GameModeType.Normal, "Normal"},
			{GameModeType.Solo, "Ranked Solo/Duo"},
			{GameModeType.Premade, "Ranked Teams"},
		};

		static EnumType ToEnumType<EnumType>(this string input, Dictionary<string, EnumType> enumDictionary)
		{
			EnumType output;
			if (enumDictionary.TryGetValue(input, out output))
				return output;
			else
				throw new Exception(string.Format("Unknown enum type: {0}", input));
		}

		public static RegionType ToRegionType(this string input)
		{
			return input.ToEnumType<RegionType>(RegionTypeDictionary);
		}

		public static MapType ToMapType(this string input)
		{
			return input.ToEnumType<MapType>(MapTypeDictionary);
		}

		public static GameModeType ToGameModeType(this string input)
		{
			return input.ToEnumType<GameModeType>(GameModeTypeDictionary);
		}

		static string ToEnumString<EnumType>(EnumType input, Dictionary<string, EnumType> enumDictionary)
		{
			foreach (var pair in enumDictionary)
			{
				if (EqualityComparer<EnumType>.Default.Equals(pair.Value, input))
					return pair.Key;
			}
			throw new Exception(string.Format("Unable to retrieve a string for enum: {0}", input));
		}

		public static string ToEnumString(this MapType input)
		{
			return ToEnumString<MapType>(input, MapTypeDictionary);
		}

		public static string ToEnumString(this GameModeType input)
		{
			return ToEnumString<GameModeType>(input, GameModeTypeDictionary);
		}

		public static string GetString(this MapType input)
		{
			return MapTypeStringDictionary[input];
		}

		public static string GetString(this GameModeType input)
		{
			return GameModeTypeStringDictionary[input];
		}
	}
}
