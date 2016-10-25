using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (FollowSortingMap))]

public class FollowSortingMapEditor : Editor
{
	
	public override void OnInspectorGUI ()
	{
		FollowSortingMap _target = (FollowSortingMap) target;

		_target.followSortingMap = EditorGUILayout.Toggle ("Follow Sorting map?", _target.followSortingMap);

		if (_target.followSortingMap)
		{
			_target.offsetOriginal = EditorGUILayout.Toggle ("Offset original Order?", _target.offsetOriginal);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
