using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberNPC), true)]
public class RememberNPCEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberNPC _target = (RememberNPC) target;
		
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Hotspot state on start:", _target.startState);
		
		SharedGUI ();
	}
	
}
