/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCamera.cs"
 * 
 *	This action controls the MainCamera's "activeCamera",
 *	i.e., which GameCamera it is attached to.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCamera : Action
{
	
	public _Camera linkedCamera;
	public float transitionTime;
	public MoveMethod moveMethod;
	public bool returnToLast;
	
	
	public ActionCamera ()
	{
		this.isDisplayed = true;
		title = "Camera: Switch";
	}
	
	
	override public float Run ()
	{

		if (!isRunning)
		{
			isRunning = true;
			
			MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			
			if (mainCam)
			{
				_Camera cam = linkedCamera;
				
				if (returnToLast && mainCam.lastNavCamera)
				{
					cam = (_Camera) mainCam.lastNavCamera;
				}
				
				if (cam)
				{
					if (mainCam.attachedCamera != cam)
					{
						mainCam.SetGameCamera (cam);
						if (transitionTime > 0f)
						{
							if (linkedCamera is GameCamera25D)
							{
								mainCam.SnapToAttached ();
								Debug.LogWarning ("Switching to a 2.5D camera (" + linkedCamera.name + ") must be instantaneous.");
							}
							else
							{
								mainCam.SmoothChange (transitionTime, moveMethod);
								
								if (willWait)
								{
									return (transitionTime);
								}
							}
						}
						else
						{
							if (!returnToLast)
							{
								linkedCamera.MoveCameraInstant ();
							}
							mainCam.SnapToAttached ();
						}
					}
				}
			}
		}
		else
		{
			isRunning = false;
		}
		
		return 0f;

	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		returnToLast = EditorGUILayout.Toggle ("Return to last gameplay?", returnToLast);
		
		if (!returnToLast)
		{
			linkedCamera = (_Camera) EditorGUILayout.ObjectField ("New camera:", linkedCamera, typeof(_Camera), true);
		}
		
		if (linkedCamera is GameCamera25D && !returnToLast)
		{
			transitionTime = 0f;
		}
		else
		{
			transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
			
			if (transitionTime > 0f)
			{
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			}
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		if (linkedCamera && !returnToLast)
		{
			labelAdd = " (" + linkedCamera.name + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}