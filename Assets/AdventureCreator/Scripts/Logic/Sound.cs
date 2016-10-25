/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Sound.cs"
 * 
 *	This script allows for easy playback of audio sources from within the ActionList system.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class Sound : MonoBehaviour
{
	public SoundType soundType;
	public bool playWhilePaused = false;
	public float relativeVolume = 1f;

	private float maxVolume = 1f;
	private float fadeStartTime;
	private float fadeEndTime;
	private FadeType fadeType;
	private bool isFading = false;
	
	private Options options;
	
	
	private void Start ()
	{
		if (GetComponent <AudioSource>())
		{
			GetComponent <AudioSource>().ignoreListenerPause = playWhilePaused;
		}
		else
		{
			Debug.LogWarning ("Sound object " + this.name + " has no AudioSource component.");
		}

		SetMaxVolume ();
	}
	
	
	private void Update ()
	{
		if (isFading && GetComponent<AudioSource>().isPlaying)
		{
			float progress = (Time.time - fadeStartTime) / (fadeEndTime - fadeStartTime);
			
			if (fadeType == FadeType.fadeIn)
			{
				if (progress > 1f)
				{
					GetComponent<AudioSource>().volume = maxVolume;
					isFading = false;
				}
				else
				{
					GetComponent<AudioSource>().volume = progress * maxVolume;
				}
			}
			else if (fadeType == FadeType.fadeOut)
			{
				if (progress > 1f)
				{
					GetComponent<AudioSource>().volume = 0f;
					GetComponent<AudioSource>().Stop ();
					isFading = false;
				}
				else
				{
					GetComponent<AudioSource>().volume = (1 - progress) * maxVolume;
				}
			}
		}
	}
	
	
	public void Interact ()
	{
		isFading = false;
		SetMaxVolume ();
		Play (GetComponent<AudioSource>().loop);
	}
	
	
	public void FadeIn (float fadeTime, bool loop)
	{
		GetComponent<AudioSource>().loop = loop;
		
		fadeStartTime = Time.time;
		fadeEndTime = Time.time + fadeTime;
		fadeType = FadeType.fadeIn;
		
		SetMaxVolume ();
		isFading = true;
		GetComponent<AudioSource>().volume = 0f;
		GetComponent<AudioSource>().Play ();
	}
	
	
	public void FadeOut (float fadeTime)
	{
		if (GetComponent<AudioSource>().isPlaying)
		{
			fadeStartTime = Time.time;
			fadeEndTime = Time.time + fadeTime;
			fadeType = FadeType.fadeOut;
			
			SetMaxVolume ();
			isFading = true;
		}
	}
	
	
	public void Play (bool loop)
	{
		GetComponent<AudioSource>().loop = loop;
		isFading = false;
		SetMaxVolume ();
		GetComponent<AudioSource>().Play ();
	}
	
	
	public void SetMaxVolume ()
	{
		maxVolume = relativeVolume;
		
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
		{
			options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
		}

		if (options && soundType != SoundType.Other)
		{
			if (soundType == SoundType.Music)
			{
				maxVolume *= (float) options.optionsData.musicVolume / 10;
			}
			else if (soundType == SoundType.SFX)
			{
				maxVolume *= (float) options.optionsData.sfxVolume / 10;
			}
		}
		
		if (!isFading)
		{
			GetComponent<AudioSource>().volume = maxVolume;
		}
	}
	
	
	public void Stop ()
	{
		GetComponent<AudioSource>().Stop ();
	}
	
}
