/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuToggle.cs"
 * 
 *	This MenuElement toggles between On and Off when clicked on.
 *	It can be used for changing boolean options.
 * 
 */

using UnityEngine;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuToggle : MenuElement
{

	public string label;
	public bool isOn;
	public bool doOutline;
	public TextAnchor anchor;
	public AC_ToggleType toggleType;
	
	
	public override void Declare ()
	{
		label = "Toggle";
		isOn = false;
		isVisible = true;
		isClickable = true;
		toggleType = AC_ToggleType.CustomScript;
		numSlots = 1;
		SetSize (new Vector2 (15f, 5f));
		anchor = TextAnchor.MiddleLeft;
		
		base.Declare ();
	}
	
	
	public void CopyToggle (MenuToggle _element)
	{
		label = _element.label;
		isOn = _element.isOn;
		doOutline = _element.doOutline;
		anchor = _element.anchor;
		toggleType = _element.toggleType;
		
		base.Copy (_element);
	}
	
	
	#if UNITY_EDITOR
	
	public override void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Label text:", label);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
			isOn = EditorGUILayout.Toggle ("Is on?", isOn);
		
			toggleType = (AC_ToggleType) EditorGUILayout.EnumPopup ("Toggle type:", toggleType);
			if (toggleType == AC_ToggleType.CustomScript)
			{
				ShowClipHelp ();
			}
		EditorGUILayout.EndVertical ();
		
		base.ShowGUI ();
	}
	
	#endif
	
	
	public override void Display (GUIStyle _style, int _slot, float zoom)
	{
		base.Display (_style, _slot, zoom);
		
		_style.alignment = anchor;
		if (zoom < 1f)
		{
			_style.fontSize = (int) ((float) _style.fontSize * zoom);
		}
		
		string toggleText = TranslateLabel (label) + " : ";
		if (isOn)
		{
			toggleText += "On";
		}
		else
		{
			toggleText += "Off";
		}
		
		if (doOutline)
		{
			AdvGame.DrawTextOutline (ZoomRect (relativeRect, zoom), toggleText, _style, Color.black, _style.normal.textColor, 2);
		}
		else
		{
			GUI.Label (ZoomRect (relativeRect, zoom), toggleText, _style);
		}
	}
	
	
	public void Toggle ()
	{
		if (isOn)
		{
			isOn = false;
		}
		else
		{
			isOn = true;
		}
		
		if (toggleType == AC_ToggleType.Subtitles)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{
				Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
				
				options.optionsData.showSubtitles = isOn;
				options.SavePrefs ();
			}
		}
	}
	
	
	public override void RecalculateSize ()
	{
		if (Application.isPlaying && toggleType == AC_ToggleType.Subtitles)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{	
				isOn = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().optionsData.showSubtitles;
			}
		}
		
		base.RecalculateSize ();
	}
	
	
	protected override void AutoSize ()
	{
		AutoSize (new GUIContent (TranslateLabel (label) + " : Off"));
	}

}