using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (ManagerPackage))]

[System.Serializable]
public class ManagerPackageEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		ManagerPackage _target = (ManagerPackage) target;

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Manager asset files", EditorStyles.boldLabel);
			_target.sceneManager = (SceneManager) EditorGUILayout.ObjectField ("Scene manager:", _target.sceneManager, typeof (SceneManager), false);
			_target.settingsManager = (SettingsManager) EditorGUILayout.ObjectField ("Settings manager:", _target.settingsManager, typeof (SettingsManager), false);
			_target.actionsManager = (ActionsManager) EditorGUILayout.ObjectField ("Actions manager:", _target.actionsManager, typeof (ActionsManager), false);
			_target.variablesManager = (VariablesManager) EditorGUILayout.ObjectField ("Variable manager:", _target.variablesManager, typeof (VariablesManager), false);
			_target.inventoryManager = (InventoryManager) EditorGUILayout.ObjectField ("Inventory manager:", _target.inventoryManager, typeof (InventoryManager), false);
			_target.speechManager = (SpeechManager) EditorGUILayout.ObjectField ("Speech manager:", _target.speechManager, typeof (SpeechManager), false);
			_target.cursorManager = (CursorManager) EditorGUILayout.ObjectField ("Cursor manager:", _target.cursorManager, typeof (CursorManager), false);
			_target.menuManager = (MenuManager) EditorGUILayout.ObjectField ("Menu manager:", _target.menuManager, typeof (MenuManager), false);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.Space ();

		if (GUILayout.Button ("Assign managers"))
		{
			if (AdvGame.GetReferences () != null)
			{
				Undo.RecordObject (AdvGame.GetReferences (), "Assign managers");

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().sceneManager = _target.sceneManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().settingsManager = _target.settingsManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().actionsManager = _target.actionsManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().variablesManager = _target.variablesManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().inventoryManager = _target.inventoryManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().speechManager = _target.speechManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().cursorManager = _target.cursorManager;
				}

				if (_target.actionsManager)
				{
					AdvGame.GetReferences ().menuManager = _target.menuManager;
				}

				Debug.Log ("Managers assigned.");
			}
			else
			{
				Debug.LogError ("Can't assign managers - no References file found in Resources folder.");
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
