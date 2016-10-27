﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2016
 *	
 *	"Dialog.cs"
 * 
 *	This class processes any dialogue line.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * A container class for an active line of dialogue.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_speech.html")]
	#endif
	public class Speech
	{

		/** The line's SpeechLog entry */
		public SpeechLog log;
		/** The display text */
		public string displayText;
		/** True if the line should play in the backround, and not interrupt Actions or gameplay. */
		public bool isBackground;
		/** How long the line should be active for */
		public float displayDuration;
		/** True if the line is active */
		public bool isAlive;
		/** If True, the speech line has an AudioClip supplied */
		public bool hasAudio;
		/** If True, then the Action that ran this speech has ended, but the speech line is still active */
		public bool continueFromSpeech = false;
		
		private int gapIndex = -1;
		private int continueIndex = -1;
		private List<SpeechGap> speechGaps = new List<SpeechGap>();
		private float endTime;
		private float continueTime;

		private AC.Char speaker;
		private bool isSkippable;
		private bool pauseGap;
		private bool holdForever = false;

		private float scrollAmount = 0f;
		private float pauseEndTime = 0f;
		private bool pauseIsIndefinite = false;

		// Rich text
		private int boldTagIndex = -1;
		private int italicTagIndex = -1;
		private int sizeTagIndex = -1;
		private int colorTagIndex = -1;

		private const string openBoldTag = "<b>";
		private const string openItalicTag = "<i>";
		private const string openSizeTag = "<size";
		private const string openColorTag = "<color";

		private const string closeBoldTag = "</b>";
		private const string closeItalicTag = "</i>";
		private const string closeSizeTag = "</size>";
		private const string closeColorTag = "</color>";
		private string closingTags = "";


		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "_speaker">The speaking character. If null, the line is considered a narration</param>
		 * <param name = "_message">The subtitle text to display</param>
		 * <param name = "lineID">The unique ID number of the line, as generated by the Speech Manager</param>
		 * <param name = "_language">The currently-selected language</param>
		 * <param name = "_isBackground">True if the line should play in the background, and not interrupt Actions or gameplay</param>
		 * <param name = "_noAnimation">True if the speaking character should not play a talking animation</param>
		 */
		public Speech (Char _speaker, string _message, int lineID, string _language, bool _isBackground, bool _noAnimation)
		{
			// Clear rich text
			boldTagIndex = italicTagIndex = sizeTagIndex = colorTagIndex = -1;
			closingTags = "";

			log.Clear ();
			isBackground = _isBackground;
			
			if (_speaker)
			{
				speaker = _speaker;
				speaker.isTalking = !_noAnimation;
				log.speakerName = _speaker.name;
				
				if (_speaker.GetComponent <Player>())
				{
					if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow || !KickStarter.speechManager.usePlayerRealName)
					{
						log.speakerName = "Player";
					}
				}
				
				if (_speaker.GetComponent <Hotspot>())
				{
					if (_speaker.GetComponent <Hotspot>().hotspotName != "")
					{
						log.speakerName = _speaker.GetComponent <Hotspot>().hotspotName;
					}
				}

				_speaker.ClearExpression ();

				if (!_noAnimation)
				{
					if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Off)
					{
						speaker.isLipSyncing = false;
					}
					else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Salsa2D || KickStarter.speechManager.lipSyncMode == LipSyncMode.FromSpeechText || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPamelaFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadSapiFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPapagayoFile)
					{
						speaker.StartLipSync (KickStarter.dialog.GenerateLipSyncShapes (KickStarter.speechManager.lipSyncMode, lineID, speaker.name, _language, _message));
					}
					else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.RogoLipSync)
					{
						RogoLipSyncIntegration.Play (_speaker, speaker.name, lineID, _language);
					}
				}
			}
			else
			{
				if (speaker)
				{
					speaker.isTalking = false;
				}
				speaker = null;			
				log.speakerName = "Narrator";
			}
			
			_message = AdvGame.ConvertTokens (_message);
			_message = DetermineGaps (_message);
			if (speechGaps.Count > 0)
			{
				gapIndex = 0;
				foreach (SpeechGap gap in speechGaps)
				{
					if (gap.expressionID < 0)
					{
						displayDuration += (float) gap.waitTime;
					}
				}
			}
			else
			{
				gapIndex = -1;
			}

			if (lineID > -1)
			{
				log.lineID = lineID;
			}

			// Play sound and time displayDuration to it
			if (lineID > -1 && log.speakerName != "" && KickStarter.speechManager.searchAudioFiles)
			{
				AudioClip clipObj = null;

				if (KickStarter.speechManager.autoNameSpeechFiles)
				{
					string fullFilename = "Speech/";
					string filename = KickStarter.speechManager.GetLineFilename (lineID);
					if (_language != "" && KickStarter.speechManager.translateAudio)
					{
						// Not in original language
						fullFilename += _language + "/";
					}
					if (KickStarter.speechManager.placeAudioInSubfolders)
					{
						fullFilename += filename + "/";
					}
					fullFilename += filename + lineID;

					clipObj = Resources.Load (fullFilename) as AudioClip;

					if (clipObj == null && KickStarter.speechManager.fallbackAudio && Options.GetLanguage () > 0)
					{
						fullFilename = "Speech/";
						if (KickStarter.speechManager.placeAudioInSubfolders)
						{
							fullFilename += filename + "/";
						}
						fullFilename += filename + lineID;
						clipObj = Resources.Load (fullFilename) as AudioClip;
					}

					if (clipObj == null)
					{
						ACDebug.Log ("Cannot find audio file: " + fullFilename);
					}
				}
				else
				{
					clipObj = KickStarter.speechManager.GetLineCustomAudioClip (lineID, Options.GetLanguage ());

					if (clipObj == null && KickStarter.speechManager.fallbackAudio && Options.GetLanguage () > 0)
					{
						clipObj = KickStarter.speechManager.GetLineCustomAudioClip (lineID, 0);
					}
				}

				if (clipObj)
				{
					AudioSource audioSource = null;

					if (_speaker != null)
					{
						if (!_noAnimation)
						{
							if (KickStarter.speechManager.lipSyncMode == LipSyncMode.FaceFX)
							{
								FaceFXIntegration.Play (speaker, log.speakerName + lineID, clipObj);
							}
						}

						if (_speaker.speechAudioSource)
						{
							audioSource = _speaker.speechAudioSource;

							if (_speaker.speechAudioSource.GetComponent <Sound>())
							{
								_speaker.speechAudioSource.GetComponent <Sound>().SetVolume (Options.optionsData.speechVolume);
							}
							else
							{
								_speaker.speechAudioSource.volume = Options.optionsData.speechVolume;
							}
						}
						else
						{
							ACDebug.LogWarning (_speaker.name + " has no audio source component!");
						}
					}
					else if (KickStarter.player && KickStarter.player.speechAudioSource)
					{
						KickStarter.player.speechAudioSource.volume = Options.optionsData.speechVolume;
						audioSource = KickStarter.player.speechAudioSource;
					}
					else
					{
						audioSource = KickStarter.dialog.GetDefaultAudioSource ();
					}
					
					if (audioSource != null)
					{
						audioSource.clip = clipObj;
						audioSource.loop = false;
						audioSource.Play();
						hasAudio = true;
					}
					
					displayDuration = clipObj.length;
				}
				else
				{
					displayDuration += KickStarter.speechManager.screenTimeFactor * GetLengthWithoutRichText (_message);
					displayDuration = Mathf.Max (displayDuration, KickStarter.speechManager.minimumDisplayTime);
				}
			}
			else
			{
				displayDuration += KickStarter.speechManager.screenTimeFactor * GetLengthWithoutRichText (_message);
				displayDuration = Mathf.Max (displayDuration, KickStarter.speechManager.minimumDisplayTime);
			}
			
			log.fullText = _message;
			KickStarter.eventManager.Call_OnStartSpeech (_speaker, log.fullText, lineID);

			if (!CanScroll ())
			{
				if (continueIndex > 0)
				{
					continueTime = (continueIndex / KickStarter.speechManager.textScrollSpeed);
				}
				
				if (speechGaps.Count > 0)
				{
					displayText = log.fullText.Substring (0, speechGaps[0].characterIndex);
				}
				else
				{
					displayText = log.fullText;
				}
			}
			else
			{
				displayText = "";
				KickStarter.eventManager.Call_OnStartSpeechScroll (_speaker, log.fullText, lineID);
			}

			isAlive = true;
			isSkippable = true;
			pauseGap = false;
			endTime = displayDuration;
		}


		/**
		 * Updates the state of the line.
		 * This is called every FixedUpdate call by StateHandler.
		 */
		public void _FixedUpdate ()
		{
			if (pauseEndTime > 0f)
			{
				pauseEndTime -= Time.deltaTime;
			}

			if (pauseGap)
			{
				if (!pauseIsIndefinite && pauseEndTime <= 0f)
				{
					EndPause ();
				}
				else
				{
					return;
				}
			}
			else
			{
				if (endTime > 0f)
				{
					endTime -= Time.deltaTime;
				}
			}

			if (CanScroll ())
			{
				if (scrollAmount < 1f)
				{
					if (!pauseGap)
					{
						scrollAmount += KickStarter.speechManager.textScrollSpeed / 100f / log.fullText.Length;
						if (scrollAmount > 1f)
						{
							StopScrolling ();
						}
						
						int currentCharIndex = (int) (scrollAmount * log.fullText.Length);
						if (gapIndex > 0)
						{
							currentCharIndex += speechGaps[gapIndex-1].characterIndex;
							if (currentCharIndex > log.fullText.Length)
							{
								currentCharIndex = log.fullText.Length;
							}
						}

						string newText = GetTextPortion (log.fullText, currentCharIndex);

						if (displayText != newText && !hasAudio)
						{
							KickStarter.dialog.PlayScrollAudio (speaker);
						}
						
						displayText = newText;
						if (gapIndex >= 0 && speechGaps.Count > gapIndex)
						{
							if (currentCharIndex == speechGaps [gapIndex].characterIndex)
							{
								SetPauseGap ();
								return;
							}
						}

						if (continueIndex >= 0)
						{
							if (currentCharIndex >= continueIndex)
							{
								continueIndex = -1;
								continueFromSpeech = true;
							}
						}
					}
					return;
				}
				displayText = log.fullText;
			}
			else
			{
				if (gapIndex >= 0 && speechGaps.Count >= gapIndex)
				{
					if (gapIndex == speechGaps.Count)
					{
						displayText = log.fullText;
					}
					else
					{
						float waitTime = (float) speechGaps[gapIndex].waitTime;
						displayText = log.fullText.Substring (0, speechGaps[gapIndex].characterIndex);
						
						if (waitTime >= 0)
						{
							pauseEndTime = waitTime;
							pauseGap = true;
						}
						else if (speechGaps[gapIndex].expressionID >= 0)
						{
							speaker.SetExpression (speechGaps [gapIndex].expressionID);
							gapIndex ++;
						}
						else
						{
							pauseIsIndefinite = true;
							pauseGap = true;
						}
					}
				}
				else
				{
					displayText = log.fullText;
				}
				
				if (continueIndex >= 0)
				{
					if (continueTime > 0f)
					{
						continueFromSpeech = true;
					}
				}
			}

			if (endTime < 0f)
			{
				if ((KickStarter.speechManager.displayForever && isBackground)
				 || !KickStarter.speechManager.displayForever)
				{
					EndMessage ();
				}
			}
		}


		/**
		 * Ends the current pause.
		 */
		public void EndPause ()
		{
			pauseEndTime = 0f;
			pauseGap = false;
			pauseIsIndefinite = false;
			gapIndex ++;
			scrollAmount = 0f;
		}


		private void EndMessage (bool forceOff = false)
		{
			if (holdForever)
			{
				continueFromSpeech = true;
				return;
			}

			endTime = 0f;
			isSkippable = false;
			
			if (speaker)
			{
				speaker.StopSpeaking ();
			}
			
			if (!forceOff && gapIndex >= 0 && gapIndex < speechGaps.Count)
			{
				gapIndex ++;
			}
			else
			{
				isAlive = false;
				KickStarter.stateHandler.UpdateAllMaxVolumes ();
			}
		}


		private bool SkipSpeechInput ()
		{
			if (KickStarter.playerInput.GetMouseState () == MouseState.SingleClick ||
			    KickStarter.playerInput.GetMouseState () == MouseState.RightClick ||
			    KickStarter.playerInput.InputGetButtonDown ("SkipSpeech"))
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Updates the state of the Speech based on the user's input.
		 * This is called every Update call by StateHandler.</summary>
		 */
		public void UpdateInput ()
		{
			if (isSkippable)
			{
				if (pauseGap && !IsBackgroundSpeech ())
				{
					if (SkipSpeechInput ())
					{
						if (speechGaps[gapIndex].waitTime < 0f)
						{
							KickStarter.playerInput.ResetMouseClick ();
							EndPause ();
						}
						else if (KickStarter.speechManager.allowSpeechSkipping)
						{
							KickStarter.playerInput.ResetMouseClick ();
							EndPause ();
						}
					}
				}
				
				else if (KickStarter.speechManager.displayForever && !IsBackgroundSpeech ())
				{
					if (SkipSpeechInput ())
					{
						KickStarter.playerInput.ResetMouseClick ();
						
						if (KickStarter.stateHandler.gameState == GameState.Cutscene)
						{
							if (KickStarter.speechManager.endScrollBeforeSkip && CanScroll () && displayText != log.fullText)
							{
								// Stop scrolling
								StopScrolling ();

								// Find last non-encountered expression
								if (speechGaps != null && speechGaps.Count > gapIndex)
								{
									for (int i=speechGaps.Count-1; i>=gapIndex; i--)
									{
										if (i >= 0 && speechGaps[i].expressionID >= 0)
										{
											speaker.SetExpression (speechGaps[i].expressionID);
											return;
										}
									}
								}
							}
							else
							{
								// Stop message
								EndMessage (true);
							}
						}
					}
				}
				
				else if ((SkipSpeechInput ()) &&
				         ((KickStarter.speechManager.allowSpeechSkipping && !IsBackgroundSpeech ())
				         || (KickStarter.speechManager.allowSpeechSkipping && KickStarter.speechManager.allowGameplaySpeechSkipping && IsBackgroundSpeech ())
				         || (KickStarter.speechManager.displayForever && KickStarter.speechManager.allowGameplaySpeechSkipping && IsBackgroundSpeech ())))
				{
					KickStarter.playerInput.ResetMouseClick ();
					
					if (KickStarter.stateHandler.gameState == GameState.Cutscene || (KickStarter.speechManager.allowGameplaySpeechSkipping && KickStarter.stateHandler.gameState == GameState.Normal))
					{
						if (KickStarter.speechManager.endScrollBeforeSkip && CanScroll () && displayText != log.fullText)
						{
							// Stop scrolling
							if (speechGaps.Count > 0 && speechGaps.Count > gapIndex)
							{
								while (gapIndex < speechGaps.Count && speechGaps[gapIndex].waitTime >= 0)
								{
									// Find next wait
									gapIndex ++;
								}
								
								if (gapIndex == speechGaps.Count)
								{
									StopScrolling ();
								}
								else
								{
									displayText = log.fullText.Substring (0, speechGaps[gapIndex].characterIndex);
									SetPauseGap ();
								}
							}
							else
							{
								StopScrolling ();
							}
						}
						else
						{
							EndMessage (true);
						}
					}
				}
			}
		}


		private void StopScrolling ()
		{
			scrollAmount = 1f;
			displayText = log.fullText;
			
			if (holdForever)
			{
				EndMessage ();
			}

			// Call event
			KickStarter.eventManager.Call_OnEndSpeechScroll (speaker, log.fullText, log.lineID);
		}


		private void SetPauseGap ()
		{
			float waitTime = speechGaps [gapIndex].waitTime;
			pauseGap = true;
			pauseIsIndefinite = false;
			if (speechGaps [gapIndex].pauseIsIndefinite)
			{
				pauseEndTime = 0f;
				pauseIsIndefinite = true;
			}
			else if (waitTime >= 0f)
			{
				pauseEndTime = waitTime;
			}
			else if (speechGaps [gapIndex].expressionID >= 0)
			{
				pauseEndTime = 0f;
				speaker.SetExpression (speechGaps [gapIndex].expressionID);
			}
			else
			{
				pauseEndTime = 0f;
			}
		}


		private string DetermineGaps (string _text)
		{
			speechGaps.Clear ();
			continueIndex = -1;
			
			if (_text != null)
			{
				if (_text.Contains ("[wait") || _text.Contains ("[expression:"))
				{
					while (_text.Contains ("[wait") || _text.Contains ("[expression:"))
					{
						int startIndex = _text.IndexOf ("[wait");
						int expressionIndex = _text.IndexOf ("[expression:");

						if (speaker != null && expressionIndex >= 0 && (startIndex == -1 || expressionIndex < startIndex))
						{
							// Expression change
							startIndex = expressionIndex;
							int endIndex = _text.IndexOf ("]", startIndex);
							string expressionText = _text.Substring (startIndex + 12, endIndex - startIndex - 12);
							int expressionID = speaker.GetExpressionID (expressionText);
							speechGaps.Add (new SpeechGap (startIndex, expressionID));
							_text = _text.Substring (0, startIndex) + _text.Substring (endIndex + 1); 
						}
						else if (_text.Substring (startIndex).StartsWith ("[wait]"))
						{
							// Indefinite wait
							speechGaps.Add (new SpeechGap (startIndex, true));
							_text = _text.Substring (0, startIndex) + _text.Substring (startIndex + 6);
						}
						else
						{
							// Timed wait
							int endIndex = _text.IndexOf ("]", startIndex);
							string waitTimeText = _text.Substring (startIndex + 6, endIndex - startIndex - 6);
							speechGaps.Add (new SpeechGap (startIndex, FloatParse (waitTimeText)));
							_text = _text.Substring (0, startIndex) + _text.Substring (endIndex + 1); 
						}
					}
				}
				
				if (_text.Contains ("[continue]"))
				{
					continueIndex = _text.IndexOf ("[continue]");
					_text = _text.Replace ("[continue]", "");
				}

				if (_text.Contains ("[hold]"))
				{
					if (continueIndex == -1)
					{
						continueIndex = _text.IndexOf ("[hold]");
					}
					_text = _text.Replace ("[hold]", "");
					holdForever = true;
				}
			}

			// Sort speechGaps
			if (speechGaps.Count > 1)
			{
				speechGaps.Sort (delegate (SpeechGap a, SpeechGap b) {return a.characterIndex.CompareTo (b.characterIndex);});
			}
			
			return _text;
		}


		private float FloatParse (string text)
		{
			float _value = 0f;
			if (text != "")
			{
				float.TryParse (text, out _value);
			}
			return _value;
		}


		private bool IsBackgroundSpeech ()
		{
			if (KickStarter.stateHandler.gameState == GameState.Normal)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Ends speech audio, if it is playing in the background.</summary>
		 * <param name = "newSpeaker">If the line's speaker matches this, the audio will not end</param>
		 */
		public void EndBackgroundSpeechAudio (AC.Char newSpeaker)
		{
			if (isBackground && hasAudio && speaker != null && speaker != newSpeaker)
			{
				if (speaker.speechAudioSource)
				{
					speaker.speechAudioSource.Stop ();
				}
			}
		}


		/**
		 * <summary>Gets the display name of the speaking character.</summary>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display name of the speaking character</returns>
		 */
		public string GetSpeaker (int languageNumber = 0)
		{
			if (speaker)
			{
				return speaker.GetName (languageNumber);
			}
			
			return "";
		}


		/**
		 * <summary>Gets the colour of the subtitle text.</summary>
		 * <returns>The colour of the subtitle text</returns>
		 */
		public Color GetColour ()
		{
			if (speaker)
			{
				return speaker.speechColor;
			}
			return Color.white;
		}
		

		/**
		 * <summary>Gets the speaking character.</summary>
		 * <returns>The speaking character</returns>
		 */
		public AC.Char GetSpeakingCharacter ()
		{
			return speaker;
		}


		/**
		 * <summary>Checks if the speech line is temporarily paused, due to a [wait] or [wait:X] token.</summary>
		 * <returns>True if the speech line is temporarily paused.</returns>
		 */
		public bool IsPaused ()
		{
			return pauseGap;
		}


		/**
		 * <summary>Checks if the line has any pause gaps.</summary>
		 * <returns>True if there are any line gaps</returns>
		 */
		public bool HasPausing ()
		{
			if (speechGaps.Count > 0)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Gets a Sprite based on the portrait graphic of the speaking character.
		 * If lipsincing is enabled, the sprite will be based on the current phoneme.</summary>
		 * <returns>The speaking character's portrait sprite</returns>
		 */
		public UnityEngine.Sprite GetPortraitSprite ()
		{
			if (speaker != null)
			{
				CursorIconBase portraitIcon = speaker.GetPortrait ();
				if (portraitIcon != null && portraitIcon.texture != null)
				{
					if (IsAnimating ())
					{
						if (speaker.isLipSyncing)
						{
							return portraitIcon.GetAnimatedSprite (speaker.GetLipSyncFrame ());
						}
						else
						{
							return portraitIcon.GetAnimatedSprite (true);
						}
					}
					else
					{
						return portraitIcon.GetSprite ();
					}
				}
			}
			return null;
		}
		

		/**
		 * <summary>Gets the portrait graphic of the speaking character.</summary>
		 * <returns>The speaking character's portrait graphic</returns>
		 */
		public Texture2D GetPortrait ()
		{
			if (speaker && speaker.GetPortrait ().texture)
			{
				return speaker.GetPortrait ().texture;
			}
			return null;
		}
		

		/**
		 * <summary>Checks if the speaking character's portrait graphic can be animated.</summary>
		 * <returns>True if the character's portrait graphic can be animated</returns>
		 */
		public bool IsAnimating ()
		{
			if (speaker && speaker.GetPortrait ().isAnimated)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Gets a Rect of the character's portrait graphic.
		 * If the graphic is animating, only the relevant portion will be returned.</summary>
		 * <returns>A Rect of the character's portrait graphic</returns>
		 */
		public Rect GetAnimatedRect ()
		{
			if (speaker != null && speaker.GetPortrait () != null)
			{
				if (speaker.isLipSyncing)
				{
					return speaker.GetPortrait ().GetAnimatedRect (speaker.GetLipSyncFrame ());
				}
				else if (speaker.isTalking)
				{
					return speaker.GetPortrait ().GetAnimatedRect ();
				}
				else
				{
					return speaker.GetPortrait ().GetAnimatedRect (0);
				}
			}
			return new Rect (0,0,0,0);
		}


		private bool CanScroll ()
		{
			if (speaker == null)
			{
				return KickStarter.speechManager.scrollNarration;
			}
			return KickStarter.speechManager.scrollSubtitles;
		}


		/**
		 * <summary>Checks if a Menu is able to show this speech line.</summary>
		 * <param name = "menu">The Menu to check against</param>
		 * <returns>True if the Menu is able to show this speech line</returns>
		 */
		public bool MenuCanShow (Menu menu)
		{
			if (menu.speechMenuLimit == SpeechMenuLimit.All ||
			    (menu.speechMenuLimit == SpeechMenuLimit.BlockingOnly && !isBackground) ||
			    (menu.speechMenuLimit == SpeechMenuLimit.BackgroundOnly && isBackground))
			{
				if (menu.speechMenuType == SpeechMenuType.All ||
				    (menu.speechMenuType == SpeechMenuType.CharactersOnly && speaker != null) ||
				    (menu.speechMenuType == SpeechMenuType.NarrationOnly && speaker == null) ||
				    (menu.speechMenuType == SpeechMenuType.SpecificCharactersOnly &&
				 		(menu.limitToCharacters.Contains (GetSpeaker ()) || (menu.limitToCharacters.Contains ("Player") && GetSpeakingCharacter () != null && GetSpeakingCharacter () is Player))
				 	)
				   )
				{
					return true;
				}
			}
			    
			return false;
		}


		private string GetTextPortion (string fullText, int originalIndex)
		{
			int newIndex = originalIndex;

			if (fullText.Length < newIndex)
			{
				return "";
			}
			else if (fullText.Length == newIndex)
			{
				return fullText.Substring (0, newIndex);
			}

			bool hasTag = (fullText[newIndex].ToString () == "<") ? true : false;
			if (!hasTag && boldTagIndex == -1 && italicTagIndex == -1 && sizeTagIndex == -1 && colorTagIndex == -1)
			{
				// No rich tags right now, so don't do anything complicated
				return fullText.Substring (0, newIndex);
			}

			string stringFromIndex = fullText.Substring (newIndex);

			if (hasTag)
			{
				// Check for opening
				if (stringFromIndex.StartsWith (openBoldTag))
				{
					boldTagIndex = stringFromIndex.IndexOf (closeBoldTag) + newIndex;
					newIndex += openBoldTag.Length;
					closingTags = closeBoldTag + closingTags;
				}
				else if (stringFromIndex.StartsWith (openItalicTag))
				{
					italicTagIndex = stringFromIndex.IndexOf (closeItalicTag) + italicTagIndex;
					newIndex += openItalicTag.Length;
					closingTags = closeItalicTag + closingTags;
				}
				else if (stringFromIndex.StartsWith (openSizeTag))
				{
					sizeTagIndex = stringFromIndex.IndexOf (closeSizeTag) + sizeTagIndex;
					int closeBracketIndex = stringFromIndex.IndexOf (">") + 1;
					newIndex += closeBracketIndex;
					closingTags = closeSizeTag + closingTags;
				}
				else if (stringFromIndex.StartsWith (openColorTag))
				{
					colorTagIndex = stringFromIndex.IndexOf (closeColorTag) + colorTagIndex;
					int closeBracketIndex = stringFromIndex.IndexOf (">") + 1;
					newIndex += closeBracketIndex;
					closingTags = closeColorTag + closingTags;
				}

				// Check for closing
				if (stringFromIndex.StartsWith (closeBoldTag))
				{
					boldTagIndex = -1;
					newIndex += closeBoldTag.Length;
					closingTags = closingTags.Replace (closeBoldTag, "");
				}
				else if (stringFromIndex.StartsWith (closeItalicTag))
				{
					italicTagIndex = -1;
					newIndex += closeItalicTag.Length;
					closingTags = closingTags.Replace (closeItalicTag, "");
				}
				else if (stringFromIndex.StartsWith (closeSizeTag))
				{
					sizeTagIndex = -1;
					newIndex += closeSizeTag.Length;
					closingTags = closingTags.Replace (closeSizeTag, "");
				}
				else if (stringFromIndex.StartsWith (closeColorTag))
				{
					colorTagIndex = -1;
					newIndex += closeColorTag.Length;
					closingTags = closingTags.Replace (closeColorTag, "");
				}

				// Modifyied currentCharIndex, so convert back to scrollAmount
				// NOTE! Only increase, don't set explicitly - otherwise errors with wait tokens
				scrollAmount += (float) (newIndex - originalIndex) / (float) fullText.Length;
			}

			return fullText.Substring (0, newIndex) + closingTags;
		}


		private float GetLengthWithoutRichText (string _message)
		{
			_message = _message.Replace (openBoldTag, "");
			_message = _message.Replace (openItalicTag, "");
			_message = _message.Replace (openSizeTag, "");
			_message = _message.Replace (openColorTag, "");

			_message = _message.Replace (closeBoldTag, "");
			_message = _message.Replace (closeItalicTag, "");
			_message = _message.Replace (closeSizeTag, "");
			_message = _message.Replace (closeColorTag, "");

			return (float) _message.Length;
		}


	}


	/**
	 * A data struct for an entry in the game's speech log.
	 */
	public struct SpeechLog
	{

		/** The full display text of the line */
		public string fullText;
		/** The display name of the speaking character */
		public string speakerName;
		/** The ID number of the line, as set by the Speech Manager */
		public int lineID;


		/**
		 * Clears the struct.
		 */
		public void Clear ()
		{
			fullText = "";
			speakerName = "";
			lineID = -1;
		}

	}


	/**
	 * A data container for an label a 'Dialogue: Play speech' Action can be tagged as
	 */
	[System.Serializable]
	public class SpeechTag
	{
		
		/** A unique identified */
		public int ID;
		/** The tag's text */
		public string label;
		
		
		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "idArray">An array of already-used ID numbers, so that a unique one can be generated</param>
		 */
		public SpeechTag (int[] idArray)
		{
			ID = 0;
			label = "";
			
			// Update id based on array
			if (idArray != null && idArray.Length > 0)
			{
				foreach (int _id in idArray)
				{
					if (ID == _id)
						ID ++;
				}
			}
		}


		/**
		 * <summary>A Constructor for the first SpeechTag.</summary>
		 * <param name = "_label">The SpeechTag's label</param>
		 */
		public SpeechTag (string _label)
		{
			ID = 0;
			label = _label;
		}
		
	}

}