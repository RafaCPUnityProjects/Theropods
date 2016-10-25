/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ConstantID.cs"
 * 
 *	This script is used by the Serialization classes to store a permanent ID
 *	of the gameObject (like InstanceID, only retained after reloading the project).
 *	To save a reference to an arbitrary object in a scene, this script must be attached to it.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ConstantID : MonoBehaviour
{
	public int constantID;
	
	private bool isNewInstance = true;
	

	protected bool GameIsPlaying ()
	{
		#if UNITY_EDITOR
		
		if (!Application.isPlaying)
		{
			return false;
		}

		#endif

		return true;
	}


	#if UNITY_EDITOR
	
	protected void Update ()
	{
		if (gameObject.activeInHierarchy)
		{
			if (constantID == 0)
			{
				SetNewID ();
			}
			
			if (isNewInstance)
			{
				isNewInstance = false;
				CheckForDuplicateIDs ();
			}
		}
	}
	

	private void SetNewID ()
	{
		constantID = GetInstanceID ();
		EditorUtility.SetDirty (this);
		Debug.Log ("Set new ID for " + this.name + " : " + constantID);
	}
	
	
	private void CheckForDuplicateIDs ()
	{
		ConstantID[] idScripts = FindObjectsOfType (typeof (ConstantID)) as ConstantID[];
		
		foreach (ConstantID idScript in idScripts)
		{
			if (idScript.constantID == constantID && idScript.GetInstanceID() != GetInstanceID())
			{
				SetNewID ();
				break;
			}
		}
	}
	
	#endif
	
}
