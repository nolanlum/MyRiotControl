using System.Data.Common;

using com.riotgames.platform.summoner;

using LibOfLegends;

namespace RiotControl
{
	partial class Worker
	{
		void UpdateSummonerByName(LookupJob job)
		{
			//Attempt to retrieve an existing account ID to work with in order to avoid looking up the account ID again
			//Perform lower case comparison to account for misspelled versions of the name
			//LoL internally merges these to a mangled "internal name" for lookups anyways
			SQLCommand nameLookup = Command("select id, account_id, summoner_name, summoner_level from summoner where region = cast(:region as region_type) and lower(summoner_name) = lower(:name)");
			nameLookup.SetEnum("region", RegionProfile.RegionEnum);
			nameLookup.Set("name", job.SummonerName);

			object[] row = nameLookup.ExecuteSingleRow();
			if (row != null)
			{
				//The summoner already exists in the database
				int id = (int) row[0];
				int accountId = (int) row[1];
				string name = (string) row[2];
				int summonerLevel = (int) row[3];
				//This is not entirely correct as the name may have changed, but whatever
				UpdateSummoner(new SummonerDescription(name, id, accountId), false);
				job.ProvideResult(name, accountId, summonerLevel);

				return;
			}

			//We might be dealing with a new summoner
			PublicSummoner publicSummoner = RPC.GetSummonerByName(job.SummonerName);
			if (publicSummoner == null)
			{
				WriteLine("No such summoner: {0}", job.SummonerName);
				job.ProvideResult(JobQueryResult.NotFound);
				return;
			}

			SQLCommand check = Command("select id from summoner where account_id = :account_id and region = cast(:region as region_type)");
			check.Set("account_id", publicSummoner.acctId);
			check.SetEnum("region", RegionProfile.RegionEnum);

			row = check.ExecuteSingleRow();
			if (row != null)
			{
				//We are dealing with an existing summoner even though the name lookup failed
				int id = (int) row[0];

				UpdateSummoner(new SummonerDescription(publicSummoner.name, id, publicSummoner.acctId), true);
			}
			else
				InsertNewSummoner(publicSummoner.acctId, publicSummoner.summonerId, publicSummoner.name, publicSummoner.internalName, publicSummoner.summonerLevel, publicSummoner.profileIconId);

			job.ProvideResult(publicSummoner.name, publicSummoner.acctId, publicSummoner.summonerLevel);
		}
	}
}
