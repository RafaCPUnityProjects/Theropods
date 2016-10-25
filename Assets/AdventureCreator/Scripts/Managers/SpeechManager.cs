/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechManager.cs"
 * 
 *	This script handles the "Speech" tab of the main wizard.
 *	It is used to auto-number lines for audio files, and handle translations.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SpeechManager : ScriptableObject
{
	
	public float textScrollSpeed = 50;
	public float screenTimeFactor = 0.1f;
	public bool allowSpeechSkipping = false;
	public bool allowGameplaySpeechSkipping = false;
	public bool searchAudioFiles = true;
	public bool forceSubtitles = true;
	public bool translateAudio = true;
	public bool scrollSubtitles = true;

	public List<SpeechLine> lines;
	public List<string> languages = new List<string>();
	public string[] sceneFiles;
	
	private List<SpeechLine> tempLines;
	private string sceneLabel;

	#if UNITY_EDITOR

	private static GUIContent
		deleteContent = new GUIContent("-", "Delete translation");
	
	
	public void ShowGUI ()
	{
		EditorGUILayout.LabelField ("Settings", EditorStyles.boldLabel);
		
		scrollSubtitles = EditorGUILayout.Toggle ("Scroll subtitles?", scrollSubtitles);
		if (scrollSubtitles)
		{
			textScrollSpeed = EditorGUILayout.FloatField ("Text scroll speed:", textScrollSpeed);
		}
		screenTimeFactor = EditorGUILayout.FloatField ("Display time factor:", screenTimeFactor);
		allowSpeechSkipping = EditorGUILayout.Toggle ("Allow speech skipping?", allowSpeechSkipping);
		if (allowSpeechSkipping)
		{
			allowGameplaySpeechSkipping = EditorGUILayout.Toggle ("Also background speech?", allowGameplaySpeechSkipping);
		}
		forceSubtitles = EditorGUILayout.Toggle ("Force subtitles if no audio?", forceSubtitles);
		searchAudioFiles = EditorGUILayout.Toggle ("Auto-play audio files?", searchAudioFiles);
		translateAudio = EditorGUILayout.Toggle ("Audio translations?", translateAudio);

		EditorGUILayout.Space ();

		LanguagesGUI ();

		EditorGUILayout.Space ();
		
		GUILayout.Label ("Speech lines", EditorStyles.boldLabel);
		GUILayout.Label ("Audio files must be placed in: /Resources/Speech");

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Gather lines", EditorStyles.miniButtonLeft))
		{
			PopulateList ();
			
			if (sceneFiles.Length > 0)
			{
				Array.Sort (sceneFiles);
			}
		}
		if (GUILayout.Button ("Create script sheet", EditorStyles.miniButtonRight))
		{
			if (lines.Count > 0)
			{
				CreateScript ();
			}
		}
		EditorGUILayout.EndHorizontal ();

		if (lines.Count > 0)
		{

			ListLines ();
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (this);
		}
	}


	private void ListLines ()
	{
		EditorGUILayout.BeginVertical ("Button");		
		
		if (sceneFiles.Length > 0)
		{
			foreach (string scene in sceneFiles)
			{
				int extentionPos = scene.IndexOf (".unity");
				sceneLabel = scene.Substring (7, extentionPos-7);
				GUILayout.Label ("Scene: " + sceneLabel, EditorStyles.largeLabel);
				
				GUILayout.Label ("Speech lines: ", EditorStyles.boldLabel);
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.Speech)
					{
						line.ShowGUI ();
					}
				}

				GUILayout.Label ("Hotspots: ", EditorStyles.boldLabel);
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.Hotspot)
					{
						line.ShowGUI ();
					}
				}

				GUILayout.Label ("Dialogue options: ", EditorStyles.boldLabel);
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.DialogueOption)
					{
						line.ShowGUI ();
					}
				}

				GUILayout.Label ("Journal pages: ", EditorStyles.boldLabel);
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.JournalEntry)
					{
						line.ShowGUI ();
					}
				}
			}
		}
		
		GUILayout.Label ("Inventory items:", EditorStyles.boldLabel);
		foreach (SpeechLine line in lines)
		{
			if (line.textType == AC_TextType.InventoryItem)
			{
				line.ShowGUI ();
			}
		}

		GUILayout.Label ("Scene-independent speech lines:", EditorStyles.boldLabel);
		foreach (SpeechLine line in lines)
		{
			if (line.textType == AC_TextType.Speech && line.scene == "")
			{
				line.ShowGUI ();
			}
		}

		GUILayout.Label ("Cursor icons:", EditorStyles.boldLabel);
		foreach (SpeechLine line in lines)
		{
			if (line.textType == AC_TextType.CursorIcon)
			{
				line.ShowGUI ();
			}
		}

		GUILayout.Label ("Hotspot prefixes:", EditorStyles.boldLabel);
		foreach (SpeechLine line in lines)
		{
			if (line.textType == AC_TextType.HotspotPrefix)
			{
				line.ShowGUI ();
			}
		}

		GUILayout.Label ("Menu elements:", EditorStyles.boldLabel);
		foreach (SpeechLine line in lines)
		{
			if ((line.textType == AC_TextType.MenuElement) || (line.textType == AC_TextType.JournalEntry && line.scene == ""))
			{
				line.ShowGUI ();
			}
		}

		EditorGUILayout.EndVertical ();
	}


	private void LanguagesGUI ()
	{
		GUILayout.Label ("Translations", EditorStyles.boldLabel);
		
		if (languages.Count == 0)
		{
			ClearLanguages ();
		}
		else
		{
			if (languages.Count > 1)
			{
				for (int i=1; i<languages.Count; i++)
				{
					EditorGUILayout.BeginHorizontal ();
					languages[i] = EditorGUILayout.TextField (languages[i]);
					
					if (GUILayout.Button ("Import CSV", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(120f)))
					{
						ImportTranslation (i);
					}
					
					if (GUILayout.Button ("Export CSV", EditorStyles.miniButtonRight, GUILayout.MaxWidth(120f)))
					{
						ExportTranslation (i);
					}
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
					{
						Undo.RecordObject (this, "Delete translation: " + languages[i]);
						DeleteLanguage (i);
					}
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			if (GUILayout.Button ("Create new translation"))
			{
				Undo.RecordObject (this, "Add translation");
				CreateLanguage ("New " + languages.Count.ToString ());
			}
		}
	}

	
	private void CreateLanguage (string name)
	{
		languages.Add (name);
		
		foreach (SpeechLine line in lines)
		{
			line.translationText.Add (line.text);
		}
	}
	
	
	private void DeleteLanguage (int i)
	{
		languages.RemoveAt (i);
		
		foreach (SpeechLine line in lines)
		{
			line.translationText.RemoveAt (i-1);
		}
		
	}
	
	
	private void ClearLanguages ()
	{
		languages.Clear ();
		
		foreach (SpeechLine line in lines)
		{
			line.translationText.Clear ();
		}
		
		languages.Add ("Original");	
		
	}
	
	
	private void PopulateList ()
	{
		string originalScene = EditorApplication.currentScene;
		
		if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
		{
			Undo.RecordObject (this, "Update speech list");
			
			// Store the lines temporarily, so that we can update the translations afterwards
			BackupTranslations ();
			
			lines.Clear ();
			
			sceneFiles = GetSceneFiles ();
			
			// First look for lines that already have an assigned lineID
			foreach (string sceneFile in sceneFiles)
			{
				GetLinesInScene (sceneFile, false);
			}
			
			GetLinesFromInventory (false);
			GetLinesFromCursors (false);
			GetLinesFromMenus (false);
			
			// Now look for new lines, which don't have a unique lineID
			foreach (string sceneFile in sceneFiles)
			{
				GetLinesInScene (sceneFile, true);
			}
			
			GetLinesFromInventory (true);
			GetLinesFromCursors (true);
			GetLinesFromMenus (true);
			
			RestoreTranslations ();
			
			if (EditorApplication.currentScene != originalScene)
			{
				EditorApplication.OpenScene (originalScene);
			}
		}
	}
	
	
	private void ExtractHotspot (Hotspot hotspot, bool onlySeekNew)
	{
		string hotspotName = hotspot.name;
		if (hotspot.hotspotName != "")
		{
			hotspotName = hotspot.hotspotName;
		}
		
		if (onlySeekNew && hotspot.lineID == -1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot);
			hotspot.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && hotspot.lineID > -1)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (hotspot.lineID, EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot));
		}
	}
	
	
	private void ExtractDialogOption (ButtonDialog dialogOption, bool onlySeekNew)
	{
		if (onlySeekNew && dialogOption.lineID < 1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
			dialogOption.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && dialogOption.lineID > 0)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (dialogOption.lineID, EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption));
		}
	}


	private void ExtractInventory (InvItem invItem, bool onlySeekNew)
	{
		if (onlySeekNew && invItem.lineID == -1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, invItem.label, languages.Count - 1, AC_TextType.InventoryItem);
			invItem.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && invItem.lineID > -1)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (invItem.lineID, EditorApplication.currentScene, invItem.label, languages.Count - 1, AC_TextType.InventoryItem));
		}
	}


	private void ExtractPrefix (HotspotPrefix prefix, bool onlySeekNew)
	{
		if (onlySeekNew && prefix.lineID == -1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
			prefix.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && prefix.lineID > -1)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (prefix.lineID, EditorApplication.currentScene, prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix));
		}
	}


	private void ExtractIcon (CursorIcon icon, bool onlySeekNew)
	{
		if (onlySeekNew && icon.lineID == -1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
			icon.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && icon.lineID > -1)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (icon.lineID, "", icon.label, languages.Count - 1, AC_TextType.CursorIcon));
		}
	}


	private void ExtractElement (MenuElement element, string elementLabel, bool onlySeekNew)
	{
		if (onlySeekNew && element.lineID == -1)
		{
			// Assign a new ID on creation
			SpeechLine newLine;
			newLine = new SpeechLine (GetIDArray(), "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
			element.lineID = newLine.lineID;
			lines.Add (newLine);
		}
		
		else if (!onlySeekNew && element.lineID > -1)
		{
			// Already has an ID, so don't replace
			lines.Add (new SpeechLine (element.lineID, "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement));
		}
	}


	private void ExtractJournalElement (MenuJournal journal, List<JournalPage> pages, bool onlySeekNew)
	{
		foreach (JournalPage page in pages)
		{
			if (onlySeekNew && page.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
				page.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && page.lineID > -1)
			{
				// Already has an ID, so don't replace
				lines.Add (new SpeechLine (page.lineID, "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry));
			}
		}
	}


	private string RemoveLineBreaks (string text)
	{
		return (text.Replace ("\n", "[break]"));
	}


	private string AddLineBreaks (string text)
	{
		return (text.Replace ("[break]", "\n"));
	}

	
	private void ExtractSpeech (ActionSpeech action, bool onlySeekNew, bool isInScene)
	{
		string speaker = "";
		
		if (action.isPlayer)
		{
			speaker = "Player";
		}
		else if (action.speaker)
		{
			speaker = action.speaker.name;
		}
		
		if (speaker != "" && action.messageText != "")
		{
			if (onlySeekNew && action.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				if (isInScene)
				{
					newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech);
				}
				else
				{
					newLine = new SpeechLine (GetIDArray(), "", speaker, action.messageText, languages.Count - 1, AC_TextType.Speech);
				}
				action.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && action.lineID > -1)
			{
				// Already has an ID, so don't replace
				if (isInScene)
				{
					lines.Add (new SpeechLine (action.lineID, EditorApplication.currentScene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech));
				}
				else
				{
					lines.Add (new SpeechLine (action.lineID, "", speaker, action.messageText, languages.Count - 1, AC_TextType.Speech));
				}
			}
		}
		else
		{
			// Remove from SpeechManager
			action.lineID = -1;
		}
	}


	private void ExtractJournalEntry (ActionMenuState action, bool onlySeekNew, bool isInScene)
	{
		if (action.changeType == ActionMenuState.MenuChangeType.AddJournalPage && action.journalText != "")
		{
			if (onlySeekNew && action.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				if (isInScene)
				{
					newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
				}
				else
				{
					newLine = new SpeechLine (GetIDArray(), "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
				}
				action.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && action.lineID > -1)
			{
				// Already has an ID, so don't replace
				if (isInScene)
				{
					lines.Add (new SpeechLine (action.lineID, EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry));
				}
				else
				{
					lines.Add (new SpeechLine (action.lineID, "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry));
				}
			}
		}
		else
		{
			// Remove from SpeechManager
			action.lineID = -1;
		}
	}


	private void GetLinesFromInventory (bool onlySeekNew)
	{
		InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
		
		if (inventoryManager)
		{
			// Unhandled combine
			if (inventoryManager.unhandledCombine != null)
			{	
				foreach (AC.Action action in inventoryManager.unhandledCombine.actions)
				{
					if (action is ActionSpeech)
					{
						ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
					}
					else if (action is ActionMenuState)
					{
						ExtractJournalEntry (action as ActionMenuState, onlySeekNew, false);
					}
				}
				EditorUtility.SetDirty (inventoryManager.unhandledCombine);
			}
			
			// Unhandled hotspot
			if (inventoryManager.unhandledHotspot != null)
			{
				foreach (AC.Action action in inventoryManager.unhandledHotspot.actions)
				{
					if (action is ActionSpeech)
					{
						ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
					}
					else if (action is ActionMenuState)
					{
						ExtractJournalEntry (action as ActionMenuState, onlySeekNew, false);
					}
				}
				EditorUtility.SetDirty (inventoryManager.unhandledHotspot);
			}
			
			// Item-specific events
			if (inventoryManager.items.Count > 0)
			{
				foreach (InvItem item in inventoryManager.items)
				{
					// Label
					ExtractInventory (item, onlySeekNew);
					
					// Use
					if (item.useActionList != null)
					{
						foreach (AC.Action action in item.useActionList.actions)
						{
							if (action is ActionSpeech)
							{
								ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
							}
							else if (action is ActionMenuState)
							{
								ExtractJournalEntry (action as ActionMenuState, onlySeekNew, false);
							}
						}
						EditorUtility.SetDirty (item.useActionList);
					}
					
					// Look
					if (item.lookActionList)
					{
						foreach (AC.Action action in item.lookActionList.actions)
						{
							if (action is ActionSpeech)
							{
								ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
							}
							else if (action is ActionMenuState)
							{
								ExtractJournalEntry (action as ActionMenuState, onlySeekNew, false);
							}
						}
						EditorUtility.SetDirty (item.lookActionList);
					}
					
					// Combines
					foreach (InvActionList actionList in item.combineActionList)
					{
						if (actionList != null)
						{
							foreach (AC.Action action in actionList.actions)
							{
								if (action is ActionSpeech)
								{
									ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
								}
								else if (action is ActionMenuState)
								{
									ExtractJournalEntry (action as ActionMenuState, onlySeekNew, false);
								}
							}
							EditorUtility.SetDirty (actionList);
						}
					}
				}
			}

			EditorUtility.SetDirty (inventoryManager);
		}
	}


	private void GetLinesFromMenus (bool onlySeekNew)
	{
		MenuManager menuManager = AdvGame.GetReferences ().menuManager;

		if (menuManager)
		{
			// Gather elements
			if (menuManager.menus.Count > 0)
			{
				foreach (Menu menu in menuManager.menus)
				{
					foreach (MenuElement element in menu.elements)
					{
						if (element is MenuButton)
						{
							MenuButton button = (MenuButton) element;
							ExtractElement (element, button.label, onlySeekNew);

							// MenuActionList
							if (button.buttonClickType == AC_ButtonClickType.RunActionList && button.actionList != null)
							{
								foreach (AC.Action action in button.actionList.actions)
								{
									if (action is ActionSpeech)
									{
										ExtractSpeech (action as ActionSpeech, onlySeekNew, false);
									}
								}
								EditorUtility.SetDirty (button.actionList);
							}
						}
						else if (element is MenuCycle)
						{
							MenuCycle button = (MenuCycle) element;
							ExtractElement (element, button.label, onlySeekNew);
						}
						else if (element is MenuInput)
						{
							MenuInput button = (MenuInput) element;
							ExtractElement (element, button.label, onlySeekNew);
						}
						else if (element is MenuLabel)
						{
							MenuLabel button = (MenuLabel) element;
							ExtractElement (element, button.label, onlySeekNew);
						}
						else if (element is MenuSlider)
						{
							MenuSlider button = (MenuSlider) element;
							ExtractElement (element, button.label, onlySeekNew);
						}
						else if (element is MenuToggle)
						{
							MenuToggle button = (MenuToggle) element;
							ExtractElement (element, button.label, onlySeekNew);
						}
						else if (element is MenuJournal)
						{
							MenuJournal journal = (MenuJournal) element;
							ExtractJournalElement (journal, journal.pages, onlySeekNew);
						}
					}
				}
			}

			EditorUtility.SetDirty (menuManager);
		}
	}


	private void GetLinesFromCursors (bool onlySeekNew)
	{
		CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
		
		if (cursorManager)
		{
			// Prefixes
			ExtractPrefix (cursorManager.hotspotPrefix1, onlySeekNew);
			ExtractPrefix (cursorManager.hotspotPrefix2, onlySeekNew);

			// Gather icons
			if (cursorManager.cursorIcons.Count > 0)
			{
				foreach (CursorIcon icon in cursorManager.cursorIcons)
				{
					ExtractIcon (icon, onlySeekNew);
				}
			}

			EditorUtility.SetDirty (cursorManager);
		}
	}
	
	
	private void GetLinesInScene (string sceneFile, bool onlySeekNew)
	{
		if (EditorApplication.currentScene != sceneFile)
		{
			EditorApplication.OpenScene (sceneFile);
		}
		
		// Speech lines and journal entries
		ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
		foreach (ActionList list in actionLists)
		{
			foreach (AC.Action action in list.actions)
			{
				if (action is ActionSpeech)
				{
					ExtractSpeech (action as ActionSpeech, onlySeekNew, true);
				}
				else if (action is ActionMenuState)
				{
					ExtractJournalEntry (action as ActionMenuState, onlySeekNew, true);
				}
			}
		}

		// Hotspots
		Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
		foreach (Hotspot hotspot in hotspots)
		{
			ExtractHotspot (hotspot, onlySeekNew);
			EditorUtility.SetDirty (hotspot);
		}
		
		// Dialogue options
		Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
		foreach (Conversation conversation in conversations)
		{
			foreach (ButtonDialog dialogOption in conversation.options)
			{
				ExtractDialogOption (dialogOption, onlySeekNew);
			}
			EditorUtility.SetDirty (conversation);
		}
		
		// Save the scene
		EditorApplication.SaveScene ();
		EditorUtility.SetDirty (this);
	}
	
	
	private string[] GetSceneFiles ()
	{
		List<string> temp = new List<string>();
		
		foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
		{
			if (S.enabled)
			{
				temp.Add(S.path);
			}
		}
		
		return temp.ToArray();
	}
	
	
	private int[] GetIDArray ()
	{
		// Returns a list of id's in the list
		
		List<int> idArray = new List<int>();
		
		foreach (SpeechLine line in lines)
		{
			idArray.Add (line.lineID);
		}
		
		idArray.Sort ();
		return idArray.ToArray ();
	}
	
	
	private void RestoreTranslations ()
	{
		// Match IDs for each entry in lines and tempLines, send over translation data
		foreach (SpeechLine tempLine in tempLines)
		{
			foreach (SpeechLine line in lines)
			{
				if (tempLine.lineID == line.lineID)
				{
					line.translationText = tempLine.translationText;
					break;
				}
			}
		}
		
		tempLines = null;
	}
	
	
	private void BackupTranslations ()
	{
		tempLines = new List<SpeechLine>();
		foreach (SpeechLine line in lines)
		{
			tempLines.Add (line);
		}
	}


	private void ImportTranslation (int i)
	{
		string fileName = EditorUtility.OpenFilePanel ("Import " + languages[i] + " translation", "Assets", "csv");
		if (fileName.Length == 0)
		{
			return;
		}

		//string fileName = "Assets" + Path.DirectorySeparatorChar.ToString () + languages[i] + ".csv";

		if (File.Exists (fileName))
		{
			string csvText = Serializer.LoadSaveFile (fileName);
			string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);

			int lineID = 0;
			string translationText = "";
			string owner = "";

			for (int y = 1; y < csvOutput.Length; y++)
			{
				try
				{
					lineID = int.Parse (csvOutput [0, y]);
					translationText = csvOutput [3, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
					string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);

					if (typeText.Contains ("JournalEntry (Page "))
					{
						owner = typeText.Replace ("JournalEntry (", "");
						owner = owner.Replace (")", "");
					}
					else
					{
						owner = "";
					}

					UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
				}
				catch
				{}
			}

			EditorUtility.SetDirty (this);
		}
		else
		{
			Debug.LogWarning ("No CSV file found.  Looking for: " + fileName);
		}
	}


	private void UpdateTranslation (int i, int _lineID, string _owner, string translationText)
	{
		foreach (SpeechLine line in lines)
		{
			if (line.lineID == _lineID)
			{
				line.translationText [i-1] = translationText;
			}
		}
	}


	private void ExportTranslation (int i)
	{
		string fileName = EditorUtility.SaveFilePanel ("Export " + languages[i] + " translation", "Assets", languages[i].ToString () + ".csv", "csv");
		if (fileName.Length == 0)
		{
			return;
		}

		bool fail = false;
		List<string[]> output = new List<string[]>();
		output.Add (new string[] {"ID", "Type", "Original line", languages[i] + " translation"});

		foreach (SpeechLine line in lines)
		{
			output.Add (new string[] 
			{
				line.lineID.ToString (),
				line.GetInfo (),
				RemoveLineBreaks (line.text),
				RemoveLineBreaks (line.translationText [i-1])
			});

			if (line.textType != AC_TextType.JournalEntry && (line.text.Contains (CSVReader.csvDelimiter) || line.translationText [i-1].Contains (CSVReader.csvDelimiter)))
			{
				fail = true;
				Debug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
			}
		}

		if (!fail)
		{
			int length = output.Count;

			StringBuilder sb = new StringBuilder();
			for (int j=0; j<length; j++)
			{
				sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
			}

			Serializer.CreateSaveFile (fileName, sb.ToString ());
		}
	}


	private void CreateScript ()
	{
		string[] s = Application.dataPath.Split('/');
		string projectName = s[s.Length - 2];
		
		string script = "Script file for " + projectName + " - created " + DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
		
		// By scene
		foreach (string scene in sceneFiles)
		{
			bool foundLinesInScene = false;
			
			foreach (SpeechLine line in lines)
			{
				if (line.scene == scene && line.textType == AC_TextType.Speech)
				{
					if (!foundLinesInScene)
					{
						script += "\n";
						script += "\n";
						script += "Scene: " + scene;
						foundLinesInScene = true;
					}
					
					script += "\n";
					script += "\n";
					script += line.Print ();
				}
			}
		}
		
		// No scene
		bool foundLinesInInventory = false;
		
		foreach (SpeechLine line in lines)
		{
			if (line.scene == "" && line.textType == AC_TextType.Speech)
			{
				if (!foundLinesInInventory)
				{
					script += "\n";
					script += "\n";
					script += "Scene-independent lines: ";
					foundLinesInInventory = true;
				}
				
				script += "\n";
				script += "\n";
				script += line.Print ();
			}
		}
		
		string fileName = "Assets" + Path.DirectorySeparatorChar.ToString () + "GameScript.txt";
		
		Serializer.CreateSaveFile (fileName, script);
	}
	
	#endif


	public static string GetTranslation (int _lineID, int i)
	{
		SpeechManager speechManager = AdvGame.GetReferences ().speechManager;

		if (_lineID == -1)
		{
			Debug.Log ("Cannot find translation because the text has not been added to the Speech Manager.");
		}
		else
		{
			foreach (SpeechLine line in speechManager.lines)
			{
				if (line.lineID == _lineID)
				{
					return line.translationText [i-1];
				}
			}
		}
		return "";
	}

}