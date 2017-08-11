using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour {

	Color currentColor;
	Color defaultColor;
	float remainingTimeNormalized;
	Renderer renderer;

	public enum Transition
	{
		Immediate,
		Gradual
	}

	void Awake ()
	{
		currentColor = Color.black;
		defaultColor = Color.black;
		renderer = GetComponentInChildren<Renderer> ();
		renderer.material.color = defaultColor;
		enabled = false;
	}

	void Update ()
	{
		remainingTimeNormalized -= Time.deltaTime / (DemoManager.instance.GetWaitSeconds () * 4f);
		renderer.material.color = Color.Lerp (defaultColor, currentColor, remainingTimeNormalized);
		if (remainingTimeNormalized <= 0f)
		{
			currentColor = defaultColor;
			enabled = false;
		}
	}

	public void ChangeColor (Color newColor, Transition transition)
	{
		remainingTimeNormalized = 1f;
		if (Transition.Immediate == transition)
		{
			currentColor = newColor;
			renderer.material.color = currentColor;
		}
		else if (Transition.Gradual == transition)
		{
			defaultColor = newColor;
		}
		enabled = true;
	}

	public void ChangeDefault(Color newDefault)
	{
		defaultColor = newDefault;
	}
}
