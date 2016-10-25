/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DetectHotspots.cs"
 * 
 *	This script is used to determine which
 *	active Hotspot is nearest the player.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class DetectHotspots : MonoBehaviour
	{

		[HideInInspector] public Hotspot nearestHotspot;
		private List<Hotspot> hotspots = new List<Hotspot>();


		private void OnLevelWasLoaded ()
		{
			hotspots.Clear ();
		}


		private void OnTriggerStay (Collider other)
		{
			if (other.GetComponent <Hotspot>() && other.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				if (nearestHotspot == null || (Vector3.Distance (transform.position, other.transform.position) <= Vector3.Distance (transform.position, nearestHotspot.transform.position)))
				{
					nearestHotspot = other.GetComponent <Hotspot>();
				}

				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot == other.GetComponent <Hotspot>())
					{
						return;
					}
				}

				hotspots.Add (other.GetComponent <Hotspot>());
			}
         }


		private void OnTriggerExit (Collider other)
		{
			if (other.GetComponent <Hotspot>())
			{
				if (nearestHotspot == other.GetComponent <Hotspot>())
				{
					nearestHotspot = null;
				}

				if (IsHotspotInTrigger (other.GetComponent <Hotspot>()))
				{
					hotspots.Remove (other.GetComponent <Hotspot>());
				}
			}
		}


		private void Update ()
		{
			if (nearestHotspot && nearestHotspot.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer))
			{
				nearestHotspot = null;
			}
		}


		public bool IsHotspotInTrigger (Hotspot hotspot)
		{
			if (hotspots.Contains (hotspot))
			{
				return true;
			}

			return false;
		}

	}

}