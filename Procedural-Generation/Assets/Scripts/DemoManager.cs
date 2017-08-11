using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour { 

	public enum State
	{
		Pause,
		Play
	}

	public State currentState;

	public enum StepTime
	{
		wait2Sec,
		wait1Sec,
		wait0_5Sec,
		wait0_25Sec,
		wait0_125Sec,
		wait0_0625Sec,
		wait0_03125Sec
	}

	public StepTime currentStepTime = StepTime.wait0_25Sec;

	public WaitForSeconds currentWait;

	private List<WaitForSeconds> waits;
	private Dictionary<StepTime, float> waitSeconds;

	public static DemoManager instance = null; 

	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject); 
		}
		DontDestroyOnLoad(gameObject);


		waits = new List<WaitForSeconds> () {
			new WaitForSeconds (2f),
			new WaitForSeconds (1f),
			new WaitForSeconds (0.5f),
			new WaitForSeconds (0.25f),
			new WaitForSeconds (0.125f),
			new WaitForSeconds (0.0625f),
			new WaitForSeconds (0.03125f)
		};

		waitSeconds = new Dictionary<StepTime, float> ()
		{
			{ StepTime.wait2Sec, 2f },
			{ StepTime.wait1Sec, 1f },
			{ StepTime.wait0_5Sec, 0.5f },
			{ StepTime.wait0_25Sec, 0.25f },
			{ StepTime.wait0_125Sec, 0.125f },
			{ StepTime.wait0_0625Sec, 0.0625f },
			{ StepTime.wait0_03125Sec, 0.03125f },
		};

		currentWait = waits[(int) currentStepTime];

		Debug.Log ("Initial Wait Seconds:" + GetWaitSeconds ());
	}

	void Start ()
	{
		currentState = State.Pause;
	}

	public void IncreaseSpeed()
	{
		if ((int)currentStepTime < waits.Count - 1) {
			currentStepTime = (StepTime)((int)currentStepTime + 1);
			currentWait = waits[(int) currentStepTime];
			Debug.Log ("New Wait Seconds: " + GetWaitSeconds ());
		}
	}

	public void DecreaseSpeed()
	{
		if ((int)currentStepTime > 0) {
			currentStepTime = (StepTime)((int)currentStepTime - 1);
			currentWait = waits[(int) currentStepTime];
			Debug.Log ("New Wait Seconds: " + GetWaitSeconds ());
		}
	}

	public float GetWaitSeconds()
	{
		return waitSeconds [currentStepTime];
	}
}
