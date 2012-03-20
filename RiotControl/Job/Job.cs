﻿using System.Threading;

namespace RiotControl
{
	enum JobQueryResult
	{
		Success,
		NotFound,
		Timeout,
	};

	class Job
	{
		public AutoResetEvent ResultEvent;

		public JobQueryResult Result;

		public Job()
		{
			ResultEvent = new AutoResetEvent(false);
		}

		public void Execute()
		{
			ResultEvent.WaitOne();
		}

		public void ProvideResult(JobQueryResult result)
		{
			Result = result;
			ResultEvent.Set();
		}
	}
}
