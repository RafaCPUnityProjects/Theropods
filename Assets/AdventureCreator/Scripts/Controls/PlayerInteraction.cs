/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInteraction.cs"
 * 
 *	This script processes cursor clicks over hotspots and NPCs
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class PlayerInteraction : MonoBehaviour
{

	[HideInInspector] public Hotspot hotspot;
	
	private Button button;
	
	private PlayerInput playerInput;
	private PlayerMenus playerMenus;
	private PlayerCursor playerCursor;
	private Player player;
	private StateHandler stateHandler;
	private RuntimeInventory runtimeInventory;
	private SettingsManager settingsManager;
	private CursorManager cursorManager;

	
	private void Awake ()
	{
		if (this.GetComponent <PlayerInput>())
		{
			playerInput = this.GetComponent <PlayerInput>();
		}
		if (this.GetComponent <PlayerCursor>())
		{
			playerCursor = this.GetComponent <PlayerCursor>();
		}
		if (this.GetComponent <PlayerMenus>())
		{
			playerMenus = this.GetComponent <PlayerMenus>();
		}

		if (AdvGame.GetReferences () == null)
		{
			Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
		}
		else
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			if (AdvGame.GetReferences ().cursorManager)
			{
				cursorManager = AdvGame.GetReferences ().cursorManager;
			}
		}
	}
	
	
	private void Start ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine))
		{
			if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				runtimeInventory = stateHandler.GetComponent <RuntimeInventory>();
			}
		}
		
		if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
		{
			player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}
	}
	
	
	private void Update ()
	{
		if (stateHandler && playerInput && settingsManager && runtimeInventory && stateHandler.gameState == GameState.Normal)			
		{
			if (!playerInput.mouseOverMenu && Camera.main && playerInput.activeArrows == null)
			{
				if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (!playerInput.interactionMenuIsOn)
					{
						MouseOverAndClickObjects ();
					}
				}
				else if (settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					DoubleClickObjects ();
				}
				else
				{
					MouseOverObjects ();
				}
			}
			else 
			{
				DisableHotspot (false);
			}
		}
	}


	private Hotspot CheckForHotspots ()
	{
		if (settingsManager)
		{
			if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && player.hotspotDetector && (settingsManager.movementMethod == MovementMethod.Direct || settingsManager.movementMethod == MovementMethod.FirstPerson))
			{
				if (player.hotspotDetector)
				{
					return (player.hotspotDetector.nearestHotspot);
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (playerInput.mousePosition);
				RaycastHit hit;
				
				if (Physics.Raycast (ray, out hit, settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer)))
				{
					if (hit.collider.gameObject.GetComponent <Hotspot>())
					{
						if (settingsManager.hotspotDetection == HotspotDetection.MouseOver)
						{
							return (hit.collider.gameObject.GetComponent <Hotspot>());
						}
						else if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && player.hotspotDetector && player.hotspotDetector.IsHotspotInTrigger (hit.collider.gameObject.GetComponent <Hotspot>()))
						{
							return (hit.collider.gameObject.GetComponent <Hotspot>());
						}
					}
				}
			}
		}

		return null;
	}


	private void MouseOverAndClickObjects ()
	{
		Hotspot newHotspot = CheckForHotspots ();

		if (hotspot && !newHotspot)
		{
			DisableHotspot (false);
		}
		else if (newHotspot)
		{
			if (newHotspot.IsSingleInteraction ())
			{
				MouseOverObjects ();
				return;
			}

			if (((settingsManager.inputMethod == InputMethod.TouchScreen && settingsManager.movementMethod != MovementMethod.PointAndClick) || settingsManager.movementMethod == MovementMethod.Drag)
			    && playerInput.dragStartPosition != Vector2.zero)
			{
				// Disable hotspots while dragging player
				DisableHotspot (false); 
			}
			else
			{
				if (newHotspot != hotspot)
				{
					if (hotspot)
					{
						hotspot.Deselect ();
					}
					
					hotspot = newHotspot;
					hotspot.Select ();
				}

				if (hotspot && playerInput.CanClick ())
				{
					if (playerInput.buttonPressed == 1)
					{
						if (runtimeInventory.selectedItem != null && settingsManager.inventoryInteractions == InventoryInteractions.ContextSensitive)
						{
							playerInput.ResetClick ();
							HandleInteraction ();
						}
						else if (playerMenus)
						{
							playerMenus.SetInteractionMenus (true);
						}
					}
					else if (playerInput.buttonPressed == 2)
					{
						hotspot.Deselect ();
					}
				}
			}
		}
		
		if (playerInput.buttonPressed > 0 && !IsMouseOverHotspot () && runtimeInventory.selectedItem != null)
		{
			runtimeInventory.SetNull ();
		}
	}
	
	
	private void DoubleClickObjects ()
	{
		if (playerInput.CanClick () && playerInput.buttonPressed == 1)
		{
			//playerInput.ResetClick ();
			
			// Check Hotspots only when click/tap
			Hotspot newHotspot = CheckForHotspots ();

			if (hotspot && !newHotspot)
			{
				DisableHotspot (false);
			}
			else if (newHotspot)
			{
				playerInput.ResetClick ();

				if (((settingsManager.inputMethod == InputMethod.TouchScreen && settingsManager.movementMethod != MovementMethod.PointAndClick) || settingsManager.movementMethod == MovementMethod.Drag)
				    && playerInput.dragStartPosition != Vector2.zero)
				{
					// Disable hotspots while dragging player
					DisableHotspot (false); 
				}
				else
				{
					if (newHotspot != hotspot)
					{
						if (hotspot)
						{
							hotspot.Deselect ();
						}
						
						hotspot = newHotspot;
						hotspot.Select ();
					}
					else if (hotspot)
					{
						playerInput.ResetClick ();
						HandleInteraction ();
					}
				}
			}
		}
		else if (playerInput.buttonPressed == 2 && playerInput.CanClick ())
		{
			playerInput.ResetClick ();
			HandleInteraction ();
		}
		else if (playerInput.buttonPressed > 0 && !IsMouseOverHotspot () && runtimeInventory.selectedItem != null)
		{
			runtimeInventory.SetNull ();
		}
	}
	
	
	private void MouseOverObjects ()
	{
		Hotspot newHotspot = CheckForHotspots ();

		if (hotspot && !newHotspot)
		{
			DisableHotspot (false);
		}
		else if (newHotspot)
		{
			if (((settingsManager.inputMethod == InputMethod.TouchScreen && settingsManager.movementMethod != MovementMethod.PointAndClick) || settingsManager.movementMethod == MovementMethod.Drag)
			    && playerInput.dragStartPosition != Vector2.zero)
			{
				// Disable hotspots while dragging player
				DisableHotspot (false); 
			}
			else if (hotspot != newHotspot)
			{
				if (hotspot)
				{
					hotspot.Deselect ();
				}

				hotspot = newHotspot;
				hotspot.Select ();
			}
		}
		
		if (playerInput.buttonPressed == 1 && hotspot == null && runtimeInventory.selectedItem != null)
		{
			runtimeInventory.SetNull ();
		}
		
		if (playerInput.CanClick () && playerInput.buttonPressed > 0 && hotspot)
		{
			playerInput.ResetClick ();
			HandleInteraction ();
		}	
	}
	
	
	public void DisableHotspot (bool isInstant)
	{
		if (hotspot)
		{
			if (isInstant)
			{
				hotspot.DeselectInstant ();
			}
			else
			{
				hotspot.Deselect ();
			}
			hotspot = null;
		}
	}
	
	
	private void HandleInteraction ()
	{
		if (hotspot)
		{
			if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (playerInput.buttonPressed == 1)
				{
					if (runtimeInventory.selectedItem == null && hotspot.provideUseInteraction)
					{
						// Perform "Use" interaction
						ClickButton (InteractionType.Use, -1);
					}
					
					else if (runtimeInventory.selectedItem != null)
					{
						// Perform "Use Inventory" interaction
						ClickButton (InteractionType.Inventory, -1);
					}
					
					else if (hotspot.provideLookInteraction)
					{
						// Perform "Look" interaction
						ClickButton (InteractionType.Examine, -1);
					}
				}
				else if (playerInput.buttonPressed == 2)
				{
					if (hotspot.provideLookInteraction)
					{
						// Perform "Look" interaction
						ClickButton (InteractionType.Examine, -1);
					}
				}
			}
			
			else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && playerCursor && cursorManager)
			{
				if (playerInput.buttonPressed == 1)
				{
					if (runtimeInventory.selectedItem == null && hotspot.provideUseInteraction && playerCursor.GetSelectedCursor () >= 0)
					{
						// Perform "Use" interaction
						ClickButton (InteractionType.Use, cursorManager.cursorIcons [playerCursor.GetSelectedCursor ()].id);
					}
					
					else if (runtimeInventory.selectedItem != null && playerCursor.GetSelectedCursor () == -2)
					{
						// Perform "Use Inventory" interaction
						playerCursor.ResetSelectedCursor ();
						ClickButton (InteractionType.Inventory, -1);
					}
				}
			}

			else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{

				if (runtimeInventory.selectedItem != null && hotspot.provideUseInteraction && settingsManager.inventoryInteractions == InventoryInteractions.ContextSensitive)
				{
					// Perform "Use Inventory" interaction
					ClickButton (InteractionType.Inventory, -1);
				}
				
				else if (runtimeInventory.selectedItem == null && hotspot.IsSingleInteraction ())
				{
					// Perform "Use" interaction
					ClickButton (InteractionType.Use, -1);
				}
			}
		}
	}
	

	public void ClickButton (InteractionType _interactionType, int selectedCursorID)
	{
		StopCoroutine ("UseObject");
		player.EndPath ();
		
		button = null;
		
		if (_interactionType == InteractionType.Use)
		{
			if (selectedCursorID == -1)
			{
				if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && hotspot.IsSingleInteraction ())
				{
					button = hotspot.useButtons[0];
				}
				else
				{
					button = hotspot.useButton;
				}
			}
			else
			{
				foreach (Button _button in hotspot.useButtons)
				{
					if (_button.iconID == selectedCursorID)
					{
						button = _button;
						break;
					}
				}
			}
		}
		else if (_interactionType == InteractionType.Examine)
		{
			button = hotspot.lookButton;
		}
		else if (_interactionType == InteractionType.Inventory && runtimeInventory.selectedItem != null)
		{
			foreach (Button invButton in hotspot.invButtons)
			{
				if (invButton.invID == runtimeInventory.selectedItem.id)
				{
					button = invButton;
					break;
				}
			}
		}

		if (button != null && button.isDisabled)
		{
			button = null;

			if (_interactionType != InteractionType.Inventory)
			{
				return;
			}
		}
		
		if (player.hotspotMovingTo == hotspot)
		{
			// Run if clicking twice
			StartCoroutine ("UseObject", true);
		}
		else
		{
			StartCoroutine ("UseObject", false);
		}
	}
	
			
	private IEnumerator UseObject (bool doRun)
	{
		if (playerInput != null && playerInput.runLock == PlayerMoveLock.AlwaysWalk)
		{
			doRun = false;
		}

		if (button != null && !button.isBlocking && (button.playerAction == PlayerAction.WalkToMarker || button.playerAction == PlayerAction.WalkTo))
		{
			stateHandler.gameState = GameState.Normal;
			player.hotspotMovingTo = hotspot;
		}
		else
		{
			stateHandler.gameState = GameState.Cutscene;
			player.hotspotMovingTo = null;
		}
		
		Hotspot _hotspot = hotspot;
		hotspot.Deselect ();
		hotspot = null;
		
		if (button != null && button.playerAction != PlayerAction.DoNothing)
		{
			Vector3 lookVector = Vector3.zero;
			Vector3 targetPos = _hotspot.transform.position;

			if (settingsManager.ActInScreenSpace ())
			{
				lookVector = AdvGame.GetScreenDirection (player.transform.position, _hotspot.transform.position);
			}
			else
			{
				lookVector = targetPos - player.transform.position;
				lookVector.y = 0;
			}

			player.SetLookDirection (lookVector, false);
			
			if (button.playerAction == PlayerAction.TurnToFace)
			{
				while (player.IsTurning ())
				{
					yield return new WaitForFixedUpdate ();			
				}
			}
			
			if (button.playerAction == PlayerAction.WalkToMarker && _hotspot.walkToMarker)
			{
				if (Vector3.Distance (player.transform.position, _hotspot.walkToMarker.transform.position) > (1.05f - settingsManager.destinationAccuracy))
				{
					if (GetComponent <NavigationManager>())
					{
						Vector3[] pointArray;
						Vector3 targetPosition = _hotspot.walkToMarker.transform.position;

						if (settingsManager.ActInScreenSpace ())
						{
							targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
						}

						pointArray = GetComponent <NavigationManager>().navigationEngine.GetPointsArray (player.transform.position, targetPosition);
						player.MoveAlongPoints (pointArray, doRun);
						targetPos = pointArray [pointArray.Length - 1];
					}

					while (player.activePath)
					{
						yield return new WaitForFixedUpdate ();
					}
				}

				if (button.faceAfter)
				{
					lookVector = _hotspot.walkToMarker.transform.forward;
					lookVector.y = 0;
					player.Halt ();
					player.SetLookDirection (lookVector, false);
					
					while (player.IsTurning ())
					{
						yield return new WaitForFixedUpdate ();			
					}
				}
			}
			
			else if (lookVector.magnitude > 2f && button.playerAction == PlayerAction.WalkTo)
			{
				if (GetComponent <NavigationManager>())
				{
					Vector3[] pointArray;
					Vector3 targetPosition = _hotspot.transform.position;
					
					if (settingsManager.ActInScreenSpace ())
					{
						targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
					}

					pointArray = GetComponent <NavigationManager>().navigationEngine.GetPointsArray (player.transform.position, targetPosition);
					player.MoveAlongPoints (pointArray, doRun);
					targetPos = pointArray [pointArray.Length - 1];
				}

				if (button.setProximity)
				{
					button.proximity = Mathf.Max (button.proximity, 1f);
					targetPos.y = player.transform.position.y;
					
					while (Vector3.Distance (player.transform.position, targetPos) > button.proximity && player.activePath)
					{
						yield return new WaitForFixedUpdate ();
					}
				}
				else
				{
					yield return new WaitForSeconds (0.6f);
				}

				if (button.faceAfter)
				{
					if (settingsManager.ActInScreenSpace ())
					{
						lookVector = AdvGame.GetScreenDirection (player.transform.position, _hotspot.transform.position);
					}
					else
					{
						lookVector = _hotspot.transform.position - player.transform.position;
						lookVector.y = 0;
					}

					player.Halt ();
					player.SetLookDirection (lookVector, false);

					while (player.IsTurning ())
					{
						yield return new WaitForFixedUpdate ();			
					}
				}
			}
		}
		else
		{
			player.charState = CharState.Decelerate;
		}
		
		player.EndPath ();
		player.hotspotMovingTo = null;
		yield return new WaitForSeconds (0.1f);
		player.EndPath ();
		player.hotspotMovingTo = null;
		_hotspot.Deselect ();
		hotspot = null;

		if (button == null)
		{
			// Unhandled event
			if (runtimeInventory.selectedItem != null && runtimeInventory.unhandledHotspot)
			{
				RuntimeActionList runtimeActionList = this.GetComponent <RuntimeActionList>();
				
				runtimeInventory.SetNull ();
				runtimeActionList.Play (runtimeInventory.unhandledHotspot);	
			}
			else
			{
				stateHandler.gameState = GameState.Normal;
			}
		}
		else
		{
			if (button.interaction)
			{
				button.interaction.Interact ();
			}
			else
			{
				Debug.Log ("No interaction object found for " + _hotspot.name);
				stateHandler.gameState = GameState.Normal;
			}
		}

		runtimeInventory.SetNull ();
	}
	
	
	public string GetLabel ()
	{
		string label = "";
		
		if (cursorManager && cursorManager.inventoryHandling != InventoryHandling.ChangeCursor && settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction && runtimeInventory.selectedItem != null)
		{
			if (Options.GetLanguage () > 0)
			{
				label = SpeechManager.GetTranslation (cursorManager.hotspotPrefix1.lineID, Options.GetLanguage ()) + " " + runtimeInventory.selectedItem.label + " " + SpeechManager.GetTranslation (cursorManager.hotspotPrefix1.lineID, Options.GetLanguage ()) + " ";
			}
			else
			{
				label = cursorManager.hotspotPrefix1.label + " " + runtimeInventory.selectedItem.label + " " + cursorManager.hotspotPrefix2.label + " ";
			}
		}
		else if (cursorManager && cursorManager.addHotspotPrefix)
		{
			if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (hotspot && hotspot.provideUseInteraction)
				{
					label = cursorManager.GetLabelFromID (hotspot.useButton.iconID);
				}
			}
			else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				label = cursorManager.GetLabelFromID (playerCursor.GetSelectedCursorID ());
			}
		}

		if (hotspot)
		{
			if (Options.GetLanguage () > 0)
			{
				label += SpeechManager.GetTranslation (hotspot.lineID, Options.GetLanguage ());
			}
			else if (hotspot.hotspotName != "")
			{
				label += hotspot.hotspotName;
			}
			else
			{
				label += hotspot.name;
			}
		}

		return (label);		
	}
	
	
	public void StopInteraction ()
	{
		StopCoroutine ("UseObject");
	}


	public Vector2 GetHotspotScreenCentre ()
	{
		Vector3 screenPosition = Camera.main.WorldToViewportPoint (hotspot.transform.position);

		if (hotspot.GetComponent<Collider>() is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = (CapsuleCollider) hotspot.GetComponent<Collider>();
			screenPosition.y += capsuleCollider.center.y / Screen.height * 100f;
		}

		return (new Vector2 (screenPosition.x, 1 - screenPosition.y));
	}

	
	public bool IsMouseOverHotspot ()
	{
		Ray ray = Camera.main.ScreenPointToRay (playerInput.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.hotspotLayer)))
		{
			if (hit.collider.gameObject.GetComponent <Hotspot>())
			{
				return true;
			}
		}
		
		return false;
	}
	
	
	private void OnDestroy ()
	{
		playerInput = null;
		stateHandler = null;
		runtimeInventory = null;
		player = null;
	}
}

