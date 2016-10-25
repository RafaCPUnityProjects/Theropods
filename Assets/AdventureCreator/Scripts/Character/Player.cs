/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Player.cs"
 * 
 *	This is attached to the Player GameObject, which must be tagged as Player.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

namespace AC
{

	public class Player : AC.Char
	{
		
		public bool lockedPath;
		public Hotspot hotspotMovingTo;
		public DetectHotspots hotspotDetector;


		new private void Awake ()
		{
			DontDestroyOnLoad (this);
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}
			
			base.Awake ();
		}
		
		
		new private void FixedUpdate ()
		{
			if (activePath && !pausePath)
			{
				if (settingsManager == null)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}

				if (stateHandler.gameState == GameState.Cutscene || settingsManager.movementMethod == MovementMethod.PointAndClick || IsMovingToHotspot ())
				{
					charState = CharState.Move;
				}
				
				if (!lockedPath)
				{
					CheckIfStuck ();
				}
			}
			
			base.FixedUpdate ();
		}
		
		
		new public void EndPath ()
		{
			lockedPath = false;
			
			base.EndPath ();
		}


		public bool IsMovingToHotspot ()
		{
			if (hotspotMovingTo != null)
			{
				return true;
			}

			return false;
		}
		
		
		public void SetLockedPath (Paths pathOb)
		{
			// Ignore if using "point and click" or first person methods
			if (settingsManager)
			{
				if (settingsManager.movementMethod == MovementMethod.Direct && settingsManager.inputMethod != InputMethod.TouchScreen)
				{
					lockedPath = true;
					
					if (pathOb.pathSpeed == PathSpeed.Run)
					{
						isRunning = true;
					}
					else
					{
						isRunning = false;
					}
				
					if (pathOb.affectY)
					{
						transform.position = pathOb.transform.position;
					}
					else
					{
						transform.position = new Vector3 (pathOb.transform.position.x, transform.position.y, pathOb.transform.position.z);
					}
			
					activePath = pathOb;
					targetNode = 1;
					charState = CharState.Idle;
				}
				else
				{
					Debug.LogWarning ("Path-constrained player movement is only available with Direct control for Point And Click and Controller input only.");
				}
			}
		}

	}

}