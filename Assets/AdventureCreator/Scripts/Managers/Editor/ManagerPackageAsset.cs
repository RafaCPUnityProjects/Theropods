using UnityEngine;
using UnityEditor;
using System;

public class ManagerPackageAsset
{
	
	[MenuItem ("Assets/Create/Adventure Creator/Manager Package")]
	
	public static void CreateAsset ()
	{
		CustomAssetUtility.CreateAsset <ManagerPackage> ();
	}
	
}