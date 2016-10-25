/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LevelStorage.cs"
 * 
 *	This script handles the loading and unloading of per-scene data.
 *	Below the main class is a series of data classes for the different object types.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using AC;

public class LevelStorage : MonoBehaviour
{
	
	[HideInInspector] public List<SingleLevelData> allLevelData;
	
	
	private void Awake ()
	{
		allLevelData = new List<SingleLevelData>();
	}


	public void ClearAllLevelData ()
	{
		allLevelData.Clear ();
		allLevelData = new List<SingleLevelData>();
	}


	public void ClearCurrentLevelData ()
	{
		foreach (SingleLevelData levelData in allLevelData)
		{
			if (levelData.sceneNumber == Application.loadedLevel)
			{
				allLevelData.Remove (levelData);
				return;
			}
		}
	}
	
	
	public void ReturnCurrentLevelData ()
	{
		foreach (SingleLevelData levelData in allLevelData)
		{
			if (levelData.sceneNumber == Application.loadedLevel)
			{
				UnloadColliderData (levelData.colliders);
				UnloadConversationData (levelData.conversations);
				UnloadHotspotData (levelData.hotspots);
				UnloadNameData (levelData.names);
				UnloadNavMesh (levelData.navMesh);
				UnloadNPCData (levelData.npcs);
				UnloadTransformData (levelData.transforms);
				UnloadVisibilityData (levelData.visibilitys);
				break;
			}
		}
	}
	
	
	public void StoreCurrentLevelData ()
	{
		List <ColliderData> thisLevelColliders = PopulateColliderData ();
		List <ConversationData> thisLevelConversations = PopulateConversationData ();
		List <HotspotData> thisLevelHotspots = PopulateHotspotData ();
		List <NameData> thisLevelNames = PopulateNameData ();
		List <NPCData> thisLevelNPCs = PopulateNPCData ();
		List <TransformData> thisLevelTransforms = PopulateTransformData ();
		List <VisibilityData> thisLevelVisibilitys = PopulateVisibilityData ();
		
		SingleLevelData thisLevelData = new SingleLevelData ();
		thisLevelData.sceneNumber = Application.loadedLevel;
		
		SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		if (sceneSettings && sceneSettings.navMesh && sceneSettings.navMesh.GetComponent <ConstantID>())
		{
			thisLevelData.navMesh = Serializer.GetConstantID (sceneSettings.navMesh.gameObject);
		}
			
		thisLevelData.colliders = thisLevelColliders;
		thisLevelData.conversations = thisLevelConversations;
		thisLevelData.hotspots = thisLevelHotspots;
		thisLevelData.names = thisLevelNames;
		thisLevelData.npcs = thisLevelNPCs;
		thisLevelData.transforms = thisLevelTransforms;
		thisLevelData.visibilitys = thisLevelVisibilitys;
		
		bool found = false;
		for (int i=0; i<allLevelData.Count; i++)
		{
			if (allLevelData[i].sceneNumber == Application.loadedLevel)
			{
				allLevelData[i] = thisLevelData;
				found = true;
				break;
			}
		}
		
		if (!found)
		{
			allLevelData.Add (thisLevelData);
		}
	}

	
	private void UnloadNavMesh (int navMeshInt)
	{
		NavigationMesh navMesh = Serializer.returnComponent <NavigationMesh> (navMeshInt);
		SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		
		if (navMesh && navMesh.GetComponent<Collider>() && sceneSettings && sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider && sceneSettings.navMesh)
		{
			NavigationMesh oldNavMesh = sceneSettings.navMesh;
			oldNavMesh.TurnOff ();
			navMesh.GetComponent<Collider>().GetComponent <NavigationMesh>().TurnOn ();
			
			sceneSettings.navMesh = navMesh;
		}
	}


