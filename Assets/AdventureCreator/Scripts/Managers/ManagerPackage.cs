/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ManagerPackage.cs"
 * 
 *	This script is used to store references to Manager assets,
 *	so that they can be quickly loaded into the game engine in bulk.
 * 
 */

using UnityEngine;
using System;

[System.Serializable]
public class ManagerPackage : ScriptableObject
{

	public ActionsManager actionsManager;
	public SceneManager sceneManager;
	public SettingsManager settingsManager;
	public InventoryManager inventoryManager;
	public VariablesManager variablesManager;
	public SpeechManager speechManager;
	public CursorManager cursorManager;
	public MenuManager menuManager;

}