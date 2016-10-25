/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Actions" tab of the main wizard.
 *	Custom actions can be added and removed by selecting them with this.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionsManager : ScriptableObject
{
	
	public string folderPath = "AdventureCreator/Scripts/Actions";	// Where the action files are kept
	
	public int defaultClass;

	public List<ActionType> AllActions;
	public List<ActionType> EnabledActions;

	private Vector2 scrollPos;
	
	
	public string GetDefaultAction ()
	{
		if (EnabledActions.Count > 0 && EnabledActions.Count > defaultClass)
		{
			return EnabledActions[defaultClass].fileName;
		}

		return "";
	}
	
	
	#if UNITY_EDITOR
	
	private static GUILayoutOption
		titleWidth = GUILayout.MaxWidth (160),
		nameWidth = GUILayout.MaxWidth (130);
	
	
	public void ShowGUI ()
	{
		#if UNITY_WEBPLAYER
		EditorGUILayout.HelpBox ("Locating Actions requires a non-WebPlayer platform.  Please switch platform and try again.", MessageType.Warning);
		#endif
		
		GUILayout.Space (10);
		
		GUILayout.Label ("Path to Action subclass scripts (Relative to Asset folder)", EditorStyles.label);
		
		GUILayout.BeginHorizontal ();
		
			folderPath = GUILayout.TextField (folderPath, GUILayout.MaxWidth (260));
		
			if (GUILayout.Button ("Refresh list"))
			{
				RefreshList ();
			}

		GUILayout.EndHorizontal ();
		
		if (AllActions.Count > 0)
		{
			GUILayout.Space (10);
			
			defaultClass = EditorGUILayout.Popup ("Default action:", defaultClass, GetDefaultPopUp ());
			GUILayout.Space (10);
			
			GUILayout.BeginHorizontal ();
			
				GUILayout.Label ("Title", titleWidth);
				GUILayout.Label ("Filename", nameWidth);
				GUILayout.Label ("Enabled?", GUILayout.MaxWidth (50));
			
			GUILayout.EndHorizontal ();
			
			foreach (ActionType subclass in AllActions)
			{
				GUILayout.BeginVertical ("Button");
					GUILayout.BeginHorizontal ();
		
						GUILayout.Label (subclass.title, titleWidth);
						GUILayout.Label (subclass.fileName, nameWidth);
						
						subclass.isEnabled = GUILayout.Toggle (subclass.isEnabled, "");
					
					GUILayout.EndHorizontal();
				GUILayout.EndVertical ();
			}
			
			SetEnabled ();
			
			if (defaultClass > EnabledActions.Count - 1)
			{
				defaultClass = EnabledActions.Count - 1;
			}

		}
		else
		{
			EditorGUILayout.HelpBox ("No Action subclass files found.", MessageType.Warning);
		}
		
		EditorUtility.SetDirty (this);
	}
	

	public void RefreshList ()
	{
		#if !UNITY_WEBPLAYER
		
		Undo.RecordObject (this, "Refresh list");

		if (folderPath != "")
		{
			DirectoryInfo dir = new DirectoryInfo ("Assets/" + folderPath);
			FileInfo[] info = dir.GetFiles ("*.cs");
	
			AllActions.Clear ();
			
			foreach (FileInfo f in info) 
			{
				int extentionPosition = f.Name.IndexOf (".cs");
				string className = f.Name.Substring (0, extentionPosition);
				AC.Action tempAction = (AC.Action) CreateInstance (className);
				string title = tempAction.title;
				AllActions.Add (new ActionType (className, title));
			}
			
			AllActions.Sort (delegate(ActionType i1, ActionType i2) { return i1.title.CompareTo(i2.title); });
		}
		
		#endif
	}
	
	
	#endif
	
	private void SetEnabled ()
	{
		EnabledActions.Clear ();
		
		foreach (ActionType subclass in AllActions)
		{
			if (subclass.isEnabled)
			{
				EnabledActions.Add (subclass);
			}
		}
	}
	
	
	private string[] GetDefaultPopUp ()
	{
		List<string> defaultPopUp = new List<string> ();

		foreach (ActionType subclass in EnabledActions)
		{
			defaultPopUp.Add (subclass.title);
		}
	
		return (defaultPopUp.ToArray ());
	}
	
	
	public string GetActionName (int i)
	{
		return (EnabledActions [i].fileName);
	}
	
	
	public int GetActionsSize ()
	{
		return (EnabledActions.Count);
	}
	
	
	public string[] GetActionTitles ()
	{
		List<string> titles = new List<string>();
	
		foreach (ActionType type in EnabledActions)
		{
			titles.Add (type.title);
		}
		
		return (titles.ToArray ());
	}
	

}