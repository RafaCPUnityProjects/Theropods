/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Scene" tab of the main wizard.
 *	It is used to create the prefabs needed to run the game,
 *	as well as provide easy-access to game logic.
 * 
 */

using UnityEngine;
using System.IO;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneManager : ScriptableObject
{
	
	#if UNITY_EDITOR
	
	private DirectoryInfo dir;
	private FileInfo[] info;
	
	private string assetFolder = "Assets/AdventureCreator/Prefabs/";
		
	private string newFolderName = "";
	private string prefabName;
	private int index_name;
	private int index_dot;
	
	private GameObject gameEngine;
	
	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(120f);

	
	public void ShowGUI ()
	{
		GUILayout.Label ("Basic structure", EditorStyles.boldLabel);

		if (GUILayout.Button ("Organise room objects"))
		{
			InitialiseObjects ();
		}
		
		gameEngine = GameObject.FindWithTag (Tags.gameEngine);
		if (gameEngine && AdvGame.GetReferences ().settingsManager)
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

			GUILayout.BeginHorizontal ();
				newFolderName = GUILayout.TextField (newFolderName);
				
				if (GUILayout.Button ("Create new folder", buttonWidth))
				{
					if (newFolderName != "")
					{
						GameObject newFolder = new GameObject();
						
						if (!newFolderName.StartsWith ("_"))
							newFolder.name = "_" + newFolderName;
						else
							newFolder.name = newFolderName;
							
						Undo.RegisterCreatedObjectUndo (newFolder, "Create folder " + newFolder.name);
						
						if (Selection.activeGameObject)
						{
							newFolder.transform.parent = Selection.activeGameObject.transform;
						}
						
						Selection.activeObject = newFolder;
					}
				}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			
			if (gameEngine.GetComponent <SceneSettings>())
			{
				GUILayout.Label ("Scene settings", EditorStyles.boldLabel);
				gameEngine.GetComponent <SceneSettings>().navigationMethod = (AC_NavigationMethod) EditorGUILayout.EnumPopup ("Pathfinding method:", gameEngine.GetComponent <SceneSettings>().navigationMethod);
				gameEngine.GetComponent <NavigationManager>().ResetEngine ();
				if (gameEngine.GetComponent <NavigationManager>().navigationEngine != null)
				{
					gameEngine.GetComponent <NavigationManager>().navigationEngine.SceneSettingsGUI ();
				}
				gameEngine.GetComponent <SceneSettings>().defaultPlayerStart = (PlayerStart) EditorGUILayout.ObjectField ("Default PlayerStart:", gameEngine.GetComponent <SceneSettings>().defaultPlayerStart, typeof (PlayerStart), true);
				gameEngine.GetComponent <SceneSettings>().sortingMap = (SortingMap) EditorGUILayout.ObjectField ("Sorting map:", gameEngine.GetComponent <SceneSettings>().sortingMap, typeof (SortingMap), true);

				GUILayout.Label ("Scene cutscenes", EditorStyles.boldLabel);
				gameEngine.GetComponent <SceneSettings>().cutsceneOnStart = (Cutscene) EditorGUILayout.ObjectField ("On start:", gameEngine.GetComponent <SceneSettings>().cutsceneOnStart, typeof (Cutscene), true);
				gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad = (Cutscene) EditorGUILayout.ObjectField ("On load:", gameEngine.GetComponent <SceneSettings>().cutsceneOnLoad, typeof (Cutscene), true);
				gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange = (Cutscene) EditorGUILayout.ObjectField ("On variable change:", gameEngine.GetComponent <SceneSettings>().cutsceneOnVarChange, typeof (Cutscene), true);
				EditorGUILayout.Space ();
			}
			
			GUILayout.Label ("Visibility", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Triggers", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetTriggerVisibility (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetTriggerVisibility (false);
				}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Collision", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetCollisionVisiblity (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetCollisionVisiblity (false);
				}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Hotspots", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					SetHotspotVisibility (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					SetHotspotVisibility (false);
				}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
				GUILayout.Label ("NavMesh", buttonWidth);
				if (GUILayout.Button ("On", EditorStyles.miniButtonLeft))
				{
					gameEngine.GetComponent <NavigationManager>().navigationEngine.SetVisibility (true);
				}
				if (GUILayout.Button ("Off", EditorStyles.miniButtonRight))
				{
					gameEngine.GetComponent <NavigationManager>().navigationEngine.SetVisibility (false);
				}
			GUILayout.EndHorizontal ();
			
			GUILayout.Label ("Create new object", EditorStyles.boldLabel);
			
			GUILayout.Label ("Camera", EditorStyles.boldLabel);
			
			if (settingsManager == null || settingsManager.cameraPerspective == CameraPerspective.ThreeD)
			{
				PrefabButton ("Camera", "GameCamera");
				PrefabButton ("Camera", "GameCameraThirdPerson");
			}
			else
			{
				if (settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					PrefabButton ("Camera", "GameCamera2D");
				}
				else
				{
					PrefabButton ("Camera", "GameCamera2.5D");
					
					GUILayout.Label ("Set geometry", EditorStyles.boldLabel);
					PrefabButton ("SetGeometry", "BackgroundImage");
				}
			}
			
			GUILayout.Label ("Logic", EditorStyles.boldLabel);
			PrefabButton ("Logic", "ArrowPrompt");
			PrefabButton ("Logic", "Conversation");
			PrefabButton ("Logic", "Cutscene");
			PrefabButton ("Logic", "DialogueOption");
			PrefabButton ("Logic", "Hotspot");
			PrefabButton ("Logic", "Interaction");
			PrefabButton ("Logic", "Sound");
			PrefabButton ("Logic", "Trigger");
			
			GUILayout.Label ("Navigation", EditorStyles.boldLabel);
			PrefabButton ("Navigation", "SortingMap");
			PrefabButton ("Navigation", "CollisionCube");
			PrefabButton ("Navigation", "CollisionCylinder");
			PrefabButton ("Navigation", "Marker");
			if (gameEngine.GetComponent <NavigationManager>().navigationEngine.GetPrefabName () != "")
			{
				PrefabButton ("Navigation", gameEngine.GetComponent <NavigationManager>().navigationEngine.GetPrefabName ());
			}
			PrefabButton ("Navigation", "Path");
			PrefabButton ("Navigation", "PlayerStart");

			if (GUI.changed)
			{
				EditorUtility.SetDirty (gameEngine.GetComponent <SceneSettings>());
				EditorUtility.SetDirty (gameEngine.GetComponent <PlayerMovement>());
			}
		}
	}
	
	
	private void PrefabButton (string subFolder, string prefabName)
	{
		if (GUILayout.Button (prefabName))
		{
			AddPrefab (subFolder, prefabName, true, true, true);
		}	
	}


	private void PrefabButton (string subFolder, string prefabName, Texture icon)
	{
		if (GUILayout.Button (icon))
		{
			AddPrefab (subFolder, prefabName, true, true, true);
		}	
	}

	
	private void InitialiseObjects ()
	{
		CreateFolder ("_Cameras");
		CreateFolder ("_Cutscenes");
		CreateFolder ("_DialogueOptions");
		CreateFolder ("_Interactions");
		CreateFolder ("_Lights");
		CreateFolder ("_Logic");
		CreateFolder ("_Navigation");
		CreateFolder ("_NPCs");
		CreateFolder ("_Sounds");
		CreateFolder ("_SetGeometry");
		
		// Create subfolders
		CreateSubFolder ("_Cameras", "_GameCameras");

		CreateSubFolder ("_Logic", "_ArrowPrompts");
		CreateSubFolder ("_Logic", "_Conversations");
		CreateSubFolder ("_Logic", "_Hotspots");
		CreateSubFolder ("_Logic", "_Triggers");
		
		CreateSubFolder ("_Navigation", "_CollisionCubes");
		CreateSubFolder ("_Navigation", "_CollisionCylinders");
		CreateSubFolder ("_Navigation", "_Markers");
		CreateSubFolder ("_Navigation", "_NavMeshSegments");
		CreateSubFolder ("_Navigation", "_NavMesh");
		CreateSubFolder ("_Navigation", "_Paths");
		CreateSubFolder ("_Navigation", "_PlayerStarts");
		CreateSubFolder ("_Navigation", "_SortingMaps");

		// Delete default main camera
		if (GameObject.FindWithTag (Tags.mainCamera))
		{
			GameObject mainCam = GameObject.FindWithTag (Tags.mainCamera);
			if (mainCam.GetComponent <MainCamera>() == null)
				DestroyImmediate (mainCam);
		}
		
		// Create main camera
		AddPrefab ("Automatic", "MainCamera", false, false, false);
		PutInFolder (GameObject.FindWithTag (Tags.mainCamera), "_Cameras");
		
		// Create Background Camera (if 2.5D)
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		if (settingsManager && settingsManager.cameraPerspective == CameraPerspective.TwoPointFiveD)
		{
			CreateSubFolder ("_SetGeometry", "_BackgroundImages");
			
			AddPrefab ("Automatic", "BackgroundCamera", false, false, false);
			PutInFolder (GameObject.FindWithTag (Tags.backgroundCamera), "_Cameras");
		}

		// Create Game engine
		AddPrefab ("Automatic", "GameEngine", false, false, false);
		
		// Assign Player Start
		SceneSettings startSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		if (startSettings && startSettings.defaultPlayerStart == null)
		{
			PlayerStart playerStart = AddPrefab ("Navigation", "PlayerStart", false, false, true).GetComponent <PlayerStart>();
			startSettings.defaultPlayerStart = playerStart;
		}

	}

	
	private void SetHotspotVisibility (bool isVisible)
	{
		Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
		Undo.RecordObjects (hotspots, "Hotspot visibility");

		foreach (Hotspot hotspot in hotspots)
		{
			hotspot.showInEditor = isVisible;
			EditorUtility.SetDirty (hotspot);
		}
	}
	
	
	private void SetCollisionVisiblity (bool isVisible)
	{
		_Collision[] colls = FindObjectsOfType (typeof (_Collision)) as _Collision[];
		Undo.RecordObjects (colls, "Collision visibility");
		
		foreach (_Collision coll in colls)
		{
			coll.showInEditor = isVisible;
			EditorUtility.SetDirty (coll);
		}
	}

	
	private void SetTriggerVisibility (bool isVisible)
	{
		AC_Trigger[] triggers = FindObjectsOfType (typeof (AC_Trigger)) as AC_Trigger[];
		Undo.RecordObjects (triggers, "Trigger visibility");
		
		foreach (AC_Trigger trigger in triggers)
		{
			trigger.showInEditor = isVisible;
			EditorUtility.SetDirty (trigger);
		}
	}
	
	
	private void RenameObject (GameObject ob, string resourceName)
	{
		ob.name = AdvGame.GetName (resourceName);
	}
	

	public GameObject AddPrefab (string folderName, string prefabName, bool canCreateMultiple, bool selectAfter, bool putInFolder)
	{
		if (canCreateMultiple || !GameObject.Find (AdvGame.GetName (prefabName)))
		{
			string fileName = assetFolder + folderName + Path.DirectorySeparatorChar.ToString () + prefabName + ".prefab";

			GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
			newOb.name = "Temp";

			if (folderName != "" && putInFolder)
			{
				if (!PutInFolder (newOb, "_" + prefabName + "s"))
				{
					string newName = "_" + prefabName;
					
					if (newName.Contains ("2D"))
					{
						newName = newName.Substring (0, newName.IndexOf ("2D"));
						
						if (!PutInFolder (newOb, newName + "s"))
						{
							PutInFolder (newOb, newName);
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
					else if (newName.Contains ("2.5D"))
					{
						newName = newName.Substring (0, newName.IndexOf ("2.5D"));
						
						if (!PutInFolder (newOb, newName + "s"))
						{
							PutInFolder (newOb, newName);
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
					else if (newName.Contains ("ThirdPerson"))
					{
						newName = newName.Substring (0, newName.IndexOf ("ThirdPerson"));
						
						if (!PutInFolder (newOb, newName + "s"))
						{
							PutInFolder (newOb, newName);
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
					else
					{
						PutInFolder (newOb, newName);
					}
				}
			}

			if (newOb.GetComponent <GameCamera2D>())
			{
				newOb.GetComponent <GameCamera2D>().SetCorrectRotation ();
			}
			
			RenameObject (newOb, prefabName);

			Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);
			
			// Select the object
			if (selectAfter)
			{
				Selection.activeObject = newOb;
			}
			
			return newOb;
		}

		return null;
	}
	

	private bool PutInFolder (GameObject ob, string folderName)
	{
		if (ob && GameObject.Find (folderName))
		{
			if (GameObject.Find (folderName).transform.position == Vector3.zero && folderName.Contains ("_"))
			{
				ob.transform.parent = GameObject.Find (folderName).transform;

				return true;
			}
		}
		
		return false;
	}
	

	private void CreateFolder (string folderName)
	{
		if (!GameObject.Find (folderName))
		{
			GameObject newFolder = new GameObject();
			newFolder.name = folderName;
			Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);
		}
	}
	
	
	private void CreateSubFolder (string baseFolderName, string subFolderName)
	{
		if (!GameObject.Find (subFolderName))
		{
			GameObject newFolder = new GameObject ();
			newFolder.name = subFolderName;
			Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);

			if (newFolder != null && GameObject.Find (baseFolderName))
			{
				newFolder.transform.parent = GameObject.Find (baseFolderName).transform;
			}
			else
			{
				Debug.Log ("Folder " + baseFolderName + " does not exist!");
			}
		}
	}

	#endif

}