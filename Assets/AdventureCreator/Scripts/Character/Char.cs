/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Char.cs"
 * 
 *	This is the base class for both NPCs and the Player.
 *	It contains the functions needed for animation and movement.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class Char : MonoBehaviour
	{
		
		public AnimationEngine animationEngine = AnimationEngine.SpritesUnity;	// Enum
		public AnimEngine animEngine;											// ScriptableObject
		public TalkingAnimation talkingAnimation = TalkingAnimation.Standard;
		
		public Texture2D portraitGraphic;
		
		// 3D variables

		public Transform leftHandBone;
		public Transform rightHandBone;

		// Legacy variables
		
		public AnimationClip idleAnim;
		public AnimationClip walkAnim;
		public AnimationClip runAnim;
		public AnimationClip talkAnim;
		public AnimationClip turnLeftAnim;
		public AnimationClip turnRightAnim;
		
		public Transform upperBodyBone;
		public Transform leftArmBone;
		public Transform rightArmBone;
		public Transform neckBone;

		public float animCrossfadeSpeed = 0.2f;

		// Mecanim variables

		public string moveSpeedParameter = "Speed";
		public string turnParameter = "";
		public string talkParameter = "IsTalking";
		public bool relyOnRootMotion = false;

		// 2D variables
		
		public Animator animator;
		public Transform spriteChild;
		
		public string idleAnimSprite = "idle";
		public string walkAnimSprite = "walk";
		public string runAnimSprite = "run";
		public string talkAnimSprite = "talk";
		
		public bool doDirections = true;
		public bool crossfadeAnims = false;
		public bool doDiagonals = false;
		public bool isTalking = false;
		public AC_2DFrameFlipping frameFlipping = AC_2DFrameFlipping.None;
		
		private bool flipFrames = false;
		private string spriteDirection = "D";
		
		// Movement variables
		
		public float walkSpeedScale = 2f;
		public float runSpeedScale = 6f;
		public float turnSpeed = 7f;
		public float acceleration = 6f;
		public float deceleration = 0f;
		public float sortingMapScale = 1f;

		// Rigidbody variables

		public bool ignoreGravity = false;
		public bool freezeRigidbodyWhenIdle = false;

		// Sound variables
		
		public AudioClip walkSound;
		public AudioClip runSound;
		public Sound soundChild;
		protected AudioSource audioSource;
		
		public Paths activePath = null;
		public bool isRunning { get; set; }
		
		public CharState charState;
		
		protected float moveSpeed;
		protected Vector3 moveDirection; 
		
		protected int targetNode = 0;
		protected bool pausePath = false;
		
		private Vector3 lookDirection;
		private float pausePathTime;
		private int prevNode = 0;
		private Vector3 oldPosition;
		
		private bool tankTurning = false;
		private bool isTurning = false;
		private bool isTurningLeft = false;
		
		protected StateHandler stateHandler;
		protected SettingsManager settingsManager;
		private MainCamera mainCamera;
		
		
		protected void Awake ()
		{
			ResetAnimationEngine ();
			ResetBaseClips ();
			
			if (spriteChild && spriteChild.GetComponent <Animator>())
			{
				animator = spriteChild.GetComponent <Animator>();
			}
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}
			
			if (GetComponent <Animator>())
			{
				animator = GetComponent <Animator>();
			}
		}
		
		
		private void Start ()
		{
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag(Tags.persistentEngine).GetComponent<StateHandler>();
			}
		}
		
		
		private void Update ()
		{
			if (mainCamera == null)
			{
				mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			}
			
			if (spriteChild && mainCamera)
			{
				UpdateSpriteChild (settingsManager.IsTopDown ());
			}
		}
		
		
		protected void FixedUpdate ()
		{
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			PathUpdate ();
			SpeedUpdate ();
			PhysicsUpdate ();
			AnimUpdate ();
			MoveUpdate ();
		}

		
		protected void PathUpdate ()
		{
			if (activePath && activePath.nodes.Count > 0)
			{
				if (pausePath)
				{
					if (Time.time > pausePathTime)
					{
						pausePath = false;
						SetNextNodes ();
					}
				}
				else
				{
					Vector3 direction = activePath.nodes[targetNode] - transform.position;
					
					if (!activePath.affectY)
					{
						direction.y = 0;
					}
					
					SetLookDirection (direction, false);
					SetMoveDirectionAsForward ();
					
					float nodeThreshold = 0.1f;
					if (settingsManager)
					{
						nodeThreshold = 1.05f - settingsManager.destinationAccuracy;
					}
					if (isRunning)
					{
						nodeThreshold *= runSpeedScale / walkSpeedScale;
					}
					
					if (direction.magnitude < nodeThreshold)
					{
						if ((targetNode == 0 && prevNode == 0) || activePath.nodePause <= 0)
						{
							SetNextNodes ();
						}
						else
						{
							PausePath ();
						}
					}
				}
			}
		}
		
		
		private void SpeedUpdate ()
		{
			if (charState == CharState.Move)
			{
				Accelerate ();
			}
			else if (charState == CharState.Decelerate || charState == CharState.Custom)
			{
				Decelerate ();
			}
		}
		
		
		private void PhysicsUpdate ()
		{
			if (GetComponent<Rigidbody>())
			{
				if (ignoreGravity)
				{
					GetComponent<Rigidbody>().useGravity = false;
				}

				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					GetComponent<Rigidbody>().useGravity = false;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						GetComponent<Rigidbody>().useGravity = false;
					}
					else
					{
						GetComponent<Rigidbody>().useGravity = true;
					}
				}
			}
			else
			{
				Debug.LogWarning ("No rigidbody attached");
			}
		}
		
		
		private void AnimUpdate ()
		{
			if (charState == CharState.Idle || charState == CharState.Decelerate)
			{
				if (isTurning)
				{
					if (isTurningLeft)
					{
						animEngine.PlayTurnLeft ();
					}
					else if (!isTurningLeft)
					{
						animEngine.PlayTurnRight ();
					}
					else
					{
						animEngine.PlayIdle ();
					}
				}
				else
				{
					if (isTalking && talkingAnimation == TalkingAnimation.Standard)
					{
						animEngine.PlayTalk ();
					}
					else
					{
						animEngine.PlayIdle ();
					}
				}
				
				StopStandardAudio ();
			}
			
			else if (charState == CharState.Move)
			{
				if (isRunning)
				{
					animEngine.PlayRun ();
				}
				else
				{
					animEngine.PlayWalk ();
				}
				
				PlayStandardAudio ();
			}
			
			else
			{
				StopStandardAudio ();
			}
		}
		
		
		
		private void MoveUpdate ()
		{
			if (animEngine)
			{
				if (moveSpeed > 0.01f && GetComponent<Rigidbody>() && (!animEngine.rootMotion || !relyOnRootMotion))
				{
					Vector3 newVel;
					newVel = moveDirection * moveSpeed * walkSpeedScale * sortingMapScale;

					if (settingsManager && settingsManager.IsTopDown ())
					{
						newVel.z *= settingsManager.verticalReductionFactor;
					}

					GetComponent<Rigidbody>().MovePosition (GetComponent<Rigidbody>().position + newVel * Time.deltaTime);
				}
				
				if (isTurning)
				{
					if (animEngine && animEngine.turningIsLinear && moveSpeed > 0.01f)
					{
						Turn (true);
					}
					else
					{
						Turn (false);
					}
				}
			}

			if (GetComponent<Rigidbody>())
			{
				if (freezeRigidbodyWhenIdle && (charState == CharState.Custom || charState == CharState.Idle))
				{
					GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				}
				else
				{
					GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
				}
			}
		}
		
		
		private void Accelerate ()
		{
			float targetSpeed;
			
			if (isRunning)
			{
				targetSpeed = moveDirection.magnitude * runSpeedScale / walkSpeedScale;
			}
			else
			{
				targetSpeed = moveDirection.magnitude;
			}
			
			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, Time.deltaTime * acceleration);
		}
		
		
		private void Decelerate ()
		{
			if (deceleration <= 0f)
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * acceleration);
			}
			else
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * deceleration);
			}
			
			if (moveSpeed < 0.01f)
			{
				moveSpeed = 0f;
				
				if (charState != CharState.Custom)
				{
					charState = CharState.Idle;
				}
			}
		}
		
		
		public bool IsTurning ()
		{
			return isTurning;
		}
		
		
		public void TankTurnLeft ()
		{
			lookDirection = -transform.right;
			tankTurning = true;
			isTurning = true;
		}
		
		
		public void TankTurnRight ()
		{
			lookDirection = transform.right;
			tankTurning = true;
			isTurning = true;
		}
		
		
		public void StopTurning ()
		{
			lookDirection = transform.forward;
			tankTurning = false;
			isTurning = false;
		}
		
		
		public void Turn (bool isInstant)
		{
			if (lookDirection != Vector3.zero)
			{
				float actualTurnSpeed = turnSpeed * Time.deltaTime;
				if (tankTurning || moveSpeed == 0f)
				{
					actualTurnSpeed /= 2;
				}
				
				Quaternion targetRotation = Quaternion.LookRotation (lookDirection, Vector3.up);
				Quaternion newRotation;
				
				if (animEngine && animEngine.turningIsLinear)
				{
					newRotation = Quaternion.RotateTowards (GetComponent<Rigidbody>().rotation, targetRotation, 5f);
				}
				else
				{
					newRotation = Quaternion.Lerp (GetComponent<Rigidbody>().rotation, targetRotation, actualTurnSpeed);
				}
				
				if (isInstant)
				{
					isTurning = false;
					GetComponent<Rigidbody>().rotation = targetRotation;
				}
				else
				{
					isTurning = true;
					GetComponent<Rigidbody>().MoveRotation (newRotation);
					
					// Determine if character is turning left or right (courtesy of Duck: http://answers.unity3d.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html)
					
					Vector3 forwardA = targetRotation * Vector3.forward;
					Vector3 forwardB = transform.rotation * Vector3.forward;
					
					float angleA = Mathf.Atan2 (forwardA.x, forwardA.z) * Mathf.Rad2Deg;
					float angleB = Mathf.Atan2 (forwardB.x, forwardB.z) * Mathf.Rad2Deg;
					
					float angleDiff = Mathf.DeltaAngle( angleA, angleB );
					
					if (angleDiff < 0f)
					{
						isTurningLeft = false;
					}
					else
					{
						isTurningLeft = true;
					}
					
					if (Quaternion.Angle (Quaternion.LookRotation (lookDirection), transform.rotation) < 3f)
					{
						isTurning = false;
					}
				}
			}
		}
		
		
		public void SetLookDirection (Vector3 _direction, bool isInstant)
		{
			lookDirection = _direction;
			Turn (isInstant);
		}
		
		
		public void SetMoveDirection (Vector3 _direction)
		{
			Quaternion targetRotation = Quaternion.LookRotation (_direction, Vector3.up);
			moveDirection = targetRotation * Vector3.forward;
			moveDirection.Normalize ();
			
		}
		
		
		public void SetMoveDirectionAsForward ()
		{
			moveDirection = transform.forward;
			moveDirection.Normalize ();
		}
		
		
		public void SetMoveDirectionAsBackward ()
		{
			moveDirection = -transform.forward;
			moveDirection.Normalize ();
		}
		
		
		public Vector3 GetMoveDirection ()
		{
			return moveDirection;	
		}
		
		
		private void SetNextNodes ()
		{
			int tempPrev = targetNode;
			
			if (this.GetComponent <Player>() && stateHandler.gameState == GameState.Normal)
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, true);
			}
			else
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, false);
			}
			
			prevNode = tempPrev;
			
			if (targetNode == -1)
			{
				EndPath ();
			}
		}
		
		
		public void EndPath ()
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>())
			{
				activePath.nodes.Clear ();
			}

			activePath = null;
			targetNode = 0;
			charState = CharState.Decelerate;
		}
		
		
		public void Halt ()
		{
			activePath = null;
			targetNode = 0;
			charState = CharState.Idle;
			moveSpeed = 0f;
		}
		
		
		protected void ReverseDirection ()
		{
			int tempPrev = targetNode;
			targetNode = prevNode;
			prevNode = tempPrev;
		}
		
		
		private void PausePath ()
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = Time.time + activePath.nodePause;
		}
		
		
		public void SetPath (Paths pathOb, PathSpeed _speed)
		{
			activePath = pathOb;
			targetNode = 0;
			prevNode = 0;
			
			if (pathOb)
			{
				if (_speed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			
			charState = CharState.Idle;
		}
		
		
		public void SetPath (Paths pathOb)
		{
			activePath = pathOb;
			targetNode = 0;
			prevNode = 0;
			
			if (pathOb)
			{
				if (pathOb.pathSpeed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			
			charState = CharState.Idle;
		}
		
		
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode)
		{
			activePath = pathOb;
			targetNode = _targetNode;
			prevNode = _prevNode;
			
			if (pathOb)
			{
				if (pathOb.pathSpeed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			
			charState = CharState.Idle;
		}
		
		
		protected void CheckIfStuck ()
		{
			// Check for null movement error: if not moving on a path, end the path
			
			/*Vector3 newPosition = rigidbody.position;
			if (oldPosition == newPosition)
			{
				Debug.Log ("Stuck in active path - removing");
				EndPath ();
			}
			
			oldPosition = newPosition;*/
		}

		
		public Paths GetPath ()
		{
			return activePath;
		}


		public int GetTargetNode ()
		{
			return targetNode;
		}
		
		
		public int GetPrevNode ()
		{
			return prevNode;
		}
		
		
		public void MoveToPoint (Vector3 point, bool run)
		{
			List<Vector3> pointData = new List<Vector3>();
			pointData.Add (point);
			MoveAlongPoints (pointData.ToArray (), run);
		}
		
		
		public void MoveAlongPoints (Vector3[] pointData, bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				path.BuildNavPath (pointData);
				
				if (run)
				{
					SetPath (path, PathSpeed.Run);
				}
				else
				{
					SetPath (path, PathSpeed.Walk);
				}
			}
			else
			{
				Debug.LogWarning (this.name + " cannot pathfind without a Paths component");
			}
		}
		
		
		public void ResetBaseClips ()
		{
			// Remove all animations except Idle, Walk, Run and Talk
			
			if (GetComponent<Animation>())
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in GetComponent<Animation>())
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					GetComponent<Animation>().RemoveClip (_clip);
				}
			}
			
		}
		
		
		public string GetSpriteDirection ()
		{
			return ("_" + spriteDirection);
		}
		
		
		private string SetSpriteDirection (float rightAmount, float forwardAmount)
		{
			float angle = Vector2.Angle (new Vector2 (1f, 0f), new Vector2 (rightAmount, forwardAmount));
			
			if (doDiagonals)
			{
				if (forwardAmount > 0f)
				{
					if (angle > 22.5f && angle < 67.5f)
					{
						return "UR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "UL";
					}
				}
				else
				{
					if (angle > 22.5f && angle < 67.55f)
					{
						return "DR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "DL";
					}
				}
			}
			
			if (forwardAmount > 0f)
			{
				if (angle > 45f && angle < 135f)
				{
					return "U";
				}
			}
			else
			{
				if (angle > 45f && angle < 135f)
				{
					return "D";
				}
			}
			
			if (rightAmount > 0f)
			{
				return "R";
			}
			
			return "L";
		}
		
		
		private void StopStandardAudio ()
		{
			if (audioSource && audioSource.isPlaying)
			{
				if ((runSound && audioSource.clip == runSound) || (walkSound && audioSource.clip == walkSound))
				{
					audioSource.Stop ();
				}
			}
		}
		
		
		private void PlayStandardAudio ()
		{
			if (audioSource && !audioSource.isPlaying)
			{
				if (isRunning && runSound)
				{
					audioSource.loop = false;
					audioSource.clip = runSound;
					audioSource.Play ();
				}
				
				else if (walkSound)
				{
					audioSource.loop = false;
					audioSource.clip = walkSound;
					audioSource.Play ();
				}
			}
		}
		
		
		public void ResetAnimationEngine ()
		{
			string className = "AnimEngine_" + animationEngine.ToString ();
			
			if (animEngine == null || animEngine.ToString () != className)
			{
				animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
				animEngine.Declare (this);
			}
		}


		private void UpdateSpriteChild (bool isTopDown)
		{
			float forwardAmount = 0f;
			float rightAmount = 0f;

			if (isTopDown)
			{
				forwardAmount = Vector3.Dot (Vector3.forward, transform.forward.normalized);
				rightAmount = Vector3.Dot (Vector3.right, transform.forward.normalized);
			}
			else
			{
				forwardAmount = Vector3.Dot (mainCamera.ForwardVector ().normalized, transform.forward.normalized);
				rightAmount = Vector3.Dot (mainCamera.RightVector ().normalized, transform.forward.normalized);
			}

			if (charState == CharState.Custom)
			{
				flipFrames = false;
			}
			else
			{
				spriteDirection = SetSpriteDirection (rightAmount, forwardAmount);
				
				if (frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight && spriteDirection.Contains ("L"))
				{
					spriteDirection = spriteDirection.Replace ("L", "R");
					flipFrames = true;
				}
				else if (frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft && spriteDirection.Contains ("R"))
				{
					spriteDirection = spriteDirection.Replace ("R", "L");
					flipFrames = true;
				}
				else
				{
					flipFrames = false;
				}
			}
			
			if ((flipFrames && spriteChild.localScale.x > 0f) || (!flipFrames && spriteChild.localScale.x < 0f))
			{
				spriteChild.localScale = new Vector3 (-spriteChild.localScale.x, spriteChild.localScale.y, spriteChild.localScale.z);
			}
			
			if (isTopDown)
			{
				spriteChild.rotation = Quaternion.Euler (90f, 0, 0);
			}
			else
			{
				spriteChild.rotation = Quaternion.Euler (spriteChild.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, spriteChild.rotation.eulerAngles.z);
			}

			if (spriteChild.GetComponent <FollowSortingMap>())
			{
				float spriteScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalScale ();

				if (spriteScale != 0f)
				{
					if (spriteChild.localScale.x > 0f)
					{
						spriteChild.localScale = new Vector3 (spriteScale, spriteScale, spriteScale);
					}
					else
					{
						spriteChild.localScale = new Vector3 (-spriteScale, spriteScale, spriteScale);
					}

					sortingMapScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalSpeed ();
				}
			}
		}


		public void AssignStandardAnimClipFromResource (AnimStandard standard, string clipName)
		{
			AnimationClip clip = AdvGame.FindAnimClipResource (clipName);
			if (clip != null)
			{
				if (standard == AnimStandard.Idle)
				{
					idleAnim = clip;
				}
				else if (standard == AnimStandard.Run)
				{
					runAnim = clip;
				}
				else if (standard == AnimStandard.Walk)
				{
					walkAnim = clip;
				}
				else if (standard == AnimStandard.Talk)
				{
					talkAnim = clip;
				}
			}
		}


		public string GetStandardAnimClipName (AnimStandard standard)
		{
			if (standard == AnimStandard.Idle && idleAnim != null)
			{
				return idleAnim.name;
			}
			else if (standard == AnimStandard.Walk && walkAnim != null)
			{
				return walkAnim.name;
			}
			else if (standard == AnimStandard.Run && runAnim != null)
			{
				return runAnim.name;
			}
			else if (standard == AnimStandard.Talk && talkAnim != null)
			{
				return talkAnim.name;
			}

			return "";
		}
	
	}
}
