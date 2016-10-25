/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SaveSystem.cs"
 * 
 *	This script processes saved game data to and from the scene objects.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AC;

public class SaveSystem : MonoBehaviour
{
	
	public enum AC_SaveMethod { Binary, XML };
	
	public bool isLoadingNewScene { get; set; }

	#if !UNITY_WEBPLAYER
	private string saveDirectory = Application.persistentDataPath;
	private string saveExtention = ".save";
	#endif
	
	private SaveData saveData;
	private LevelStorage levelStorage;

	
	private void Awake ()
	{
		levelStorage = this.GetComponent <LevelStorage>();
	}
	

	public AC_SaveMethod GetSaveMethod ()
	{
		#if UNITY_IPHONE
		
		return AC_SaveMethod.XML;
		
		#endif
		
		return AC_SaveMethod.Binary;
	}


	public static void LoadAutoSave ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
			
			if (File.Exists (saveSystem.GetSaveFileName (0)))
			{
				saveSystem.LoadSaveGame (0);
			}
			else
			{
				Debug.LogWarning ("Could not load game: file " + saveSystem.GetSaveFileName (0) + " does not exist.");
			}
		}
	}
	
	
	public static void LoadGame (int slot)
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().LoadSaveGame (slot);
		}
	}
	
	
	public void LoadSaveGame (int slot)
	{
		if (!HasAutoSave ())
		{
			slot ++;
		}

		string allData = "";

		#if UNITY_WEBPLAYER

		allData = Serializer.LoadSaveFile (GetSaveFileName (slot));

		#else

		if (File.Exists (GetSaveFileName (slot)))
		{
			allData = Serializer.LoadSaveFile (GetSaveFileName (slot));
		}
		else
		{
			Debug.LogWarning ("Could not load game: file " + GetSaveFileName (slot) + " does not exist.");
		}

		#endif

		if (allData.ToString () != "")
		{
			string mainData;
			string roomData;
			
			int divider = allData.IndexOf ("||");
			mainData = allData.Substring (0, divider);
			roomData = allData.Substring (divider + 2);
			
			if (GetSaveMethod () == AC_SaveMethod.XML)
			{
				saveData = (SaveData) Serializer.DeserializeObjectXML <SaveData> (mainData);
				levelStorage.allLevelData = (List<SingleLevelData>) Serializer.DeserializeObjectXML <List<SingleLevelData>> (roomData);
			}
			else
			{
				saveData = Serializer.DeserializeObjectBinary <SaveData> (mainData);
				levelStorage.allLevelData = Serializer.DeserializeRoom (roomData);
			}
			
			// Stop any current-running ActionLists, dialogs and interactions
			KillActionLists ();
			
			// Load correct scene
			if (saveData.mainData.currentScene != Application.loadedLevel)
			{
				isLoadingNewScene = true;
				
				if (this.GetComponent <SceneChanger>())
				{
					SceneChanger sceneChanger = this.GetComponent <SceneChanger> ();
					sceneChanger.ChangeScene (saveData.mainData.currentScene, false);
				}
			}
			else
			{
				OnLevelWasLoaded ();
			}
		}
	}
	
	
	private void OnLevelWasLoaded ()
	{
		if (saveData != null)
		{
			if (GameObject.FindWithTag (Tags.gameEngine))
			{
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>())
				{
					Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
					dialog.KillDialog ();
				}
				
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>())
				{
					PlayerInteraction playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
					playerInteraction.StopInteraction ();
				}
			}
				
			ReturnMainData ();
			levelStorage.ReturnCurrentLevelData ();
		}
		
		saveData = null;
	}
	
	
	public static void SaveNewGame ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().SaveNewSaveGame ();
		}
	}
	
	
	public void SaveNewSaveGame ()
	{
		int slot = GetNumSlots ();
		SaveGame (slot);
	}
	
	
	public static void SaveGame (int slot)
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().SaveSaveGame (slot);
		}
	}
	
	
	public void SaveSaveGame (int slot)
	{
		if (!HasAutoSave () || slot == -1)
		{
			slot ++;
		}
		
		levelStorage.StoreCurrentLevelData ();
		
		saveData = new SaveData ();
		
		Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
		MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		RuntimeInventory runtimeInventory = this.GetComponent <RuntimeInventory>();
		RuntimeVariables runtimeVariables = runtimeInventory.GetComponent <RuntimeVariables>();
		SceneChanger sceneChanger = this.GetComponent <SceneChanger>();
		
		if (player && playerInput && playerMenus && mainCamera && runtimeInventory && runtimeVariables && sceneChanger)
		{
			// Assign "Main Data"
			saveData.mainData.currentScene = Application.loadedLevel;
			saveData.mainData.previousScene = sceneChanger.previousScene;
			
			saveData.mainData.playerLocX = player.transform.position.x;
			saveData.mainData.playerLocY = player.transform.position.y;
			saveData.mainData.playerLocZ = player.transform.position.z;
			saveData.mainData.playerRotY = player.transform.eulerAngles.y;

			saveData.mainData.playerWalkSpeed = player.walkSpeedScale;
			saveData.mainData.playerRunSpeed = player.runSpeedScale;

			// Animation clips
			if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
			{
				saveData.mainData.playerIdleAnim = player.idleAnimSprite;
				saveData.mainData.playerWalkAnim = player.walkAnimSprite;
				saveData.mainData.playerRunAnim = player.runAnimSprite;
				saveData.mainData.playerTalkAnim = player.talkAnimSprite;
			}
			else if (player.animationEngine == AnimationEngine.Legacy)
			{
				saveData.mainData.playerIdleAnim = player.GetStandardAnimClipName (AnimStandard.Idle);
				saveData.mainData.playerWalkAnim = player.GetStandardAnimClipName (AnimStandard.Walk);
				saveData.mainData.playerRunAnim = player.GetStandardAnimClipName (AnimStandard.Run);
				saveData.mainData.playerTalkAnim = player.GetStandardAnimClipName (AnimStandard.Talk);
			}
			else if (player.animationEngine == AnimationEngine.Mecanim)
			{
				saveData.mainData.playerWalkAnim = player.moveSpeedParameter;
				saveData.mainData.playerTalkAnim = player.talkParameter;
				saveData.mainData.playerRunAnim = player.turnParameter;
			}

			if (player.GetPath ())
			{
				saveData.mainData.playerTargetNode = player.GetTargetNode ();
				saveData.mainData.playerPrevNode = player.GetPrevNode ();
				saveData.mainData.playerIsRunning = player.isRunning;

				if (player.GetComponent <Paths>() && player.GetPath () == player.GetComponent <Paths>())
				{
					saveData.mainData.playerPathData = Serializer.CreatePathData (player.GetComponent <Paths>());
					saveData.mainData.playerActivePath = 0;
					saveData.mainData.playerLockedPath = false;
				}
				else
				{
					saveData.mainData.playerPathData = "";
					saveData.mainData.playerActivePath = Serializer.GetConstantID (player.GetPath ().gameObject);
					saveData.mainData.playerLockedPath = player.lockedPath;
				}
			}

			if (playerInput.activeArrows)
			{
				saveData.mainData.playerActiveArrows = Serializer.GetConstantID (playerInput.activeArrows.gameObject);
			}
			
			if (playerInput.activeConversation)
			{
				saveData.mainData.playerActiveConversation = Serializer.GetConstantID (playerInput.activeConversation.gameObject);
			}
			
			saveData.mainData.playerUpLock = playerInput.isUpLocked;
			saveData.mainData.playerDownLock = playerInput.isDownLocked;
			saveData.mainData.playerLeftlock = playerInput.isLeftLocked;
			saveData.mainData.playerRightLock = playerInput.isRightLocked;
			saveData.mainData.playerRunLock = (int) playerInput.runLock;
			saveData.mainData.playerInventoryLock = runtimeInventory.isLocked;
			
			saveData.mainData.timeScale = playerInput.timeScale;
			
			if (mainCamera.attachedCamera)
			{
				saveData.mainData.gameCamera = Serializer.GetConstantID (mainCamera.attachedCamera.gameObject);
			}
			
			mainCamera.StopShaking ();
			saveData.mainData.mainCameraLocX = mainCamera.transform.position.x;
			saveData.mainData.mainCameraLocY = mainCamera.transform.position.y;
			saveData.mainData.mainCameraLocZ = mainCamera.transform.position.z;
			
			saveData.mainData.mainCameraRotX = mainCamera.transform.eulerAngles.x;
			saveData.mainData.mainCameraRotY = mainCamera.transform.eulerAngles.y;
			saveData.mainData.mainCameraRotZ = mainCamera.transform.eulerAngles.z;
			
			saveData.mainData.inventoryData = CreateInventoryData (runtimeInventory);
			if (runtimeInventory.selectedItem != null)
			{
				saveData.mainData.selectedInventoryID = runtimeInventory.selectedItem.id;
			}
			else
			{
				saveData.mainData.selectedInventoryID = -1;
			}
			saveData.mainData.variablesData = CreateVariablesData (runtimeVariables);

			saveData.mainData.menuLockData = CreateMenuLockData (playerMenus.GetMenus ());
			saveData.mainData.menuElementVisibilityData = CreateMenuElementVisibilityData (playerMenus.GetMenus ());
			saveData.mainData.menuJournalData = CreateMenuJournalData (playerMenus.GetMenus ());
			
			string mainData = "";
			string levelData = "";
			
			if (GetSaveMethod () == AC_SaveMethod.XML)
			{
				mainData = Serializer.SerializeObjectXML <SaveData> (saveData);
				levelData = Serializer.SerializeObjectXML <List<SingleLevelData>> (levelStorage.allLevelData);
			}
			else
			{
				mainData = Serializer.SerializeObjectBinary (saveData);
				levelData = Serializer.SerializeObjectBinary (levelStorage.allLevelData);
			}
			string allData = mainData + "||" + levelData;
	
			Serializer.CreateSaveFile (GetSaveFileName (slot), allData);

		}
		else
		{
			if (player == null)
			{
				Debug.LogWarning ("Save failed - no Player found.");
			}
			if (playerInput == null)
			{
				Debug.LogWarning ("Save failed - no PlayerInput found.");
			}
			if (playerMenus == null)
			{
				Debug.LogWarning ("Save failed - no PlayerMenus found.");
			}
			if (mainCamera == null)
			{
				Debug.LogWarning ("Save failed - no MainCamera found.");
			}
			if (runtimeInventory == null)
			{
				Debug.LogWarning ("Save failed - no RuntimeInventory found.");
			}
			if (runtimeVariables == null)
			{
				Debug.LogWarning ("Save failed - no RuntimeVariables found.");
			}
			if (sceneChanger == null)
			{
				Debug.LogWarning ("Save failed - no SceneChanger found.");
			}
		}

		saveData = null;
	}
	
	
	public static int GetNumSlots ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();

			#if UNITY_WEBPLAYER

			int count = 0;

			for (int i=0; i<50; i++)
			{
				if (PlayerPrefs.HasKey (saveSystem.GetProjectName () + "_" + i.ToString ()))
				{
					count ++;
				}
			}

			return count;

			#else
			
				DirectoryInfo dir = new DirectoryInfo (saveSystem.saveDirectory);
				FileInfo[] info = dir.GetFiles (saveSystem.GetProjectName() + "_*" + saveSystem.saveExtention);
			
				return info.Length;
		
			#endif
		}
		
		return 0;		
	}
	
	
	public bool HasAutoSave ()
	{
		#if UNITY_WEBPLAYER

		return (PlayerPrefs.HasKey (GetProjectName () + "_0"));

		#else

		if (File.Exists (saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_0" + saveExtention))
		{
			return true;
		}
		
		return false;
		
		#endif
	}
	
	
	private string GetProjectName ()
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		if (settingsManager)
		{
			if (settingsManager.saveFileName == "")
			{
				settingsManager.saveFileName = SetProjectName ();
			}
			
			if (settingsManager.saveFileName != "")
			{
				return settingsManager.saveFileName;
			}
		}
		
		return SetProjectName ();
	}
	
	
	
	public static string SetProjectName ()
	{
		string[] s = Application.dataPath.Split ('/');
		string projectName = s[s.Length - 2];
		return projectName;
	}
	
	
	private string GetSaveFileName (int slot)
	{
		string fileName = "";

		#if UNITY_WEBPLAYER

		fileName = GetProjectName () + "_" + slot.ToString ();

		#else

		fileName = saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_" + slot.ToString () + saveExtention;

		#endif

		return (fileName);
	}
	
	
	private void KillActionLists ()
	{
		ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
		actionListManager.KillAllLists ();

		Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
		foreach (Moveable moveable in moveables)
		{
			moveable.Kill ();
		}
	}

	
	public static string GetSaveSlotName (int slot)
	{
		string fileName = "Save test (01/01/2001 12:00:00)";

		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();

			#if UNITY_WEBPLAYER

			if (!saveSystem.HasAutoSave ())
			{
				fileName = "Save " + (slot + 1).ToString ();
			}
			else
			{
				fileName = "Save " + slot.ToString ();
			}
			
			if (slot == 0 && saveSystem.HasAutoSave())
			{
				fileName = "Autosave";
			}

			#else
		
			DirectoryInfo dir = new DirectoryInfo (saveSystem.saveDirectory);
			FileInfo[] info = dir.GetFiles (saveSystem.GetProjectName() + "_*" + saveSystem.saveExtention);
			
			if (!saveSystem.HasAutoSave ())
			{
				fileName = "Save " + (slot + 1).ToString ();
			}
			else
			{
				fileName = "Save " + slot.ToString ();
			}
			
			if (slot == 0 && saveSystem.HasAutoSave())
			{
				fileName = "Autosave";
			}
			
			if (slot < info.Length)
			{
				string creationTime = info[slot].LastWriteTime.ToString ();
				creationTime = creationTime.Substring (0, creationTime.IndexOf (" "));
				fileName += " (" + creationTime + ")";
			}

			#endif
		}

		return fileName;
	}
	
	
	private void ReturnMainData ()
	{
		Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
		MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		RuntimeInventory runtimeInventory = this.GetComponent <RuntimeInventory>();
		RuntimeVariables runtimeVariables = runtimeInventory.GetComponent <RuntimeVariables>();
		SceneChanger sceneChanger = this.GetComponent <SceneChanger>();
		
		if (player && playerInput && playerMenus && mainCamera && runtimeInventory && runtimeVariables)
		{
			sceneChanger.previousScene = saveData.mainData.previousScene;
			
			player.transform.position = new Vector3 (saveData.mainData.playerLocX, saveData.mainData.playerLocY, saveData.mainData.playerLocZ);
			player.transform.eulerAngles = new Vector3 (0f, saveData.mainData.playerRotY, 0f);
			player.SetLookDirection (Vector3.zero, true);

			player.walkSpeedScale = saveData.mainData.playerWalkSpeed;
			player.runSpeedScale = saveData.mainData.playerRunSpeed;
			
			// Animation clips
			if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
			{
				player.idleAnimSprite = saveData.mainData.playerIdleAnim;
				player.walkAnimSprite = saveData.mainData.playerWalkAnim;
				player.talkAnimSprite = saveData.mainData.playerTalkAnim;
				player.runAnimSprite = saveData.mainData.playerRunAnim;
			}
			else if (player.animationEngine == AnimationEngine.Legacy)
			{
				player.AssignStandardAnimClipFromResource (AnimStandard.Idle, saveData.mainData.playerIdleAnim);
				player.AssignStandardAnimClipFromResource (AnimStandard.Walk, saveData.mainData.playerWalkAnim);
				player.AssignStandardAnimClipFromResource (AnimStandard.Talk, saveData.mainData.playerTalkAnim);
				player.AssignStandardAnimClipFromResource (AnimStandard.Run, saveData.mainData.playerRunAnim);
			}
			else if (player.animationEngine == AnimationEngine.Mecanim)
			{
				player.moveSpeedParameter = saveData.mainData.playerWalkAnim;
				player.talkParameter = saveData.mainData.playerTalkAnim;
				player.turnParameter = saveData.mainData.playerRunAnim;
			}

			// Active path
			player.Halt ();
			if (saveData.mainData.playerPathData != null && saveData.mainData.playerPathData != "" && player.GetComponent <Paths>())
			{
				Paths savedPath = player.GetComponent <Paths>();
				savedPath = Serializer.RestorePathData (savedPath, saveData.mainData.playerPathData);
				player.SetPath (savedPath, saveData.mainData.playerTargetNode, saveData.mainData.playerPrevNode);
				player.isRunning = saveData.mainData.playerIsRunning;
				player.lockedPath = false;
			}
			else if (saveData.mainData.playerActivePath != 0)
			{
				Paths savedPath = Serializer.returnComponent <Paths> (saveData.mainData.playerActivePath);
				if (savedPath)
				{
					player.lockedPath = saveData.mainData.playerLockedPath;

					if (player.lockedPath)
					{
						player.SetLockedPath (savedPath);
					}
					else
					{
						player.SetPath (savedPath, saveData.mainData.playerTargetNode, saveData.mainData.playerPrevNode);
					}
				}
			}

			// Active screen arrows
			playerInput.RemoveActiveArrows ();
			ArrowPrompt loadedArrows = Serializer.returnComponent <ArrowPrompt> (saveData.mainData.playerActiveArrows);
			if (loadedArrows)
			{
				loadedArrows.TurnOn ();
			}
			
			// Active conversation
			playerInput.activeConversation = Serializer.returnComponent <Conversation> (saveData.mainData.playerActiveConversation);
			
			playerInput.isUpLocked = saveData.mainData.playerUpLock;
			playerInput.isDownLocked = saveData.mainData.playerDownLock;
			playerInput.isLeftLocked = saveData.mainData.playerLeftlock;
			playerInput.isRightLocked = saveData.mainData.playerRightLock;
			playerInput.runLock = (PlayerMoveLock) saveData.mainData.playerRunLock;
			runtimeInventory.isLocked = saveData.mainData.playerInventoryLock;
			
			playerInput.timeScale = saveData.mainData.timeScale;
			
			mainCamera.StopShaking ();
			mainCamera.SetGameCamera (Serializer.returnComponent <_Camera> (saveData.mainData.gameCamera));
			mainCamera.ResetMoving ();
			mainCamera.transform.position = new Vector3 (saveData.mainData.mainCameraLocX, saveData.mainData.mainCameraLocY, saveData.mainData.mainCameraLocZ);
			mainCamera.transform.eulerAngles = new Vector3 (saveData.mainData.mainCameraRotX, saveData.mainData.mainCameraRotY, saveData.mainData.mainCameraRotZ);
			mainCamera.SnapToAttached ();
			mainCamera.ResetProjection ();

			if (mainCamera.attachedCamera)
			{
				mainCamera.attachedCamera.MoveCameraInstant ();
			}
			else
			{
				Debug.LogWarning ("MainCamera has no attached GameCamera");
			}
			
			// Inventory
			AssignInventory (runtimeInventory, saveData.mainData.inventoryData);
			if (saveData.mainData.selectedInventoryID > -1)
			{
				runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID);
			}
			else
			{
				runtimeInventory.SetNull ();
			}
			foreach (Menu menu in playerMenus.GetMenus ())
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuInventoryBox)
					{
						MenuInventoryBox invBox = (MenuInventoryBox) element;
						invBox.ResetOffset ();
					}
				}
			}
			
			// Variables
			AssignVariables (runtimeVariables, saveData.mainData.variablesData);

			// Menus
			AssignMenuLocks (playerMenus.GetMenus (), saveData.mainData.menuLockData);
			AssignMenuElementVisibility (playerMenus.GetMenus (), saveData.mainData.menuElementVisibilityData);
			AssignMenuJournals (playerMenus.GetMenus (), saveData.mainData.menuJournalData);

			// StateHandler
			StateHandler stateHandler = runtimeInventory.GetComponent <StateHandler>();
			stateHandler.gameState = GameState.Cutscene;

			// Fade in camera
			mainCamera.FadeIn (0.5f);

			Invoke ("ReturnToGameplay", 0.01f);
		}
		else
		{
			if (player == null)
			{
				Debug.LogWarning ("Load failed - no Player found.");
			}
			if (playerInput == null)
			{
				Debug.LogWarning ("Load failed - no PlayerInput found.");
			}
			if (playerInput == null)
			{
				Debug.LogWarning ("Load failed - no PlayerMenus found.");
			}
			if (mainCamera == null)
			{
				Debug.LogWarning ("Load failed - no MainCamera found.");
			}
			if (runtimeInventory == null)
			{
				Debug.LogWarning ("Load failed - no RuntimeInventory found.");
			}
			if (runtimeVariables == null)
			{
				Debug.LogWarning ("Load failed - no RuntimeVariables found.");
			}
			if (sceneChanger == null)
			{
				Debug.LogWarning ("Load failed - no SceneChanger found.");
			}
		}
	}


	private void ReturnToGameplay ()
	{
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

		if (playerInput.activeConversation)
		{
			stateHandler.gameState = GameState.DialogOptions;
		}
		else
		{
			stateHandler.gameState = GameState.Normal;
		}

		playerInput.ResetClick ();

		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			sceneSettings.OnLoad ();
		}
	}
	
	
	private void AssignVariables (RuntimeVariables runtimeVariables, string variablesData)
	{
		if (runtimeVariables)
		{
			if (variablesData.Length > 0)
			{
				string[] varsArray = variablesData.Split ("|"[0]);
				
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
		
					if (runtimeVariables.GetVarType (_id) != VariableType.String)
					{
						string _text = chunkData[1];
						runtimeVariables.SetValue (_id, _text);
					}
					else
					{
						int _value = 0;
						int.TryParse (chunkData[1], out _value);
						runtimeVariables.SetValue (_id, _value, SetVarMethod.SetValue);
					}
				}
			}
		}
	}

	
	private void AssignInventory (RuntimeInventory runtimeInventory, string inventoryData)
	{
		if (runtimeInventory)
		{
			runtimeInventory.localItems.Clear ();
			
			if (inventoryData.Length > 0)
			{
				string[] countArray = inventoryData.Split ("|"[0]);
				
				foreach (string chunk in countArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
		
					int _count = 0;
					int.TryParse (chunkData[1], out _count);
					
					runtimeInventory.Add (_id, _count);
				}
			}
		}
	}


	private void AssignMenuLocks (List<Menu> menus, string menuLockData)
	{
		if (menuLockData.Length > 0)
		{
			string[] lockArray = menuLockData.Split ("|"[0]);

			foreach (string chunk in lockArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);
				
				bool _lock = false;
				bool.TryParse (chunkData[1], out _lock);
				
				foreach (Menu _menu in menus)
				{
					if (_menu.id == _id)
					{
						_menu.isLocked = _lock;
						break;
					}
				}
			}
		}
	}


	private void AssignMenuElementVisibility (List<Menu> menus, string menuElementVisibilityData)
	{
		if (menuElementVisibilityData.Length > 0)
		{
			string[] visArray = menuElementVisibilityData.Split ("|"[0]);
			
			foreach (string chunk in visArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _menuID = 0;
				int.TryParse (chunkData[0], out _menuID);

				foreach (Menu _menu in menus)
				{
					if (_menu.id == _menuID)
					{
						// Found a match
						string[] perMenuData = chunkData[1].Split ("+"[0]);
						
						foreach (string perElementData in perMenuData)
						{
							string [] chunkData2 = perElementData.Split ("="[0]);
							
							int _elementID = 0;
							int.TryParse (chunkData2[0], out _elementID);
							
							bool _elementVisibility = false;
							bool.TryParse (chunkData2[1], out _elementVisibility);
							
							foreach (MenuElement _element in _menu.elements)
							{
								if (_element.ID == _elementID && _element.isVisible != _elementVisibility)
								{
									_element.isVisible = _elementVisibility;
									break;
								}
							}
						}

						_menu.ResetVisibleElements ();
						_menu.Recalculate ();
						break;
					}
				}
			}
		}
	}


	private void AssignMenuJournals (List<Menu> menus, string menuJournalData)
	{
		if (menuJournalData.Length > 0)
		{
			string[] journalArray = menuJournalData.Split ("|"[0]);
			
			foreach (string chunk in journalArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int menuID = 0;
				int.TryParse (chunkData[0], out menuID);
				
				int elementID = 0;
				int.TryParse (chunkData[1], out elementID);

				foreach (Menu _menu in menus)
				{
					if (_menu.id == menuID)
					{
						foreach (MenuElement _element in _menu.elements)
						{
							if (_element.ID == elementID && _element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) _element;
								journal.pages = new List<JournalPage>();
								journal.showPage = 1;

								string[] pageArray = chunkData[2].Split ("~"[0]);

								foreach (string chunkData2 in pageArray)
								{
									string[] chunkData3 = chunkData2.Split ("*"[0]);

									int lineID = -1;
									int.TryParse (chunkData3[0], out lineID);

									journal.pages.Add (new JournalPage (lineID, chunkData3[1]));
								}

								break;
							}
						}
					}
				}
			}
		}
	}


	private string CreateInventoryData (RuntimeInventory runtimeInventory)
	{
		System.Text.StringBuilder inventoryString = new System.Text.StringBuilder ();
		
		foreach (InvItem item in runtimeInventory.localItems)
		{
			inventoryString.Append (item.id.ToString ());
			inventoryString.Append (":");
			inventoryString.Append (item.count.ToString ());
			inventoryString.Append ("|");
		}
		
		if (runtimeInventory && runtimeInventory.localItems.Count > 0)
		{
			inventoryString.Remove (inventoryString.Length-1, 1);
		}
		
		return inventoryString.ToString ();		
	}
	
		
	private string CreateVariablesData (RuntimeVariables runtimeVariables)
	{
		System.Text.StringBuilder variablesString = new System.Text.StringBuilder ();
		
		foreach (GVar _var in runtimeVariables.localVars)
		{
			variablesString.Append (_var.id.ToString ());
			variablesString.Append (":");
			if (_var.type == VariableType.String)
			{
				string textVal = _var.textVal;
				if (textVal.Contains ("|"))
				{
					textVal = textVal.Replace ("|", "");
					Debug.LogWarning ("Removed pipe delimeter from variable " + _var.label);
				}
				variablesString.Append (textVal);
			}
			else
			{
				variablesString.Append (_var.val.ToString ());
			}
			variablesString.Append ("|");
		}
		
		if (runtimeVariables && runtimeVariables.localVars.Count > 0)
		{
			variablesString.Remove (variablesString.Length-1, 1);
		}
		
		return variablesString.ToString ();		
	}


	private string CreateMenuLockData (List<Menu> menus)
	{
		System.Text.StringBuilder menuString = new System.Text.StringBuilder ();

		foreach (Menu _menu in menus)
		{
			menuString.Append (_menu.id.ToString ());
			menuString.Append (":");
			menuString.Append (_menu.isLocked.ToString ());
			menuString.Append ("|");
		}

		if (menus.Count > 0)
		{
			menuString.Remove (menuString.Length-1, 1);
		}

		return menuString.ToString ();
	}


	private string CreateMenuElementVisibilityData (List<Menu> menus)
	{
		System.Text.StringBuilder visibilityString = new System.Text.StringBuilder ();
		
		foreach (Menu _menu in menus)
		{
			visibilityString.Append (_menu.id.ToString ());
			visibilityString.Append (":");

			foreach (MenuElement _element in _menu.elements)
			{
				visibilityString.Append (_element.ID.ToString ());
				visibilityString.Append ("=");
				visibilityString.Append (_element.isVisible.ToString ());
				visibilityString.Append ("+");
			}

			if (_menu.elements.Count > 0)
			{
				visibilityString.Remove (visibilityString.Length-1, 1);
			}

			visibilityString.Append ("|");
		}
		
		if (menus.Count > 0)
		{
			visibilityString.Remove (visibilityString.Length-1, 1);
		}

		return visibilityString.ToString ();
	}


	private string CreateMenuJournalData (List<Menu> menus)
	{
		System.Text.StringBuilder journalString = new System.Text.StringBuilder ();

		foreach (Menu _menu in menus)
		{
			foreach (MenuElement _element in _menu.elements)
			{
				if (_element is MenuJournal)
				{
					MenuJournal journal = (MenuJournal) _element;
					journalString.Append (_menu.id.ToString ());
					journalString.Append (":");
					journalString.Append (journal.ID);
					journalString.Append (":");

					foreach (JournalPage page in journal.pages)
					{
						journalString.Append (page.lineID);
						journalString.Append ("*");
						journalString.Append (page.text);
						journalString.Append ("~");
					}

					if (journal.pages.Count > 0)
					{
						journalString.Remove (journalString.Length-1, 1);
					}

					journalString.Append ("|");
				}

			}
		}

		if (journalString.ToString () != "")
		{
			journalString.Remove (journalString.Length-1, 1);
		}

		return journalString.ToString ();
	}

}
