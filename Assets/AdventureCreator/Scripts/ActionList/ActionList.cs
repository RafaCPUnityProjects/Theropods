/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionList.cs"
 * 
 *	This script stores, and handles the sequentual triggering of, actions.
 *	It is derived by Cutscene, Hotspot, Trigger, and DialogOption.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AC;

[System.Serializable]
public class ActionList : MonoBehaviour
{

	public float triggerTime = 0f;
	public bool autosaveAfter = false;
	public ActionListType actionListType = ActionListType.PauseGameplay;
	public List<AC.Action> actions = new List<AC.Action>();
	public Conversation conversation = null;
	
	protected int nextActionNumber = -1; 	// Set as -1 to stop running
	protected LayerMask LayerHotspot;
	protected LayerMask LayerOff;
	protected StateHandler stateHandler;
	protected ActionListManager actionListManager;
	
	
	private void Awake ()
	{
		LayerHotspot = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
		LayerOff = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);

		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>())
		{
			actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
		}
	}
	

	private void Start ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}
	}


	public virtual void Interact ()
	{
		if (actions.Count > 0)
		{
			if (triggerTime > 0f)
			{
				StartCoroutine ("PauseUntilStart");
			}
			else
			{
				actionListManager.AddToList (this, 0);
				BeginActionList (0);
			}
		}
	}


	public void Interact (int i)
	{
		if (actions.Count > 0 && actions.Count > i)
		{
			BeginActionList (i);
			actionListManager.AddToList (this, i);
		}
	}


	private IEnumerator PauseUntilStart ()
	{
		if (actions.Count > 0)
		{
			if (triggerTime > 0f)
			{
				yield return new WaitForSeconds (triggerTime);
			}

			BeginActionList (0);
			actionListManager.AddToList (this, 0);
		}
	}


	public void BeginActionList (int i)
	{
		nextActionNumber = i;
		ProcessAction (i);
	}

		
	private void ProcessAction (int thisActionNumber)
	{
		if (nextActionNumber > -1 && nextActionNumber < actions.Count && actions [thisActionNumber] is AC.Action)
		{
			if (!actions [thisActionNumber].isEnabled)
			{
				if (actions.Count > (thisActionNumber+1))
				{
					ProcessAction (thisActionNumber + 1);
				}
				else
				{
					EndCutscene ();
				}
			}
			else
			{
				nextActionNumber = thisActionNumber + 1;
				StartCoroutine ("RunAction", actions [thisActionNumber]);
			}
		}
		else
		{
			EndCutscene ();
		}
	}

	
	private IEnumerator RunAction (AC.Action action)
	{
		action.isRunning = false;
		
		float waitTime = action.Run ();		
		if (waitTime > 0f)
		{
			while (action.isRunning)
			{
				yield return new WaitForSeconds (waitTime);
				waitTime = action.Run ();
			}
		}

		int actionEnd = action.End (this.actions);

		if (actionEnd != 0)
		{
			nextActionNumber = actionEnd;
		}

		if (action.linkedCutscene)
		{
			action.linkedCutscene.SendMessage ("Interact");
		}

		if (actionEnd < 0)
		{
			EndCutscene ();
		}
		else
		{
			ProcessAction (nextActionNumber);
		}
	}

	
	protected virtual void EndCutscene ()
	{
		actionListManager.EndList (this);
	}
	

	private void TurnOn ()
	{
		gameObject.layer = LayerHotspot;
	}
	
	
	private void TurnOff ()
	{
		gameObject.layer = LayerOff;
	}
	
	
	public void Kill ()
	{
		nextActionNumber = -1;
		StopCoroutine ("RunAction");
		StopCoroutine ("InteractCoroutine");
	}


	private void OnDestroy()
	{
		actionListManager = null;
		stateHandler = null;
	}
}
