/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSceneCheck.cs"
 * 
 *	This action checks the player's last-visited scene,
 *	useful for running specific "player enters the room" cutscenes.
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
public class ActionSceneCheck : Action
{
	
	public int sceneNumber;
	public enum IntCondition { EqualTo, NotEqualTo };
	public IntCondition intCondition;
	
	public ResultAction resultActionTrue;
	public ResultAction resultActionFail;

	public int skipActionTrue;
	public AC.Action skipActionTrueActual;
	public Cutscene linkedCutsceneTrue;
	
	public int skipActionFail;
	public AC.Action skipActionFailActual;
	public Cutscene linkedCutsceneFail;
	
	
	public ActionSceneCheck ()
	{
		this.isDisplayed = true;
		title = "Engine: Check previous scene";
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
				
				return -2;
			}
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
				
				return -2;
			}
		}
		
		return 0;
	}
	
	
	private bool CheckCondition ()
	{
		SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();

		int actualSceneNumber = sceneChanger.previousScene;
	
		if (intCondition == IntCondition.EqualTo)
		{
			if (actualSceneNumber == sceneNumber)
			{
				return true;
			}
		}
		
		else if (intCondition == IntCondition.NotEqualTo)
		{
			if (actualSceneNumber != sceneNumber)
			{
				return true;
			}
		}
		
		return false;
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("Previous scene:");
			intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
			sceneNumber = EditorGUILayout.IntField (sceneNumber);
		EditorGUILayout.EndHorizontal();
	}


	override public void SkipActionGUI (List<Action> actions, bool showGUI)
	{
		if (showGUI)
		{
			resultActionTrue = (Action.ResultAction) EditorGUILayout.EnumPopup("If condition is met:", (Action.ResultAction) resultActionTrue);
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
			resultActionFail = (Action.ResultAction) EditorGUILayout.EnumPopup("If condition is not met:", (Action.ResultAction) resultActionFail);
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
	
	#endif

}