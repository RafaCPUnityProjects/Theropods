/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvItem.cs"
 * 
 *	This script is a container class for individual inventory items.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InvItem
{

	public int count;
	public Texture2D tex;
	public bool carryOnStart;
	public bool canCarryMultiple;
	public string label;
	public int id;
	public int lineID = -1;
	public int binID;
	public bool isEditing = false;
	
	public InvActionList useActionList;
	public InvActionList lookActionList;
	public List<InvInteraction> interactions;
	public List<InvActionList> combineActionList;
	public List<int> combineID;

	
	/*public InvItem ()
	{
		count = 0;
		tex = null;
		label = "Inventory item " + (id + 1).ToString ();
		id = 0;
		lineID = -1;
		binID = -1;
	}*/
	
	
	public InvItem (int[] idArray)
	{
		count = 0;
		tex = null;
		id = 0;
		binID = -1;

		interactions = new List<InvInteraction>();

		combineActionList = new List<InvActionList>();
		combineID = new List<int>();

		// Update id based on array
		foreach (int _id in idArray)
		{
			if (id == _id)
				id ++;
		}

		label = "Inventory item " + (id + 1).ToString ();
	}
	
	
	public InvItem (InvItem assetItem)
	{
		count = assetItem.count;
		tex = assetItem.tex;
		id = assetItem.id;
		label = assetItem.label;
		useActionList = assetItem.useActionList;
		lookActionList = assetItem.lookActionList;
		combineActionList = assetItem.combineActionList;
		combineID = assetItem.combineID;
		interactions = assetItem.interactions;
		binID = assetItem.binID;
	}
	
}