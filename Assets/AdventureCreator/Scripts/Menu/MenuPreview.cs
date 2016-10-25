using UnityEngine;
using System.Collections;
using AC;

[ExecuteInEditMode]
public class MenuPreview : MonoBehaviour
{
	
	private MenuManager menuManager;
	private GUIStyle normalStyle;
	private GUIStyle highlightedStyle;

	
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
	
	
	private void OnGUI ()
	{
		if (!Application.isPlaying)
		{
			if (AdvGame.GetReferences ())
			{
				menuManager = AdvGame.GetReferences ().menuManager;
				
				if (menuManager && menuManager.drawInEditor)
				{
					if (menuManager.GetSelectedMenu () != null)
					{
						Menu menu = menuManager.GetSelectedMenu ();

						if ((menu.appearType == AppearType.Manual || menu.appearType == AppearType.OnInputKey) && menu.pauseWhenEnabled && menuManager.pauseTexture)
						{
							GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), menuManager.pauseTexture, ScaleMode.ScaleToFit, true, 0f);
						}
						
						if ((menu.positionType == AC_PositionType.FollowCursor || menu.positionType == AC_PositionType.AppearAtCursorAndFreeze || menu.positionType == AC_PositionType.OnHotspot || menu.positionType == AC_PositionType.AboveSpeakingCharacter) && AdvGame.GetReferences ().cursorManager && AdvGame.GetReferences ().cursorManager.pointerTexture)
						{
							GUI.DrawTexture (AdvGame.GUIBox (new Vector2 (AdvGame.GetMainGameViewSize ().x / 2f, AdvGame.GetMainGameViewSize ().y / 2f), AdvGame.GetReferences ().cursorManager.normalCursorSize), AdvGame.GetReferences ().cursorManager.pointerTexture, ScaleMode.ScaleToFit, true, 0f);
						}
						
						menu.StartDisplay ();
										
						foreach (MenuElement element in menu.visibleElements)
						{
							SetStyles (element);
							
							for (int i=0; i<element.GetNumSlots (); i++)
							{
								if (menuManager.GetSelectedElement () == element && element.isClickable && i == 0)
								{
									element.Display (highlightedStyle, i, 1f);
								}
								
								else
								{
									element.Display (normalStyle, i, 1f);
								}
							}
						}
				
						menu.EndDisplay ();
						
						if (menuManager.drawOutlines)
						{
							menu.DrawOutline (menuManager.GetSelectedElement ());
						}
					}
				}
			}
		}
	}

}