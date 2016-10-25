/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SettingsManager.cs"
 * 
 *	This script handles the "Settings" tab of the main wizard.
 *	It is used to define the player, and control methods of the game.
 * 
 */

using UnityEngine;
using System.IO;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SettingsManager : ScriptableObject
{
	// Save settings
	public string saveFileName = "";
	
	// Character settings
	public Player player;

	// Interface settings
	public MovementMethod movementMethod = MovementMethod.PointAndClick;
	public InputMethod inputMethod = InputMethod.MouseAndKeyboard;
	public AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
	public HotspotDetection hotspotDetection = HotspotDetection.MouseOver;
	public CancelInteractions cancelInteractions = CancelInteractions.CursorLeavesMenus;
	public InventoryInteractions inventoryInteractions = InventoryInteractions.ChooseInventoryThenInteraction;

	// Movement settings
	public Transform clickPrefab;
	public DirectMovementType directMovementType = DirectMovementType.RelativeToCamera;
	public float destinationAccuracy = 0.9f;
	public float walkableClickRange = 0.5f;
	public DragAffects dragAffects = DragAffects.Movement;
	public float verticalReductionFactor = 0.7f;

	// TouchScreen settings
	public float freeAimTouchSpeed = 0.1f;
	public float dragWalkThreshold = 5f;
	public float dragRunThreshold = 20f;
	public bool drawDragLine = false;
	public float dragLineWidth = 3f;
	public Color dragLineColor = Color.white;
	
	// Camera settings
	public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;
	private int cameraPerspective_int;
	#if UNITY_EDITOR
	private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
	#endif
	public MovingTurning movingTurning = MovingTurning.TopDown;
	
	// Raycast settings
	public float navMeshRaycastLength = 100f;
	public float hotspotRaycastLength = 100f;

	// Layer names
	public string hotspotLayer = "Default";
	public string navMeshLayer = "NavMesh";
	public string backgroundImageLayer = "BackgroundImage";
	public string deactivatedLayer = "Ignore Raycast";
		
	// Options data
	#if UNITY_EDITOR
	private OptionsData optionsData = new OptionsData ();
	private string ppKey = "Options";
	private string optionsBinary = "";


	public void ShowGUI ()
	{
		EditorGUILayout.LabelField ("Save game settings", EditorStyles.boldLabel);
		
		if (saveFileName == "")
		{
			saveFileName = SaveSystem.SetProjectName ();
		}
		saveFileName = EditorGUILayout.TextField ("Save filename:", saveFileName);
		
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Character settings:", EditorStyles.boldLabel);
		
		player = (Player) EditorGUILayout.ObjectField ("Player:", player, typeof (Player), false);

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Interface settings", EditorStyles.boldLabel);
		
		movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
		inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
		interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);
		hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);

		if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
		{
			inventoryInteractions = (InventoryInteractions) EditorGUILayout.EnumPopup ("Inventory interactions:", inventoryInteractions);
		}
		else
		{
			cancelInteractions = (CancelInteractions) EditorGUILayout.EnumPopup ("Cancel interactions with:", cancelInteractions);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox ("The following input axes are available for the chosen interface settings:" + GetInputList (), MessageType.Info);

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Movement settings", EditorStyles.boldLabel);

		if ((inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.PointAndClick) || movementMethod == MovementMethod.Drag)
		{
			dragWalkThreshold = EditorGUILayout.FloatField ("Walk threshold:", dragWalkThreshold);
			dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
			
			if (inputMethod == InputMethod.TouchScreen && movementMethod == MovementMethod.FirstPerson)
			{
				freeAimTouchSpeed = EditorGUILayout.FloatField ("Freelook speed:", freeAimTouchSpeed);
			}

			drawDragLine = EditorGUILayout.Toggle ("Draw drag line?", drawDragLine);
			if (drawDragLine)
			{
				dragLineWidth = EditorGUILayout.FloatField ("Drag line width:", dragLineWidth);
				dragLineColor = EditorGUILayout.ColorField ("Drag line colour:", dragLineColor);
			}
		}
		else if (movementMethod == MovementMethod.Direct)
		{
			directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Direct-movement type:", directMovementType);
		}
		else if (movementMethod == MovementMethod.PointAndClick)
		{
			clickPrefab = (Transform) EditorGUILayout.ObjectField ("Click marker:", clickPrefab, typeof (Transform), false);
			walkableClickRange = EditorGUILayout.Slider ("NavMesh search %:", walkableClickRange, 0f, 1f);
		}
		if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen)
		{
			dragAffects = (DragAffects) EditorGUILayout.EnumPopup ("Touch-drag affects:", dragAffects);
		}

		destinationAccuracy = EditorGUILayout.Slider ("Destination accuracy:", destinationAccuracy, 0f, 1f);
		
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
		
		cameraPerspective_int = (int) cameraPerspective;
		cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
		cameraPerspective = (CameraPerspective) cameraPerspective_int;
		if (movementMethod == MovementMethod.FirstPerson)
		{
			cameraPerspective = CameraPerspective.ThreeD;
		}
		if (cameraPerspective == CameraPerspective.TwoD)
		{
			movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
			if (movingTurning == MovingTurning.TopDown)
			{
				verticalReductionFactor = EditorGUILayout.Slider ("Vertical movement factor:", verticalReductionFactor, 0.1f, 1f);
			}
		}

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Raycast settings", EditorStyles.boldLabel);
		
		navMeshRaycastLength = EditorGUILayout.FloatField ("NavMesh ray length:", navMeshRaycastLength);
		hotspotRaycastLength = EditorGUILayout.FloatField ("Hotspot ray length:", hotspotRaycastLength);

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Layer names", EditorStyles.boldLabel);

		hotspotLayer = EditorGUILayout.TextField ("Hotspot:", hotspotLayer);
		navMeshLayer = EditorGUILayout.TextField ("Nav mesh:", navMeshLayer);
		if (cameraPerspective == CameraPerspective.TwoPointFiveD)
		{
			backgroundImageLayer = EditorGUILayout.TextField ("Background image:", backgroundImageLayer);
		}
		deactivatedLayer = EditorGUILayout.TextField ("Deactivated:", deactivatedLayer);

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Options data", EditorStyles.boldLabel);

		if (!PlayerPrefs.HasKey (ppKey))
		{
			optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
		}

		optionsBinary = PlayerPrefs.GetString (ppKey);
		optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);

		optionsData.speechVolume = EditorGUILayout.IntSlider ("Speech volume:", optionsData.speechVolume, 0, 10);
		optionsData.musicVolume = EditorGUILayout.IntSlider ("Music volume:", optionsData.musicVolume, 0, 10);
		optionsData.sfxVolume = EditorGUILayout.IntSlider ("SFX volume:", optionsData.sfxVolume, 0, 10);
		optionsData.showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", optionsData.showSubtitles);
		optionsData.language = EditorGUILayout.IntField ("Language:", optionsData.language);

		optionsBinary = Serializer.SerializeObjectBinary (optionsData);
		PlayerPrefs.SetString (ppKey, optionsBinary);

		if (GUILayout.Button ("Reset options data"))
		{
			PlayerPrefs.DeleteKey ("Options");
			optionsData = new OptionsData ();
			Debug.Log ("PlayerPrefs cleared");
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (this);
		}
	}
	
	#endif


	private string GetInputList ()
	{
		string result = "";

		if (inputMethod == InputMethod.KeyboardOrController)
		{
			result += "\n";
			result += "- InteractionA";
			result += "\n";
			result += "- InteractionB";
			result += "\n";
			result += "- CursorHorizontal";
			result += "\n";
			result += "- CursorVertical";
		}

		if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
		{
			if (inputMethod != InputMethod.TouchScreen)
			{
				result += "\n";
				result += "- Horizontal";
				result += "\n";
				result += "- Vertical";
				result += "\n";
				result += "- Run";
			}

			if (movementMethod == MovementMethod.FirstPerson)
			{
				result = "\n";
				result += "- ToggleCursor";

				if (inputMethod == InputMethod.MouseAndKeyboard)
				{
					result = "\n";
					result += "- MouseScrollWheel";
					result += "\n";
					result += "- CursorHorizontal";
					result += "\n";
					result += "- CursorVertical";
				}
			}
		}

		result += "\n";
		result += "- FlashHotspots";
		result += "\n";
		result += "- Menu";

		return result;
	}


	public bool ActInScreenSpace ()
	{
		if (movingTurning == MovingTurning.ScreenSpace && cameraPerspective == CameraPerspective.TwoD)
		{
			return true;
		}

		return false;
	}


	public bool IsTopDown ()
	{
		if (movingTurning == MovingTurning.TopDown && cameraPerspective == CameraPerspective.TwoD)
		{
			return true;
		}
		
		return false;
	}


	public bool IsFirstPersonDragRotation ()
	{
		if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Rotation)
		{
			return true;
		}

		return false;
	}

}