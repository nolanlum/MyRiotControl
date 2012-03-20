﻿using System.Collections.Generic;

using LibOfLegends;

namespace RiotControl
{
	public class Configuration
	{
		//In seconds
		public int AutomaticUpdateInterval;

		public WebConfiguration Web;
		public DatabaseConfiguration Database;
		public AuthenticationProfile Authentication;
		public ProxyProfile Proxy;
		public List<EngineRegionProfile> RegionProfiles;

		public Configuration()
		{
			Database = new DatabaseConfiguration();
			Authentication = new AuthenticationProfile();
			Proxy = new ProxyProfile();
			RegionProfiles = new List<EngineRegionProfile>();
		}
	}
}
