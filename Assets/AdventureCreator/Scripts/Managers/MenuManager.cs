/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuManager.cs"
 * 
 *	This script handles the "Menu" tab of the main wizard.
 *	It is used to define the menus that make up the game's GUI.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MenuManager : ScriptableObject
{
	
	public List<Menu> menus = new List<Menu>();
	public bool drawOutlines = true;
	public bool drawInEditor = false;
	public Texture2D pauseTexture = null;
	
	private Menu selectedMenu = null;
	private MenuElement selectedMenuElement = null;
	
	#if UNITY_EDITOR
	
	private bool oldVisibility;
	private int typeNumber = 0;
	private string[] elementTypes = { "Button", "Cycle", "DialogList", "Input", "Interaction", "InventoryBox", "Journal", "Label", "SavesList", "Slider", "Timer", "Toggle" };
   
	private static GUIContent
		moveUpContent = new GUIContent("<", "Move up"),
		moveDownContent = new GUIContent(">", "Move down"),
		deleteContent = new GUIContent("-", "Delete");

	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth (20f);	
	
	
	public void OnEnable ()
	{
		if (menus == null)
		{
			menus = new List<Menu>();
		}
	}
	
	
	public void ShowGUI ()
	{
		EditorGUILayout.BeginVertical ("Button");
			drawInEditor = EditorGUILayout.Toggle ("Test in Game Window?", drawInEditor);
			drawOutlines = EditorGUILayout.Toggle ("Draw outlines?", drawOutlines);
			EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Pause background texture:", GUILayout.Width (255f));
				pauseTexture = (Texture2D) EditorGUILayout.ObjectField (pauseTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
			EditorGUILayout.EndHorizontal ();
		
			if (drawInEditor && GameObject.FindWithTag (Tags.gameEngine) == null)
			{	
				EditorGUILayout.HelpBox ("A GameEngine prefab is required to display menus while editing - please click Organise Room Objects within the Scene Manager.", MessageType.Info);
			}
			else if (Application.isPlaying)
			{
				EditorGUILayout.HelpBox ("Changes made to the menus will not be registed by the game until the game is restarted.", MessageType.Info);
			}
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.Space ();
			
		EditorGUILayout.LabelField ("Menus", EditorStyles.boldLabel);
		CreateMenusGUI ();
		
		if (selectedMenu != null)
		{
			EditorGUILayout.Space ();
			
			string menuTitle = selectedMenu.title;
			if (menuTitle == "")
			{
				menuTitle = "(Untitled)";
			}
			
			EditorGUILayout.LabelField ("Menu '" + menuTitle + "' properties", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical ("Button");
				selectedMenu.ShowGUI ();
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField (menuTitle + " elements", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical ("Button");
				CreateElementsGUI (selectedMenu);
			EditorGUILayout.EndVertical ();
			
			if (selectedMenuElement != null)
			{
				EditorGUILayout.Space ();
				
				string elementName = selectedMenuElement.title;
				if (elementName == "")
				{
					elementName = "(Untitled)";
				}
				
				EditorGUILayout.LabelField (selectedMenuElement.GetType().ToString() + " '" + elementName + "' properties", EditorStyles.boldLabel);
				oldVisibility = selectedMenuElement.isVisible;
				selectedMenuElement.ShowGUIStart ();
				if (selectedMenuElement.isVisible != oldVisibility)
				{
					selectedMenu.Recalculate ();
				}
			}
		}
		
		if (GUI.changed)
		{
			if (!Application.isPlaying)
			{
				SaveAllMenus ();
			}
			EditorUtility.SetDirty (this);
		}
	}
	
	
	private void SaveAllMenus ()
	{
		foreach (Menu menu in menus)
		{
			menu.Recalculate ();
		}
	}
	
	
	private void CreateMenusGUI ()
	{
		foreach (Menu _menu in menus)
		{
			EditorGUILayout.BeginHorizontal ();
			
				string buttonLabel = _menu.title;
				if (buttonLabel == "")
				{
					buttonLabel = "(Untitled)";	
				}
				if (GUILayout.Toggle (_menu.isEditing, _menu.id + ": " + buttonLabel, "Button"))
				{
					if (selectedMenu != _menu)
					{
						DeactivateAllMenus ();
						ActivateMenu (_menu);
					}
				}
				
				if (menus[0] != _menu)
				{
					if (GUILayout.Button (moveUpContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Shift menu up");
						
						int i = menus.LastIndexOf (_menu);
						menus = SwapMenus (menus, i, i-1);
						_menu.ResetVisibleElements ();
						AssetDatabase.SaveAssets();

						break;
					}
				}
				
				if (menus.LastIndexOf (_menu) < menus.Count -1)
				{
					if (GUILayout.Button (moveDownContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Shift menu down");
						
						int i = menus.LastIndexOf (_menu);
						menus = SwapMenus (menus, i, i+1);
						_menu.ResetVisibleElements ();
						AssetDatabase.SaveAssets();

						break;
					}
				}
				
				if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (this, "Delete menu: " + _menu.title);
				
					if (_menu == selectedMenu)
					{
						DeactivateAllElements (_menu);
						DeleteAllElements (_menu);
						selectedMenuElement = null;
					}
					DeactivateAllMenus ();
					menus.Remove (_menu);
					
					UnityEngine.Object.DestroyImmediate (_menu, true);
					AssetDatabase.SaveAssets();
				
					break;
				}
		
			EditorGUILayout.EndHorizontal ();
		}

		if (GUILayout.Button("Create new menu"))
		{
			Undo.RecordObject (this, "Add menu");
			
			Menu newMenu = (Menu) CreateInstance <Menu>();
			newMenu.Declare (GetIDArray ());
			menus.Add (newMenu);
			
			DeactivateAllMenus ();
			ActivateMenu (newMenu);
			
			AssetDatabase.AddObjectToAsset (newMenu, this);
			AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newMenu));
		}
	}
	
	
	private void CreateElementsGUI (Menu _menu)
	{
		
		if (_menu.elements != null && _menu.elements.Count > 0)
		{
			foreach (MenuElement _element in _menu.elements)
			{
				if (_element != null)
				{
					string elementName = _element.title;
					
					if (elementName == "")
					{
						elementName = "(Untitled)";
					}
					
					EditorGUILayout.BeginHorizontal ();
					
						if (GUILayout.Toggle (_element.isEditing, _element.ID + ": " + elementName, "Button"))
						{
							if (selectedMenuElement != _element)
							{
								DeactivateAllElements (_menu);
								ActivateElement (_element);
							}
						}
					
						if (_menu.elements[0] != _element)
						{
							if (GUILayout.Button (moveUpContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (this, "Shift menu element up");
								
								int i = _menu.elements.LastIndexOf (_element);
								_menu.elements = SwapElements (_menu.elements, i, i-1);
								_menu.ResetVisibleElements ();
								break;
							}
						}
					
						if (_menu.elements.LastIndexOf (_element) < _menu.elements.Count -1)
						{
							if (GUILayout.Button (moveDownContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (this, "Shift menu element down");
								
								int i = _menu.elements.LastIndexOf (_element);
								_menu.elements = SwapElements (_menu.elements, i, i+1);
								_menu.ResetVisibleElements ();
								break;
							}
						}
						
						if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
						{
							Undo.RecordObject (this, "Delete menu element");
							DeactivateAllElements (_menu);
							selectedMenuElement = null;
							_menu.elements.Remove (_element);
						
							UnityEngine.Object.DestroyImmediate (_element, true);
							AssetDatabase.SaveAssets();
						
							break;
						}
				
					EditorGUILayout.EndHorizontal ();
				}
			}
		}

		EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Element type:", GUILayout.Width (80f));
			typeNumber = EditorGUILayout.Popup (typeNumber, elementTypes);
			
			if (GUILayout.Button ("Add new"))
			{
				AddElement (elementTypes[typeNumber], _menu);
			}
		EditorGUILayout.EndHorizontal ();

	}
	
	
	private void ActivateMenu (Menu menu)
	{
		menu.isEditing = true;
		selectedMenu = menu;
	}
	
	
	private void DeactivateAllMenus ()
	{
		foreach (Menu menu in menus)
		{
			menu.isEditing = false;
		}
		selectedMenu = null;
		selectedMenuElement = null;
	}
	
	
	private void ActivateElement (MenuElement menuElement)
	{
		menuElement.isEditing = true;
		selectedMenuElement = menuElement;
	}
	
	
	private void DeleteAllElements (Menu menu)
	{
		foreach (MenuElement menuElement in menu.elements)
		{
			UnityEngine.Object.DestroyImmediate (menuElement, true);
			AssetDatabase.SaveAssets();
		}
	}
	
	
	private void DeactivateAllElements (Menu menu)
	{
		foreach (MenuElement menuElement in menu.elements)
		{
			menuElement.isEditing = false;
		}
	}
				
	
	private int[] GetIDArray ()
	{
		// Returns a list of id's in the list
		List<int> idArray = new List<int>();
		
		foreach (Menu menu in menus)
		{
			if (menu != null)
			{
				idArray.Add (menu.id);
			}
		}
		
		idArray.Sort ();
		return idArray.ToArray ();
	}
	
	
	private void AddElement (string className, Menu _menu)
	{
		List<int> idArray = new List<int>();
		
		foreach (MenuElement _element in _menu.elements)
		{
			if (_element != null)
			{
				idArray.Add (_element.ID);
			}
		}

		idArray.Sort ();
		
		className = "Menu" + className;
		MenuElement newElement = (MenuElement) CreateInstance (className);
		newElement.Declare ();
		newElement.title = className.Substring (4);
		
		// Update id based on array
		foreach (int _id in idArray.ToArray())
		{
			if (newElement.ID == _id)
			{
				newElement.ID ++;
			}
		}
		
		_menu.elements.Add (newElement);
		_menu.AutoResize ();
		DeactivateAllElements (_menu);
		newElement.isEditing = true;
		selectedMenuElement = newElement;
		
		AssetDatabase.AddObjectToAsset (newElement, this);
		AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newElement));
	}
	
	#endif


	private List<Menu> SwapMenus (List<Menu> list, int a1, int a2)
	{
		Menu tempMenu = list[a1];
		list[a1] = list[a2];
		list[a2] = tempMenu;
		return (list);
	}

	
	private List<MenuElement> SwapElements (List<MenuElement> list, int a1, int a2)
	{
		MenuElement tempElement = list[a1];
		list[a1] = list[a2];
		list[a2] = tempElement;
		return (list);
	}
	
	
	public Menu GetSelectedMenu ()
	{
		return selectedMenu;
	}
	
	
	public MenuElement GetSelectedElement ()
	{
		return selectedMenuElement;
	}
	
}
