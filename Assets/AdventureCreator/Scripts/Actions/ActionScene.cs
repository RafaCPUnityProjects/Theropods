/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionScene : Action
{
	
	public int sceneNumber;
	
	
	public ActionScene ()
	{
		this.isDisplayed = true;
		title = "Engine: Change scene";
	}
	
	
	override public float Run ()
	{
		if (sceneNumber > -1)
		{
			SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
			sceneChanger.ChangeScene (sceneNumber, true);
		}
		
		return 0f;
	}
	

	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (sceneNumber > 0)
		{
			labelAdd = " (" + sceneNumber + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}