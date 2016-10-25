/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVarCheck.cs"
 * 
 *	This action checks to see if a Variable has been assigned a certain value,
 *	and performs something accordingly.
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
public class ActionVarCheck : Action
{
	
	public int variableID;
	public int variableNumber;
	
	public int intValue;
	public enum IntCondition { EqualTo, NotEqualTo, LessThan, MoreThan };
	public IntCondition intCondition;
	public bool isAdditive = false;
	
	public BoolValue boolValue;
	public enum BoolCondition { EqualTo, NotEqualTo };
	public BoolCondition boolCondition;

	public string stringValue;
	
	public ResultAction resultActionTrue;
	public ResultAction resultActionFail;

	public int skipActionTrue;
	public AC.Action skipActionTrueActual;
	public Cutscene linkedCutsceneTrue;
	
	public int skipActionFail;
	public AC.Action skipActionFailActual;
	public Cutscene linkedCutsceneFail;
	
	private VariablesManager variablesManager;
	
	
	public ActionVarCheck ()
	{
		this.isDisplayed = true;
		title = "Variable: Check";
	}

	
	override public int End (List<AC.Action> actions)
	{
		RuntimeVariables runtimeVariables = GameObject.FindWithTag(Tags.persistentEngine).GetComponent <RuntimeVariables>();

		variableNumber = GetVarNumber (AdvGame.GetReferences ().variablesManager.vars);

		if (runtimeVariables && variableNumber != -1)
		{
			bool result = false;
			result = CheckCondition (runtimeVariables.localVars[variableNumber]);
			
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

		}
		
		return 0;
	}
	
	
	private bool CheckCondition (GVar _var)
	{
		if (_var.type == VariableType.Boolean)
		{
			int fieldValue = _var.val;

			if (boolCondition == BoolCondition.EqualTo)
			{
				if (fieldValue == (int) boolValue)
				{
					return true;
				}
			}
			else
			{
				if (fieldValue != (int) boolValue)
				{
					return true;
				}
			}
		}

		else if (_var.type == VariableType.Integer)
		{
			int fieldValue = _var.val;

			if (intCondition == IntCondition.EqualTo)
			{
				if (fieldValue == intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (fieldValue != intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.LessThan)
			{
				if (fieldValue < intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.MoreThan)
			{
				if (fieldValue > intValue)
				{
					return true;
				}
			}
		}

		else if (_var.type == VariableType.String)
		{
			string fieldValue = _var.textVal;

			if (boolCondition == BoolCondition.EqualTo)
			{
				if (fieldValue == stringValue)
				{
					return true;
				}
			}
			else
			{
				if (fieldValue != stringValue)
				{
					return true;
				}
			}
		}
		
		return false;
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (!variablesManager)
		{
			variablesManager = AdvGame.GetReferences ().variablesManager;
		}
		
		if (variablesManager)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			variableNumber = -1;
			
			if (variablesManager.vars.Count > 0)
			{
				foreach (GVar _var in variablesManager.vars)
				{
					labelList.Add (_var.label);
				}

				variableNumber = GetVarNumber (variablesManager.vars);

				if (variableNumber == -1)
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}
		
				EditorGUILayout.BeginHorizontal();
				
					variableNumber = EditorGUILayout.Popup (variableNumber, labelList.ToArray());
					variableID = variablesManager.vars[variableNumber].id;
					
					if (variablesManager.vars [variableNumber].type == VariableType.Boolean)
					{
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
						boolValue = (BoolValue) EditorGUILayout.EnumPopup (boolValue);
					}
					else if (variablesManager.vars [variableNumber].type == VariableType.Integer)
					{
						intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
						intValue = EditorGUILayout.IntField (intValue);
					}
					else if (variablesManager.vars [variableNumber].type == VariableType.String)
					{
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
						stringValue = EditorGUILayout.TextField (stringValue);
					}
				
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.HelpBox ("No global variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}
		}		
	}


	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (variablesManager)
		{
			if (variablesManager.vars.Count > 0 && variablesManager.vars.Count > variableNumber)
			{
				if (variableNumber > -1)
				{
					labelAdd = " (" + variablesManager.vars[variableNumber].label + ")";
				}
			}
		}
		
		return labelAdd;
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


	private int GetVarNumber (List<GVar> vars)
	{
		int i = 0;

		foreach (GVar _var in vars)
		{
			if (_var.id == variableID)
			{
				return i;
			}
			
			i++;
		}

		return -1;
	}


}