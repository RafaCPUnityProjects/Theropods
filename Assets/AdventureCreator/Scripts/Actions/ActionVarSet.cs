/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySet.cs"
 * 
 *	This action is used to set the value of integer and boolean Variables, defined in the Variables Manager.
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
public class ActionVarSet : Action
{
	
	public SetVarMethod setVarMethod;
	public SetVarMethodString setVarMethodString = SetVarMethodString.EnteredHere;
	public SetVarMethodIntBool setVarMethodIntBool = SetVarMethodIntBool.EnteredHere;
	
	public int variableID;
	public int variableNumber;
	
	public int intValue;
	public BoolValue boolValue;
	public string stringValue;

	public string menuName;
	public string elementName;

	public Animator animator;
	public string parameterName;

	private VariablesManager variablesManager;

	#if UNITY_EDITOR
	private static GUILayoutOption
		intWidth = GUILayout.MaxWidth (40);
	#endif
	
	
	public ActionVarSet ()
	{
		this.isDisplayed = true;
		title = "Variable: Set";
	}

	
	override public float Run ()
	{
		RuntimeVariables runtimeVariables = GameObject.FindWithTag(Tags.persistentEngine).GetComponent <RuntimeVariables>();
		
		if (runtimeVariables)
		{
			if (variableID != -1 && runtimeVariables.localVars.Count > 0)
			{
				if (runtimeVariables.GetVarType (variableID) == VariableType.Integer)
				{
					int _value = 0;

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						_value = intValue;
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						if (animator && parameterName != "")
						{
							_value = animator.GetInteger (parameterName);
						}	
					}

					runtimeVariables.SetValue (variableID, _value, setVarMethod);
				}
				else if (runtimeVariables.GetVarType (variableID) == VariableType.Boolean)
				{
					int _value = 0;

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						_value = (int) boolValue;
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						if (animator && parameterName != "")
						{
							if (animator.GetBool (parameterName))
							{
								_value = 1;
							}
						}
					}

					runtimeVariables.SetValue (variableID, _value, SetVarMethod.SetValue);
				}
				else if (runtimeVariables.GetVarType (variableID) == VariableType.String)
				{
					string _value = "";

					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						_value = stringValue;
					}
					else if (setVarMethodString == SetVarMethodString.SetAsMenuInputLabel)
					{
						if (PlayerMenus.GetElementWithName (menuName, elementName) != null)
						{
							MenuInput menuInput = (MenuInput) PlayerMenus.GetElementWithName (menuName, elementName);
							_value = menuInput.label;
						}
						else
						{
							Debug.LogWarning ("Could not find MenuInput '" + elementName + "' in Menu '" + menuName + "'");
						}
					}

					runtimeVariables.SetValue (variableID, _value);
				}

				if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
				{
					GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().VarChanged ();
				}
			}
		}
		
		return 0f;
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
			
			int i = 0;
			variableNumber = -1;
			
			if (variablesManager.vars.Count > 0)
			{
				foreach (GVar _var in variablesManager.vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i ++;
				}
				
				if (variableNumber == -1)
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}
		
				
				EditorGUILayout.BeginHorizontal();
				
				variableNumber = EditorGUILayout.Popup (variableNumber, labelList.ToArray(), GUILayout.Width (150));
				variableID = variablesManager.vars[variableNumber].id;
				
				if (variablesManager.vars[variableNumber].type == VariableType.Boolean)
				{
					EditorGUILayout.LabelField ("=", intWidth);

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						boolValue = (BoolValue) EditorGUILayout.EnumPopup (boolValue);
					}

					EditorGUILayout.EndHorizontal();

					if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}

					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("Source:", setVarMethodIntBool);
				}
				else if (variablesManager.vars[variableNumber].type == VariableType.Integer)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							EditorGUILayout.LabelField ("+=", intWidth);
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							EditorGUILayout.LabelField ("=", intWidth);
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							EditorGUILayout.LabelField ("= 0 to", intWidth);
						}

						intValue = EditorGUILayout.IntField (intValue);
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						EditorGUILayout.LabelField ("=", intWidth);
					}
						
					EditorGUILayout.EndHorizontal();

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);
						
						if (setVarMethod == SetVarMethod.SetAsRandom && intValue < 0)
						{
							intValue = 0;
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}

					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("Source:", setVarMethodIntBool);
				}
				else if (variablesManager.vars[variableNumber].type == VariableType.String)
				{
					EditorGUILayout.LabelField ("=", intWidth);
					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						stringValue = EditorGUILayout.TextField (stringValue);
					}
					EditorGUILayout.EndHorizontal();

					if (setVarMethodString == SetVarMethodString.SetAsMenuInputLabel)
					{
						menuName = EditorGUILayout.TextField ("Menu name:", menuName);
						elementName = EditorGUILayout.TextField ("Input element name:", elementName);
					}

					setVarMethodString = (SetVarMethodString) EditorGUILayout.EnumPopup ("Source:", setVarMethodString);
				}
				
				AfterRunningOption ();
			}
			else
			{
				EditorGUILayout.LabelField ("No global variables exist!");
				variableID = -1;
				variableNumber = -1;
			}
		}
	}


	private void ShowMecanimGUI ()
	{
		animator = (Animator) EditorGUILayout.ObjectField ("Animator:", animator, typeof (Animator), true);
		parameterName = EditorGUILayout.TextField ("Parameter name:", parameterName);
	}


	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (variablesManager)
		{
			if (variablesManager.vars.Count > 0)
			{
				if (variableNumber > -1 && variablesManager.vars.Count > variableNumber)
				{
					labelAdd = " (" + variablesManager.vars [variableNumber].label + ")";
				}
			}
		}
		
		return labelAdd;
	}

	#endif

}