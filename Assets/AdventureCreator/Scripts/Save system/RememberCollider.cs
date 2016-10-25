/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberCollider.cs"
 * 
 *	This script is attached to Colliders in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class RememberCollider : ConstantID
{
	
	public AC_OnOff startState = AC_OnOff.On;
	
	
	public void Awake ()
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		
		if (settingsManager && GameIsPlaying () && GetComponent<Collider>())
		{
			if (startState == AC_OnOff.On)
			{
				GetComponent<Collider>().enabled = true;
			}
			else
			{
				GetComponent<Collider>().enabled = false;
			}
		}
	}
	
	
	public ColliderData SaveData ()
	{
		ColliderData colliderData = new ColliderData ();

		colliderData.objectID = constantID;
		colliderData.isOn = false;

		if (GetComponent<Collider>())
		{
			colliderData.isOn = GetComponent<Collider>().enabled;
		}

		return (colliderData);
	}
	
	
	public void LoadData (ColliderData data)
	{
		if (GetComponent<Collider>())
		{
			if (data.isOn)
			{
				GetComponent<Collider>().enabled = true;
			}
			else
			{
				GetComponent<Collider>().enabled = false;
			}
		}
	}

}


[System.Serializable]
public class ColliderData
{
	public int objectID;
	public bool isOn;

	public ColliderData () { }
}