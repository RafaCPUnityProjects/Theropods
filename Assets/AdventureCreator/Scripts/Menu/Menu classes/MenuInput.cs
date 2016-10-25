/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInput.cs"
 * 
 *	This MenuElement acts like a label, whose text can be changed with keyboard input.
 * 
 */

using UnityEngine;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuInput : MenuElement
{
	
	public string label = "Element";
	public TextAnchor anchor;
	public bool doOutline;
	public AC_InputType inputType;
	public int characterLimit = 10;

	private float lastInputTime = 0f;

	
	public override void Declare ()
	{
		label = "Input";
		isVisible = true;
		isClickable = true;
		numSlots = 1;
		anchor = TextAnchor.MiddleCenter;
		SetSize (new Vector2 (10f, 5f));
		inputType = AC_InputType.AlphaNumeric;
		characterLimit = 10;
		lastInputTime = 0f;
		
		base.Declare ();
	}
	
	
	public void CopyInput (MenuInput _element)
	{
		label = _element.label;
		anchor = _element.anchor;
		doOutline = _element.doOutline;
		inputType = _element.inputType;
		characterLimit = _element.characterLimit;
		
		base.Copy (_element);
	}
	
	
	#if UNITY_EDITOR
	
	public override void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
		label = EditorGUILayout.TextField ("Default text:", label);
		inputType = (AC_InputType) EditorGUILayout.EnumPopup ("Input type:", inputType);
		characterLimit = EditorGUILayout.IntSlider ("Character limit:", characterLimit, 1, 50);
		anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
		doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
		EditorGUILayout.EndVertical ();
		
		base.ShowGUI ();
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
		
		if (doOutline)
		{
			AdvGame.DrawTextOutline (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style, Color.black, _style.normal.textColor, 2);
		}
		else
		{
			GUI.Label (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style);
		}
	}


	public void CheckForInput (string input)
	{
		if (Time.time > lastInputTime + 0.1f)
		{
			lastInputTime = Time.time;

			if (input == "Backspace")
			{
				label = label.Substring (0, label.Length - 1);
			}
			else if (input != "None")
			{
				if (input.Contains ("Alpha") || inputType == AC_InputType.AlphaNumeric)
				{
					input = input.Replace ("Alpha", "");
					if (characterLimit == 1)
					{
						label = input;
					}
					else if (label.Length < characterLimit)
					{
						label += input;
					}
				}
			}
		}
	}

	
	protected override void AutoSize ()
	{
		GUIContent content = new GUIContent (TranslateLabel (label));
		AutoSize (content);
	}
	
}