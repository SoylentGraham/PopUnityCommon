using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{

    [Header("Normal triggers")]
	public UnityEvent OnTriggered1;
	public UnityEvent OnTriggered2;
	public UnityEvent OnTriggered3;
	public UnityEvent OnTriggered4;
	public UnityEvent OnTriggered5;

    public void Trigger1()
	{
		OnTriggered1.Invoke();
	}
	public void Trigger2()
	{
		OnTriggered2.Invoke();
	}
	public void Trigger3()
	{
		OnTriggered3.Invoke();
    }
    public void Trigger4()
    {
		OnTriggered4.Invoke();
    }
    public void Trigger5()
    {
		OnTriggered5.Invoke();
    }


}
