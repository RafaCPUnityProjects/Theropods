/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationManager.cs"
 * 
 *	This script instantiates the chosen
 *	NavigationEngine subclass at runtime.
 * 
 */

using UnityEngine;

public class NavigationManager : MonoBehaviour
{
	
	public NavigationEngine navigationEngine = null;
	
	
	private void Awake ()
	{
		navigationEngine = null;
		ResetEngine ();
	}


	public void ResetEngine ()
	{
		if (GetComponent <SceneSettings>())
		{
			string className = "NavigationEngine_" + GetComponent <SceneSettings>().navigationMethod.ToString ();

			if (navigationEngine == null || !navigationEngine.ToString ().Contains (className))
			{
				navigationEngine = (NavigationEngine) ScriptableObject.CreateInstance (className);
				navigationEngine.Awake ();
			}
		}
	}

}
