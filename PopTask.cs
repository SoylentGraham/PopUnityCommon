using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TTask
{
	public System.Action	Function;
	public string			Description;

	public TTask(string _Description,System.Action _Function)
	{
		this.Function = _Function;
		this.Description = _Description;
	}
};




[ExecuteInEditMode]
public abstract class PopTaskManager : MonoBehaviour {

	public PopTaskProgressBar	Progress;
	protected List<TTask>		Tasks;

	abstract protected void		PopulateTasks();
	virtual protected void		OnTaskException(System.Exception e)
	{
	}

	IEnumerator RunTasks()
	{
		Tasks = new List<TTask>();
		PopulateTasks();

		if ( Progress )
			Progress.InitProgress( this.name, (uint)Tasks.Count );

		for (int t = 0; t < Tasks.Count; t++)
		{
			var Task = Tasks[t];

			try
			{
				if ( Progress )
					Progress.UpdateProgress(Task.Description);
				Task.Function.Invoke();
			}
			catch (System.Exception e)
			{
				OnTaskException(e);
				Debug.LogException(e);
				if ( Progress )
					Progress.FinishProgress(false);
				yield break;
			}

			yield return null;
		}

		if ( Progress )
			Progress.FinishProgress(true);
	}

	public void Execute()
	{
		StartCoroutine( RunTasks() );
	}
};

