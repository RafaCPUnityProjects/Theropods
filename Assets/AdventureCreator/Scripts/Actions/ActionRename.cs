/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionRename.cs"
 * 
 *	This action renames Hotspots. A "Remember Name" script needs to be
 *	attached to said hotspot if the renaming is to carry across saved games.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionRename : Action
{
	
	public Hotspot hotspot;
	public string newName;
	
	
	public ActionRename ()
	{
		this.isDisplayed = true;
		title = "Hotspot: Rename";
	}
	
	
	override public float Run ()
	{
		if (hotspot && newName != "")
		{
			hotspot.hotspotName = newName;
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to rename:", hotspot, typeof (Hotspot), true);
		newName = EditorGUILayout.TextField ("New label:", newName);
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (hotspot && newName != "")
		{
			labelAdd = " (" + hotspot.name + " to " + newName + ")";
		}
		
		return labelAdd;
	}

	#endif

}