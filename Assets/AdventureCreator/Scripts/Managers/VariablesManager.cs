/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"VariablesManager.cs"
 * 
 *	This script handles the "Variables" tab of the main wizard.
 *	Boolean and integer, which can be used regardless of scene, are defined here.
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
public class VariablesManager : ScriptableObject
{

	public List<GVar> vars = new List<GVar>();
	
	#if UNITY_EDITOR

	private GVar selectedVar = null;
	private string[] boolType = {"False", "True"};
	private string filter = "";
	

	private static GUILayoutOption
		valueWidth = GUILayout.MaxWidth (100f);
	
	
	public void ShowGUI ()
	{

		filter = EditorGUILayout.TextField ("Filter by name:", filter);
		EditorGUILayout.Space ();

		// List variables
		foreach (GVar _var in vars)
		{
			if (filter == "" || _var.label.ToLower ().Contains (filter.ToLower ()))
			{
				EditorGUILayout.BeginVertical("Button");
					EditorGUILayout.BeginHorizontal ();
					
						EditorGUILayout.LabelField (_var.id.ToString (), EditorStyles.miniLabel, GUILayout.Width (10f));
						_var.type = (VariableType) EditorGUILayout.EnumPopup (_var.type, GUILayout.Width (80f));
						_var.label = EditorGUILayout.TextField (_var.label);
						
						if (_var.type == VariableType.Boolean)
						{
							if (_var.val != 1)
							{
								_var.val = 0;
							}
							_var.val = EditorGUILayout.Popup (_var.val, boolType, valueWidth);
						}
						else if (_var.type == VariableType.Integer)
						{
							_var.val = EditorGUILayout.IntField (_var.val, valueWidth);
						}
						else if (_var.type == VariableType.String)
						{
							_var.textVal = EditorGUILayout.TextField (_var.textVal, valueWidth);
						}

						Texture2D icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
						if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (_var);
						}
				
					EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical();
			}
		}

		EditorGUILayout.Space ();
		if (GUILayout.Button("Create new variable"))
		{
			ResetFilter ();
			Undo.RecordObject (this, "Add variable");
			vars.Add (new GVar (GetIDArray ()));
		}
		

		if (GUI.changed)
		{
			EditorUtility.SetDirty (this);
		}
	}


	private void ResetFilter ()
	{
		filter = "";
	}


	private void SideMenu (GVar _var)
	{
		GenericMenu menu = new GenericMenu ();
		selectedVar = _var;
		
		menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
		if (vars.Count > 1)
		{
			menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
		}
		if (vars.IndexOf (_var) > 0 || vars.IndexOf (_var) < vars.Count-1)
		{
			menu.AddSeparator ("");
		}
		if (vars.IndexOf (_var) > 0)
		{
			menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
		}
		if (vars.IndexOf (_var) < vars.Count-1)
		{
			menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void Callback (object obj)
	{
		if (selectedVar != null)
		{
			ResetFilter ();

			int i = vars.LastIndexOf (selectedVar);

			switch (obj.ToString ())
			{
			case "Insert after":
				Undo.RecordObject (this, "Insert variable");
				vars.Insert (i+1, new GVar (GetIDArray ()));
				break;
				
			case "Delete":
				Undo.RecordObject (this, "Delete variable");
				vars.Remove (selectedVar);
				break;

			case "Move up":
				Undo.RecordObject (this, "Move variable up");
				vars.Remove (selectedVar);
				vars.Insert (i-1, selectedVar);
				AssetDatabase.SaveAssets();
				break;

			case "Move down":
				Undo.RecordObject (this, "Move variable down");
				vars.Remove (selectedVar);
				vars.Insert (i+1, selectedVar);
				AssetDatabase.SaveAssets();
				break;
			}
		}

		selectedVar = null;
	}

	#endif

	
	private int[] GetIDArray ()
	{
		// Returns a list of id's in the list
		
		List<int> idArray = new List<int>();
		
		foreach (GVar variable in vars)
		{
			idArray.Add (variable.id);
		}
		
		idArray.Sort ();
		return idArray.ToArray ();
	}

}