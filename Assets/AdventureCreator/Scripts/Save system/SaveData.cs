/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SaveData.cs"
 * 
 *	This script contains all the non-scene-specific data we wish to save.
 * 
 */

[System.Serializable]
public class SaveData
{

	public MainData mainData;
	public SaveData() { }

}


[System.Serializable]
public struct MainData
{

	public int currentScene;
	public int previousScene;
	
	public float playerLocX;
	public float playerLocY;
	public float playerLocZ;		
	public float playerRotY;

	public float playerWalkSpeed;
	public float playerRunSpeed;

	public string playerIdleAnim;
	public string playerWalkAnim;
	public string playerTalkAnim;
	public string playerRunAnim;
	
	public int playerTargetNode;
	public int playerPrevNode;
	public string playerPathData;
	public bool playerIsRunning;
	public bool playerLockedPath;

	public int playerActivePath;
	public int playerActiveArrows;
	public int playerActiveConversation;
	
	public bool playerUpLock;
	public bool playerDownLock;
	public bool playerLeftlock;
	public bool playerRightLock;
	public int playerRunLock;
	public bool playerInventoryLock;
	
	public float timeScale;
	
	public int gameCamera;
	public float mainCameraLocX;
	public float mainCameraLocY;
	public float mainCameraLocZ;
	
	public float mainCameraRotX;
	public float mainCameraRotY;
	public float mainCameraRotZ;
	
	public string inventoryData;
	public string variablesData;

	public string menuLockData;
	public string menuElementVisibilityData;
	public string menuJournalData;

	public int selectedInventoryID;

}