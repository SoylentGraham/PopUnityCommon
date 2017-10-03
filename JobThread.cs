using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


//	from http://answers.unity3d.com/questions/754873/c-how-to-make-a-job-queue-for-noise-function.html
public abstract class JobPool<JOBTYPE>
{
	class TJob
	{
		public JOBTYPE			job;
		public ManualResetEvent	doneEvent; // a flag to signal when the work is complete
		public JobPool<JOBTYPE>	parent;

		public TJob(JOBTYPE _job,JobPool<JOBTYPE> _parent)
		{
			job = _job;
			parent = _parent;
			doneEvent = new ManualResetEvent(false);
			doneEvent.Reset();
		}
			
		public void		ThreadPoolCallback (System.Object o)
		{
			doneEvent.Reset();
			parent.DoExecuteJob (this.job);
			parent.RemoveJob (this);
			doneEvent.Set ();
		}
	};


	public bool				debug = true;
	public float			jobProgress
	{
		get
		{
			return jobsCompleted / (float)Mathf.Max(1,jobsPending+jobsCompleted);
		}
	}
	public int				jobsCompleted
	{
		get {
			return jobsCompletedCount + jobsExceptionCount;
		}		
	}
	public int				jobsPending
	{
		get
		{
			return (jobs!=null) ? jobs.Count : 0;
		}
	}
	int						jobsCompletedCount = 0;
	int						jobsExceptionCount = 0;

	List<TJob>				jobs = new List<TJob>();
	List<ManualResetEvent>	jobDoneEvents = new List<ManualResetEvent> ();
	bool					run = true;
	Thread					thread;

	protected abstract void	ExecuteJob (JOBTYPE Job);

	private void			DoExecuteJob(JOBTYPE Job)
	{
		try {
			ExecuteJob (Job);
			jobsCompletedCount++;
		} catch {
			jobsExceptionCount++;
		}
	}


	public void				PushJob(JOBTYPE Job)
	{
		var NewJob = new TJob (Job, this);
		lock (jobDoneEvents) {
			jobs.Add (NewJob);
			jobDoneEvents.Add (NewJob.doneEvent);
		}
		ThreadPool.QueueUserWorkItem ( NewJob.ThreadPoolCallback );
	}

	private void				RemoveJob(TJob Job)
	{
		lock (jobDoneEvents) {
			jobDoneEvents.Remove (Job.doneEvent);
			jobs.Remove (Job);
		}
	}

	public void				WaitForJobs(int TimeoutMs)
	{
		var NonNullEvents = new List<ManualResetEvent> ();
		int NullCount = 0;
		lock (jobDoneEvents) {
			foreach (var e in jobDoneEvents) {
				if (e != null)
					NonNullEvents.Add (e);
				NullCount++;
			}
		}
		var WaitHandles = NonNullEvents.ToArray ();
		Debug.Log ("Wait for jobs x" + WaitHandles.Length + " (" + NullCount + " nulls");
		if (WaitHandles == null)
			return;

		if ( TimeoutMs == -1 )
			WaitHandle.WaitAll (WaitHandles);
		else
			WaitHandle.WaitAll (WaitHandles,TimeoutMs);
	}

	public void				Shutdown()
	{
		WaitForJobs (-1);
	}
};



public class JobPool_Action : JobPool<System.Action>
{
	protected override void	ExecuteJob (System.Action Job)
	{
		if (Job != null)
			Job.Invoke ();
	}
}



public abstract class JobThread_Base<JOBTYPE>
{

	public bool				debug = true;
	public float			jobProgress
	{
		get
		{
			return jobsCompleted / (float)Mathf.Max(1,jobsPending+jobsCompleted);
		}
	}
	public int				jobsCompleted
	{
		get {
			return jobsCompletedCount;
		}		
	}
	public int				jobsPending
	{
		get
		{
			return (jobs!=null) ? jobs.Count : 0;
		}
	}
	int						jobsCompletedCount = 0;

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
			while ( run )
			//while (true)
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
					jobsCompletedCount++;
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
