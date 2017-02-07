using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


public abstract class JobThread_Base<JOBTYPE>
{
	public bool				debug = true;

	List<JOBTYPE>			jobs = new List<JOBTYPE>();
	bool					run = true;
	Thread					thread;

	ManualResetEvent		IsIdle = new ManualResetEvent(true);

	public abstract void	ExecuteJob (JOBTYPE Job);

	public JobThread_Base()
	{
	}

	public void				PushJob(JOBTYPE Job)
	{
		jobs.Add (Job);
		IsIdle.Reset ();	//	no longer idle
		Wake();
	}

	public void				Shutdown()
	{
		run = false;

		if (thread != null) {

			Debug.Log ("Shutdown Worker thread joining...");
			thread.Join ();
			thread = null;
			Debug.Log ("Shutdown Worker thread null'd");
		}
	}

	void Wake()
	{
		//	thread has finished or thrown exception
		if ( thread != null && !thread.IsAlive) {

			//	hiccup? mark as idle so WaitFor doesn't block
			OnIdle();

			if ( debug )
				Debug.Log ("Worker thread not alive: " + jobs.Count + " jobs");

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
					Debug.Log(jobs.Count + " Jobs todo");

				while (jobs.Count > 0) {
					var Job0 = jobs [0];
					ExecuteJob( Job0 );
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

	public void				WaitForJobs(int TimeoutMs=0)
	{
		Wake ();

		if ( debug )
			Debug.Log ("Waiting for " + jobs.Count + " jobs.");
		
		if (jobs.Count > 0) {
			if ( TimeoutMs == 0 )
				IsIdle.WaitOne ();
			else
				IsIdle.WaitOne (TimeoutMs);
		}
	}
}


public class JobThread : JobThread_Base<System.Action>
{
	public override void	ExecuteJob (System.Action Job)
	{
		if (Job != null)
			Job.Invoke ();
	}

}
