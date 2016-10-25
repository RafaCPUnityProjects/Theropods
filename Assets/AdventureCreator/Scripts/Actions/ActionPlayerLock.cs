/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionPlayerLock : Action
{
	
	public enum LockType {Enabled, Disabled, NoChange};
	
	public LockType doUpLock = LockType.Enabled;
	public LockType doDownLock = LockType.Enabled;
	public LockType doLeftLock = LockType.Enabled;
	public LockType doRightLock = LockType.Enabled;
	
	public PlayerMoveLock doRunLock = PlayerMoveLock.Free;
	public LockType doInventoryLock = LockType.Enabled;
	public LockType doSaveLock = LockType.Enabled;
	public Paths movePath;

	
	public ActionPlayerLock ()
	{
		this.isDisplayed = true;
		title = "Player: Constrain";
	}
	
	
	override public float Run ()
	{
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		PlayerMenus playerMenus = playerInput.GetComponent <PlayerMenus>();
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
		Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		
		if (playerInput)
		{
			if (doUpLock == LockType.Disabled)
			{
				playerInput.isUpLocked = true;
			}
			else if (doUpLock == LockType.Enabled)
			{
				playerInput.isUpLocked = false;
			}
	
			if (doDownLock == LockType.Disabled)
			{
				playerInput.isDownLocked = true;
			}
			else if (doDownLock == LockType.Enabled)
			{
				playerInput.isDownLocked = false;
			}
			
			if (doLeftLock == LockType.Disabled)
			{
				playerInput.isLeftLocked = true;
			}
			else if (doLeftLock == LockType.Enabled)
			{
				playerInput.isLeftLocked = false;
			}
	
			if (doRightLock == LockType.Disabled)
			{
				playerInput.isRightLocked = true;
			}
			else if (doRightLock == LockType.Enabled)
			{
				playerInput.isRightLocked = false;
			}
			
			if (doRunLock != PlayerMoveLock.NoChange)
			{
				playerInput.runLock = doRunLock;
			}
		}
		
		if (runtimeInventory)
		{
			if (doInventoryLock == LockType.Disabled)
			{
				runtimeInventory.isLocked = true;
			}
			else if (doInventoryLock == LockType.Enabled && runtimeInventory.localItems.Count > 0)
			{
				runtimeInventory.isLocked = false;		
			}
		}
		
		if (playerMenus)
		{
			if (doSaveLock == LockType.Disabled)
			{
				playerMenus.lockSave = true;
			}
			else if (doSaveLock == LockType.Enabled)
			{
				playerMenus.lockSave = false;
			}
		}
		
		if (player)
		{
			if (movePath)
			{
				player.SetLockedPath (movePath);
				player.SetMoveDirectionAsForward ();
			}
			else if (player.activePath)
			{
				player.SetPath (null);
			}
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);

		doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);

		doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);

		doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
		
		doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);

		doInventoryLock = (LockType) EditorGUILayout.EnumPopup ("Inventory:", doInventoryLock);
		
		doSaveLock = (LockType) EditorGUILayout.EnumPopup ("Save:", doSaveLock);
		
		movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);
		
		AfterRunningOption ();
	}
	
	#endif

}