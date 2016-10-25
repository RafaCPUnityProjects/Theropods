/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharHold.cs"
 * 
 *	This action parents a GameObject to a character's hand.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharHold : Action
{
	
	public GameObject objectToHold;
	public bool isPlayer;
	public Char _char;
	public bool rotate90;
	
	public enum Hand { Left, Right };
	public Hand hand;
	
	
	public ActionCharHold ()
	{
		this.isDisplayed = true;
		title = "Character: Hold object";
	}
	
	
	override public float Run ()
	{
		if (isPlayer)
		{
			_char = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
		}
		
		if (_char)
		{
			if (_char.animEngine == null)
			{
				_char.ResetAnimationEngine ();
			}
			
			if (_char.animEngine != null)
			{
				_char.animEngine.ActionCharHoldRun (this);
			}
		}
		else
		{
			Debug.LogError ("Could not create animation engine!");
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				_char = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
			}
			else
			{
				_char = AdvGame.GetReferences ().settingsManager.player;
			}
		}
		else
		{
			_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
		}
		
		if (_char)
		{
			if (_char.animEngine == null)
			{
				_char.ResetAnimationEngine ();
			}
			if (_char.animEngine)
			{
				_char.animEngine.ActionCharHoldGUI (this);
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
		}
		
		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		string labelAdd = "";
		
		if (_char && objectToHold)
		{
			labelAdd = "(" + _char.name + " hold " + objectToHold.name + ")";
		}
		
		return labelAdd;
	}
	
	#endif
	
	
}
