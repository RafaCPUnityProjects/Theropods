using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor (typeof (Sound))]
public class SoundEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		Sound _target = (Sound) target;
		
		_target.soundType = (SoundType) EditorGUILayout.EnumPopup ("Sound type:", _target.soundType);
		_target.playWhilePaused = EditorGUILayout.Toggle ("Play while game paused?", _target.playWhilePaused);
		_target.relativeVolume = EditorGUILayout.Slider ("Relative volume:", _target.relativeVolume, 0f, 1f);
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
}
