/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Legacy.cs"
 * 
 *	This script uses the Legacy
 *	system for 3D animation.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Legacy : AnimEngine
{

	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR

		EditorGUILayout.LabelField ("Standard 3D animations:", EditorStyles.boldLabel);

		character.talkingAnimation = (TalkingAnimation) EditorGUILayout.EnumPopup ("Talk animation style:", character.talkingAnimation);
		character.idleAnim = (AnimationClip) EditorGUILayout.ObjectField ("Idle:", character.idleAnim, typeof (AnimationClip), false);
		character.walkAnim = (AnimationClip) EditorGUILayout.ObjectField ("Walk:", character.walkAnim, typeof (AnimationClip), false);
		character.runAnim = (AnimationClip) EditorGUILayout.ObjectField ("Run:", character.runAnim, typeof (AnimationClip), false);
		if (character.talkingAnimation == TalkingAnimation.Standard)
		{
			character.talkAnim = (AnimationClip) EditorGUILayout.ObjectField ("Talk:", character.talkAnim, typeof (AnimationClip), false);
		}
		character.turnLeftAnim = (AnimationClip) EditorGUILayout.ObjectField ("Turn left:", character.turnLeftAnim, typeof (AnimationClip), false);
		character.turnRightAnim = (AnimationClip) EditorGUILayout.ObjectField ("Turn right:", character.turnRightAnim, typeof (AnimationClip), false);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Bone transforms:", EditorStyles.boldLabel);
		
		character.upperBodyBone = (Transform) EditorGUILayout.ObjectField ("Upper body:", character.upperBodyBone, typeof (Transform), true);
		character.neckBone = (Transform) EditorGUILayout.ObjectField ("Neck bone:", character.neckBone, typeof (Transform), true);
		character.leftArmBone = (Transform) EditorGUILayout.ObjectField ("Left arm:", character.leftArmBone, typeof (Transform), true);
		character.rightArmBone = (Transform) EditorGUILayout.ObjectField ("Right arm:", character.rightArmBone, typeof (Transform), true);
		character.leftHandBone = (Transform) EditorGUILayout.ObjectField ("Left hand:", character.leftHandBone, typeof (Transform), true);
		character.rightHandBone = (Transform) EditorGUILayout.ObjectField ("Right hand:", character.rightHandBone, typeof (Transform), true);

		#endif
	}


	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom || action.method == ActionCharAnim.AnimMethodChar.StopCustom)
		{
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
			{
				action.layer = (AnimLayer) EditorGUILayout.EnumPopup ("Layer:", action.layer);
				
				if (action.layer == AnimLayer.Base)
				{
					EditorGUILayout.LabelField ("Blend mode:", "Blend");
					action.playModeBase = (AnimPlayModeBase) EditorGUILayout.EnumPopup ("Play mode:", action.playModeBase);
				}
				else
				{
					action.blendMode = (AnimationBlendMode) EditorGUILayout.EnumPopup ("Blend mode:", action.blendMode);
					action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
				}
			}
			
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
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
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip)
			{
				AdvGame.CleanUnusedClips (action.animChar.GetComponent<Animation>());
				
				WrapMode wrap = WrapMode.Once;
				Transform mixingTransform = null;
				
				if (action.layer == AnimLayer.Base)
				{
					action.animChar.charState = CharState.Custom;
					action.blendMode = AnimationBlendMode.Blend;
					action.playMode = (AnimPlayMode) action.playModeBase;
				}
				else if (action.layer == AnimLayer.UpperBody)
				{
					mixingTransform = action.animChar.upperBodyBone;
				}
				else if (action.layer == AnimLayer.LeftArm)
				{
					mixingTransform = action.animChar.leftArmBone;
				}
				else if (action.layer == AnimLayer.RightArm)
				{
					mixingTransform = action.animChar.rightArmBone;
				}
				else if (action.layer == AnimLayer.Neck || action.layer == AnimLayer.Head || action.layer == AnimLayer.Face || action.layer == AnimLayer.Mouth)
				{
					mixingTransform = action.animChar.neckBone;
				}
				
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrap = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrap = WrapMode.Loop;
				}
				
				AdvGame.PlayAnimClip (action.animChar.GetComponent <Animation>(), (int) action.layer, action.clip, action.blendMode, wrap, action.fadeTime, mixingTransform);
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom && action.clip)
			{
				if (action.clip != action.animChar.idleAnim && action.clip != action.animChar.walkAnim)
				{
					action.animChar.GetComponent<Animation>().Blend (action.clip.name, 0f, action.fadeTime);
				}
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.animChar.ResetBaseClips ();
				
				action.animChar.charState = CharState.Idle;
				AdvGame.CleanUnusedClips (action.animChar.GetComponent<Animation>());
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				if (action.clip != null)
				{
					if (action.standard == AnimStandard.Idle)
					{
						action.animChar.idleAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						action.animChar.talkAnim = action.clip;
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
			
			if (action.willWait && action.clip)
			{
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return action.defaultPauseTime;
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
				{
					return action.fadeTime;
				}
			}
		}	
		
		else
		{
			if (action.animChar.GetComponent<Animation>() [action.clip.name] && action.animChar.GetComponent<Animation>() [action.clip.name].normalizedTime < 1f && action.animChar.GetComponent<Animation>().IsPlaying (action.clip.name))
			{
				return action.defaultPauseTime;
			}
			else
			{
				action.isRunning = false;
				
				if (action.playMode == AnimPlayMode.PlayOnce)
				{
					action.animChar.GetComponent<Animation>().Blend (action.clip.name, 0f, action.fadeTime);
					
					if (action.layer == AnimLayer.Base && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
					{
						action.animChar.charState = CharState.Idle;
						action.animChar.ResetBaseClips ();
					}
				}
				
				AdvGame.CleanUnusedClips (action.animChar.GetComponent<Animation>());
				
				return 0f;
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


	public override void ActionSpeechGUI (ActionSpeech action)
	{
		#if UNITY_EDITOR

		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace)
		{
			action.headClip = (AnimationClip) EditorGUILayout.ObjectField ("Head animation:", action.headClip, typeof (AnimationClip), true);
			action.mouthClip = (AnimationClip) EditorGUILayout.ObjectField ("Mouth animation:", action.mouthClip, typeof (AnimationClip), true);
		}

		#endif
	}


	public override void ActionSpeechRun (ActionSpeech action)
	{
		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace && (action.headClip || action.mouthClip))
		{
			AdvGame.CleanUnusedClips (action.speaker.GetComponent <Animation>());	
			
			if (action.headClip)
			{
				AdvGame.PlayAnimClip (action.speaker.GetComponent <Animation>(), (int) AnimLayer.Head, action.headClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone);
			}
			
			if (action.mouthClip)
			{
				AdvGame.PlayAnimClip (action.speaker.GetComponent <Animation>(), (int) AnimLayer.Mouth, action.mouthClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone);
			}
		}
	}


	public override void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR

		if (action.method == ActionAnim.AnimMethod.PlayCustom)
		{
			action._anim = (Animation) EditorGUILayout.ObjectField ("Object:", action._anim, typeof (Animation), true);
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
			action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
			action.blendMode = (AnimationBlendMode) EditorGUILayout.EnumPopup ("Blend mode:",action.blendMode);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
		}
		else if (action.method == ActionAnim.AnimMethod.StopCustom)
		{
			action._anim = (Animation) EditorGUILayout.ObjectField ("Object:", action._anim, typeof (Animation), true);
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
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
		}
		
		action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);

		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action._anim)
		{
			label = action._anim.name;
			
			if (action.method == ActionAnim.AnimMethod.PlayCustom && action.clip)
			{
				label += " - Play " + action.clip.name;
			}
			else if (action.method == ActionAnim.AnimMethod.StopCustom && action.clip)
			{
				label += " - Stop " + action.clip.name;
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
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionAnim.AnimMethod.PlayCustom && action._anim && action.clip)
			{
				AdvGame.CleanUnusedClips (action._anim);
				
				WrapMode wrap = WrapMode.Once;
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrap = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrap = WrapMode.Loop;
				}
				
				AdvGame.PlayAnimClip (action._anim, 0, action.clip, action.blendMode, wrap, action.fadeTime, null);
			}
			
			else if (action.method == ActionAnim.AnimMethod.StopCustom && action._anim && action.clip)
			{
				AdvGame.CleanUnusedClips (action._anim);
				action._anim.GetComponent<Animation>().Blend (action.clip.name, 0f, action.fadeTime);
			}
			
			else if (action.method == ActionAnim.AnimMethod.BlendShape && action.shapeObject && action.shapeKey > -1)
			{
				action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);
			}
			
			if (action.willWait)
			{
				return (action.defaultPauseTime);
			}
		}
		else
		{

			if (action.method == ActionAnim.AnimMethod.PlayCustom && action._anim && action.clip)
			{
				if (!action._anim.IsPlaying (action.clip.name))
				{
					action.isRunning = false;
				}
				else
				{
					return action.defaultPauseTime;
				}
			}
			else if (action.method == ActionAnim.AnimMethod.BlendShape && action.shapeObject)
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
		PlayStandardAnim (character.idleAnim, true);
	}
	
	
	public override void PlayWalk ()
	{
		PlayStandardAnim (character.walkAnim, true);
	}
	
	
	public override void PlayRun ()
	{
		PlayStandardAnim (character.runAnim, true);
	}


	public override void PlayTalk ()
	{
		PlayStandardAnim (character.talkAnim, true);
	}
	
	
	public override void PlayTurnLeft ()
	{
		if (character.turnLeftAnim)
		{
			PlayStandardAnim (character.turnLeftAnim, false);
		}
		else
		{
			PlayIdle ();
		}
	}
	
	
	public override void PlayTurnRight ()
	{
		if (character.turnRightAnim)
		{
			PlayStandardAnim (character.turnRightAnim, false);
		}
		else
		{
			PlayIdle ();
		}
	}
	
	
	private void PlayStandardAnim (AnimationClip clip, bool doLoop)
	{
		if (clip != null && character != null && character.GetComponent<Animation>()[clip.name] != null)
		{
			if (!character.GetComponent<Animation>() [clip.name].enabled)
			{
				if (doLoop)
				{
					AdvGame.PlayAnimClip (character.GetComponent<Animation>(), (int) AnimLayer.Base, clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null);
				}
				else
				{
					AdvGame.PlayAnimClip (character.GetComponent<Animation>(), (int) AnimLayer.Base, clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null);
				}
			}
		}
		else
		{
			if (doLoop)
			{
				AdvGame.PlayAnimClip (character.GetComponent<Animation>(), (int) AnimLayer.Base, clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null);
			}
			else
			{
				AdvGame.PlayAnimClip (character.GetComponent<Animation>(), (int) AnimLayer.Base, clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null);
			}
		}
	}

}

