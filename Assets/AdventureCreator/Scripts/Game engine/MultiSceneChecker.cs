﻿using UnityEngine;
using System.Collections;

namespace AC
{

	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_multi_scene_checker.html")]
	#endif
	public class MultiSceneChecker : MonoBehaviour
	{

		private KickStarter activeKickStarter;


		private void Awake ()
		{
			UnityVersionHandler.EnsureSingleScene ();

			if (!UnityVersionHandler.ObjectIsInActiveScene (gameObject))
			{
				return;
			}

			activeKickStarter = FindObjectOfType <KickStarter>();

			if (activeKickStarter != null)
			{
				KickStarter.mainCamera.OnAwake ();
				activeKickStarter.OnAwake ();
				KickStarter.playerInput.OnAwake ();
				KickStarter.playerQTE.OnAwake ();
				KickStarter.sceneSettings.OnAwake ();
				KickStarter.dialog.OnAwake ();
				KickStarter.navigationManager.OnAwake ();
				KickStarter.actionListManager.OnAwake ();

				KickStarter.stateHandler.RegisterWithGameEngine ();
			}
			else
			{
				ACDebug.LogError ("No KickStarter component found in the scene!");
			}
		}


		private void Start ()
		{
			if (activeKickStarter != null)
			{
				KickStarter.sceneSettings.OnStart ();
				KickStarter.playerMovement.OnStart ();
				KickStarter.mainCamera.OnStart ();
			}
		}


		#if UNITY_EDITOR

		/**
		 * <summary>Allows the Scene and Variables Managers to show UI controls for the currently-active scene, if multiple scenes are being edited.</summary>
		 * <returns>The name of the currently-open scene.</summary>
		 */
		public static string EditActiveScene ()
		{
			string openScene = UnityVersionHandler.GetActiveSceneName ();

			if (openScene != "" && !Application.isPlaying)
			{
				if (FindObjectOfType <KickStarter>() != null)
				{
					FindObjectOfType <KickStarter>().ClearVariables ();
				}
			}

			return openScene;
		}

		#endif
		
	}

}