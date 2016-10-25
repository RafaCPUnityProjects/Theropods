/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCycle.cs"
 * 
 *	This MenuElement is like a label, only it's text cycles through an array when clicked on.
 * 
 */

using UnityEngine;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuCycle : MenuElement
{

	public string label = "Element";
	public bool doOutline;
	public TextAnchor anchor;
	public int selected;
	public string[] optionsArray;
	public AC_CycleType cycleType;
	
	
	public override void Declare ()
	{
		label = "Cycle";
		selected = 0;
		isVisible = true;
		isClickable = true;
		numSlots = 1;
		SetSize (new Vector2 (15f, 5f));
		anchor = TextAnchor.MiddleLeft;
		cycleType = AC_CycleType.CustomScript;
		
		base.Declare ();
	}
	
	
	public void CopyCycle (MenuCycle _element)
	{
		label = _element.label;
		doOutline = _element.doOutline;
		anchor = _element.anchor;
		selected = _element.selected;
		optionsArray = _element.optionsArray;
		cycleType = _element.cycleType;
				
		base.Copy (_element);
	}
	
	
	#if UNITY_EDITOR
	
	public override void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			label = EditorGUILayout.TextField ("Label text:", label);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
			cycleType = (AC_CycleType) EditorGUILayout.EnumPopup ("Cycle type:", cycleType);
			if (cycleType == AC_CycleType.CustomScript)
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
		
		if (Application.isPlaying)
		{
			if (optionsArray.Length > selected && selected > -1)
			{
				toggleText += optionsArray [selected];
			}
			else
			{
				Debug.Log ("Could not gather options options for MenuCycle " + label);
				selected = 0;
			}
		}
		else
		{
			toggleText += "Default option";	
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
	
	
	public void Cycle ()
	{
		selected ++;
		if (selected > optionsArray.Length-1)
		{
			selected = 0;
		}
		
		if (cycleType == AC_CycleType.Language)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{
				Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
				
				options.optionsData.language = selected;
				options.SavePrefs ();
			}
			else
			{
				Debug.LogWarning ("Could not find Options data!");
			}
		}
	}
	
	
	public override void RecalculateSize ()
	{
		if (Application.isPlaying && cycleType == AC_CycleType.Language)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager && GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{	
				SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
				Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
				
				optionsArray = speechManager.languages.ToArray ();
				selected = options.optionsData.language;
			}
		}
		
		base.RecalculateSize ();
	}
	
	
	protected override void AutoSize ()
	{
		AutoSize (new GUIContent (TranslateLabel (label) + " : Default option"));
	}
	
}