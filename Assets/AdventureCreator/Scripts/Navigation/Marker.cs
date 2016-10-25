/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Marker.cs"
 * 
 *	This script allows a simple way of teleporting
 *	characters and objects around the scene.
 * 
 */

using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour
{

	private void Awake ()
	{
		if (this.GetComponent<Renderer>())
		{
			this.GetComponent<Renderer>().enabled = false;
		}
	}
	
}
