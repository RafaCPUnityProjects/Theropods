/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeInventory.cs"
 * 
 *	This script creates a local copy of the InventoryManager's items.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class RuntimeInventory : MonoBehaviour
{

	[HideInInspector] public List<InvItem> localItems;
	[HideInInspector] public InvActionList unhandledCombine;
	[HideInInspector] public InvActionList unhandledHotspot;
	
	[HideInInspector] public bool isLocked = true;
	[HideInInspector] public InvItem selectedItem = null;
	
	private InventoryManager inventoryManager;
	private RuntimeActionList runtimeActionList;
	
	
	public void Awake ()
	{
		selectedItem = null;
		GetReferences ();

		localItems.Clear ();
		GetItemsOnStart ();
		
		if (inventoryManager)
		{
			unhandledCombine = inventoryManager.unhandledCombine;
			unhandledHotspot = inventoryManager.unhandledHotspot;
		}
		else
		{
			Debug.LogError ("An Inventory Manager is required - please use the Adventure Creator window to create one.");
		}
		
	}
	
	
	private void GetReferences ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>())
		{
			runtimeActionList = GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>();
		}
		
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}
	}

	
	public void SetNull ()
	{
		selectedItem = null;
	}
	
	
	public void SelectItemByID (int _id)
	{
		// Only select if carrying
		bool found = false;
		
		foreach (InvItem item in localItems)
		{
			if (item.id == _id)
			{
				selectedItem = item;
				found = true;
			}
		}
		
		if (!found)
		{
			SetNull ();
			GetReferences ();
			Debug.LogWarning ("Want to select inventory item " + inventoryManager.GetLabel (_id) + " but player is not carrying it.");
		}
	}
	
	
	public void SelectItem (InvItem item)
	{
				
		if (selectedItem == item)
		{
			selectedItem = null;
		}
		else
		{
			selectedItem = item;
		}

	}
	
	
	private void GetItemsOnStart ()
	{
		if (inventoryManager)
		{
			foreach (InvItem item in inventoryManager.items)
			{
				if (item.carryOnStart)
				{
					item.count = 1;
					isLocked = false;
					localItems.Add (item);
				}
			}
		}
		else
		{
			Debug.LogError ("No Inventory Manager found - please use the Adventure Creator window to create one.");
		}
	}
	
	
	public void Add (int _id, int amount)
	{
		// Raise "count" by 1 for appropriate ID
		
		if (localItems.Count == 0)
		{
			isLocked = false;
		}
		
		foreach (InvItem item in localItems)
		{
			if (item.id == _id)
			{
				if (item.canCarryMultiple)
				{
					item.count += amount;
				}

				return;
			}
		}
		
		GetReferences ();

		if (inventoryManager)
		{
			// Not already carrying the item

			foreach (InvItem assetItem in inventoryManager.items)
			{
				if (assetItem.id == _id)
				{
					InvItem newItem = assetItem;
					
					if (!newItem.canCarryMultiple)
					{
						amount = 1;
					}
					
					newItem.count = amount;
					localItems.Add (newItem);
				}
			}
		}
	
	}
	
	
	public void Remove (int _id, int amount)
	{
		// Reduce "count" by 1 for appropriate ID

		foreach (InvItem item in localItems)
		{
			if (item.id == _id)
			{
				if (item.count > 0)
				{
					item.count -= amount;
				}
				if (item.count < 1)
				{
					localItems.Remove (item);
				}
				
				if (localItems.Count == 0)
				{
					isLocked = true;
				}
				
				break;
			}
		}
	}
	
	
	public string GetLabel (InvItem item)
	{
		if (Options.GetLanguage () > 0)
		{
			return (SpeechManager.GetTranslation (item.lineID, Options.GetLanguage ()));
		}
		else
		{
			return (item.label);
		}
	}
	
	
	public int GetCount (int _id)
	{
		// Return the count of inventory with ID _id
		
		foreach (InvItem item in localItems)
		{
			if (item.id == _id)
			{
				return (item.count);
			}
		}
		
		return 0;
	}


	public void Look (InvItem item)
	{
		GetReferences ();
		
		if (runtimeActionList && item.lookActionList)
		{
			runtimeActionList.Play (item.lookActionList);
		}
	}
	
	
	public void Use (InvItem item)
	{
		GetReferences ();
		
		if (runtimeActionList)
		{
			if (item.useActionList)
			{
				selectedItem = null;
				runtimeActionList.Play (item.useActionList);
			}
			else
			{
				SelectItem (item);
			}
		}
	}


	public void RunInteraction (int iconID)
	{
		GetReferences ();

		if (runtimeActionList)
		{
			foreach (InvInteraction interaction in selectedItem.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if (interaction.actionList)
					{
						runtimeActionList.Play (interaction.actionList);
					}
					break;
				}
			}
		}

		selectedItem = null;
	}


	public void ShowInteractions (InvItem item)
	{
		selectedItem = item;

		PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
		playerMenus.SetInteractionMenus (true);
	}
	
	
	public void Combine (InvItem item)
	{
		GetReferences ();
		
		if (item == selectedItem)
		{
			selectedItem = null;
		}
		else if (runtimeActionList)
		{
			bool foundMatch = false;
			for (int i=0; i<item.combineID.Count; i++)
			{
				if (item.combineID[i] == selectedItem.id && item.combineActionList[i])
				{
					selectedItem = null;
					runtimeActionList.Play (item.combineActionList [i]);
					foundMatch = true;
					break;
				}
			}
			
			if (!foundMatch)
			{
				selectedItem = null;
				
				if (unhandledCombine)
				{
					runtimeActionList.Play (unhandledCombine);
				}
			}
		}	
	}
	
	
	public List<InvItem> MatchInteractions ()
	{
		List<InvItem> items = new List<InvItem>();

		if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>())
		{
			PlayerInteraction playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
			
			if (playerInteraction.hotspot)
			{
				foreach (Button button in playerInteraction.hotspot.invButtons)
				{
					foreach (InvItem item in localItems)
					{
						if (item.id == button.invID && !button.isDisabled)
						{
							items.Add (item);
							break;
						}
					}
				}
			}

			else if (selectedItem != null)
			{
				foreach (int combineID in selectedItem.combineID)
				{
					foreach (InvItem item in localItems)
					{
						if (item.id == combineID)
						{
							items.Add (item);
							break;
						}
					}
				}
			}
		}
		
		return items;
	}
	
	
	private void OnEnable ()
	{
		runtimeActionList = null;
		inventoryManager = null;
	}

}