/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GVar.cs"
 * 
 *	This script is a data class for project-wide variables.
 * 
 */

using AC;

[System.Serializable]
public class GVar
{
	
	public string label;
	public int val;				// For bools, 0 = false, 1 = true
	public string textVal;
	public VariableType type;
	public int id;				// Internal ID to allow order-independence
	
	
	public GVar ()
	{
		val = 0;
		textVal = "";
		type = VariableType.Boolean;
		id = 0;

		label = "Variable " + (id + 1).ToString ();
	}
	
	
	public GVar (int[] idArray)
	{
		val = 0;
		textVal = "";
		type = VariableType.Boolean;
		id = 0;
		
		// Update id based on array
		foreach (int _id in idArray)
		{
			if (id == _id)
			{
				id ++;
			}
		}
		
		label = "Variable " + (id + 1).ToString ();
	}
	
	
	public GVar (GVar assetVar)
	{
		// Duplicates Asset to Runtime instance
		// (Do it the long way to ensure no connections remain to the asset file)
		
		val = assetVar.val;
		textVal = assetVar.textVal;
		type = assetVar.type;
		id = assetVar.id;
		label = assetVar.label;
	}
	
}