using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisualHandler : MonoBehaviour
{
	public Toggle ForceToggle;

	public Toggle PlaneToggle;

	// A method to change the "force" toggle red and green
	public void changeForceToggleColor()
	{
		if (ForceToggle.isOn)
		{
			Color cl = Color.green;
			ForceToggle.GetComponent<Image>().color = cl;
		}
		else
		{
			Color cl = Color.red;
			ForceToggle.GetComponent<Image>().color = cl;
		}
	}

	public void changePlaneToggleColor()
	{
		if (PlaneToggle.isOn)
		{
			Color cl = Color.green;
			PlaneToggle.GetComponent<Image>().color = cl;
		}
		else
		{
			Color cl = Color.red;
			PlaneToggle.GetComponent<Image>().color = cl;
		}
	}
}
