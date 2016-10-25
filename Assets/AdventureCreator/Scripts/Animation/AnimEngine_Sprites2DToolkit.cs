/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Sprites2DToolkit.cs"
 * 
 *	This script uses the 2D Toolkit
 *	sprite engine for animation.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Sprites2DToolkit : AnimEngine
{

	public override void Declare (AC.Char _character)
	{
		character = _character;
		turningIsLinear = true;
		rootMotion = false;
	}


	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR

		EditorGUILayout.LabelField ("Standard 2D animations:", EditorStyles.boldLabel);

		character.talkingAnimation = TalkingAnimation.Standard;
		character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Sprite child:", character.spriteChild, typeof (Transform), true);
		character.idleAnimSprite = EditorGUILayout.TextField ("Idle name:", character.idleAnimSprite);
		character.walkAnimSprite = EditorGUILayout.TextField ("Walk name:", character.walkAnimSprite);
		character.runAnimSprite = EditorGUILayout.TextField ("Run name:", character.runAnimSprite);
		character.talkAnimSprite = EditorGUILayout.TextField ("Talk name:", character.talkAnimSprite);
		character.doDiagonals = EditorGUILayout.Toggle ("Diagonal sprites?", character.doDiagonals);
		character.frameFlipping = (AC_2DFrameFlipping) EditorGUILayout.EnumPopup ("Frame flipping:", character.frameFlipping);

		#endif
	}


	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.includeDirection = EditorGUILayout.Toggle ("Add directional suffix?", action.includeDirection);
			
			action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
			if (action.playMode == AnimPlayMode.Loop)
			{
				action.willWait = false;
			}
			else
			{
				action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
			}
			
			action.layer = AnimLayer.Base;
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
		{
			EditorGUILayout.HelpBox ("This Action does not work for Sprite-based characters.", MessageType.Info);
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.standard = (AnimStandard) EditorGUILayout.EnumPopup ("Change:", action.standard);

			if (action.standard == AnimStandard.Walk || action.standard == AnimStandard.Run)
			{
				action.changeSpeed = EditorGUILayout.Toggle ("Change speed?", action.changeSpeed);
				if (action.changeSpeed)
				{
					action.newSpeed = EditorGUILayout.FloatField ("New speed:", action.newSpeed);
				}
			}
		}

		#endif
	}


	public override float ActionCharAnimRun (ActionCharAnim action)
	{
		string clip2DNew = action.clip2D;
		if (action.includeDirection)
		{
			clip2DNew += action.animChar.GetSpriteDirection ();
		}
		
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != "")
			{
				action.animChar.charState = CharState.Custom;
				
				if (action.playMode == AnimPlayMode.Loop)
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Loop);
					action.willWait = false;
				}
				else
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Once);
				}
			}

			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.animChar.ResetBaseClips ();
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				if (action.clip2D != "")
				{
					if (action.standard == AnimStandard.Idle)
					{
						action.animChar.idleAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						action.animChar.talkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runAnimSprite = action.clip2D;
					}
				}

				if (action.changeSpeed)
				{
					if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkSpeedScale = action.newSpeed;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runSpeedScale = action.newSpeed;
					}
				}
			}
			
			if (action.willWait && action.clip2D != "")
			{
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return (action.defaultPauseTime);
				}
			}
		}	
		
		else
		{
			if (tk2DIntegration.IsAnimationPlaying (action.animChar.spriteChild, clip2DNew))
			{
				return (Time.deltaTime);
			}
			else
			{
				action.isRunning = false;
				
				if (action.playMode == AnimPlayMode.PlayOnce)
				{
					action.animChar.charState = CharState.Idle;
				}
				
				return 0f;
			}
		}

		return 0f;
	}


	public override void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR

		action._anim2D = (Transform) EditorGUILayout.ObjectField ("Object:", action._anim2D, typeof (Transform), true);
		
		if (action.method == ActionAnim.AnimMethod.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.wrapMode2D = (ActionAnim.WrapMode2D) EditorGUILayout.EnumPopup ("Play mode:", action.wrapMode2D);
			
			if (action.wrapMode2D == ActionAnim.WrapMode2D.Once)
			{
				action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
			}
			else
			{
				action.willWait = false;
			}
		}
		else if (action.method == ActionAnim.AnimMethod.BlendShape)
		{
			EditorGUILayout.HelpBox ("BlendShapes are not available in 2D animation.", MessageType.Info);
		}

		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action._anim2D)
		{
			label = action._anim2D.name;
			
			if (action.method == ActionAnim.AnimMethod.PlayCustom && action.clip2D != "")
			{
				label += " - " + action.clip2D;
			}
		}
		
		return label;
	}

	
	public override float ActionAnimRun (ActionAnim action)
	{
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action._anim2D && action.clip2D != "")
			{
				if (action.method == ActionAnim.AnimMethod.PlayCustom)
				{
					if (action.wrapMode2D == ActionAnim.WrapMode2D.Loop)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Loop);
					}
					else if (action.wrapMode2D == ActionAnim.WrapMode2D.PingPong)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.PingPong);
					}
					else
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Once);
					}
					
					if (action.willWait)
					{
						return (Time.deltaTime);
					}
				}
				
				else if (action.method == ActionAnim.AnimMethod.StopCustom)
				{
					tk2DIntegration.StopAnimation (action._anim2D);
				}
				
				else if (action.method == ActionAnim.AnimMethod.BlendShape)
				{
					Debug.LogWarning ("BlendShapes not available for 2D animation.");
				}
			}

		}
		else
		{
			if (action._anim2D && action.clip2D != "")
			{
				if (!tk2DIntegration.IsAnimationPlaying (action._anim2D, action.clip2D))
				{
					action.isRunning = false;
				}
				else
				{
					return (Time.deltaTime);
				}
			}
		}

		return 0f;
	}


	public override void PlayIdle ()
	{
		PlayStandardAnim (character.idleAnimSprite, true);
	}
	
	
	public override void PlayWalk ()
	{
		PlayStandardAnim (character.walkAnimSprite, true);
	}
	
	
	public override void PlayRun ()
	{
		PlayStandardAnim (character.runAnimSprite, true);
	}
	
	
	public override void PlayTalk ()
	{
		PlayStandardAnim (character.talkAnimSprite, true);
	}
	
	
	private void PlayStandardAnim (string clip, bool includeDirection)
	{
		if (clip != "" && character != null)
		{
			string newClip = clip;
			
			if (includeDirection)
			{
				newClip += character.GetSpriteDirection ();
			}
			
			if (tk2DIntegration.PlayAnimation (character.spriteChild, newClip) == false)
			{
				tk2DIntegration.PlayAnimation (character.spriteChild, clip);
			}
		}
	}

}
