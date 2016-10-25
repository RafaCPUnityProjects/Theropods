/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeVariables.cs"
 * 
 *	This script creates a local copy of the VariableManager's vars.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class RuntimeVariables : MonoBehaviour
{
	
	[HideInInspector] public List<GVar> localVars = new List<GVar>();
	
	
	public void Awake ()
	{
		// Transfer the vars set in VariablesManager to self on runtime
		UpdateSelf ();
	}
	
	
	private void UpdateSelf ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
		{
			VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;

			localVars.Clear ();
			foreach (GVar assetVar in variablesManager.vars)
			{
				localVars.Add (new GVar (assetVar));
			}
		}
	}
	
	
	public void SendVars (List<GVar> vars)
	{
		localVars = vars;
	}


	public string GetVarValue (int _id)
	{
		foreach (GVar _var in localVars)
		{
			if (_var.id == _id)
			{
				if (_var.type == VariableType.Integer)
				{
					return _var.val.ToString ();
				}
				else if (_var.type == VariableType.String)
				{
					return _var.textVal;
				}
				else
				{
					if (_var.val == 0)
					{
						return "False";
					}
					else
					{
						return "True";
					}
				}
			}
		}
		
		Debug.LogWarning ("Variable not found!");
		return "";
	}
	
	
	public VariableType GetVarType (int _id)
	{
		foreach (GVar _var in localVars)
		{
			if (_var.id == _id)
			{
				return _var.type;
			}
		}
		
		Debug.LogWarning ("Variable not found!");
		return VariableType.String;
	}


	public void SetValue (int _id, string newValue)
	{
		foreach (GVar _var in localVars)
		{
			if (_var.id == _id)
			{
				if (_var.type == VariableType.String)
				{
					_var.textVal = newValue;
				}

				return;
			}
		}
	}
	
	
	public void SetValue (int _id, int newValue, SetVarMethod setVarMethod)
	{
		foreach (GVar _var in localVars)
		{
			if (_var.id == _id)
			{
				if (setVarMethod == SetVarMethod.IncreaseByValue)
				{
					_var.val += newValue;
				}
				else if (setVarMethod == SetVarMethod.SetValue)
				{
					_var.val = newValue;
				}
				else if (setVarMethod == SetVarMethod.SetAsRandom)
				{
					_var.val = Random.Range (0, newValue);
				}
				
				if (_var.type == VariableType.Boolean)
				{
					if (_var.val > 0)
					{
						_var.val = 1;
					}
					else
					{
						_var.val = 0;
					}
				}
			}
		}
	}
	
}
