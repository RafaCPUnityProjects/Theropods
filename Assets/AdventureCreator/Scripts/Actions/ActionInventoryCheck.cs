/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventoryCheck.cs"
 * 
 *	This action checks to see if a particular inventory item
 *	is held by the player, and performs something accordingly.
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
public class ActionInventoryCheck : Action
{
	
	public int invID;
	private int invNumber;
	
	public bool doCount;
	public int intValue = 1;
	public enum IntCondition { EqualTo, NotEqualTo, LessThan, MoreThan };
	public IntCondition intCondition;
	
	public ResultAction resultActionTrue;
	public int skipActionTrue;
	public AC.Action skipActionTrueActual;
	public Cutscene linkedCutsceneTrue;
	
	public ResultAction resultActionFail;
	public int skipActionFail;
	public AC.Action skipActionFailActual;
	public Cutscene linkedCutsceneFail;
	
	private InventoryManager inventoryManager;
	
	
	public ActionInventoryCheck ()
	{
		this.isDisplayed = true;
		title = "Inventory: Check";
	}
	
	
	override public int End (List<AC.Action> actions)
	{
		bool result = false;
		result = CheckCondition ();
			
		if (result)
		{
			if (resultActionTrue == ResultAction.Continue)
			{
				return 0;
			}
			
			else if (resultActionTrue == ResultAction.Stop)
			{
				return -1;
			}
			
			else if (resultActionTrue == ResultAction.Skip)
			{
				int skip = skipActionTrue;
				if (skipActionTrueActual && actions.IndexOf (skipActionTrueActual) > 0)
				{
					skip = actions.IndexOf (skipActionTrueActual);
				}
				
				return (skip);
			}
			
			else if (resultActionTrue == ResultAction.RunCutscene)
			{
				if (linkedCutsceneTrue)
				{
					linkedCutsceneTrue.SendMessage ("Interact");
				}
				
				return -1;
			}
			
			return 0;
		}
		else
		{
			if (resultActionFail == ResultAction.Continue)
			{
				return 0;
			}
			
			else if (resultActionFail == ResultAction.Stop)
			{
				return -1;
			}
			
			else if  (resultActionFail == ResultAction.Skip)
			{
				int skip = skipActionFail;
				if (skipActionFailActual && actions.IndexOf (skipActionFailActual) > 0)
				{
					skip = actions.IndexOf (skipActionFailActual);
				}
				
				return (skip);						
			}
			
			else if (resultActionFail == ResultAction.RunCutscene)
			{
				if (linkedCutsceneFail)
				{
					linkedCutsceneFail.SendMessage ("Interact");
				}
				
				return -1;
			}
		}
		
		return 0;
	}
	
	
	private bool CheckCondition ()
	{
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
		
		int count = runtimeInventory.GetCount (invID);
		
		if (doCount)
		{
			if (intCondition == IntCondition.EqualTo)
			{
				if (count == intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (count != intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.LessThan)
			{
				if (count < intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.MoreThan)
			{
				if (count > intValue)
				{
					return true;
				}
			}
		}
		
		else if (count > 0)
		{
			return true;
		}
		
		return false;	
	}
	

	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (!inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}
		
		if (inventoryManager)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			invNumber = -1;
			
			if (inventoryManager.items.Count > 0)
			{
			
				foreach (InvItem _item in inventoryManager.items)
				{
					labelList.Add (_item.label);
					
					// If an item has been removed, make sure selected variable is still valid
					if (_item.id == invID)
					{
						invNumber = i;
					}
					
					i++;
				}
				
				if (invNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					Debug.LogWarning ("Previously chosen item no longer exists!");
					
					invNumber = 0;
					invID = 0;
				}
				
				EditorGUILayout.BeginHorizontal();
					invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
					invID = inventoryManager.items[invNumber].id;
				EditorGUILayout.EndHorizontal();
				
				if (inventoryManager.items[invNumber].canCarryMultiple)
				{
					doCount = EditorGUILayout.Toggle ("Query count?", doCount);
				
					if (doCount)
					{
						EditorGUILayout.BeginHorizontal ("");
							EditorGUILayout.LabelField ("Count is:", GUILayout.MaxWidth (70));
							intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
							intValue = EditorGUILayout.IntField (intValue);
						
							if (intValue < 1)
							{
								intValue = 1;
							}
						EditorGUILayout.EndHorizontal ();
					}
				}
				else
				{
					doCount = false;
				}
			}

			else
			{
				EditorGUILayout.LabelField ("No inventory items exist!");
				invID = -1;
				invNumber = -1;
			}
		}
	}


	override public void SkipActionGUI (List<Action> actions, bool showGUI)
	{
		if (showGUI)
		{
			if (doCount)
			{
				resultActionTrue = (Action.ResultAction) EditorGUILayout.EnumPopup("If condition is met:", (Action.ResultAction) resultActionTrue);
			}
			else
			{
				resultActionTrue = (Action.ResultAction) EditorGUILayout.EnumPopup("If player is carrying:", (Action.ResultAction) resultActionTrue);
			}
		}

		if (resultActionTrue == Action.ResultAction.RunCutscene && showGUI)
		{
			linkedCutsceneTrue = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", linkedCutsceneTrue, typeof (Cutscene), true);
		}
		else if (resultActionTrue == Action.ResultAction.Skip)
		{
			SkipActionTrueGUI (actions, showGUI);
		}

		if (showGUI)
		{
			if (doCount)
			{
				resultActionFail = (Action.ResultAction) EditorGUILayout.EnumPopup("If condition is not met:", (Action.ResultAction) resultActionFail);
			}
			else
			{
				resultActionFail = (Action.ResultAction) EditorGUILayout.EnumPopup("If player is not carrying:", (Action.ResultAction) resultActionFail);
			}
		}

		if (resultActionFail == Action.ResultAction.RunCutscene && showGUI)
		{
			linkedCutsceneFail = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", linkedCutsceneFail, typeof (Cutscene), true);
		}
		else if (resultActionFail == Action.ResultAction.Skip)
		{
			SkipActionFailGUI (actions, showGUI);
		}
	}
	
	
	private void SkipActionTrueGUI (List<Action> actions, bool showGUI)
	{
		int tempSkipAction = skipActionTrue;
		int offset = actions.IndexOf (this) + 1;
		List<string> labelList = new List<string>();
		
		if (skipActionTrueActual)
		{
			bool found = false;
			
			if (offset <= actions.Count)
			{
				for (int i = 0; i < actions.Count - offset; i++)
				{
					labelList.Add ((i + offset).ToString () + ": " + actions [i + offset].title);
					
					if (skipActionTrueActual == actions [i + offset])
					{
						skipActionTrue = i + offset;
						found = true;
					}
				}
			}
			
			if (!found)
			{
				skipActionTrue = tempSkipAction;
			}
		}
		
		if (skipActionTrue < offset)
		{
			skipActionTrue = offset;
		}
		
		if (skipActionTrue >= actions.Count)
		{
			if (offset == actions.Count)
			{
				skipActionTrue = 0;
			}
			else
			{
				skipActionTrue = actions.Count - 1;
			}
		}
		
		if (showGUI)
		{
			if (skipActionTrue > 0)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField ("  Action to skip to:");
				tempSkipAction = EditorGUILayout.Popup (skipActionTrue - offset, labelList.ToArray());
				skipActionTrue = tempSkipAction + offset;
				EditorGUILayout.EndHorizontal();
				skipActionTrueActual = actions [skipActionTrue];
			}
			else
			{
				EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
			}
		}
		else
		{
			if (skipActionTrue > 0)
			{
				skipActionTrueActual = actions [skipActionTrue];
			}
		}
	}
	
	
	private void SkipActionFailGUI (List<Action> actions, bool showGUI)
	{
		int tempSkipAction = skipActionFail;
		int offset = actions.IndexOf (this) + 1;
		List<string> labelList = new List<string>();
		
		if (skipActionFailActual)
		{
			bool found = false;
			
			if (offset <= actions.Count)
			{
				for (int i = 0; i < actions.Count - offset; i++)
				{
					labelList.Add ((i + offset).ToString () + ": " + actions [i + offset].title);
					
					if (skipActionFailActual == actions [i + offset])
					{
						skipActionFail = i + offset;
						found = true;
					}
				}
			}
			
			if (!found)
			{
				skipActionFail = tempSkipAction;
			}
		}
		
		if (skipActionFail < offset)
		{
			skipActionFail = offset;
		}
		
		if (skipActionFail >= actions.Count)
		{
			if (offset == actions.Count)
			{
				skipActionFail = 0;
			}
			else
			{
				skipActionFail = actions.Count - 1;
			}
		}
		
		if (showGUI)
		{
			if (skipActionFail > 0)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField ("  Action to skip to:");
				tempSkipAction = EditorGUILayout.Popup (skipActionFail - offset, labelList.ToArray());
				skipActionFail = tempSkipAction + offset;
				EditorGUILayout.EndHorizontal();
				skipActionFailActual = actions [skipActionFail];
			}
			else
			{
				EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
			}
		}
		else
		{
			if (skipActionFail > 0)
			{
				skipActionFailActual = actions [skipActionFail];
			}
		}
	}
	

	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (inventoryManager)
		{
			if (inventoryManager.items.Count > 0)
			{
				if (invNumber > -1)
				{
					labelAdd = " (" + inventoryManager.items[invNumber].label + ")";
				}
			}
		}
		
		return labelAdd;
	}

	#endif
	
}