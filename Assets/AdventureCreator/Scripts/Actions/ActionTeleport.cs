/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionTeleport.cs"
 * 
 *	This action moves an object to a specified GameObject's position.
 *	Markers are helpful in this regard.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionTeleport : Action
{
	
	public bool isPlayer;
	public GameObject obToMove;
	public Marker teleporter;
	public bool copyRotation;
	
	
	public ActionTeleport ()
	{
		this.isDisplayed = true;
		title = "Object: Teleport";
	}
	
	
	override public float Run ()
	{
		if (isPlayer)
		{
			obToMove = GameObject.FindWithTag ("Player");
		}
		
		if (teleporter && obToMove)
		{
			obToMove.transform.position = teleporter.transform.position;
			
			if (copyRotation)
			{
				if (obToMove.GetComponent <Char>())
				{
					// Is a character, so set the lookDirection, otherwise will revert back to old rotation
					obToMove.GetComponent <Char>().SetLookDirection (teleporter.transform.forward, true);
					obToMove.GetComponent <Char>().Halt ();
				}
			
				obToMove.transform.rotation = teleporter.transform.rotation;
			}
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (!isPlayer)
		{
			obToMove = (GameObject) EditorGUILayout.ObjectField ("Object to move:", obToMove, typeof(GameObject), true);
		}

		teleporter = (Marker) EditorGUILayout.ObjectField ("Teleport to:", teleporter, typeof (Marker), true);
		copyRotation = EditorGUILayout.Toggle ("Copy rotation?", copyRotation);
		
		AfterRunningOption ();
	}

	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (teleporter)
		{
			if (obToMove)
			{
				labelAdd = " (" + obToMove.name + " to " + teleporter.name + ")";
			}
			else if (isPlayer)
			{
				labelAdd = " (Player to " + teleporter.name + ")";
			}
		}
		
		return labelAdd;
	}

	#endif
}