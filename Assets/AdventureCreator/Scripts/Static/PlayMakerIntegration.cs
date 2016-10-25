/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayMakerIntegration.cs"
 * 
 *	This script contains static functions for use
 *	in calling PlayMaker FSMs.
 *
 *	To allow for PlayMaker integration, the 'PlayMakerIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'PlayMakerIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 * 
 */

using UnityEngine;
using System.Collections;

public class PlayMakerIntegration : ScriptableObject
{
	
	public static bool IsDefinePresent ()
	{
		#if PlayMakerIsPresent
		return true;
		#else
		return false;
		#endif
	}


	public static void CallEvent (GameObject linkedObject, string eventName)
	{
		#if PlayMakerIsPresent

		if (linkedObject.GetComponent <PlayMakerFSM>())
		{
			PlayMakerFSM playMakerFSM = linkedObject.GetComponent <PlayMakerFSM>();
			playMakerFSM.Fsm.Event (eventName);
		}

		#endif
	}

}
