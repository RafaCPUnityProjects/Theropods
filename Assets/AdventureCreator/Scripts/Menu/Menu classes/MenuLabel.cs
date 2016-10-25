/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuLabel.cs"
 * 
 *	This MenuElement provides a basic label.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuLabel : MenuElement
{
	
	public string label = "Element";
	public TextAnchor anchor;
	public bool doOutline;
	public AC_LabelType labelType;

	public int variableID;
	public int variableNumber;

	private string newLabel = "";

	
	public override void Declare ()
	{
		label = "Label";
		isVisible = true;
		isClickable = false;
		numSlots = 1;
		anchor = TextAnchor.MiddleCenter;
		SetSize (new Vector2 (10f, 5f));
		labelType = AC_LabelType.Normal;
		variableID = 0;
		variableNumber = 0;
		
		base.Declare ();
	}
	
	
	public void CopyLabel (MenuLabel _element)
	{
		label = _element.label;
		anchor = _element.anchor;
		doOutline = _element.doOutline;
		labelType = _element.labelType;
		variableID = _element.variableID;
		variableNumber = _element.variableNumber;
		
		base.Copy (_element);
	}
	
	
	#if UNITY_EDITOR
	
	public override void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Label text:", label);
			labelType = (AC_LabelType) EditorGUILayout.EnumPopup ("Label type:", labelType);

			if (labelType == AC_LabelType.Variable)
			{
				VariableGUI ();
			}

			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
		EditorGUILayout.EndVertical ();

		base.ShowGUI ();
	}


	private void VariableGUI ()
	{
		if (AdvGame.GetReferences ().variablesManager)
		{
			VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;

			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			variableNumber = -1;
			
			if (variablesManager.vars.Count > 0)
			{
				foreach (GVar _var in variablesManager.vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i++;
				}
				
				if (variableNumber == -1)
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}
				
				variableNumber = EditorGUILayout.Popup (variableNumber, labelList.ToArray());
				variableID = variablesManager.vars[variableNumber].id;
			}
			else
			{
				EditorGUILayout.HelpBox ("No global variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("No Variables Manager exists!", MessageType.Info);
			variableID = -1;
			variableNumber = -1;
		}
	}
	
	#endif
	
	
	public override void Display (GUIStyle _style, int _slot, float zoom)
	{
		base.Display (_style, _slot, zoom);
		
		_style.wordWrap = true;
		_style.alignment = anchor;
		if (zoom < 1f)
		{
			_style.fontSize = (int) ((float) _style.fontSize * zoom);
		}

		if (Application.isPlaying)
		{
			if (labelType == AC_LabelType.Hotspot)
			{
				if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>())
				{
					PlayerMenus playerMenus = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerMenus>();
					if (playerMenus.GetHotspotLabel () != "")
					{
						newLabel = playerMenus.GetHotspotLabel ();
					}
				}
			}
			else if (labelType == AC_LabelType.Normal)
			{
				newLabel = TranslateLabel (label);
			}
			else if (labelType == AC_LabelType.Variable)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>())
				{
					newLabel = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>().GetVarValue (variableID);
				}
			}
			else if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>())
			{
				Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
				if (labelType == AC_LabelType.DialogueLine)
				{
					if (dialog.GetLine () != "")
					{
						newLabel = dialog.GetLine ();
					
						if (sizeType == AC_SizeType.Manual)
						{
							GUIContent content = new GUIContent (newLabel);
							relativeRect.height = _style.CalcHeight (content, relativeRect.width);
						}
					}
				}
				else if (labelType == AC_LabelType.DialogueSpeaker)
				{
					newLabel = dialog.GetSpeaker ();
				}
				else if (labelType == AC_LabelType.DialoguePortrait)
				{
					backgroundTexture = dialog.GetPortrait ();
					
					if (backgroundTexture == null)
					{
						newLabel = dialog.GetSpeaker ();
					}
					else
					{
						newLabel = "";
					}
				}
			}
		}
		else
		{
			newLabel = label;
		}
		
		if (doOutline)
		{
			AdvGame.DrawTextOutline (ZoomRect (relativeRect, zoom), newLabel, _style, Color.black, _style.normal.textColor, 2);
		}
		else
		{
			GUI.Label (ZoomRect (relativeRect, zoom), newLabel, _style);
		}
	}
	
	
	protected override void AutoSize ()
	{
		if (labelType == AC_LabelType.DialogueLine)
		{
			GUIContent content = new GUIContent (TranslateLabel (label));

			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				AutoSize (content);
				return;
			}
			#endif

			GUIStyle normalStyle = new GUIStyle();
			normalStyle.font = font;
			normalStyle.fontSize = (int) (AdvGame.GetMainGameViewSize ().x * fontScaleFactor / 100);
			Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
			string line = " " + dialog.GetLine () + " ";
			if (line.Length > 40)
			{
				line = line.Insert (line.Length / 2, " \n ");
			}
			content = new GUIContent (line);
			AutoSize (content);
		}

		else if (label == "" && backgroundTexture != null)
		{
			GUIContent content = new GUIContent (backgroundTexture);
			AutoSize (content);
		}
		else
		{
			GUIContent content = new GUIContent (TranslateLabel (label));
			AutoSize (content);
		}
	}
	
}