/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Inventory" tab of the main wizard.
 *	Inventory items are defined with this.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class InventoryManager : ScriptableObject
{

	public List<InvItem> items;
	public List<InvBin> bins;
	public InvActionList unhandledCombine;
	public InvActionList unhandledHotspot;

	#if UNITY_EDITOR

	private SettingsManager settingsManager;
	
	private InvItem selectedItem;
	private int invNumber = 0;
	private int binNumber = -1;

	private static GUIContent
		moveUpContent = new GUIContent("<", "Move up"),
		moveDownContent = new GUIContent(">", "Move down"),
		deleteContent = new GUIContent("-", "Delete item");

	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth (20f);
	

	public void ShowGUI ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences().settingsManager;
		}

		BinsGUI ();

		if (settingsManager == null || settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && settingsManager.inventoryInteractions == InventoryInteractions.ContextSensitive))
		{
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Unhandled events", EditorStyles.boldLabel);
			unhandledCombine = (InvActionList) EditorGUILayout.ObjectField ("Combine:", unhandledCombine, typeof (InvActionList), false);
			unhandledHotspot = (InvActionList) EditorGUILayout.ObjectField ("Use on hotspot:", unhandledHotspot, typeof (InvActionList), false);
		}

		List<string> labelList = new List<string>();
		foreach (InvItem _item in items)
		{
			labelList.Add (_item.label);
		}

		List<string> binList = new List<string>();
		foreach (InvBin bin in bins)
		{
			binList.Add (bin.label);
		}
		
		EditorGUILayout.Space ();
		CreateItemsGUI ();
		EditorGUILayout.Space ();

		if (selectedItem != null && items.Contains (selectedItem))
		{
			EditorGUILayout.LabelField ("Inventory item '" + selectedItem.label + "' properties", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical("Button");
				selectedItem.label = EditorGUILayout.TextField ("Name:", selectedItem.label);

				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
					if (bins.Count > 0)
					{
						binNumber = GetBinSlot (selectedItem.binID);
						binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
						selectedItem.binID = bins[binNumber].id;
					}
					else
					{
						selectedItem.binID = -1;
						EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
					}
				EditorGUILayout.EndHorizontal ();	

				selectedItem.tex = (Texture2D) EditorGUILayout.ObjectField ("Texture:", selectedItem.tex, typeof (Texture2D), false);
				selectedItem.carryOnStart = EditorGUILayout.Toggle ("Carry on start?", selectedItem.carryOnStart);
				selectedItem.canCarryMultiple = EditorGUILayout.Toggle ("Can carry multiple?", selectedItem.canCarryMultiple);

				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Standard interactions", EditorStyles.boldLabel);
				if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && settingsManager.inventoryInteractions == InventoryInteractions.ChooseInventoryThenInteraction && AdvGame.GetReferences ().cursorManager)
				{
					CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;

					List<string> iconList = new List<string>();
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						iconList.Add (icon.label);
					}

					if (cursorManager.cursorIcons.Count > 0)
					{
						foreach (InvInteraction interaction in selectedItem.interactions)
						{
							EditorGUILayout.BeginHorizontal ();
							invNumber = GetIconSlot (interaction.icon.id);
							invNumber = EditorGUILayout.Popup (invNumber, iconList.ToArray());
							interaction.icon = cursorManager.cursorIcons[invNumber];

							interaction.actionList = (InvActionList) EditorGUILayout.ObjectField (interaction.actionList, typeof (InvActionList), false);
							
							if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (this, "Delete interaction");
								selectedItem.interactions.Remove (interaction);
								break;
							}
							EditorGUILayout.EndHorizontal ();
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("No interaction icons defined - please use the Cursor Manager", MessageType.Warning);
					}
					if (GUILayout.Button ("Add interaction"))
					{
						Undo.RecordObject (this, "Add new interaction");
						selectedItem.interactions.Add (new InvInteraction (cursorManager.cursorIcons[0]));
					}
				}
				else
				{
					selectedItem.useActionList = (InvActionList) EditorGUILayout.ObjectField ("Use:", selectedItem.useActionList, typeof (InvActionList), false);
					selectedItem.lookActionList = (InvActionList) EditorGUILayout.ObjectField ("Examine:", selectedItem.lookActionList, typeof (InvActionList), false);
				}
				
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Combine interactions", EditorStyles.boldLabel);
				for (int i=0; i<selectedItem.combineActionList.Count; i++)
				{
					EditorGUILayout.BeginHorizontal ();
						invNumber = GetArraySlot (selectedItem.combineID[i]);
						invNumber = EditorGUILayout.Popup (invNumber, labelList.ToArray());
						selectedItem.combineID[i] = items[invNumber].id;
					
						selectedItem.combineActionList[i] = (InvActionList) EditorGUILayout.ObjectField (selectedItem.combineActionList[i], typeof (InvActionList), false);
					
						if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
						{
							Undo.RecordObject (this, "Delete combine event");
							selectedItem.combineActionList.RemoveAt (i);
							selectedItem.combineID.RemoveAt (i);
							break;
						}
					EditorGUILayout.EndHorizontal ();
				}
				if (GUILayout.Button ("Add combine event"))
				{
					Undo.RecordObject (this, "Add new combine event");
					selectedItem.combineActionList.Add (null);
					selectedItem.combineID.Add (0);
				}
				
			EditorGUILayout.EndVertical();
			
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (this);
		}
	}


	private void BinsGUI ()
	{
		EditorGUILayout.LabelField ("Categories", EditorStyles.boldLabel);
		
		foreach (InvBin bin in bins)
		{
			EditorGUILayout.BeginHorizontal ();
			bin.label = EditorGUILayout.TextField (bin.label);
			
			if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
			{
				Undo.RecordObject (this, "Delete category: " + bin.label);
				bins.Remove (bin);
				break;
			}
			EditorGUILayout.EndHorizontal ();

		}
		if (GUILayout.Button ("Create new category"))
		{
			Undo.RecordObject (this, "Add category");
			List<int> idArray = new List<int>();
			foreach (InvBin bin in bins)
			{
				idArray.Add (bin.id);
			}
			idArray.Sort ();
			bins.Add (new InvBin (idArray.ToArray ()));
		}
	}


	private void CreateItemsGUI ()
	{
		EditorGUILayout.LabelField ("Inventory items", EditorStyles.boldLabel);

		foreach (InvItem item in items)
		{
			EditorGUILayout.BeginHorizontal ();
			
			string buttonLabel = item.label;
			if (buttonLabel == "")
			{
				buttonLabel = "(Untitled)";	
			}
			if (GUILayout.Toggle (item.isEditing, item.id + ": " + buttonLabel, "Button"))
			{
				if (selectedItem != item)
				{
					DeactivateAllItems ();
					ActivateItem (item);
				}
			}

			if (items[0] != item)
			{
				if (GUILayout.Button (moveUpContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (this, "Shift item up");
					
					int i = items.LastIndexOf (item);
					InvItem tempItem = item;
					items.Remove (item);
					items.Insert (i-1, tempItem);
					AssetDatabase.SaveAssets();
					
					break;
				}
			}
			
			if (items.LastIndexOf (item) < items.Count -1)
			{
				if (GUILayout.Button (moveDownContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (this, "Shift item down");

					int i = items.LastIndexOf (item);
					InvItem tempItem = item;
					items.Remove (item);
					items.Insert (i+1, tempItem);
					AssetDatabase.SaveAssets();
					
					break;
				}
			}

			if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (this, "Delete inventory item: " + item.label);
				
				DeactivateAllItems ();
				items.Remove (item);
				break;
			}
			
			EditorGUILayout.EndHorizontal ();
		}
		
		if (GUILayout.Button("Create new item"))
		{
			Undo.RecordObject (this, "Create inventory item");

			InvItem newItem = new InvItem (GetIDArray ());
			items.Add (newItem);
			DeactivateAllItems ();
			ActivateItem (newItem);
		}
	}


	private void ActivateItem (InvItem item)
	{
		item.isEditing = true;
		selectedItem = item;
	}
	
	
	private void DeactivateAllItems ()
	{
		foreach (InvItem item in items)
		{
			item.isEditing = false;
		}
		selectedItem = null;
	}
	
	#endif
	
	
	int[] GetIDArray ()
	{
		// Returns a list of id's in the list
		
		List<int> idArray = new List<int>();
		
		foreach (InvItem item in items)
		{
			idArray.Add (item.id);
		}
		
		idArray.Sort ();
		return idArray.ToArray ();
	}
	

	public string GetLabel (int _id)
	{
		// Return the label of inventory with ID _id
		string result = "";
		foreach (InvItem item in items)
		{
			if (item.id == _id)
			{
				result = item.label;
			}
		}
		
		return result;
	}


	private int GetIconSlot (int _id)
	{
		int i = 0;
		foreach (CursorIcon icon in AdvGame.GetReferences ().cursorManager.cursorIcons)
		{
			if (icon.id == _id)
			{
				return i;
			}
			i++;
		}

		return 0;
	}
	
	
	private int GetArraySlot (int _id)
	{
		int i = 0;
		foreach (InvItem item in items)
		{
			if (item.id == _id)
			{
				return i;
			}
			i++;
		}
		
		return 0;
	}


	private int GetBinSlot (int _id)
	{
		int i = 0;
		foreach (InvBin bin in bins)
		{
			if (bin.id == _id)
			{
				return i;
			}
			i++;
		}
		
		return 0;
	}


}