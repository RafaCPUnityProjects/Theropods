/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Mecanim.cs"
 * 
 *	This script uses the Mecanim
 *	system for 3D animation.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Mecanim : AnimEngine
{

	public override void Declare (AC.Char _character)
	{
		character = _character;
		turningIsLinear = false;
		rootMotion = true;
	}


	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.LabelField ("Mecanim parameters:", EditorStyles.boldLabel);

		character.moveSpeedParameter = EditorGUILayout.TextField ("Move speed float:", character.moveSpeedParameter);
		character.turnParameter = EditorGUILayout.TextField ("Turn float:", character.turnParameter);
		character.talkParameter = EditorGUILayout.TextField ("Talk bool:", character.talkParameter);
		character.talkingAnimation = TalkingAnimation.Standard;
		character.relyOnRootMotion = EditorGUILayout.Toggle ("Rely on Root Motion?", character.relyOnRootMotion);

		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Bone transforms:", EditorStyles.boldLabel);
		
		character.leftHandBone = (Transform) EditorGUILayout.ObjectField ("Left hand:", character.leftHandBone, typeof (Transform), true);
		character.rightHandBone = (Transform) EditorGUILayout.ObjectField ("Right hand:", character.rightHandBone, typeof (Transform), true);
		
		#endif
	}


	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.methodMecanim = (AnimMethodCharMecanim) EditorGUILayout.EnumPopup ("Method:", action.methodMecanim);

		if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
		{
			action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
			action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
			action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
		}

		else if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
		{
			action.mecanimCharParameter = (MecanimCharParameter) EditorGUILayout.EnumPopup ("Parameter to change:", action.mecanimCharParameter);
			action.parameterName = EditorGUILayout.TextField ("New parameter name:", action.parameterName);
		}

		#endif
	}
	
	
	public override float ActionCharAnimRun (ActionCharAnim action)
	{
		if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
		{
			if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
			{
				action.animChar.moveSpeedParameter = action.parameterName;
			}
			else if (action.mecanimCharParameter == MecanimCharParameter.TalkBool)
			{
				action.animChar.talkParameter = action.parameterName;
			}
			else if (action.mecanimCharParameter == MecanimCharParameter.TurnFloat)
			{
				action.animChar.turnParameter = action.parameterName;
			}
		}

		else if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
		{
			if (action.animChar.GetComponent <Animator>())
			{
				Animator animator = action.animChar.GetComponent <Animator>();

				if (action.mecanimParameterType == MecanimParameterType.Float)
				{
					animator.SetFloat (action.parameterName, action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Int)
				{
					animator.SetInteger (action.parameterName, (int) action.parameterValue);
				}

				if (action.mecanimParameterType == MecanimParameterType.Bool)
				{
					bool paramValue = false;
					if (action.parameterValue > 0f)
					{
						paramValue = true;
					}
					animator.SetBool (action.parameterName, paramValue);
				}

				if (action.mecanimParameterType == MecanimParameterType.Trigger)
				{
					animator.SetTrigger (action.parameterName);
				}
			}
		}

		return 0f;
	}


	public override void ActionCharHoldGUI (ActionCharHold action)
	{
		#if UNITY_EDITOR
		
		action.objectToHold = (GameObject) EditorGUILayout.ObjectField ("Object to hold:", action.objectToHold, typeof (GameObject), true);
		action.hand = (ActionCharHold.Hand) EditorGUILayout.EnumPopup ("Hand:", action.hand);
		action.rotate90 = EditorGUILayout.Toggle ("Rotate 90 degrees?", action.rotate90);
		
		#endif
	}
	
	
	public override void ActionCharHoldRun (ActionCharHold action)
	{
		if (action.objectToHold)
		{
			Transform handTransform;
			
			if (action.hand == ActionCharHold.Hand.Left)
			{
				handTransform = action._char.leftHandBone;
			}
			else
			{
				handTransform = action._char.rightHandBone;
			}
			
			if (handTransform)
			{
				action.objectToHold.transform.parent = handTransform;
				action.objectToHold.transform.localPosition = Vector3.zero;
				
				if (action.rotate90)
				{
					action.objectToHold.transform.localEulerAngles = new Vector3 (0f, 0f, 90f);
				}
				else
				{
					action.objectToHold.transform.localEulerAngles = Vector3.zero;
				}
			}
			else
			{
				Debug.Log ("Cannot parent object - no hand bone found.");
			}
		}
	}


	public override void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR
		
		if (action.method == ActionAnim.AnimMethod.PlayCustom)
		{
			action.animator = (Animator) EditorGUILayout.ObjectField ("Animator:", action.animator, typeof (Animator), true);
			action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
			action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
			action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
		}
		else if (action.method == ActionAnim.AnimMethod.StopCustom)
		{
			EditorGUILayout.HelpBox ("This method is not compatible with Mecanim.", MessageType.Info);
		}
		else if (action.method == ActionAnim.AnimMethod.BlendShape)
		{
			action.isPlayer = EditorGUILayout.Toggle ("Is player?", action.isPlayer);
			if (!action.isPlayer)
			{
				action.shapeObject = (Shapeable) EditorGUILayout.ObjectField ("Object:", action.shapeObject, typeof (Shapeable), true);
			}
			action.shapeKey = EditorGUILayout.IntField ("Shape key:", action.shapeKey);
			action.shapeValue = EditorGUILayout.Slider ("Shape value:", action.shapeValue, 0f, 100f);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
		}
		
		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action.animator)
		{
			label = action.animator.name;
			
			if (action.method == ActionAnim.AnimMethod.PlayCustom && action.parameterName != "")
			{
				label += " - " + action.parameterName;
			}
			else if (action.method == ActionAnim.AnimMethod.BlendShape)
			{
				label += " - Shapekey";
			}
		}
		
		return label;
	}


	public override float ActionAnimRun (ActionAnim action)
	{
		if (action.method == ActionAnim.AnimMethod.StopCustom)
		{
			return 0f;
		}

		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionAnim.AnimMethod.PlayCustom && action.animator && action.parameterName != "")
			{
				if (action.mecanimParameterType == MecanimParameterType.Float)
				{
					action.animator.SetFloat (action.parameterName, action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Int)
				{
					action.animator.SetInteger (action.parameterName, (int) action.parameterValue);
				}
				
				if (action.mecanimParameterType == MecanimParameterType.Bool)
				{
					bool paramValue = false;
					if (action.parameterValue > 0f)
					{
						paramValue = true;
					}
					action.animator.SetBool (action.parameterName, paramValue);
				}
				
				if (action.mecanimParameterType == MecanimParameterType.Trigger)
				{
					action.animator.SetTrigger (action.parameterName);
				}

				return 0f;
			}
			
			else if (action.method == ActionAnim.AnimMethod.BlendShape && action.shapeObject && action.shapeKey > -1)
			{
				action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);

				if (action.willWait)
				{
					return (action.defaultPauseTime);
				}
			}
		}
		else
		{
			if (action.method == ActionAnim.AnimMethod.BlendShape && action.shapeObject)
			{
				if (!action.shapeObject.IsChanging ())
				{
					action.isRunning = false;
				}
				else
				{
					return action.defaultPauseTime;
				}
			}
		}
		
		return 0f;
	}


	public override void PlayIdle ()
	{
		if (character.GetComponent <Animator>())
		{
			if (character.moveSpeedParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.moveSpeedParameter, 0f);
			}

			if (character.talkParameter != "")
			{
				character.GetComponent <Animator>().SetBool (character.talkParameter, false);
			}

			if (character.turnParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.turnParameter, 0f);
			}
		}
	}


	public override void PlayWalk ()
	{
		if (character.moveSpeedParameter != "" && character.GetComponent <Animator>())
		{
			character.GetComponent <Animator>().SetFloat (character.moveSpeedParameter, character.walkSpeedScale);
		}
	}


	public override void PlayRun ()
	{
		if (character.moveSpeedParameter != "" && character.GetComponent <Animator>())
		{
			character.GetComponent <Animator>().SetFloat (character.moveSpeedParameter, character.runSpeedScale);
		}
	}


	public override void PlayTalk ()
	{
		if (character.talkParameter != "" && character.GetComponent <Animator>())
		{
			character.GetComponent <Animator>().SetBool (character.talkParameter, true);
		}
	}


	public override void PlayTurnLeft ()
	{
		if (character.GetComponent <Animator>())
		{
			if (character.turnParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.turnParameter, -1f);
			}

			if (character.talkParameter != "")
			{
				character.GetComponent <Animator>().SetBool (character.talkParameter, false);
			}

			if (character.moveSpeedParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.moveSpeedParameter, 0f);
			}
		}
	}
	
	
	public override void PlayTurnRight ()
	{
		if (character.GetComponent <Animator>())
		{
			if (character.turnParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.turnParameter, 1f);
			}
			
			if (character.talkParameter != "")
			{
				character.GetComponent <Animator>().SetBool (character.talkParameter, false);
			}
			
			if (character.moveSpeedParameter != "")
			{
				character.GetComponent <Animator>().SetFloat (character.moveSpeedParameter, 0f);
			}
		}
	}

}
