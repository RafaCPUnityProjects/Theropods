/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionAnim.cs"
 * 
 *	This action is used for standard animation playback for GameObjects.
 *	It is fairly simplistic, and not meant for characters.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionAnim : Action
{

	// 3D variables
	
	public Animation _anim;
	public AnimationClip clip;
	public float fadeTime = 0f;
	
	// 2D variables
	
	public Transform _anim2D;
	public Animator animator;
	public string clip2D;
	public enum WrapMode2D { Once, Loop, PingPong };
	public WrapMode2D wrapMode2D;
	public int layerInt;

	// BlendShape variables

	public Shapeable shapeObject;
	public int shapeKey = 0;
	public float shapeValue = 0f;
	public bool isPlayer = false;

	// Mecanim variables

	public MecanimParameterType mecanimParameterType;
	public string parameterName;
	public float parameterValue;

	// Regular variables
	
	public enum AnimMethod { PlayCustom, StopCustom, BlendShape };
	public AnimMethod method;
	
	public AnimationBlendMode blendMode = AnimationBlendMode.Blend;
	public AnimPlayMode playMode;
	
	public AnimationEngine animationEngine = AnimationEngine.Legacy;
	public AnimEngine animEngine;

	
	public ActionAnim ()
	{
		this.isDisplayed = true;
		title = "Object: Animate";
	}
	
	
	override public float Run ()
	{
		if (method == AnimMethod.BlendShape && isPlayer)
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Shapeable>())
			{
				shapeObject = GameObject.FindWithTag (Tags.player).GetComponent <Shapeable>();
			}
			else
			{
				shapeObject = null;
				Debug.LogWarning ("Cannot BlendShape Player since cannot find Shapeable script on Player.");
				return 0f;
			}
		}

		ResetAnimationEngine ();

		if (!isRunning)
		{
			isRunning = true;
			
			if (_anim2D && clip2D != "" && animationEngine == AnimationEngine.Sprites2DToolkit)
			{
				if (method == AnimMethod.PlayCustom)
				{
					if (wrapMode2D == WrapMode2D.Loop)
					{
						tk2DIntegration.PlayAnimation (_anim2D, clip2D, true, WrapMode.Loop);
					}
					else if (wrapMode2D == WrapMode2D.PingPong)
					{
						tk2DIntegration.PlayAnimation (_anim2D, clip2D, true, WrapMode.PingPong);
					}
					else
					{
						tk2DIntegration.PlayAnimation (_anim2D, clip2D, true, WrapMode.Once);
					}
					
					if (willWait)
					{
						return (defaultPauseTime);
					}
				}
				
				else if (method == AnimMethod.StopCustom)
				{
					tk2DIntegration.StopAnimation (_anim2D);
				}

				else if (method == AnimMethod.BlendShape)
				{
					Debug.LogWarning ("BlendShapes not available for 2D animation.");
					return 0f;
				}
			}

			else if (animator && clip2D != "" && animationEngine == AnimationEngine.SpritesUnity)
			{
				if (method == AnimMethod.PlayCustom)
				{
					animator.CrossFade (clip2D, fadeTime, layerInt);

					if (willWait)
					{
						return (defaultPauseTime);
					}
				}

				else if (method == AnimMethod.BlendShape)
				{
					Debug.LogWarning ("BlendShapes not available for 2D animation.");
					return 0f;
				}
			}
			
			else if (animationEngine == AnimationEngine.Legacy)
			{	
				if (method == AnimMethod.PlayCustom && _anim && clip)
				{
					AdvGame.CleanUnusedClips (_anim);
					
					WrapMode wrap = WrapMode.Once;
					if (playMode == AnimPlayMode.PlayOnceAndClamp)
					{
						wrap = WrapMode.ClampForever;
					}
					else if (playMode == AnimPlayMode.Loop)
					{
						wrap = WrapMode.Loop;
					}
						
					AdvGame.PlayAnimClip (_anim, 0, clip, blendMode, wrap, fadeTime, null);
				}
				
				else if (method == AnimMethod.StopCustom && _anim && clip)
				{
					AdvGame.CleanUnusedClips (_anim);
					_anim.GetComponent<Animation>().Blend (clip.name, 0f, fadeTime);
				}

				else if (method == AnimMethod.BlendShape && shapeObject && shapeKey > -1)
				{
					shapeObject.Change (shapeKey, shapeValue, fadeTime);
				}
				
				if (willWait)
				{
					return (defaultPauseTime);
				}
			}

			else if (animationEngine == AnimationEngine.Mecanim)
			{
				if (method == ActionAnim.AnimMethod.PlayCustom && animator && parameterName != "")
				{
					if (mecanimParameterType == MecanimParameterType.Float)
					{
						animator.SetFloat (parameterName, parameterValue);
					}
					else if (mecanimParameterType == MecanimParameterType.Int)
					{
						animator.SetInteger (parameterName, (int) parameterValue);
					}
					
					if (mecanimParameterType == MecanimParameterType.Bool)
					{
						bool paramValue = false;
						if (parameterValue > 0f)
						{
							paramValue = true;
						}
						animator.SetBool (parameterName, paramValue);
					}
					
					if (mecanimParameterType == MecanimParameterType.Trigger)
					{
						animator.SetTrigger (parameterName);
					}
					
					return 0f;
				}
				
				else if (method == ActionAnim.AnimMethod.BlendShape && shapeObject && shapeKey > -1)
				{
					shapeObject.Change (shapeKey, shapeValue, fadeTime);
					
					if (willWait)
					{
						return (defaultPauseTime);
					}
				}
			}

			return 0f;
		}
		else
		{
			if (animationEngine == AnimationEngine.Sprites2DToolkit)
			{
				if (_anim2D && clip2D != "")
				{
					if (!tk2DIntegration.IsAnimationPlaying (_anim2D, clip2D))
					{
						isRunning = false;
						return 0f;
					}
					else
					{
						return (defaultPauseTime / 6f);
					}
				}
			}

			else if (animationEngine == AnimationEngine.SpritesUnity)
			{
				if (animator && clip2D != "")
				{
					if (animator.GetCurrentAnimatorStateInfo (layerInt).normalizedTime < 1f)
					{
						return (defaultPauseTime / 6f);
					}
					else
					{
						isRunning = false;
						return 0f;
					}
				}
			}

			else if (animationEngine == AnimationEngine.Legacy)
			{
     			if (method == AnimMethod.PlayCustom && _anim && clip)
				{
					if (!_anim.IsPlaying (clip.name))
					{
						isRunning = false;
						return 0f;
					}
					else
					{
						return defaultPauseTime;
					}
				}
				else if (method == AnimMethod.BlendShape && shapeObject)
				{
					if (!shapeObject.IsChanging ())
					{
						isRunning = false;
						return 0f;
					}
					else
					{
						return defaultPauseTime;
					}
				}
			}

			else if (animationEngine == AnimationEngine.Mecanim)
			{
				if (method == AnimMethod.BlendShape && shapeObject)
				{
					if (!shapeObject.IsChanging ())
					{
						isRunning = false;
						return 0f;
					}
					else
					{
						return defaultPauseTime;
					}
				}
			}
			
			return 0f;
		}
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		ResetAnimationEngine ();
		
		animationEngine = (AnimationEngine) EditorGUILayout.EnumPopup ("Animation engine:", animationEngine);

		method = (AnimMethod) EditorGUILayout.EnumPopup ("Method:", method);

		if (animEngine)
		{
			animEngine.ActionAnimGUI (this);
		}

		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";

		if (animEngine)
		{
			labelAdd = " (" + animEngine.ActionAnimLabel (this) + ")";
		}

		return labelAdd;
	}
	
	#endif


	private void ResetAnimationEngine ()
	{
		string className = "AnimEngine_" + animationEngine.ToString ();

		if (animEngine == null || animEngine.ToString () != className)
		{
			animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
		}
	}

}
