/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuElement.cs"
 * 
 *	This is the base class for all menu elements.  It should never
 *	be added itself to a menu, as it is only a container of shared data.
 * 
 */

using UnityEngine;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MenuElement : ScriptableObject
{
	public int ID;
	public bool isEditing = false;
	public string title = "Element";
	public Vector2 slotSize;
	public AC_SizeType sizeType;
	public AC_PositionType2 positionType;
	public int lineID = -1;

	public Font font;
	public float fontScaleFactor = 60f;
	public Color fontColor = Color.white;
	public Color fontHighlightColor = Color.white;
	public Texture2D highlightTexture;
	
	public bool isVisible;
	public bool isClickable;
	public ElementOrientation orientation = ElementOrientation.Vertical;
	public int gridWidth = 3;

	public Texture2D backgroundTexture;
	
	[SerializeField] protected Rect relativeRect;
	[SerializeField] protected int numSlots;
	

	public virtual void Declare ()
	{
		fontScaleFactor = 2f;
		fontColor = Color.white;
		fontHighlightColor = Color.white;
		highlightTexture = null;
		orientation = ElementOrientation.Vertical;
		positionType = AC_PositionType2.Aligned;
		sizeType = AC_SizeType.Automatic;
		gridWidth = 3;
		lineID = -1;
	}
	
	
	public virtual void Copy (MenuElement _element)
	{
		ID = _element.ID;
		isEditing = false;
		title = _element.title;
		slotSize = _element.slotSize;
		sizeType = _element.sizeType;
		positionType = _element.positionType;
		relativeRect = _element.relativeRect;
		numSlots = _element.numSlots;
		lineID = _element.lineID;
	
		font = _element.font;
		fontScaleFactor = _element.fontScaleFactor;
		fontColor = _element.fontColor;
		fontHighlightColor = _element.fontHighlightColor;
		highlightTexture = _element.highlightTexture;
		
		isVisible = _element.isVisible;
		isClickable = _element.isClickable;
		orientation = _element.orientation;
		gridWidth = _element.gridWidth;

		backgroundTexture = _element.backgroundTexture;
	}


	protected string TranslateLabel (string label)
	{
		if (Options.GetLanguage () > 0 && lineID > -1)
		{
			return (SpeechManager.GetTranslation (lineID, Options.GetLanguage ()));
		}
		else
		{
			return (label);
		}
	}

	
	#if UNITY_EDITOR
	
	public void ShowGUIStart ()
	{
		EditorGUILayout.BeginVertical ("Button");
			title = EditorGUILayout.TextField ("Element name:", title);
			isVisible = EditorGUILayout.Toggle ("Is visible?", isVisible);
		EditorGUILayout.EndVertical ();
		
		ShowGUI ();
	}
	
	
	public virtual void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			font = (Font) EditorGUILayout.ObjectField ("Font:", font, typeof (Font), false);
			fontScaleFactor = EditorGUILayout.Slider ("Font size:", fontScaleFactor, 1f, 4f);
			fontColor = EditorGUILayout.ColorField ("Font colour:", fontColor);
			fontHighlightColor = EditorGUILayout.ColorField ("Font colour (highlighted):", fontHighlightColor);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			if (isClickable)
			{
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Highlight texture:", GUILayout.Width (145f));
					highlightTexture = (Texture2D) EditorGUILayout.ObjectField (highlightTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			
			EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Background texture:", GUILayout.Width (145f));
				backgroundTexture = (Texture2D) EditorGUILayout.ObjectField (backgroundTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
			EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
		
		EndGUI ();
	}
	
	
	public void EndGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			positionType = (AC_PositionType2) EditorGUILayout.EnumPopup ("Position:", positionType);
			if (positionType == AC_PositionType2.Manual)
			{
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("X:", GUILayout.Width (15f));
					relativeRect.x = EditorGUILayout.FloatField (relativeRect.x);
					EditorGUILayout.LabelField ("Y:", GUILayout.Width (15f));
					relativeRect.y = EditorGUILayout.FloatField (relativeRect.y);
				EditorGUILayout.EndHorizontal ();
			}
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			sizeType = (AC_SizeType) EditorGUILayout.EnumPopup ("Size:", sizeType);
			if (sizeType == AC_SizeType.Manual)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("W:", GUILayout.Width (15f));
				slotSize.x = EditorGUILayout.Slider (slotSize.x, 0f, 100f);
				EditorGUILayout.LabelField ("H:", GUILayout.Width (15f));
				slotSize.y = EditorGUILayout.Slider (slotSize.y, 0f, 100f);
				EditorGUILayout.EndHorizontal ();
			}
		EditorGUILayout.EndVertical ();
	}
	
	
	protected void ShowClipHelp ()
	{
		EditorGUILayout.HelpBox ("MenuSystem.OnElementClick will be run when this element is clicked.", MessageType.Info);
	}
	
	#endif


	public virtual void Display (GUIStyle _style, int _slot, float zoom)
	{
		if (backgroundTexture && _slot == 0)
		{
			GUI.DrawTexture (ZoomRect (relativeRect, zoom), backgroundTexture, ScaleMode.StretchToFill, true, 0f);
		}
	}


	protected virtual Rect ZoomRect (Rect rect, float zoom)
	{
		if (zoom == 1f)
		{
			return rect;
		}

		return (new Rect (rect.x * zoom, rect.y * zoom, rect.width * zoom, rect.height * zoom));
	}
	
	
	public Vector2 GetSize ()
	{
		Vector2 size = new Vector2 (relativeRect.width, relativeRect.height);
		return (size);
	}
	
	
	public Vector2 GetSizeFromCorner ()
	{
		Vector2 size = new Vector2 (relativeRect.width + relativeRect.x, relativeRect.height + relativeRect.y);
		return (size);
	}
	
	
	public void SetSize (Vector2 _size)
	{
		slotSize = new Vector2 (_size.x, _size.y);
	}
	
	
	protected void SetAbsoluteSize (Vector2 _size)
	{
		slotSize = new Vector2 (_size.x * 100f / AdvGame.GetMainGameViewSize ().x, _size.y * 100f / AdvGame.GetMainGameViewSize ().y);
	}
	
	
	public int GetNumSlots ()
	{
		return numSlots;
	}
	
	
	public Rect GetSlotRectRelative (int _slot)
	{
		Rect positionRect = relativeRect;
		positionRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x;
		positionRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y;
		
		if (_slot > numSlots)
		{
			_slot = numSlots;
		}
		
		if (orientation == ElementOrientation.Horizontal)
		{
			positionRect.x += slotSize.x / 100f * _slot * AdvGame.GetMainGameViewSize ().x;
		}
		else if (orientation == ElementOrientation.Vertical)
		{
			positionRect.y += slotSize.y / 100f * _slot * AdvGame.GetMainGameViewSize ().y;
		}
		else if (orientation == ElementOrientation.Grid)
		{
			int xOffset = _slot + 1;
			float numRows = Mathf.CeilToInt ((float) xOffset / gridWidth) - 1;
			while (xOffset > gridWidth)
			{
				xOffset -= gridWidth;
			}
			xOffset -= 1;

			positionRect.x += slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x * (float) xOffset;
			positionRect.y += slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y * numRows;
		}
		
		return (positionRect);
	}
	
	
	public virtual void RecalculateSize ()
	{
		if (sizeType == AC_SizeType.Automatic)
		{
			AutoSize ();
		}
		
		if (orientation == ElementOrientation.Horizontal)
		{
			relativeRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x * numSlots;
			relativeRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y;
		}
		else if (orientation == ElementOrientation.Vertical)
		{
			relativeRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x;
			relativeRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y * numSlots;
		}
		else if (orientation == ElementOrientation.Grid)
		{
			if (numSlots < gridWidth)
			{
				relativeRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x * numSlots;
				relativeRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y;
			}
			else
			{
				float numRows = Mathf.CeilToInt ((float) numSlots / gridWidth);

				relativeRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x * gridWidth;
				relativeRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y * numRows;
			}
		}
	}
	
	
	protected void AutoSize (GUIContent content)
	{
		GUIStyle normalStyle = new GUIStyle();
		normalStyle.font = font;
		normalStyle.fontSize = (int) (AdvGame.GetMainGameViewSize ().x * fontScaleFactor / 100);
	
		Vector2 size = GetSize ();
		size = normalStyle.CalcSize (content);
		
		SetAbsoluteSize (size);
	}
	
	
	protected virtual void AutoSize ()
	{
		GUIContent content = new GUIContent (backgroundTexture);
		AutoSize (content);
	}
	
	
	public void SetPosition (Vector2 _position)
	{
		relativeRect.x = _position.x;
		relativeRect.y = _position.y;
	}
	
	
	public void AutoSetVisibility ()
	{
		if (numSlots == 0)
		{
			isVisible = false;
		}
		else
		{
			isVisible = true;
		}
	}

}