	private void UnloadNameData (List <NameData> _names)
	{
		foreach (NameData _name in _names)
		{
			RememberName saveObject = Serializer.returnComponent <RememberName> (_name.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_name);
			}
		}
	}


	private void UnloadColliderData (List <ColliderData> _colliders)
	{
		foreach (ColliderData _collider in _colliders)
		{
			RememberCollider saveObject = Serializer.returnComponent <RememberCollider> (_collider.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_collider);
			}
		}
	}

	
	private void UnloadTransformData (List <TransformData> _transforms)
	{
		foreach (TransformData _transform in _transforms)
		{
			RememberTransform saveObject = Serializer.returnComponent <RememberTransform> (_transform.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_transform);
			}
		}
	}
	
	
	private void UnloadNPCData (List <NPCData> _npcs)
	{
		foreach (NPCData _npc in _npcs)
		{
			RememberNPC saveObject = Serializer.returnComponent <RememberNPC> (_npc.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_npc);
			}
		}
	}
	
	
	private void UnloadHotspotData (List <HotspotData> _hotspots)
	{
		foreach (HotspotData _hotspot in _hotspots)
		{
			RememberHotspot saveObject = Serializer.returnComponent <RememberHotspot> (_hotspot.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_hotspot);
			}
		}
	}


	private void UnloadVisibilityData (List <VisibilityData> _visibilitys)
	{
		foreach (VisibilityData _visibility in _visibilitys)
		{
			RememberVisibility saveObject = Serializer.returnComponent <RememberVisibility> (_visibility.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_visibility);
			}
		}
	}
	
	
	private void UnloadConversationData (List <ConversationData> _conversations)
	{
		foreach (ConversationData _conversation in _conversations)
		{
			RememberConversation saveObject = Serializer.returnComponent <RememberConversation> (_conversation.objectID);
			
			if (saveObject != null)
			{
				saveObject.LoadData (_conversation);
			}
		}
	}
	
	
	private List <TransformData> PopulateTransformData ()
	{
		List <TransformData> allTransformData = new List<TransformData>();
		RememberTransform[] transforms = FindObjectsOfType (typeof (RememberTransform)) as RememberTransform[];
		
		foreach (RememberTransform _transform in transforms)
		{
			if (_transform.constantID != 0)
			{
				allTransformData.Add (_transform.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _transform.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allTransformData;
	}


	private List <ColliderData> PopulateColliderData ()
	{
		List <ColliderData> allColliderData = new List<ColliderData>();
		RememberCollider[] colliders = FindObjectsOfType (typeof (RememberCollider)) as RememberCollider[];
		
		foreach (RememberCollider _collider in colliders)
		{
			if (_collider.constantID != 0)
			{
				allColliderData.Add (_collider.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _collider.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allColliderData;
	}


	private List <NameData> PopulateNameData ()
	{
		List <NameData> allNameData = new List<NameData>();
		RememberName[] names = FindObjectsOfType (typeof (RememberName)) as RememberName[];
		
		foreach (RememberName _name in names)
		{
			if (_name.constantID != 0)
			{
				allNameData.Add (_name.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _name.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allNameData;
	}
	
	
	private List <NPCData> PopulateNPCData ()
	{
		List <NPCData> allNPCData = new List<NPCData>();
		RememberNPC[] npcs = FindObjectsOfType (typeof (RememberNPC)) as RememberNPC[];
		
		foreach (RememberNPC _npc in npcs)
		{
			if (_npc.constantID != 0)
			{
				allNPCData.Add (_npc.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _npc.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allNPCData;
	}


	private List <VisibilityData> PopulateVisibilityData ()
	{
		List <VisibilityData> allVisibilityData = new List<VisibilityData>();
		RememberVisibility[] visibilitys = FindObjectsOfType (typeof (RememberVisibility)) as RememberVisibility[];
		
		foreach (RememberVisibility _visibility in visibilitys)
		{
			if (_visibility.constantID != 0)
			{
				allVisibilityData.Add (_visibility.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _visibility.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allVisibilityData;
	}
	
	
	private List <HotspotData> PopulateHotspotData ()
	{
		List <HotspotData> allHotspotData = new List<HotspotData>();
		
		RememberHotspot[] hotspots = FindObjectsOfType (typeof (RememberHotspot)) as RememberHotspot[];
		
		foreach (RememberHotspot _hotspot in hotspots)
		{
			if (_hotspot.constantID != 0)
			{
				allHotspotData.Add (_hotspot.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _hotspot.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allHotspotData;
	}
	
	
	private List <ConversationData> PopulateConversationData ()
	{
		List <ConversationData> allConversationData = new List<ConversationData>();
		
		RememberConversation[] conversations = FindObjectsOfType (typeof (RememberConversation)) as RememberConversation[];
		
		foreach (RememberConversation _conversation in conversations)
		{
			if (_conversation.constantID != 0)
			{
				allConversationData.Add (_conversation.SaveData ());
			}
			else
			{
				Debug.LogWarning ("GameObject " + _conversation.name + " was not saved because it's ConstantID has not been set!");
			}
		}
		
		return allConversationData;
	}


	private void AssignMenuLocks (List<Menu> menus, string menuLockData)
	{
		if (menuLockData.Length == 0)
		{
			return;
		}

		string[] lockArray = menuLockData.Split ("|"[0]);
		
		foreach (string chunk in lockArray)
		{
			string[] chunkData = chunk.Split (":"[0]);
			
			int _id = 0;
			int.TryParse (chunkData[0], out _id);
			
			bool _lock = false;
			bool.TryParse (chunkData[1], out _lock);
			
			foreach (Menu _menu in menus)
			{
				if (_menu.id == _id)
				{
					_menu.isLocked = _lock;
					break;
				}
			}
		}
	}

}
	

[System.Serializable]
public class SingleLevelData
{
	
	public List<ColliderData> colliders;
	public List<ConversationData> conversations;
	public List<VisibilityData> visibilitys;
	public List<TransformData> transforms;
	public List<HotspotData> hotspots;
	public List<NameData> names;
	public List<NPCData> npcs;
	public int sceneNumber;
	public int navMesh;
	
	public SingleLevelData () { }
	
}