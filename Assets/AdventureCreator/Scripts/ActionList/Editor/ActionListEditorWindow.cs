using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ActionListEditorWindow : EditorWindow
{
	
	private ActionList _target;
	private int selected;
	private Vector2 scrollPosition = Vector2.zero;
	private int labelHeight = 20;
	private int labelWidth = 350;
	private int labelGap = 50;
	private int listWidth = 510;
	private int dividerWidth = 400;
	private int dividerHeight = 60;
	
	
	private void OnEnable ()
	{
		UnmarkAll ();
	}
	
	
	private void OnGUI ()
	{
		if (Selection.activeGameObject && Selection.activeGameObject.GetComponent <ActionList>())
		{
			_target = Selection.activeGameObject.GetComponent<ActionList>();
		}
		else
		{
			_target = null;
			selected = -1;
		}
		
		if (_target != null)
		{
			GUILayout.BeginArea (new Rect (5, 0, dividerWidth - 10, dividerHeight));
			GUILayout.Space (10f);
			GUILayout.Label ("Editing: " + _target.gameObject.name, EditorStyles.largeLabel);
			BulkGUI ();
			GUILayout.EndArea ();
			DrawStraightLine.Draw (new Vector2 (0, dividerHeight), new Vector2 (dividerWidth, dividerHeight), Color.grey, 1, false);
			SelectedActionGUI ();
			DrawStraightLine.Draw (new Vector2 (dividerWidth, 0), new Vector2 (dividerWidth, 900), Color.grey, 1, false);
			NodesGUI ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		else
		{
			GUILayout.BeginArea (new Rect (5, 0, dividerWidth - 10, dividerHeight));
			GUILayout.Space (10f);
			GUILayout.Label ("No ActionList object selected", EditorStyles.largeLabel);
			GUILayout.EndArea ();
		}
	}


	private void OnInspectorUpdate ()
	{
		Repaint();
	}
	
	
	private void SelectedActionGUI ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			
			GUILayout.BeginArea (new Rect (0, dividerHeight, dividerWidth, 800));
			
			if (selected >= _target.actions.Count)
			{
				selected = -1;
			}
			
			GUILayout.Space (10f);
			if (selected > -1 && _target.actions [selected] != null)
			{
				GUILayout.Label (selected.ToString () + ": " + _target.actions [selected].title, EditorStyles.largeLabel);
				GUILayout.Space (10f);
				
				int typeNumber = GetTypeNumber (selected);
				typeNumber = EditorGUILayout.Popup("Action type:", typeNumber, actionsManager.GetActionTitles ());
				EditorGUILayout.Space ();
				
				// Rebuild constructor if Subclass and type string do not match
				if (_target.actions[selected].GetType().ToString() != actionsManager.GetActionName (typeNumber))
				{
					_target.actions[selected] = ActionListEditor.RebuildAction (_target.actions[selected], typeNumber);
				}
				
				ActionListEditor.ShowActionGUI (_target.actions [selected], _target.gameObject);
				
				if (_target.actions[selected].endAction == AC.Action.ResultAction.Skip || _target.actions[selected] is ActionVarCheck || _target.actions[selected] is ActionInventoryCheck || _target.actions[selected] is ActionSceneCheck)
				{
					_target.actions [selected].SkipActionGUI (_target.actions, true);
				}
				
				GUILayout.Space (30f);
				GUILayout.BeginHorizontal ();
				if (_target.actions.Count > 1)
				{
					if (GUILayout.Button ("Cut", EditorStyles.miniButton))
					{
						ActionListEditor.ModifyAction (_target, _target.actions [selected], "Cut");
					}
				}
				else
				{
					GUI.enabled = false;
					GUILayout.Button ("Cut", EditorStyles.miniButton);
					GUI.enabled = true;
				}
				if (GUILayout.Button ("Copy", EditorStyles.miniButton))
				{
					ActionListEditor.ModifyAction (_target, _target.actions [selected], "Copy");
				}
				if (AdvGame.copiedActions != null && AdvGame.copiedActions.Count > 0)
				{
					if (GUILayout.Button ("Paste", EditorStyles.miniButton))
					{
						ActionListEditor.ModifyAction (_target, _target.actions [selected], "Paste after");
						UnmarkAll ();
					}
				}
				else
				{
					GUI.enabled = false;
					GUILayout.Button ("Paste", EditorStyles.miniButton);
					GUI.enabled = true;
				}
				if (_target.actions.Count > 1)
				{
					if (GUILayout.Button ("Delete", EditorStyles.miniButton))
					{
						ActionListEditor.ModifyAction (_target, _target.actions [selected], "Delete");
					}
				}
				else
				{
					GUI.enabled = false;
					GUILayout.Button ("Delete", EditorStyles.miniButton);
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal ();
			}
			else
			{
				GUILayout.Label (" No Action selected", EditorStyles.largeLabel);
			}
			
			GUILayout.EndArea ();
		}
	}
	
	
	
	private void NodesGUI ()
	{
		int numActions = _target.actions.Count;
		if (numActions < 1)
		{
			numActions = 1;
			string defaultAction = AdvGame.GetReferences ().actionsManager.GetDefaultAction ();
			_target.actions.Add ((AC.Action) CreateInstance(defaultAction));
		}
		
		for (int i=0; i<_target.actions.Count; i++)
		{
			int labelTop = (i+1) * (labelHeight + labelGap);
			if (!(_target.actions[i] is ActionScene || _target.actions[i] is ActionConversation || _target.actions[i] is ActionEndGame))
			{
				ConnectActions (new Vector2 (50 + labelWidth/2, labelTop + labelHeight), i);
			}
		}
		
		scrollPosition = GUI.BeginScrollView (new Rect (dividerWidth, 0, position.width - dividerWidth, position.height), scrollPosition, new Rect (0, 0, listWidth, (labelHeight + labelGap) * (_target.actions.Count + 2)), false, false);
		
		if (GUI.Button (new Rect (40 + labelWidth/2, labelGap - labelHeight, 20, labelHeight), "+"))
		{
			ActionListEditor.ModifyAction (_target, null, "Insert after");
			selected = 0;
			UnmarkAll ();
		}
		
		for (int i=0; i<_target.actions.Count; i++)
		{
			int labelTop = (i+1) * (labelHeight + labelGap);
			
			bool isSelected = false;
			if (selected == i)
			{
				isSelected = true;
			}
			
			_target.actions[i].isMarked = GUI.Toggle (new Rect (20, labelTop+2, labelHeight, labelHeight), _target.actions[i].isMarked, "");

			string actionLabel = _target.actions[i].SetLabel ();
			if (actionLabel.Length > 40)
			{
				actionLabel = actionLabel.Substring (0, 40);
			}
			if (GUI.Toggle (new Rect (50, labelTop, labelWidth, labelHeight), isSelected, i.ToString () + ": " + _target.actions[i].title + actionLabel, "Button"))
			{
				selected = i;
			}
			
			if (isSelected && _target.actions.Count > 1)
			{
				// Move to top
				if (i > 0)
				{
					if (GUI.Button (new Rect (70 + labelWidth, labelTop, 25, labelHeight), "<<"))
					{
						ActionListEditor.ModifyAction (_target, _target.actions[i], "Move to top");
						selected = 0;
					}
				}
				
				// Move up
				if (GUI.Button (new Rect (95 + labelWidth, labelTop, 20, labelHeight), "<"))
				{
					ActionListEditor.ModifyAction (_target, _target.actions[i], "Move up");
					selected --;
				}
				
				// Move down
				if (GUI.Button (new Rect (115 + labelWidth, labelTop, 20, labelHeight), ">"))
				{
					ActionListEditor.ModifyAction (_target, _target.actions[i], "Move down");
					selected ++;
				}
				
				// Move to bottom
				if (i < _target.actions.Count - 1)
				{
					if (GUI.Button (new Rect (135 + labelWidth, labelTop, 25, labelHeight), ">>"))
					{
						ActionListEditor.ModifyAction (_target, _target.actions[i], "Move to bottom");
						selected = _target.actions.Count - 1;
					}
				}
			}
			
			if (_target.actions[i].endAction == AC.Action.ResultAction.Skip || _target.actions[i] is ActionVarCheck || _target.actions[i] is ActionInventoryCheck || _target.actions[i] is ActionSceneCheck)
			{
				_target.actions[i].SkipActionGUI (_target.actions, false);
			}
			
			if (GUI.Button (new Rect (40 + labelWidth/2, labelTop + labelHeight + 15, 20, labelHeight), "+"))
			{
				ActionListEditor.ModifyAction (_target, _target.actions[i], "Insert after");
				numActions ++;
				selected = i+1;
				UnmarkAll ();
			}
			
			_target.actions = ActionListEditor.ResizeList (_target.actions, numActions);
		}
		
		GUI.EndScrollView ();
	}
	
	
	private void UnmarkAll ()
	{
		if (_target && _target.actions.Count > 0)
		{
			foreach (AC.Action action in _target.actions)
			{
				action.isMarked = false;
			}
		}
	}
	
	
	private void ConnectActions (Vector2 start, int i)
	{
		if (i > -1 && i < _target.actions.Count - 1)
		{
			AC.Action action = _target.actions[i];
			
			start.x += dividerWidth - (int) scrollPosition.x;
			start.y -= (int) scrollPosition.y;
			
			Vector2 end = new Vector2 (start.x, start.y + labelGap);
			
			if (action is ActionVarCheck || action is ActionInventoryCheck || action is ActionSceneCheck)
			{
				AC.Action.ResultAction resultActionTrue = AC.Action.ResultAction.Stop;
				AC.Action.ResultAction resultActionFail = AC.Action.ResultAction.Stop;
				int skipActionTrue = 0;
				int skipActionFail = 0;
				
				if (action is ActionVarCheck)
				{
					ActionVarCheck tempAction = (ActionVarCheck) action;
					resultActionTrue = tempAction.resultActionTrue;
					resultActionFail = tempAction.resultActionFail;
					skipActionTrue = tempAction.skipActionTrue;
					skipActionFail = tempAction.skipActionFail;;
				}
				else if (action is ActionInventoryCheck)
				{
					ActionInventoryCheck tempAction = (ActionInventoryCheck) action;
					resultActionTrue = tempAction.resultActionTrue;
					resultActionFail = tempAction.resultActionFail;
					skipActionTrue = tempAction.skipActionTrue;
					skipActionFail = tempAction.skipActionFail;
				}
				else if (action is ActionSceneCheck)
				{
					ActionSceneCheck tempAction = (ActionSceneCheck) action;
					resultActionTrue = tempAction.resultActionTrue;
					resultActionFail = tempAction.resultActionFail;
					skipActionTrue = tempAction.skipActionTrue;
					skipActionFail = tempAction.skipActionFail;
				}
				
				if (resultActionTrue == AC.Action.ResultAction.Continue)
				{
					DrawStraightLine.Draw (start + new Vector2 (-labelWidth/4, 0), end, Color.blue, 2, false);
				}
				else if (resultActionTrue == AC.Action.ResultAction.Skip)
				{
					DrawStraightLine.Draw (start + new Vector2 (-labelWidth/4, 0), end + new Vector2 (0, (skipActionTrue - 1 -i) * (labelHeight + labelGap)), Color.blue, 2, false);
				}
				
				if (resultActionFail == AC.Action.ResultAction.Continue)
				{
					DrawStraightLine.Draw (start + new Vector2 (+labelWidth/4, 0), end, Color.blue, 2, false);
				}
				else if (resultActionFail == AC.Action.ResultAction.Skip)
				{
					DrawStraightLine.Draw (start + new Vector2 (+labelWidth/4, 0), end + new Vector2 (0, (skipActionFail - 1 -i) * (labelHeight + labelGap)), Color.blue, 2, false);
				}
			}
			else
			{
				if (action.endAction == AC.Action.ResultAction.Continue || (action.endAction == AC.Action.ResultAction.Skip && action.skipAction == i+1))
				{
					DrawStraightLine.Draw (start, end, Color.blue, 2, false);
				}
				else if (action.endAction == AC.Action.ResultAction.Skip)
				{
					DrawStraightLine.Draw (start, end + new Vector2 (labelWidth/4, (action.skipAction - 1 -i) * (labelHeight + labelGap)), Color.blue, 2, false);
				}
			}
		}
	}
	
	
	private void BulkGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Mark all", EditorStyles.miniButtonLeft))
		{
			foreach (AC.Action action in _target.actions)
			{
				action.isMarked = true;
			}
		}
		if (GUILayout.Button ("Unmark all", EditorStyles.miniButtonMid))
		{
			foreach (AC.Action action in _target.actions)
			{
				action.isMarked = false;
			}
		}
		if (GUILayout.Button ("Copy marked", EditorStyles.miniButtonMid))
		{
			List<AC.Action> copyList = new List<AC.Action>();
			foreach (AC.Action action in _target.actions)
			{
				if (action.isMarked)
				{
					AC.Action copyAction = Object.Instantiate (action) as AC.Action;
					copyList.Add (copyAction);
				}
			}
			AdvGame.copiedActions = copyList;
		}
		if (GUILayout.Button ("Delete marked", EditorStyles.miniButtonRight))
		{
			while (AreAnyActionsMarked (_target.actions))
			{
				foreach (AC.Action action in _target.actions)
				{
					if (action.isMarked)
					{
						_target.actions.Remove (action);
						DestroyImmediate (action);
						break;
					}
				}
			}
			if (_target.actions.Count == 0)
			{
				_target.actions.Add (ActionListEditor.GetDefaultAction ());
			}
		}
		
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
	}
	
	
	private int GetTypeNumber (int i)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		
		int number = 0;
		
		if (actionsManager)
		{
			for (int j=0; j<actionsManager.GetActionsSize(); j++)
			{
				try
				{
					if (_target.actions[i].GetType().ToString() == actionsManager.GetActionName(j))
					{
						number = j;
						break;
					}
				}
				
				catch
				{
					string defaultAction = actionsManager.GetDefaultAction ();
					_target.actions[i] = (AC.Action) CreateInstance (defaultAction);
				}
			}
		}
		
		return number;
	}
	
	
	private bool AreAnyActionsMarked (List<AC.Action> actions)
	{
		foreach (AC.Action action in actions)
		{
			if (action.isMarked)
			{
				return true;
			}
		}
		
		return false;
	}
	
}
