/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ButtonDialog.cs"
 * 
 *	This script is a container class for dialogue options
 *	that are linked to Conversations.
 * 
 */

using UnityEngine;

[System.Serializable]
public class ButtonDialog
{
	
	public string label = "(Not set)";
	public Texture2D icon;
	public bool isOn;
	public bool isLocked;
	public bool returnToConversation;
	public int lineID = -1;
	public bool isEditing = false;
	
	public DialogueOption dialogueOption;
	
	public ButtonDialog ()
	{
		label = "";
		icon = null;
		isOn = true;
		isLocked = false;
		returnToConversation = false;
		dialogueOption = null;
		lineID = -1;
		isEditing = false;
	}

}
