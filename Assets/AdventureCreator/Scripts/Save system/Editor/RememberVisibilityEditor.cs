using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberVisibility), true)]
public class RememberVisibilityEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberVisibility _target = (RememberVisibility) target;
		
		_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Visibility on start:", _target.startState);
		
		SharedGUI ();
	}
	
}
