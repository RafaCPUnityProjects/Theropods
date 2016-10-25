/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Collision.cs"
 * 
 *	This script allows colliders that block the Player's movement
 *	to be turned on and off easily via actions.
 * 
 */

using UnityEngine;
using System.Collections;

public class _Collision : MonoBehaviour
{
	
	[HideInInspector] public bool showInEditor = false;
	
	
	void TurnOn ()
	{
		this.GetComponent<Collider>().enabled = true;
		this.gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
	}
	
	
	void TurnOff ()
	{
		this.GetComponent<Collider>().enabled = false;
		this.gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
	}
	
	
	private void OnDrawGizmos ()
	{
		if (showInEditor)
		{
			DrawGizmos ();
		}
	}
	
	
	private void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}
	
	
	private void DrawGizmos ()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color (0f, 1f, 1f, 0.8f);
		Gizmos.DrawCube (Vector3.zero, Vector3.one);
	}
	
}
