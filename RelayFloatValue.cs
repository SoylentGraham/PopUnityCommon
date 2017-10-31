using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class RelayFloatValue : MonoBehaviour
{

	public bool SetDefaultOnStart = true;
	public bool SetDefaultOnDestroy = true;
	public float DefaultValue;
    public float Value;
    public UnityEvent_float OnUpdate;

    void Start()
    {
		if ( SetDefaultOnStart )
	        OnUpdate.Invoke(DefaultValue);
    }

    void Update()
    {
        OnUpdate.Invoke(Value);
    }

    void OnDestroy()
    {
		if ( SetDefaultOnDestroy )
	        OnUpdate.Invoke(DefaultValue);
    }
}
