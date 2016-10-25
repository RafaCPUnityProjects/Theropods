/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharFollow.cs"
 * 
 *	This action causes NPCs to follow other characters.
 *	If they are moved in any other way, their following
 *	state will reset
 * 
*/

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharFollow : Action
{
	
	public NPC npcToMove;
	public Char charToFollow;
	public bool followPlayer;
	public float updateFrequency = 2f;
	public float followDistance = 1f;
	public enum FollowType { StartFollowing, StopFollowing };
	public FollowType followType;
	
	
	public ActionCharFollow ()
	{
		this.isDisplayed = true;
		title = "Character: NPC follow";
	}
	
	
	override public float Run ()
	{
		if (npcToMove)
		{
			if (followType == FollowType.StopFollowing)
			{
				npcToMove.FollowReset ();
				return 0f;
			}

			if (followPlayer || charToFollow != (Char) npcToMove)
			{
				npcToMove.FollowAssign (charToFollow, followPlayer, updateFrequency, followDistance);
			}
		}

		return 0f;
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		npcToMove = (NPC) EditorGUILayout.ObjectField ("NPC to affect:", npcToMove, typeof(NPC), true);

		followType = (FollowType) EditorGUILayout.EnumPopup ("Follow type:", followType);
		if (followType == FollowType.StartFollowing)
		{
			followPlayer = EditorGUILayout.Toggle ("Follow Player?", followPlayer);
			
			if (!followPlayer)
			{
				charToFollow = (Char) EditorGUILayout.ObjectField ("Character to follow:", charToFollow, typeof(Char), true);
				if (charToFollow && charToFollow == (Char) npcToMove)
				{
					charToFollow = null;
					Debug.LogWarning ("An NPC cannot follow themselves!");
				}
			}

			updateFrequency = EditorGUILayout.FloatField ("Update frequency (s):", updateFrequency);
			if (updateFrequency == 0f || updateFrequency < 0f)
			{
				EditorGUILayout.HelpBox ("Update frequency must be greater than zero.", MessageType.Warning);
			}
			followDistance = EditorGUILayout.FloatField ("Minimum distance:", followDistance);
			if (followDistance == 0f || followDistance < 0f)
			{
				EditorGUILayout.HelpBox ("Minimum distance must be greater than zero.", MessageType.Warning);
			}
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		if (npcToMove)
		{
			if (followType == FollowType.StopFollowing)
			{
				return (" (Stop " + npcToMove + ")");
			}
			else
			{
				if (followPlayer)
				{
					return (" (" + npcToMove.name + " to Player)");
				}
				else if (charToFollow)
				{
						return (" (" + npcToMove.name + " to " + charToFollow.name + ")");
				}
			}
		}

		return "";
	}

	#endif
	
}