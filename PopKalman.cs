using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//	gr: from my old opencl kalman filter
//	https://gitlab.com/NewChromantics/PopLib/blob/master/data/TKalman.cl
//	which was originally from http://www.dzone.com/snippets/simple-kalman-filter-c
//	todo: document noise filter values. Generally use 0.001 to 1
//	todo: generic this, Everywhere with Vector3 can be swapped for another type
public class PopKalman_Vector3
{
	Vector3 x_est_last;
	Vector3 lastcorrected;
	float P_last;
	float QNoise;
	float RNoise;
	float TotalError;


	public PopKalman_Vector3(Vector3 InitialPos, float QNoise, float RNoise)
	{
		//initialize with a measurement
		//x_est_last = InitialPos;
		lastcorrected = InitialPos;
		P_last = 0;
		TotalError = 0;
		this.QNoise = QNoise;
		this.RNoise = RNoise;
	}

	//	returns error amount (deviation)
	public float Push(Vector3 Position)
	{
		//do a prediction
		var x_temp_est = x_est_last;
		var P_temp = P_last + QNoise;

		//calculate the Kalman gain
		//	gr: timestep here?
		var KGain = P_temp * (1.0f / (P_temp + RNoise));

		//	measured input
		var Accell = Position - lastcorrected;
		var z_measured = Accell;

		//	do correction to the estimate
		var x_est = x_temp_est + KGain * (z_measured - x_temp_est);
		float P = (1 - KGain) * P_temp;

		//	error is difference from the corrected estimate
		var Error = Vector3.Distance(z_measured, x_est);
		TotalError += Error;

		//	update our last's
		P_last = P;
		x_est_last = x_est;

		lastcorrected = lastcorrected + x_est_last;

		return Error;
	}

	public Vector3 GetAcceleration()
	{
		return x_est_last;
	}


	public Vector3 GetFilteredPosition()
	{
		return GetEstimatedPosition(0);
	}

	public Vector3 GetEstimatedPosition(float TimeDelta)
	{
		var Accell = x_est_last;
		var NewPos = lastcorrected + (Accell * TimeDelta);
		return NewPos;
	}
}


public class PopKalman_Vector2
{
	Vector2 x_est_last;
	Vector2 lastcorrected;
	float P_last;
	float QNoise;
	float RNoise;
	float TotalError;


	public PopKalman_Vector2(Vector2 InitialPos, float QNoise, float RNoise)
	{
		//initialize with a measurement
		//x_est_last = InitialPos;
		lastcorrected = InitialPos;
		P_last = 0;
		TotalError = 0;
		this.QNoise = QNoise;
		this.RNoise = RNoise;
	}

	//	returns error amount (deviation)
	public float Push(Vector2 Position)
	{
		//do a prediction
		var x_temp_est = x_est_last;
		var P_temp = P_last + QNoise;

		//calculate the Kalman gain
		//	gr: timestep here?
		var KGain = P_temp * (1.0f / (P_temp + RNoise));

		//	measured input
		var Accell = Position - lastcorrected;
		var z_measured = Accell;

		//	do correction to the estimate
		var x_est = x_temp_est + KGain * (z_measured - x_temp_est);
		float P = (1 - KGain) * P_temp;

		//	error is difference from the corrected estimate
		var Error = Vector2.Distance(z_measured, x_est);
		TotalError += Error;

		//	update our last's
		P_last = P;
		x_est_last = x_est;

		lastcorrected = lastcorrected + x_est_last;

		return Error;
	}

	public Vector2 GetAcceleration()
	{
		return x_est_last;
	}


	public Vector2 GetFilteredPosition()
	{
		return GetEstimatedPosition(0);
	}

	public Vector2 GetEstimatedPosition(float TimeDelta)
	{
		var Accell = x_est_last;
		var NewPos = lastcorrected + (Accell * TimeDelta);
		return NewPos;
	}
}

