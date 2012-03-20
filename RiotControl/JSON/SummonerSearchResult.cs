﻿namespace RiotControl
{
	class SummonerSearchResult
	{
		public string Result;
		public string SummonerName;
		public int? AccountID;
		public int? SummonerLevel;

		public SummonerSearchResult(LookupJob job)
		{
			SummonerName = null;
			AccountID = null;
			switch (job.Result)
			{
				case JobQueryResult.Success:
					Result = "Success";
					SummonerName = job.RealSummonerName;
					AccountID = job.AccountId;
					SummonerLevel = job.SummonerLevel;
					break;

				case JobQueryResult.NotFound:
					Result = "NotFound";
					break;

				case JobQueryResult.Timeout:
					Result = "Timeout";
					break;
			}
		}
	}
}
