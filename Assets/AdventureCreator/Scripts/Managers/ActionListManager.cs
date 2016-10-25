/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListManager.cs"
 * 
 *	This script keeps track of which ActionLists
 *	are running in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class ActionListManager : MonoBehaviour
{

	public bool showActiveActionLists = false;
	private Conversation conversationOnEnd;
	private List<ActionList> activeLists = new List<ActionList>();
	private RuntimeActionList runtimeActionList;
	private StateHandler stateHandler;


	private void Awake ()
	{
		activeLists.Clear ();

		runtimeActionList = GetComponent <RuntimeActionList>();
	}


	private void Start ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}
	}


	#if UNITY_EDITOR

	private void OnGUI ()
	{
		if (showActiveActionLists)
		{
			if (activeLists.Count > 0 || runtimeActionList.IsRunning ())
			{
				GUILayout.Label ("Current ActionLists running:", "Button");
				GUILayout.Space (10f);

				foreach (ActionList list in activeLists)
				{
					GUILayout.Label (list.gameObject.name, "Button");
				}

				if (runtimeActionList.IsRunning ())
				{
					GUILayout.Label ("Runtime ActionList", "Button");
				}
				GUILayout.Space (10f);
				GUILayout.Label ("Pausing gameplay: " + IsGameplayBlocked (), "Button");
			}
			else
			{
				GUILayout.Label ("No ActionLists are running", "Button");
			}
		}
	}

	#endif


	public void AddToList (ActionList _list, int startAction)
	{
		if (!IsListRunning (_list))
		{
			activeLists.Add (_list);
		}

		if (_list.conversation)
		{
			conversationOnEnd = _list.conversation;
		}

		SetCorrectGameState ();
	}


	public void EndRuntimeList ()
	{
		runtimeActionList.Kill ();

		if (runtimeActionList.pauseAfterEnd)
		{
			stateHandler.gameState = GameState.Paused;
			stateHandler.UpdateLastGameplayState ();
		}
		else
		{
			SetCorrectGameState ();
		}
	}


	public void EndList (ActionList _list)
	{
		if (IsListRunning (_list))
		{
			activeLists.Remove (_list);
		}

		if (_list.conversation == conversationOnEnd && _list.conversation != null)
		{
			if (stateHandler)
			{
				stateHandler.gameState = GameState.Cutscene;
			}
			else
			{
				Debug.LogWarning ("Could not set correct GameState!");
			}

			conversationOnEnd.Interact ();
			conversationOnEnd = null;
		}
		else
		{
			SetCorrectGameState ();
		}

		if (_list.autosaveAfter)
		{
			if (!IsGameplayBlocked ())
			{
				SaveSystem.SaveGame (-1);
			}
			else
			{
				Debug.LogWarning ("Cannot autosave because another cutscene has started.");
			}
		}
	}


	public bool IsListRunning (ActionList _list)
	{
		foreach (ActionList list in activeLists)
		{
			if (list == _list)
			{
				return true;
			}
		}

		return false;
	}


	public void KillAllLists ()
	{
		runtimeActionList.Kill ();

		foreach (ActionList list in activeLists)
		{
			list.Kill ();
		}

		activeLists.Clear ();
	}


	private void SetCorrectGameState ()
	{
		if (stateHandler == null)
		{
			Start ();
		}

		if (stateHandler != null)
		{
			if (IsGameplayBlocked ())
			{
				stateHandler.gameState = AC.GameState.Cutscene;
			}
			else
			{
				if (stateHandler.gameState == GameState.Cutscene)
				{
					stateHandler.gameState = AC.GameState.Normal;
				}
			}
		}
		else
		{
			Debug.LogWarning ("Could not set correct GameState!");
		}
	}


	private bool IsGameplayBlocked ()
	{
		if (runtimeActionList.IsRunning ())
		{
			return true;
		}

		foreach (ActionList list in activeLists)
		{
			if (list.actionListType == AC.ActionListType.PauseGameplay)
			{
				return true;
			}
		}

		return false;
	}


	private void OnDestroy ()
	{
		activeLists.Clear ();
	}

}
