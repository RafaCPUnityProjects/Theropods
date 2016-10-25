/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberVisibility.cs"
 * 
 *	This script is attached to scene objects
 *	whose renderer.enabled state we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class RememberVisibility : ConstantID
{
	
	public AC_OnOff startState = AC_OnOff.On;
	
	
	public void Awake ()
	{
		if (GetComponent<Renderer>() && GameIsPlaying ())
		{
			if (startState == AC_OnOff.On)
			{
				GetComponent<Renderer>().enabled = true;
			}
			else
			{
				GetComponent<Renderer>().enabled = false;
			}
		}
	}


	public VisibilityData SaveData ()
	{
		VisibilityData visibilityData = new VisibilityData ();
		visibilityData.objectID = constantID;
		
		if (GetComponent<Renderer>())
		{
			visibilityData.isOn = GetComponent<Renderer>().enabled;
		}
		
		return (visibilityData);
	}


	public void LoadData (VisibilityData data)
	{
		if (GetComponent<Renderer>())
		{
			GetComponent<Renderer>().enabled = data.isOn;
		}
	}
	
}


[System.Serializable]
public class VisibilityData
{
	public int objectID;
	public bool isOn;
	
	public VisibilityData () { }
}