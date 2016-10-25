using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (ConstantID), true)]
public class ConstantIDEditor : Editor
{

	public override void OnInspectorGUI()
    {
		SharedGUI ();
	}
	
	
	protected void SharedGUI()
	{
		ConstantID _target = (ConstantID) target;
		
		if (!_target.gameObject.activeInHierarchy)
		{
			_target.constantID = 0;
		}

		EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("ID: " + _target.constantID);
			if (GUILayout.Button ("Copy number"))
			{
				EditorGUIUtility.systemCopyBuffer = _target.constantID.ToString ();
			}
		EditorGUILayout.EndHorizontal ();

		EditorUtility.SetDirty(_target);
	}

}
