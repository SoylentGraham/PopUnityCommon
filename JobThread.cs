using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


class JobThread
{
	public bool				debug = true;

	List<System.Action>		jobs = new List<System.Action>();
	bool					run = true;
	Thread					thread;

	ManualResetEvent		IsIdle = new ManualResetEvent(true);

	public JobThread()
	{
	}

	public void				PushJob(System.Action Job)
	{
		jobs.Add (Job);
		IsIdle.Reset ();	//	no longer idle
		Wake();
	}

	void Wake()
	{
		//	thread has finished or thrown exception
		if ( thread != null && !thread.IsAlive) {

			//	hiccup? mark as idle so WaitFor doesn't block
			OnIdle();

			if ( debug )
				Debug.Log ("Worker thread not alive: " + jobs.Count + " jobs");
			run = false;
			if ( debug )
				Debug.Log ("Worker thread joining...");
			thread.Join ();
			thread = null;
	
			if ( debug )
				Debug.Log ("Worker thread null'd");
		}

		if ( thread == null && run )
		{
			thread = new Thread (new ThreadStart (Iterator));
			thread.Start();
			//	spin to startup thread
			while (!thread.IsAlive) {
				if ( debug )
					Debug.Log ("Spinning to startup thread");
			}
		}
	}

	void					Iterator()
	{
		try
		{
			//while ( run )
			while (true)
			{
				if (jobs.Count == 0) {
					OnIdle ();

					//	wait for job notification here
					Thread.Sleep (1);
					continue;
				} 

				//	run through all the jobs, lock so we know they're busy
				if ( debug )
					print(jobs.Count + " Jobs todo");

				while (jobs.Count > 0) {
					var Job0 = jobs [0];
					if ( Job0 != null )
					{
						Job0.Invoke ();
					}
					jobs.RemoveAt (0);
				}
			}
		}
		catch(System.Exception e) {
			Debug.Log ("Worker thread exception... clearing " + jobs.Count + " jobs");
			Debug.LogException (e);
			jobs.Clear ();
			OnIdle ();
		}
	}

	void OnIdle()
	{
		IsIdle.Set ();
	}

	public void				WaitForJobs()
	{
		Wake ();

		if ( debug )
			print ("Waiting for " + jobs.Count + " jobs.");
		
		if (jobs.Count > 0) {
			IsIdle.WaitOne ();
		}
	}
}


