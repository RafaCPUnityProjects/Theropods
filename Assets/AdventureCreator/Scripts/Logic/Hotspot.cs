/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Hotspot.cs"
 * 
 *	This script handles all the possible
 *	interactions on both hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class Hotspot : MonoBehaviour
{

	public bool showInEditor = true;
	
	public string hotspotName;
	public Highlight highlight;
	public bool playUseAnim;
	public Marker walkToMarker;
	public int lineID = -1;
	
	public bool provideUseInteraction;
	public Button useButton = new Button();
	
	public List<Button> useButtons = new List<Button>();
	public bool oneClick = false;
	
	public bool provideLookInteraction;
	public Button lookButton = new Button();
	
	public bool provideInvInteraction;
	public List<Button> invButtons = new List<Button>();

	
	private void TurnOn ()
	{
		gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
	}
	
	
	private void TurnOff ()
	{
		gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
	}
	
	
	public bool IsOn ()
	{
		if (gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer))
		{
			return false;
		}

		return true;
	}
	
	
	public void Select ()
	{
		if (highlight)
		{
			highlight.HighlightOn ();
		}
	}
	
	
	public void Deselect ()
	{
		if (highlight)
		{
			highlight.HighlightOff ();
		}
	}


	public bool IsSingleInteraction ()
	{
		if (oneClick && provideUseInteraction && useButtons != null && useButtons.Count == 1 && !useButtons[0].isDisabled && (invButtons == null || invButtons.Count == 0))
		{
			return true;
		}

		return false;
	}
	
	
	public void DeselectInstant ()
	{
		if (highlight)
		{
			highlight.HighlightOffInstant ();
		}
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
		if (this.GetComponent <AC.Char>() == null)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color (1f, 1f, 0f, 0.6f);
			Gizmos.DrawCube (Vector3.zero, Vector3.one);
		}
	}


	public bool HasContextUse ()
	{
		if (!oneClick && provideUseInteraction && useButton != null && !useButton.isDisabled)
		{
			return true;
		}

		if (oneClick && provideUseInteraction && useButtons != null && useButtons.Count == 1 && !useButtons[0].isDisabled)
		{
			return true;
		}

		return false;
	}


	public bool HasContextLook ()
	{
		if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
		{
			return true;
		}

		return false;
	}

}