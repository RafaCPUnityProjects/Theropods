using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Conversation))]
public class ConversationEditor : Editor
{
	private static GUIContent
		deleteContent = new GUIContent("-", "Delete this option");

	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(20f);
	
	private Conversation _target;
	private ButtonDialog selectedOption;
 
	
    public void OnEnable()
    {
        _target = (Conversation) target;
    }

	
	public override void OnInspectorGUI()
    {
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Conversation settings", EditorStyles.boldLabel);
			_target.isTimed = EditorGUILayout.Toggle ("Is timed?", _target.isTimed);
			if (_target.isTimed)
			{
				_target.timer = EditorGUILayout.FloatField ("Timer length (s):", _target.timer);
			}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.Space ();
		CreateOptionsGUI ();
		EditorGUILayout.Space ();

		if (selectedOption != null)
		{
			EditorGUILayout.LabelField ("Dialogue option '" + selectedOption.label + "' properties", EditorStyles.boldLabel);
			EditOptionGUI (selectedOption);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}

	}



	private void CreateOptionsGUI ()
	{

		EditorGUILayout.LabelField ("Dialogue options", EditorStyles.boldLabel);
		
		foreach (ButtonDialog option in _target.options)
		{
			EditorGUILayout.BeginHorizontal ();
			
			string buttonLabel = option.label;
			if (buttonLabel == "")
			{
				buttonLabel = "(Untitled)";	
			}
			if (_target.isTimed && _target.options.IndexOf (option) == _target.defaultOption)
			{
				buttonLabel += " (Default)";
			}

			if (GUILayout.Toggle (option.isEditing, buttonLabel, "Button"))
			{
				if (selectedOption != option)
				{
					DeactivateAllOptions ();
					ActivateOption (option);
				}
			}
			
			if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (this, "Delete option: " + option.label);
				
				DeactivateAllOptions ();
				_target.options.Remove (option);
				break;
			}
			
			EditorGUILayout.EndHorizontal ();
		}

		if (GUILayout.Button ("Add new dialogue option"))
		{
			Undo.RecordObject (_target, "Create dialogue option");
			ButtonDialog newOption = new ButtonDialog ();
			_target.options.Add (newOption);
			ActivateOption (newOption);
		}
	}


	private void ActivateOption (ButtonDialog option)
	{
		option.isEditing = true;
		selectedOption = option;
	}
	
	
	private void DeactivateAllOptions ()
	{
		foreach (ButtonDialog option in _target.options)
		{
			option.isEditing = false;
		}
		selectedOption = null;
	}


	private void EditOptionGUI (ButtonDialog option)
	{
		EditorGUILayout.BeginVertical ("Button");
		
		if (option.lineID > -1)
		{
			EditorGUILayout.LabelField ("Speech Manager ID:", option.lineID.ToString ());
		}
		
		option.label = EditorGUILayout.TextField ("Label:", option.label);
		
		EditorGUILayout.BeginHorizontal ();
		option.dialogueOption = (DialogueOption) EditorGUILayout.ObjectField ("Interaction:", option.dialogueOption, typeof (DialogueOption), true);
		
		if (option.dialogueOption == null)
		{
			if (GUILayout.Button ("Auto-create", GUILayout.MaxWidth (90f)))
			{
				Undo.RecordObject (_target, "Auto-create dialogue option");
				DialogueOption newDialogueOption = AdvGame.GetReferences ().sceneManager.AddPrefab ("Logic", "DialogueOption", true, false, true).GetComponent <DialogueOption>();
				
				newDialogueOption.gameObject.name = AdvGame.UniqueName (_target.gameObject.name + "_Option");
				option.dialogueOption = newDialogueOption;
			}
		}
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Icon texture:", GUILayout.Width (155f));
		option.icon = (Texture2D) EditorGUILayout.ObjectField (option.icon, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
		EditorGUILayout.EndHorizontal ();
		
		option.isOn = EditorGUILayout.Toggle ("Is enabled?", option.isOn);
		option.returnToConversation = EditorGUILayout.Toggle ("Return to conversation?", option.returnToConversation);
		
		if (_target.isTimed)
		{
			if (_target.options.IndexOf (option) != _target.defaultOption)
			{
				if (GUILayout.Button ("Make default", GUILayout.MaxWidth (80)))
				{
					Undo.RecordObject (_target, "Change default conversation option");
					_target.defaultOption = _target.options.IndexOf (option);
					EditorUtility.SetDirty (_target);
				}
			}
		}
		
		EditorGUILayout.EndVertical ();
	}

}