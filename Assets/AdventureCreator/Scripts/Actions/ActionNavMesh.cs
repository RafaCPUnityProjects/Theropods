/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionNavMesh.cs"
 * 
 *	This action changes the active NavMesh.
 *	All NavMeshes must be on the same unique layer.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionNavMesh : Action
{
	
	public NavigationMesh newNavMesh;
	
	
	public ActionNavMesh ()
	{
		this.isDisplayed = true;
		title = "Engine: Change NavMesh";
	}
	
	
	override public float Run ()
	{
		if (newNavMesh)
		{
			SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			NavigationMesh oldNavMesh = sceneSettings.navMesh;
			oldNavMesh.TurnOff ();
			newNavMesh.TurnOn ();
			sceneSettings.navMesh = newNavMesh;
		}
		
		return 0f;
	}
	

	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		SceneSettings sceneSettings = null;
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		}
		
		if ((sceneSettings && sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider) || (sceneSettings == null))
		{
			newNavMesh = (NavigationMesh) EditorGUILayout.ObjectField ("New NavMesh:", newNavMesh, typeof (NavigationMesh), true);
		}
		else
		{
			GUILayout.Label ("This action is only compatible with the Mesh Collider\nNavigation method, as set in the Scene Manager.");
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (newNavMesh)
		{
			labelAdd = " (" + newNavMesh.gameObject.name + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}