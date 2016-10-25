using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberHotspot), true)]
public class RememberHotspotEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberHotspot _target = (RememberHotspot) target;

		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Hotspot state on start:", _target.startState);

		SharedGUI ();
	}

}
