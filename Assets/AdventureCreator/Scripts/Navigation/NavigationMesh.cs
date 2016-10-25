/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationMesh.cs"
 * 
 *	This script is used by the MeshCollider
 *  navigation method to define the pathfinding area.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AC;

public class NavigationMesh : MonoBehaviour
{
	
	private int originalLayer;


	private void Awake ()
	{
		Hide ();
	}
	
	
	public void TurnOn ()
	{
		SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		
		if (sceneSettings && sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider)
		{
			if (LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.navMeshLayer) == -1)
			{
				Debug.LogWarning ("Can't find layer " + AdvGame.GetReferences ().settingsManager.navMeshLayer + " - please define it in the Tags Manager and list it in the Settings Manager.");
			}
			else if (AdvGame.GetReferences ().settingsManager.navMeshLayer != "")
			{
				gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.navMeshLayer);
			}
			
			if (GetComponent <Collider>() == null)
			{
				Debug.LogWarning ("A Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
			}
		}
		else if (sceneSettings)
		{
			Debug.LogWarning ("Cannot enable NavMesh " + this.name + " as this scene's Navigation Method is Unity Navigation.");
		}
		else
		{
			Debug.LogWarning ("Cannot enable NavMesh - no SceneSettings found.");
		}
	}
	
	
	public void TurnOff ()
	{
		gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
	}
	
	

	public void Hide ()
	{
		if (this.GetComponent <MeshRenderer>())
		{
			this.GetComponent <MeshRenderer>().enabled = false;
		}
	}
	
	
	public void Show ()
	{
		if (this.GetComponent <MeshRenderer>() && this.GetComponent <MeshFilter>() && this.GetComponent <MeshCollider>() && this.GetComponent <MeshCollider>().sharedMesh)
		{
			this.GetComponent <MeshFilter>().mesh = this.GetComponent <MeshCollider>().sharedMesh;
			this.GetComponent <MeshRenderer>().enabled = true;
			this.GetComponent <MeshRenderer>().castShadows = false;
			this.GetComponent <MeshRenderer>().receiveShadows = false;
		}
	}
	
}
