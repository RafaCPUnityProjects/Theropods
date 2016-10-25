/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSound.cs"
 * 
 *	This action triggers the sound component of any GameObject, overriding that object's play settings.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionSound : Action
{
	
	public Sound soundObject;
	public AudioClip audioClip;
	public enum SoundAction { Play, FadeIn, FadeOut, Stop }
	public float fadeTime;
	public bool loop;
	public SoundAction soundAction;
	
	
	public ActionSound ()
	{
		this.isDisplayed = true;
		title = "Engine: Play sound";
	}
	
	
	override public float Run ()
	{
		if (soundObject)
		{
			if (audioClip && soundObject.GetComponent <AudioSource>())
			{
				if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
				{
					soundObject.GetComponent <AudioSource>().clip = audioClip;
				}
			}
			
			if (soundAction == SoundAction.Play)
			{
				soundObject.Play (loop);
			}
			else if (soundAction == SoundAction.FadeIn)
			{
				if (fadeTime == 0f)
				{
					soundObject.Play (loop);
				}
				else
				{
					soundObject.FadeIn (fadeTime, loop);
				}
			}
			else if (soundAction == SoundAction.FadeOut)
			{
				if (fadeTime == 0f)
				{
					soundObject.Stop ();
				}
				else
				{
					soundObject.FadeOut (fadeTime);
				}
			}
			else if (soundAction == SoundAction.Stop)
			{
				soundObject.Stop ();
			}
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		soundObject = (Sound) EditorGUILayout.ObjectField ("Sound object:", soundObject, typeof(Sound), true);
		soundAction = (SoundAction) EditorGUILayout.EnumPopup ("Sound action:", (SoundAction) soundAction);
		
		if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
		{
			loop = EditorGUILayout.Toggle ("Loop?", loop);
			audioClip = (AudioClip) EditorGUILayout.ObjectField ("New clip (optional)", audioClip, typeof (AudioClip), false);
		}
		
		if (soundAction == SoundAction.FadeIn || soundAction == SoundAction.FadeOut)
		{
			fadeTime = EditorGUILayout.Slider ("Fade time:", fadeTime, 0f, 10f);
		}
		
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		if (soundObject)
		{
			labelAdd = " (" + soundAction.ToString ();
			labelAdd += " " + soundObject.name + ")";
		}
		
		return labelAdd;
	}

	#endif

}