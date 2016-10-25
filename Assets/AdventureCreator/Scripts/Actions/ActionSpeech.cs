/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSpeech.cs"
 * 
 *	This action handles the displaying of messages, and talking of characters.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionSpeech : Action
{
	
	public bool isPlayer;
	public Char speaker;
	public string messageText;
	public int lineID;
	public bool isBackground = false;
	public AnimationClip headClip;
	public AnimationClip mouthClip;

	public bool play2DHeadAnim = false;
	public string headClip2D = "";
	public int headLayer;
	public bool play2DMouthAnim = false;
	public string mouthClip2D = "";
	public int mouthLayer;
	
	private Dialog dialog;
	private StateHandler stateHandler;
	private SpeechManager speechManager;

	
	public ActionSpeech ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Play speech";
		lineID = -1;
	}
	

	override public float Run ()
	{
		dialog = GameObject.FindWithTag(Tags.gameEngine).GetComponent <Dialog>();
		stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

		if (dialog && stateHandler)
		{
			if (!isRunning)
			{
				isRunning = true;
				
				string _text = messageText;
				string _language = "";
				
				if (Options.GetLanguage () > 0)
				{
					// Not in original language, so pull translation in from Speech Manager
					if (!speechManager)
					{
						speechManager = AdvGame.GetReferences ().speechManager;
					}
					
					_text = SpeechManager.GetTranslation (lineID, Options.GetLanguage ());
					_language = speechManager.languages [Options.GetLanguage ()];
				}

				_text = ConvertTokens (_text);
				
				if (_text != "")
				{
					dialog.KillDialog ();
					
					if (isPlayer)
					{
						speaker = GameObject.FindWithTag(Tags.player).GetComponent <Player>();
					}
					
					if (speaker)
					{
						dialog.StartDialog (speaker, _text, lineID, _language);

						if (speaker.animEngine == null)
						{
							speaker.ResetAnimationEngine ();
						}
						
						if (speaker.animEngine != null)
						{
							speaker.animEngine.ActionSpeechRun (this);
						}
					}
					else
					{
						dialog.StartDialog (_text);
					}
					
					if (!isBackground)
					{
						return defaultPauseTime;
					}
				}
	
				return 0f;
			}
			else
			{
				if (!dialog.isMessageAlive)
				{
					isRunning = false;
					stateHandler.gameState = GameState.Cutscene;
					return 0f;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}
		
		return 0f;
	}
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (lineID > -1)
		{
			EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
		}
		
		isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				speaker = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
			}
			else
			{
				speaker = AdvGame.GetReferences ().settingsManager.player;
			}
		}
		else
		{
			speaker = (Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(Char), true);
		}

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Line text:", GUILayout.Width (145f));
		EditorStyles.textField.wordWrap = true;
		messageText = EditorGUILayout.TextArea (messageText);
		EditorGUILayout.EndHorizontal ();
		
		if (speaker)
		{
			if (speaker.animEngine == null)
			{
				speaker.ResetAnimationEngine ();
			}
			if (speaker.animEngine)
			{
				speaker.animEngine.ActionSpeechGUI (this);
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
		}
		
		isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);

		AfterRunningOption ();
	}


	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (messageText != "")
		{
			string shortMessage = messageText;
			if (shortMessage != null)
			{
				if (shortMessage.Contains ("\n"))
				{
					shortMessage = shortMessage.Replace ("\n", "");
				}
				if (shortMessage.Length > 30)
				{
					shortMessage = shortMessage.Substring (0, 28) + "..";
				}
			}

			labelAdd = " (" + shortMessage + ")";
		}
		
		return labelAdd;
	}

	#endif

	private string ConvertTokens (string _text)
	{
		if (_text.Contains ("[var:"))
		{
			RuntimeVariables runtimeVariables = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>();
			foreach (GVar _var in runtimeVariables.localVars)
			{
				string tokenText = "[var:" + _var.id + "]";
				if (_text.Contains (tokenText))
				{
					_text = _text.Replace (tokenText, runtimeVariables.GetVarValue (_var.id));
				}
			}
		}

		return _text;
	}

}