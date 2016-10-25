/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NPC.cs"
 * 
 *	This is attached to all non-Player characters.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

namespace AC
{

	public class NPC : AC.Char
	{

		public Char followTarget = null;
		public bool followTargetIsPlayer = false;
		public float followFrequency = 0f;
		public float followDistance = 0f;

		LayerMask LayerOn;
		LayerMask LayerOff;
		
		
		new private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				LayerOn = LayerMask.NameToLayer (settingsManager.hotspotLayer);
				LayerOff = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
			}

			base.Awake ();
		}

		
		new private void FixedUpdate ()
		{
			if (activePath && followTarget)
			{
				FollowCheckDistance ();
			}

			if (activePath && !pausePath)
			{
				charState = CharState.Move;
				CheckIfStuck ();
			}

			base.FixedUpdate ();
		}


		public void FollowReset ()
		{
			FollowStop ();

			followTarget = null;
			followTargetIsPlayer = false;
			followFrequency = 0f;
			followDistance = 0f;
		}


		private void FollowUpdate ()
		{
			if (followTarget)
			{
				if (FollowCheckDistance ())
				{
					Paths path = GetComponent <Paths>();
					if (path == null)
					{
						Debug.LogWarning ("Cannot move a character with no Paths component");
					}
					else
					{
						path.pathType = AC_PathType.ForwardOnly;
						path.pathSpeed = PathSpeed.Walk;
						path.affectY = true;
						
						Vector3[] pointArray;
						Vector3 targetPosition = followTarget.transform.position;
						
						SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
						if (settingsManager && settingsManager.ActInScreenSpace ())
						{
							targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
						}
						
						if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>())
						{
							pointArray = GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>().navigationEngine.GetPointsArray (transform.position, targetPosition);
						}
						else
						{
							List<Vector3> pointList = new List<Vector3>();
							pointList.Add (targetPosition);
							pointArray = pointList.ToArray ();
						}
						
						MoveAlongPoints (pointArray, false);
					}
				}

				Invoke ("FollowUpdate", followFrequency);
			}
		}


		private bool FollowCheckDistance ()
		{
			// Returns true if need to move

			if (followTarget)
			{
				if (Vector3.Distance (followTarget.transform.position, transform.position) > followDistance)
				{
					return true;
				}
				else
				{
					EndPath ();
				}
			}

			return false;
		}


		private void FollowStop ()
		{
			StopCoroutine ("FollowUpdate");

			if (followTarget != null)
			{
				EndPath ();
			}
		}


		public void FollowAssign (Char _followTarget, bool _followTargetIsPlayer, float _followFrequency, float _followDistance)
		{
			if (_followTargetIsPlayer)
			{
				_followTarget = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			}

			if (_followTarget == null || _followFrequency == 0f || _followFrequency < 0f || _followDistance == 0f || _followDistance < 0f)
			{
				FollowReset ();
				return;
			}

			followTarget = _followTarget;
			followTargetIsPlayer = _followTargetIsPlayer;
			followFrequency = _followFrequency;
			followDistance = _followDistance;

			FollowUpdate ();
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerOn;
		}
		

		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}
		
	}

}