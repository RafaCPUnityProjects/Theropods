/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMenus.cs"
 * 
 *	This script handles the displaying of each of the menus defined in MenuSystem.
 *	It avoids referencing specific menus and menu elements as much as possible,
 *	so that the menu can be completely altered using just the MenuSystem script.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class PlayerMenus : MonoBehaviour
{
	
	[HideInInspector] public bool lockSave = false;

	private bool isPaused;
	private string hotspotLabel = "";
	private float pauseAlpha = 0f;
	private List<Menu> menus;
	private Texture2D pauseTexture;
	
	private Menu crossFadeTo;
	private Menu crossFadeFrom;
	
	public GUIStyle normalStyle;
	private GUIStyle highlightedStyle;
	
	private Dialog dialog;
	private PlayerInput playerInput;
	private PlayerInteraction playerInteraction;
	private MenuSystem menuSystem;
	private StateHandler stateHandler;
	private Options options;
	private SettingsManager settingsManager;
	private CursorManager cursorManager;
	private SpeechManager speechManager;
	private RuntimeInventory runtimeInventory;
	
	
	private void Awake ()
	{
		menus = new List<Menu>();
		
		if (AdvGame.GetReferences ())
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
				speechManager = AdvGame.GetReferences ().speechManager;
				cursorManager = AdvGame.GetReferences ().cursorManager;
			}
			if (AdvGame.GetReferences ().menuManager)
			{
				pauseTexture = AdvGame.GetReferences ().menuManager.pauseTexture;
				
				foreach (Menu _menu in AdvGame.GetReferences ().menuManager.menus)
				{
					Menu newMenu = ScriptableObject.CreateInstance <Menu>();
					newMenu.Copy (_menu);
					menus.Add (newMenu);
				}
			}
		}
		
		playerInput = this.GetComponent <PlayerInput>();
		playerInteraction = this.GetComponent <PlayerInteraction>();
		menuSystem = this.GetComponent <MenuSystem>();
		dialog = this.GetComponent <Dialog>();
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
				options = stateHandler.GetComponent <Options>();
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
			}
		}
		
		
		foreach (Menu menu in menus)
		{
			menu.Recalculate ();
		}
	}
	
	
	public void ShowPauseBackground (bool fadeIn)
	{
		float fadeSpeed = 0.5f;
		if (fadeIn)
		{
			if (pauseAlpha < 1f)
			{
				pauseAlpha += (0.2f * fadeSpeed);
			}				
			else
			{
				pauseAlpha = 1f;
			}
		}
		
		else
		{
			if (pauseAlpha > 0f)
			{
				pauseAlpha -= (0.2f * fadeSpeed);
			}
			else
			{
				pauseAlpha = 0f;
			}
		}
		
		Color tempColor = GUI.color;
		tempColor.a = pauseAlpha;
		GUI.color = tempColor;
		GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), pauseTexture, ScaleMode.ScaleToFit, true, 0f);
	}
	
	
	private void OnGUI ()
	{
		if (playerInteraction && playerInput && menuSystem && stateHandler && settingsManager)
		{
			hotspotLabel = playerInteraction.GetLabel ();

			if (pauseTexture)
			{
				isPaused = false;
				foreach (Menu menu in menus)
				{
					if (menu.IsEnabled ())
					{
						if ((menu.appearType == AppearType.Manual || menu.appearType == AppearType.OnInputKey) && menu.pauseWhenEnabled)
						{
							isPaused = true;
						}
					}
				}

				if (isPaused)
				{
					ShowPauseBackground (true);
				}
				else
				{
					ShowPauseBackground (false);
				}
			}

			foreach (Menu menu in menus)
			{
				if (menu.IsEnabled ())
				{
					if (menu.transitionType == MenuTransition.Fade || menu.transitionType == MenuTransition.FadeAndPan)
					{
						Color tempColor = GUI.color;
						tempColor.a = menu.transitionProgress;
						GUI.color = tempColor;
					}

					menu.StartDisplay ();
					
					foreach (MenuElement element in menu.elements)
					{
						if (element.isVisible)
						{
							SetStyles (element);
							
							for (int i=0; i<element.GetNumSlots (); i++)
							{
								if (menu.IsVisible () && element.isClickable &&
								    ((settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointerOverSlot (element, i, playerInput.invertedMouse)) ||
								 (settingsManager.inputMethod == InputMethod.TouchScreen && menu.IsPointerOverSlot (element, i, playerInput.invertedMouse)) ||
								 (settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState == GameState.Normal && menu.IsPointerOverSlot (element, i, playerInput.invertedMouse)) ||
								 ((settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState != GameState.Normal && menu.selected_element == element && menu.selected_slot == i))))
								{
									float zoom = 1;
									if (menu.transitionType == MenuTransition.Zoom)
									{
										zoom = menu.GetZoom ();
									}

									if (!playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction)
									{
										element.Display (highlightedStyle, i, zoom);
									}
									else
									{
										element.Display (normalStyle, i, zoom);
									}
									
									if (element is MenuInventoryBox)
									{
										if (stateHandler.gameState == GameState.Normal)
										{
											MenuInventoryBox inventoryBox = (MenuInventoryBox) element;
											if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default)
											{
												if (!playerInput.interactionMenuIsOn)
												{
													hotspotLabel = inventoryBox.GetLabel (i);
												}
												else if (runtimeInventory.selectedItem != null)
												{
													hotspotLabel = runtimeInventory.selectedItem.label;
												}
											}
											else
											{
												if (playerInput.CanClick ())
												{
													if (cursorManager.addHotspotPrefix)
													{
														if (runtimeInventory.selectedItem != null)
														{
															hotspotLabel = runtimeInventory.selectedItem.label;
														}

														if ((runtimeInventory.selectedItem == null && !playerInput.interactionMenuIsOn) || playerInput.interactionMenuIsOn)
														{
															hotspotLabel = cursorManager.hotspotPrefix1.label + " " + inventoryBox.GetLabel (i) + " " + cursorManager.hotspotPrefix2.label + " " + hotspotLabel;
														}
													}
												}
											}
										}
									}
									else if (element is MenuInteraction)
									{
										if (stateHandler.gameState == GameState.Normal)
										{
											if (runtimeInventory.selectedItem != null)
											{
												hotspotLabel = runtimeInventory.selectedItem.label;
											}

											if (cursorManager.addHotspotPrefix && playerInput.interactionMenuIsOn)
											{
												MenuInteraction interaction = (MenuInteraction) element;
												hotspotLabel = AdvGame.GetReferences ().cursorManager.GetLabelFromID (interaction.iconID) + hotspotLabel;
											}
										}
									}
									else if (element is MenuInput)
									{
										Event e = Event.current;
										if (e.isKey)
										{
											MenuInput input = (MenuInput) element;
											input.CheckForInput (e.keyCode.ToString ());
										}
									}
									else if (element is MenuDialogList)
									{
										if (stateHandler.gameState == GameState.DialogOptions)
										{
											MenuDialogList dialogList = (MenuDialogList) element;
											if (dialogList.displayType == ConversationDisplayType.IconOnly)
											{
												hotspotLabel = dialogList.GetOptionLabel (i);
											}
										}
									}

									if (playerInput.buttonPressed > 0 && playerInput.buttonPressed < 4 && (!playerInput.interactionMenuIsOn || menu.appearType == AppearType.OnInteraction))
									{
										CheckClick (menu, element, i, playerInput.buttonPressed);
									}
									else if (playerInput.buttonPressed == 4 && element is MenuButton)
									{
										CheckContinuousClick ((MenuButton) element);
									}
								}
								
								else
								{
									element.Display (normalStyle, i, menu.GetZoom ());
								}
							}
						}
					}
					
					menu.EndDisplay ();
				}
			}

		}
	}
	
	
	private void LateUpdate ()
	{
		if (stateHandler && settingsManager && playerInput && playerInteraction && options && dialog && menuSystem && Time.time > 0f)
		{
			if (stateHandler.gameState == GameState.Paused)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = playerInput.timeScale;
			}
			
			playerInput.mouseOverMenu = false;

			if (settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenus)
			{
				playerInput.interactionMenuIsOn = false;
			}
			
			foreach (Menu menu in menus)
			{
				if (menu.IsEnabled ())
				{
					if (menu.positionType == AC_PositionType.FollowCursor)
					{
		                menu.SetCentre (new Vector2 ((playerInput.invertedMouse.x / Screen.width) + (menu.manualPosition.x / 100f) - 0.5f,
						                             (playerInput.invertedMouse.y / Screen.height) + (menu.manualPosition.y / 100f) - 0.5f));
					}
					else if (menu.positionType == AC_PositionType.OnHotspot)
					{
						if (playerInteraction.hotspot)
						{
							menu.SetCentre (playerInteraction.GetHotspotScreenCentre ());
						}
					}
					else if (menu.positionType == AC_PositionType.AboveSpeakingCharacter)
					{
						if (dialog.GetSpeakingCharacter () != null)
						{
							menu.SetCentre (new Vector2 (dialog.GetCharScreenCentre ().x + (menu.manualPosition.x / 100f) - 0.5f,
							                             dialog.GetCharScreenCentre ().y + (menu.manualPosition.y / 100f) - 0.5f));
						}
					}
					
					menu.HandleTransition ();
				}
				
				if (settingsManager)
				{
					if (settingsManager.inputMethod == InputMethod.KeyboardOrController && menu.IsEnabled () && (stateHandler.gameState == GameState.Paused || stateHandler.gameState == GameState.DialogOptions))
					{
						playerInput.selected_option = menu.ControlSelected (playerInput.selected_option);
					}
				}
				else
				{
					Debug.LogWarning ("A settings manager is not present.");
				}
				
				if (menu.appearType == AppearType.Manual)
				{
					if (menu.IsVisible () && menu.GetRect ().Contains (playerInput.invertedMouse))
					{
						playerInput.mouseOverMenu = true;
					}
				}
				
				else if (menu.appearType == AppearType.DuringGameplay)
				{
					if (stateHandler.gameState == GameState.Normal)
					{
						menu.TurnOn (true);
						
						if (menu.GetRect ().Contains (playerInput.invertedMouse))
						{
							playerInput.mouseOverMenu = true;
						}
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff ();
					}
					else
					{
						menu.TurnOff (true);
					}
				}
				
				else if (menu.appearType == AppearType.MouseOver)
				{
					if (stateHandler.gameState == GameState.Normal && menu.GetRect ().Contains (playerInput.invertedMouse))
					{
						if (!menu.isInventoryLockable || (menu.isInventoryLockable && runtimeInventory && !runtimeInventory.isLocked))
						{
							menu.TurnOn (true);
							playerInput.mouseOverMenu = true;
						}
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff ();
					}
					else
					{
						menu.TurnOff (true);
					}
				}
				
				else if (menu.appearType == AppearType.DuringConversation)
				{
					if (playerInput.activeConversation != null && stateHandler.gameState == GameState.DialogOptions)
					{
						menu.TurnOn (true);
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff ();
					}
					else
					{
						menu.TurnOff (true);
					}
				}
				
				else if (menu.appearType == AppearType.OnInputKey)
				{
					if (menu.IsVisible () && menu.GetRect ().Contains (playerInput.invertedMouse))
					{
						playerInput.mouseOverMenu = true;
					}

					try
					{
						if (Input.GetButtonDown (menu.toggleKey))
						{
							if (!menu.IsEnabled ())
							{
								if (stateHandler.gameState == GameState.Paused)
								{
									CrossFade (menu);
								}
								else
								{
									menu.TurnOn (true);
								}
							}
							else
							{
								menu.TurnOff (true);
							}
						}
					}
					catch
					{
						if (settingsManager.inputMethod != InputMethod.TouchScreen)
						{
							Debug.LogWarning ("No '" + menu.toggleKey + "' button exists - please define one in the Input Manager.");
						}
					}
				}
				
				else if (menu.appearType == AppearType.OnHotspot)
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						if (playerInteraction.hotspot != null && playerInteraction.hotspot.useButton != null)
						{
							menu.MatchInteraction (playerInteraction.hotspot.useButton.iconID);
						}
					}
					
					if (hotspotLabel != "" && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions))
					{
						menu.TurnOn (true);
					}
					else if (stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff ();
					}
					else
					{
						menu.TurnOff (true);
					}
				}
				
				else if (menu.appearType == AppearType.OnInteraction)
				{
					if (settingsManager.cancelInteractions == CancelInteractions.ClickOffMenu)
					{
						if (menu.IsEnabled () && stateHandler.gameState == GameState.Normal)
						{
							if (playerInput.buttonPressed == 1)
							{
								playerInput.interactionMenuIsOn = false;
								menu.ForceOff ();
							}
						}
						else if (stateHandler.gameState == GameState.Paused)
						{
							playerInput.interactionMenuIsOn = false;
							menu.ForceOff ();
						}
						else if (playerInteraction.hotspot == null)
						{
							playerInput.interactionMenuIsOn = false;
							menu.TurnOff (true);
						}
					}
					else
					{
						if (menu.IsEnabled () && stateHandler.gameState == GameState.Normal)
						{
							if (menu.GetRect ().Contains (playerInput.invertedMouse) && (playerInteraction.hotspot != null || runtimeInventory.selectedItem != null))
							{
								playerInput.interactionMenuIsOn = true;
							}
							else if (playerInteraction.hotspot == null)
							{
								menu.TurnOff (true);
							}
						}
						else if (stateHandler.gameState == GameState.Paused)
						{
							menu.ForceOff ();
						}
						else if (playerInteraction.hotspot == null)
						{
							menu.TurnOff (true);
						}
					}
				}
				
				else if (menu.appearType == AppearType.WhenSpeechPlays)
				{
					if (stateHandler.gameState != GameState.Paused)
					{
						if (dialog.GetLine () != "" && stateHandler.gameState != GameState.DialogOptions)
						{
							if (options.optionsData.showSubtitles || (speechManager && speechManager.forceSubtitles && !dialog.foundAudio)) 
							{
								menu.TurnOn (true);
							}
							else
							{
								menu.TurnOff (true);	
							}
						}
						else
						{
							menu.TurnOff (true);
						}
					}
					else
					{
						menu.ForceOff ();
					}
				}
			}			
		}
	}
	
	
	public void CheckCrossfade (Menu _menu)
	{
		if (crossFadeFrom == _menu && crossFadeTo != null)
		{
			crossFadeFrom.ForceOff ();
			crossFadeTo.TurnOn (true);
			crossFadeTo = null;
		}
	}
	
	
	private void CheckClick (Menu _menu, MenuElement _element, int _slot, int _buttonPressed)
	{
		if (playerInput && playerInput.CanClick ())
		{
			playerInput.ResetClick ();
			
			if (_element is MenuDialogList)
			{
				MenuDialogList dialogList = (MenuDialogList) _element;
				dialogList.RunOption (_slot);
			}
			
			else if (_element is MenuSavesList)
			{
				MenuSavesList savesList = (MenuSavesList) _element;
				
				if (savesList.saveListType == AC_SaveListType.Save)
				{
					_menu.TurnOff (true);
					SaveSystem.SaveGame (_slot);
				}
				else if (savesList.saveListType == AC_SaveListType.Load)
				{
					_menu.TurnOff (false);
					SaveSystem.LoadGame (_slot);
				}
			}
			
			else if (_element is MenuButton)
			{
				MenuButton button = (MenuButton) _element;
				
				if (button.buttonClickType == AC_ButtonClickType.TurnOffMenu)
				{
					_menu.TurnOff (button.doFade);
				}
				else if (button.buttonClickType == AC_ButtonClickType.Crossfade)
				{
					Menu menuToSwitchTo = GetMenuWithName (button.switchMenuTitle);
					
					if (menuToSwitchTo != null)
					{
						CrossFade (menuToSwitchTo);
					}
					else
					{
						Debug.LogWarning ("Cannot find any menu of name '" + button.switchMenuTitle + "'");
					}
				}
				else if (button.buttonClickType == AC_ButtonClickType.OffsetInventory)
				{
					MenuInventoryBox inventoryToShift = (MenuInventoryBox) GetElementWithName (_menu.title, button.inventoryBoxTitle);
					
					if (inventoryToShift != null)
					{
						inventoryToShift.Shift (button.shiftInventory);
						inventoryToShift.RecalculateSize ();
					}
					else
					{
						Debug.LogWarning ("Cannot find '" + button.inventoryBoxTitle + "' inside '" + _menu.title + "'");
					}
				}
				else if (button.buttonClickType == AC_ButtonClickType.OffsetJournal)
				{
					MenuJournal journalToShift = (MenuJournal) GetElementWithName (_menu.title, button.inventoryBoxTitle);
					
					if (journalToShift != null)
					{
						journalToShift.Shift (button.shiftInventory, button.loopJournal);
						journalToShift.RecalculateSize ();
					}
					else
					{
						Debug.LogWarning ("Cannot find '" + button.inventoryBoxTitle + "' inside '" + _menu.title + "'");
					}
				}
				else if (button.buttonClickType == AC_ButtonClickType.RunActionList)
				{
					if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>() && button.actionList)
					{
						GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>().Play (button.actionList, _menu);
					}
				}
				else if (button.buttonClickType == AC_ButtonClickType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, _buttonPressed);
				}
				else if (button.buttonClickType == AC_ButtonClickType.SimulateInput)
				{
					playerInput.SimulateInput (button.simulateInput, button.inputAxis, button.simulateValue);
				}
			}
			
			else if (_element is MenuSlider)
			{
				MenuSlider slider = (MenuSlider) _element;
				
				if (slider.sliderType == AC_SliderType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, _buttonPressed);
				}
				else
				{
					slider.Change ();
				}
			}
			
			else if (_element is MenuCycle)
			{
				MenuCycle cycle = (MenuCycle) _element;
				
				if (cycle.cycleType == AC_CycleType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, _buttonPressed);
				}
				else
				{
					cycle.Cycle ();
				}
			}
			
			else if (_element is MenuToggle)
			{
				MenuToggle toggle = (MenuToggle) _element;
				
				if (toggle.toggleType == AC_ToggleType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, _buttonPressed);
				}
				else
				{
					toggle.Toggle ();
				}
			}
			
			else if (_element is MenuInventoryBox)
			{
				MenuInventoryBox inventoryBox = (MenuInventoryBox) _element;
				
				if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					MenuSystem.OnElementClick (_menu, _element, _slot, _buttonPressed);
				}
				else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.Default)
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && settingsManager.inventoryInteractions == InventoryInteractions.ContextSensitive)
					{
						inventoryBox.HandleDefaultClick (_buttonPressed, _slot, AC_InteractionMethod.ContextSensitive);
					}
					else
					{
						inventoryBox.HandleDefaultClick (_buttonPressed, _slot, settingsManager.interactionMethod);
					}
				}
				else if (inventoryBox.inventoryBoxType == AC_InventoryBoxType.HostpotBased)
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						if (playerInteraction.hotspot)
						{
							InvItem _item = inventoryBox.items [_slot];
							if (_item != null)
							{
								runtimeInventory.SelectItem (_item);
								_menu.TurnOff (false);
								playerInteraction.ClickButton (InteractionType.Inventory, -2);
								GetComponent <PlayerCursor>().ResetSelectedCursor ();
							}
						}
						else if (runtimeInventory.selectedItem != null)
						{
							runtimeInventory.Combine (inventoryBox.items [_slot]);
						}
						else
						{
							Debug.LogWarning ("Cannot handle inventory click since there is no active Hotspot.");
						}
					}
					else
					{
						Debug.LogWarning ("This type of InventoryBox only works with the Choose Hotspot Then Interaction method of interaction.");
					}
				}
			}
			
			else if (_element is MenuInteraction)
			{
				MenuInteraction interaction = (MenuInteraction) _element;

				if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					Debug.LogWarning ("This element is not compatible with the Context-Sensitive interaction method.");
				}
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (GetComponent <PlayerCursor>())
					{
						PlayerCursor playerCursor = GetComponent <PlayerCursor>();
						playerCursor.SetCursorFromID (interaction.iconID);
					}
				}
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (playerInteraction.hotspot)
					{
						_menu.TurnOff (false);
						playerInteraction.ClickButton (InteractionType.Use, interaction.iconID);
					}
					else if (runtimeInventory.selectedItem != null)
					{
						runtimeInventory.RunInteraction (interaction.iconID);
					}
				}
			}
			
		}
	}


	private void CheckContinuousClick (MenuButton button)
	{
		if (button.buttonClickType == AC_ButtonClickType.SimulateInput)
		{
			playerInput.SimulateInput (button.simulateInput, button.inputAxis, button.simulateValue);
		}
	}
	
	
	public void CrossFade (Menu _menuTo)
	{
		if (_menuTo.isLocked)
		{
			Debug.Log ("Cannot crossfade to menu " + _menuTo.title + " as it is locked.");
		}
		else if (!_menuTo.IsEnabled())
		{
			// Turn off all other menus
			crossFadeFrom = null;
			
			foreach (Menu menu in menus)
			{
				if (menu.IsVisible ())
				{
					if (menu.appearType == AppearType.OnHotspot)
					{
						menu.ForceOff ();
					}
					else
					{
						menu.TurnOff (true);
						crossFadeFrom = menu;
					}
				}
				else
				{
					menu.ForceOff ();
				}
			}
			
			if (crossFadeFrom != null)
			{
				crossFadeTo = _menuTo;
			}
			else
			{
				_menuTo.TurnOn (true);
			}
		}
	}
	
	
	public void SetInteractionMenus (bool turnOn)
	{
		foreach (Menu _menu in menus)
		{
			if (_menu.appearType == AppearType.OnInteraction)
			{
				if (turnOn)
				{
					if (playerInteraction.hotspot)
					{
						_menu.MatchInteractions (playerInteraction.hotspot.useButtons);
					}
					else if (runtimeInventory.selectedItem != null)
					{
						_menu.MatchInteractions (runtimeInventory.selectedItem);
					}

					_menu.TurnOn (true);
				}
				else
				{
					_menu.TurnOff (true);
				}
			}
		}
	}
	
	
	public string GetHotspotLabel ()
	{
		return hotspotLabel;
	}
	
	
	private void SetStyles (MenuElement element)
	{
		normalStyle = new GUIStyle();
		normalStyle.normal.textColor = element.fontColor;
		normalStyle.font = element.font;
		normalStyle.fontSize = (int) (AdvGame.GetMainGameViewSize ().x * element.fontScaleFactor / 100);
		normalStyle.alignment = TextAnchor.MiddleCenter;
		
		highlightedStyle = new GUIStyle();
		highlightedStyle.font = element.font;
		highlightedStyle.fontSize = (int) (AdvGame.GetMainGameViewSize ().x * element.fontScaleFactor / 100);
		highlightedStyle.normal.textColor = element.fontHighlightColor;
		highlightedStyle.normal.background = element.highlightTexture;
		highlightedStyle.alignment = TextAnchor.MiddleCenter;
	}
	
	
	private void OnDestroy ()
	{
		dialog = null;
		playerInput = null;
		playerInteraction = null;
		menuSystem = null;
		stateHandler = null;
		options = null;
		menus = null;
		runtimeInventory = null;
		settingsManager = null;
		cursorManager = null;
		speechManager = null;
	}
	
	
	public List<Menu> GetMenus ()
	{
		return menus;
	}
	
	
	public static Menu GetMenuWithName (string menuName)
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>())
		{
			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
			
			foreach (Menu menu in playerMenus.menus)
			{
				if (menu.title == menuName)
				{
					return menu;
				}
			}
		}
		
		return null;
	}
	
	
	public static MenuElement GetElementWithName (string menuName, string menuElementName)
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>())
		{
			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
			
			foreach (Menu menu in playerMenus.menus)
			{
				if (menu.title == menuName)
				{
					foreach (MenuElement menuElement in menu.elements)
					{
						if (menuElement.title == menuElementName)
						{
							return menuElement;
						}
					}
				}
			}
		}
		
		return null;
	}
	
	
	public static bool IsSavingLocked ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			if (stateHandler.GetLastGameplayState () != GameState.Normal)
			{
				return true;
			}
		}
		
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>())
		{
			return (GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>().lockSave);
		}
		
		return false;
	}


	public static void ResetInventoryBoxes ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>())
		{
			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
			
			foreach (Menu menu in playerMenus.menus)
			{
				foreach (MenuElement menuElement in menu.elements)
				{
					if (menuElement is MenuInventoryBox)
					{
						menu.Recalculate ();
					}
				}
			}
		}
	}
	
}

