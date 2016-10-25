using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberTransform), true)]
public class RememberTransformEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberTransform _target = (RememberTransform) target;
		
		_target.saveParent = EditorGUILayout.Toggle ("Save change in Parent?", _target.saveParent);
		
		SharedGUI ();
	}
	
}
