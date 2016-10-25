/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AdvGame.cs"
 * 
 *	This script provides a number of static functions used by various game scripts.
 * 
 * 	The "DrawOutline" function is based on Bérenger's code: http://wiki.unity3d.com/index.php/ShadowAndOutline
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AdvGame : ScriptableObject
{

	public static List<AC.Action> copiedActions = new List<AC.Action>();


	public static References GetReferences ()
	{
		References references = (References) Resources.Load (Resource.references);
		
		if (references)
		{
			return (references);
		}
		
		return (null);
	}


	public static void DuplicateActionsBuffer ()
	{

		List<AC.Action> tempList = new List<AC.Action>();
		foreach (AC.Action action in copiedActions)
		{
			AC.Action copyAction = Object.Instantiate (action) as AC.Action;
			tempList.Add (copyAction);
		}

		copiedActions.Clear ();
		copiedActions = tempList;
	}


	public static Vector3 GetScreenDirection (Vector3 originWorldPosition, Vector3 targetWorldPosition)
	{
		Vector3 originScreenPosition = Camera.main.WorldToScreenPoint (originWorldPosition);
		Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);

		Vector3 lookVector = targetScreenPosition - originScreenPosition;
		lookVector.z = lookVector.y;
		lookVector.y = 0;

		return (lookVector);
	}


	public static Vector3 GetScreenNavMesh (Vector3 targetWorldPosition)
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

		Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);
		Ray ray = Camera.main.ScreenPointToRay (targetScreenPosition);
		RaycastHit hit = new RaycastHit();
		
		if (settingsManager && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.navMeshLayer)))
		{
			return hit.point;
		}

		return targetWorldPosition;
	}


	public static Vector2 GetMainGameViewSize ()
	{
		if (Application.isPlaying)
		{
			return new Vector2 (Screen.width, Screen.height);
		}
		
		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod ("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		System.Object Res = GetSizeOfMainGameView.Invoke (null,null);
		return (Vector2) Res;
	}
	
	
	public static Matrix4x4 SetVanishingPoint (Camera _camera, Vector2 perspectiveOffset)
	{
		Matrix4x4 m = _camera.projectionMatrix;
		float w = 2f * _camera.nearClipPlane / m.m00;
		float h = 2f * _camera.nearClipPlane / m.m11;
	 
		float left = -(w / 2) + perspectiveOffset.x;
		float right = left + w;
		float bottom = -(h / 2) + perspectiveOffset.y;
		float top = bottom + h;
	 
		return (PerspectiveOffCenter (left, right, bottom, top, _camera.nearClipPlane, _camera.farClipPlane));
	}


	private static Matrix4x4 PerspectiveOffCenter (float left, float right, float bottom, float top, float near, float far)
	{
		float x =  (2f * near) / (right - left);
		float y =  (2f * near) / (top - bottom);
		float a =  (right + left) / (right - left);
		float b =  (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2f * far * near) / (far - near);
		float e = -1f;
	 
		Matrix4x4 m = new Matrix4x4();
		m[0,0] = x;		m[0,1] = 0f;	m[0,2] = a;		m[0,3] = 0f;
		m[1,0] = 0f;	m[1,1] = y;		m[1,2] = b;		m[1,3] = 0f;
		m[2,0] = 0f;	m[2,1] = 0f;	m[2,2] = c;		m[2,3] =   d;
		m[3,0] = 0f;	m[3,1] = 0f;	m[3,2] = e;		m[3,3] = 0f;
		return m;
	}
	
	
	public static string UniqueName (string name)
	{
		if (GameObject.Find (name))
		{
			string newName = name;
			
			for (int i=2; i<20; i++)
			{
				newName = name + i.ToString ();
				
				if (!GameObject.Find (newName))
				{
					break;
				}
			}
			
			return newName;
		}
		else
		{
			return name;
		}
	}
	
	
	public static string GetName (string resourceName)
	{
		int slash = resourceName.IndexOf ("/");
		string newName;
		
		if (slash > 0)
		{
			newName = resourceName.Remove (0, slash+1);
		}
		else
		{
			newName = resourceName;
		}
		
		return newName;
	}
	
	
	public static Rect GUIBox (float centre_x, float centre_y, float size)
	{
		Rect newRect;
		newRect = GUIRect (centre_x, centre_y, size, size);
		return (newRect);
	}
	
	
	public static Rect GUIRect (float centre_x, float centre_y, float width, float height)
	{
		Rect newRect;
		newRect = new Rect (Screen.width * centre_x - (Screen.width * width)/2, Screen.height * centre_y - (Screen.width * height)/2, Screen.width * width, Screen.width * height);
		return (newRect);
	}
	
	
	public static Rect GUIBox (Vector2 posVector, float size)
	{
		Rect newRect;
		newRect = GUIRect (posVector.x / Screen.width, (Screen.height - posVector.y) / Screen.height, size, size);
		return (newRect);
	}
	
	
	public static void AddAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, Transform mixingBone)
	{
		if (clip != null)
		{
			// Initialises a clip
			_animation.AddClip (clip, clip.name);
			
			if (mixingBone != null)
			{
				_animation [clip.name].AddMixingTransform (mixingBone);
			}
			
			// Set up the state
			_animation [clip.name].layer = layer;
			_animation [clip.name].normalizedTime = 0f;
			_animation [clip.name].blendMode = blendMode;
			_animation [clip.name].wrapMode = wrapMode;
			_animation [clip.name].enabled = true;
		}
	}
	
	
	public static void PlayAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone)
	{
		// Initialises and crossfades a clip

		if (clip != null)
		{
			AddAnimClip (_animation, layer, clip, blendMode, wrapMode, mixingBone);
			_animation.CrossFade (clip.name, fadeTime);
			CleanUnusedClips (_animation);
		}
	}


	public static AnimationClip FindAnimClipResource (string clipName)
	{
		if (clipName == "")
		{
			return null;
		}

		Object[] objects = Resources.FindObjectsOfTypeAll (typeof (AnimationClip));
		foreach (Object _object in objects)
		{
			if (_object.name == clipName)
			{
				return (AnimationClip) _object;
			}
		}

		return null;
	}

	
	public static void CleanUnusedClips (Animation _animation)
	{
		// Remove any non-playing animations
		
		List <string> removeClips = new List <string>();
		
		foreach (AnimationState state in _animation)
		{
			if (!_animation [state.name].enabled)
			{
				// Queued animations get " - Queued Clone" appended to it, so remove
				
				int queueIndex = state.name.IndexOf (" - Queued Clone");

				if (queueIndex > 0)
				{
					removeClips.Add (state.name.Substring (0, queueIndex));
				}
				else
				{
					removeClips.Add (state.name);
				}
			}
		}
		
		foreach (string _clip in removeClips)
		{
			_animation.RemoveClip (_clip);
		}
		
	}
	
	
	public static float SmoothTimeFactor (float startT, float deltaT)
	{
		// Return a smooth time scale
		
		float t01 = (Time.time - startT) / deltaT;
		float F = 0.5f - 0.515f * Mathf.Sin (3f * t01 + 1.5f);
		return F;
	}
	
	
	public static float LinearTimeFactor (float startT, float deltaT)
	{
		// Return a linear time scale

		float t01 = (Time.time - startT) / deltaT;
		return t01;
	}
	
	
	public static Rect Rescale (Rect _rect)
	{
		float ScaleFactor;
		ScaleFactor = Screen.width / 884.0f;
		int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
		Rect newRect = new Rect (_rect.x * ScaleFactorInt, _rect.y * ScaleFactorInt, _rect.width * ScaleFactorInt, _rect.height * ScaleFactorInt);
		
		return (newRect);
	}
	
	
	public static int Rescale (int _int)
	{
		float ScaleFactor;
		ScaleFactor = Screen.width / 884.0f;
		int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
		int returnValue;
		returnValue = _int * ScaleFactorInt;
		
		return (returnValue);
	}
	
	
	public static void DrawTextOutline (Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
	{
		float halfSize = size * 0.5F;
		GUIStyle backupStyle = new GUIStyle(style);
		Color backupColor = GUI.color;
		
		outColor.a = GUI.color.a;
		style.normal.textColor = outColor;
		GUI.color = outColor;

		rect.x -= halfSize;
		GUI.Label(rect, text, style);

		rect.x += size;
		GUI.Label(rect, text, style);

		rect.x -= halfSize;
		rect.y -= halfSize;
		GUI.Label(rect, text, style);

		rect.y += size;
		GUI.Label(rect, text, style);

		rect.y -= halfSize;
		style.normal.textColor = inColor;
		GUI.color = backupColor;
		GUI.Label(rect, text, style);

		style = backupStyle;
	}
	
}	