using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(DialogueOption))]

[System.Serializable]
public class DialogueOptionEditor : CutsceneEditor
{

	public override void OnInspectorGUI()
    {
		DialogueOption _target = (DialogueOption) target;

		// Draw all GUI elements that buttons and triggers share
		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}

    }

}
