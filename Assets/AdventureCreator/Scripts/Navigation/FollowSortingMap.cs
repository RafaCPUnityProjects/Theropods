/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"FollowSortingMap.cs"
 * 
 *	This script causes any attached Sprite Renderer
 *	to change according to the scene's Sorting Map.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Reflection;
using AC;

public class FollowSortingMap : MonoBehaviour
{

	public bool followSortingMap = false;
	public bool offsetOriginal = false;

	private int offset;
	private int sortingOrder = 0;
	private string sortingLayer = "";
	private SortingMap sortingMap;


	private void OnLevelWasLoaded ()
	{
		Awake ();
	}


	public void Awake ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>() && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap != null)
		{
			sortingMap = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap;
		}

		if (offsetOriginal && GetComponent <SpriteRenderer>())
		{
			offset = GetComponent <SpriteRenderer>().sortingOrder;
		}
		else
		{
			offset = 0;
		}
	}


	private void Update ()
	{
		if (followSortingMap && GetComponent<Renderer>() && sortingMap != null && sortingMap.sortingAreas.Count > 0)
		{
			for (int i=0; i<sortingMap.sortingAreas.Count; i++)
			{
				// Determine angle between SortingMap's normal and relative position - if <90, must be "behind" the plane
				if (Vector3.Angle (sortingMap.transform.forward, sortingMap.GetAreaPosition (i) - transform.position) < 90f)
				{
					if (sortingMap.mapType == SortingMapType.OrderInLayer)
					{
						sortingOrder = sortingMap.sortingAreas [i].order;
					}
					else if (sortingMap.mapType == SortingMapType.SortingLayer)
					{
						sortingLayer = sortingMap.sortingAreas [i].layer;
					}

					break;
				}
			}
			
			if (sortingMap.mapType == SortingMapType.OrderInLayer)
			{
				GetComponent<Renderer>().sortingOrder = sortingOrder;

				if (offsetOriginal)
				{
					GetComponent<Renderer>().sortingOrder += offset;
				}
			}
			else if (sortingMap.mapType == SortingMapType.SortingLayer)
			{
				GetComponent<Renderer>().sortingLayerName = sortingLayer;
			}
		}
	}


	public float GetLocalScale ()
	{
		if (followSortingMap && sortingMap != null && sortingMap.affectScale)
		{
			return (sortingMap.GetScale (transform.position) / 100f);
		}

		return 0f;
	}


	public float GetLocalSpeed ()
	{
		if (followSortingMap && sortingMap != null && sortingMap.affectScale && sortingMap.affectSpeed)
		{
			return (sortingMap.GetScale (transform.position) / 100f);
		}
		
		return 1f;
	}

}
