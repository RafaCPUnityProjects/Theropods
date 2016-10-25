/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MainCamera.cs"
 * 
 *	This is attached to the Main Camera, and must be tagged as "MainCamera" to work.
 *	Only one Main Camera should ever exist in the scene.
 *	Shake code adapted from http://www.mikedoesweb.com/2012/camera-shake-in-unity/
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class MainCamera : MonoBehaviour
{
	
	public Texture2D fadeTexture;
	public _Camera attachedCamera;

	[HideInInspector] public _Camera lastNavCamera;
	[HideInInspector] public bool isSmoothChanging;
	
	private bool cursorAffectsRotation;
	
	[HideInInspector] public Vector2 perspectiveOffset = new Vector2 (0f, 0f);
	private Vector2 startPerspectiveOffset = new Vector2 (0f, 0f);

	private float timeToFade = 0f;
	private int drawDepth = -1000;
	private float alpha = 0f; 
	private FadeType fadeType;
	private float fadeStartTime;
	
	private MoveMethod moveMethod;
	private float changeTime;
	
	private	Vector3 startPosition;
	private	Quaternion startRotation;
	private float startFOV;
	private float startOrtho;
	private	float startTime;
	
	private Transform LookAtPos;
	private Vector2 lookAtAmount;
	private float LookAtZ;
	private Vector3 lookAtTarget;
	
	private SettingsManager settingsManager;
	private StateHandler stateHandler;
	private PlayerInput playerInput;
	
	private float shakeDecay;
	private bool shakeMove;
	private float shakeIntensity;
	private Vector3 shakePosition;
	private Vector3 shakeRotation;
	
	
	private void Awake()
	{
		
		if (this.transform.parent.name != "_Cameras")
		{
			if (GameObject.Find ("_Cameras"))
			{
				this.transform.parent = GameObject.Find ("_Cameras").transform;
			}
			else
			{
				this.transform.parent = null;
			}
		}
		
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
		{
			playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		}
		
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}
	}	

	
	private void Start()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}
	
		foreach (Transform child in transform)
		{
			LookAtPos = child;
		}
		
		LookAtZ = LookAtPos.localPosition.z;
	}
	
	
	public void Shake (float _shakeDecay, bool _shakeMove)
	{
		shakePosition = Vector3.zero;
		shakeRotation = Vector3.zero;
		
		shakeMove = _shakeMove;
		shakeDecay = _shakeDecay;
		shakeIntensity = shakeDecay * 150f;
	}
	
	
	public bool IsShaking ()
	{
		if (shakeIntensity > 0f)
		{
			return true;
		}
		
		return false;
	}
	
	
	public void StopShaking ()
	{
		shakeIntensity = 0f;
		shakePosition = Vector3.zero;
		shakeRotation = Vector3.zero;
	}
	
	
	private void FixedUpdate ()
	{
		if (shakeIntensity > 0f)
		{
			if (shakeMove)
			{
				shakePosition = Random.insideUnitSphere * shakeIntensity * 0.5f;
			}
			
			shakeRotation = new Vector3
			(
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f
			);
			
			shakeIntensity -= shakeDecay;
		}
		
		else if (shakeIntensity < 0f)
		{
			StopShaking ();
		}
	}
	
	
	private void Update ()
	{
		if (stateHandler)
		{
			if (stateHandler.gameState == GameState.Normal)
			{
				SetFirstPerson ();
			}
			
			if (this.GetComponent <AudioListener>())
			{
				if (stateHandler.gameState == GameState.Paused)
				{
					AudioListener.pause = true;
				}
				else
				{
					AudioListener.pause = false;
				}
			}
		}
	}
	
	
	public void PrepareForBackground ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;

			GetComponent<Camera>().clearFlags = CameraClearFlags.Depth;
			
			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				GetComponent<Camera>().cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}
		else
		{
			Debug.LogError ("Could not find a Settings Manager - please set one using the main Adventure Creator window.");
		}
	}
	
	
	public void RemoveBackground ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
			
			GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
			
			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				GetComponent<Camera>().cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}
		else
		{
			Debug.LogError ("Could not find a Settings Manager - please set one using the main Adventure Creator window.");
		}
	}

	
	public void SetFirstPerson ()
	{
		if (settingsManager)
		{
			if (settingsManager.movementMethod == MovementMethod.FirstPerson)
			{
				SetGameCamera (GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <_Camera>());
				SnapToAttached ();
			}
		}

		if (attachedCamera)
		{
			lastNavCamera = attachedCamera;
		}
	}
	
	
	private void OnGUI()
	{
		if (timeToFade > 0f)
		{
			alpha = (Time.time - fadeStartTime) / timeToFade;

			if (fadeType == FadeType.fadeIn)
			{
				alpha = 1 - alpha;
			}

			alpha = Mathf.Clamp01 (alpha);
		

			if (Time.time > (fadeStartTime + timeToFade))
			{
				timeToFade = 0f;
			}
		}

		if (alpha > 0f)
		{
			Color tempColor = GUI.color;
			tempColor.a = alpha;
			GUI.color = tempColor;
			GUI.depth = drawDepth;
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeTexture);
		}
	}
	
	
	public void ResetProjection ()
	{
		if (GetComponent<Camera>())
		{
			perspectiveOffset = Vector2.zero;
			GetComponent<Camera>().projectionMatrix = AdvGame.SetVanishingPoint (GetComponent<Camera>(), perspectiveOffset);
			GetComponent<Camera>().ResetProjectionMatrix ();
		}
	}


	public void ResetMoving ()
	{
		isSmoothChanging = false;
		startTime = 0f;
		changeTime = 0f;
	}

	
	private void LateUpdate ()
	{
		if (attachedCamera && (!(attachedCamera is GameCamera25D)))
		{
			if (!isSmoothChanging)
			{
				if (attachedCamera is GameCamera2D)
				{
					GameCamera2D cam2D = (GameCamera2D) attachedCamera;
					perspectiveOffset = cam2D.perspectiveOffset;
					GetComponent<Camera>().projectionMatrix = AdvGame.SetVanishingPoint (GetComponent<Camera>(), perspectiveOffset);
				}
				
				else
				{
					GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
					transform.rotation = attachedCamera.transform.rotation;
					transform.position = attachedCamera.transform.position;
					
					if (cursorAffectsRotation)
					{
						SetLookAtPosition ();
						transform.LookAt (LookAtPos);
					}
				}
			}
			else
			{
				// Move from one GameCamera to another
				if (Time.time < startTime + changeTime)
				{
					if (attachedCamera is GameCamera2D)
					{
						GameCamera2D cam2D = (GameCamera2D) attachedCamera;
						
						if (moveMethod == MoveMethod.Linear)
						{
							perspectiveOffset.x = Mathf.Lerp (startPerspectiveOffset.x, cam2D.perspectiveOffset.x, AdvGame.LinearTimeFactor (startTime, changeTime));
							perspectiveOffset.y = Mathf.Lerp (startPerspectiveOffset.y, cam2D.perspectiveOffset.y, AdvGame.LinearTimeFactor (startTime, changeTime));
						}
						else
						{
							perspectiveOffset.x = Mathf.Lerp (startPerspectiveOffset.x, cam2D.perspectiveOffset.x, AdvGame.SmoothTimeFactor (startTime, changeTime));
							perspectiveOffset.y = Mathf.Lerp (startPerspectiveOffset.y, cam2D.perspectiveOffset.y, AdvGame.SmoothTimeFactor (startTime, changeTime));
						}
						
						GetComponent<Camera>().ResetProjectionMatrix ();
					}
					
					if (moveMethod == MoveMethod.Linear)
					{
						transform.position = Vector3.Lerp (startPosition, attachedCamera.transform.position, AdvGame.LinearTimeFactor (startTime, changeTime)); 
						transform.rotation = Quaternion.Lerp (startRotation, attachedCamera.transform.rotation, AdvGame.LinearTimeFactor (startTime, changeTime));
						GetComponent <Camera>().fieldOfView = Mathf.Lerp (startFOV, attachedCamera.GetComponent <Camera>().fieldOfView, AdvGame.LinearTimeFactor (startTime, changeTime));
						GetComponent <Camera>().orthographicSize = Mathf.Lerp (startOrtho, attachedCamera.GetComponent <Camera>().orthographicSize, AdvGame.LinearTimeFactor (startTime, changeTime));
					}
					else if (moveMethod == MoveMethod.Smooth)
					{
						transform.position = Vector3.Lerp (startPosition, attachedCamera.transform.position, AdvGame.SmoothTimeFactor (startTime, changeTime)); 
						transform.rotation = Quaternion.Lerp (startRotation, attachedCamera.transform.rotation, AdvGame.SmoothTimeFactor (startTime, changeTime));
						GetComponent <Camera>().fieldOfView = Mathf.Lerp (startFOV, attachedCamera.GetComponent <Camera>().fieldOfView, AdvGame.SmoothTimeFactor (startTime, changeTime));
						GetComponent <Camera>().orthographicSize = Mathf.Lerp (startOrtho, attachedCamera.GetComponent <Camera>().orthographicSize, AdvGame.SmoothTimeFactor (startTime, changeTime));
					}
					else
					{
						// Don't slerp y position as this will create a "bump" effect
						Vector3 newPosition = Vector3.Slerp (startPosition, attachedCamera.transform.position, AdvGame.SmoothTimeFactor (startTime, changeTime)); 
						newPosition.y = Mathf.Lerp (startPosition.y, attachedCamera.transform.position.y, AdvGame.SmoothTimeFactor (startTime, changeTime));
						transform.position = newPosition;
						
						transform.rotation = Quaternion.Slerp (startRotation, attachedCamera.transform.rotation, AdvGame.SmoothTimeFactor (startTime, changeTime));
						GetComponent <Camera>().fieldOfView = Mathf.Lerp (startFOV, attachedCamera.GetComponent <Camera>().fieldOfView, AdvGame.SmoothTimeFactor (startTime, changeTime));
						GetComponent <Camera>().orthographicSize = Mathf.Lerp (startOrtho, attachedCamera.GetComponent <Camera>().orthographicSize, AdvGame.SmoothTimeFactor (startTime, changeTime));
					}

					if (attachedCamera is GameCamera2D)
					{
						GetComponent<Camera>().projectionMatrix = AdvGame.SetVanishingPoint (GetComponent<Camera>(), perspectiveOffset);
					}
				}
				else
				{
					LookAtCentre ();
					isSmoothChanging = false;
				}
			}
			
			if (cursorAffectsRotation)
			{
				LookAtPos.localPosition = Vector3.Lerp (LookAtPos.localPosition, lookAtTarget, Time.deltaTime * 3f);	
			}
		}
		
		else if (attachedCamera && (attachedCamera is GameCamera25D))
		{
			transform.position = attachedCamera.transform.position;
			transform.rotation = attachedCamera.transform.rotation;
		}
		
		transform.position += shakePosition;
		transform.localEulerAngles += shakeRotation;
		
	}

	
	private void LookAtCentre ()
	{
		if (LookAtPos)
		{
			lookAtTarget = new Vector3 (0, 0, LookAtZ);
		}
	}
	

	private void SetLookAtPosition ()
	{
		if (stateHandler.gameState == GameState.Normal)
		{
			Vector2 mouseOffset = new Vector2 (playerInput.mousePosition.x / (Screen.width / 2) - 1, playerInput.mousePosition.y / (Screen.height / 2) - 1);
			float distFromCentre = mouseOffset.magnitude;
	
			if (distFromCentre < 1.4f)
			{
				lookAtTarget = new Vector3 (mouseOffset.x * lookAtAmount.x, mouseOffset.y * lookAtAmount.y, LookAtZ);
			}
		}
	}
	
	
	public void SnapToAttached ()
	{
		if (attachedCamera && attachedCamera.GetComponent <Camera>())
		{
			LookAtCentre ();
			isSmoothChanging = false;
			
			GetComponent <Camera>().orthographic = attachedCamera.GetComponent <Camera>().orthographic;
			GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
			transform.position = attachedCamera.transform.position;
			transform.rotation = attachedCamera.transform.rotation;
			
			if (attachedCamera is GameCamera2D)
			{
				GameCamera2D cam2D = (GameCamera2D) attachedCamera;
				perspectiveOffset = cam2D.perspectiveOffset;
			}
			else
			{
				perspectiveOffset = new Vector2 (0f, 0f);
			}
		}
	}
	
	
	public void SmoothChange (float _changeTime, MoveMethod method)
	{
		LookAtCentre ();
		moveMethod = method;
		isSmoothChanging = true;
		
		startTime = Time.time;
		changeTime = _changeTime;
		
		startPosition = transform.position;
		startRotation = transform.rotation;
		startFOV = GetComponent <Camera>().fieldOfView;
		startOrtho = GetComponent <Camera>().orthographicSize;
		
		startPerspectiveOffset = perspectiveOffset;
	}
	
	
	public void SetGameCamera (_Camera _camera)
	{
		GetComponent<Camera>().ResetProjectionMatrix ();
		attachedCamera = _camera;
		
		if (attachedCamera && attachedCamera.GetComponent <Camera>())
		{
			this.GetComponent <Camera>().farClipPlane = attachedCamera.GetComponent <Camera>().farClipPlane;
			this.GetComponent <Camera>().nearClipPlane = attachedCamera.GetComponent <Camera>().nearClipPlane;
			
			// Set projection
			if (this.GetComponent <Camera>().orthographic != attachedCamera.GetComponent <Camera>().orthographic)
			{
				this.GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
				this.GetComponent <Camera>().orthographicSize = attachedCamera.GetComponent <Camera>().orthographicSize;
				this.GetComponent <Camera>().orthographic = attachedCamera.GetComponent <Camera>().orthographic;
			}
		}
		
		// Set LookAt
		if (attachedCamera is GameCamera)
		{
			GameCamera gameCam = (GameCamera) attachedCamera;
			cursorAffectsRotation = gameCam.followCursor;
			lookAtAmount = gameCam.cursorInfluence;
			RemoveBackground ();
		}
		else
		{
			cursorAffectsRotation = false;
			RemoveBackground ();
		}
		
		// Set background
		if (attachedCamera is GameCamera25D)
		{
			GameCamera25D cam25D = (GameCamera25D) attachedCamera;
			cam25D.SetActiveBackground ();
		}
		
		// TransparencySortMode
		if (attachedCamera is GameCamera2D)
		{
			this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Orthographic;
		}
		else if (attachedCamera)
		{
			if (attachedCamera.GetComponent <Camera>().orthographic)
			{
				this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Orthographic;
			}
			else
			{
				this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Perspective;
			}
		}
	}
	
	
	public void FadeIn (float _timeToFade)
	{
		timeToFade = _timeToFade;
		alpha = 1f;
		fadeType = FadeType.fadeIn;
		fadeStartTime = Time.time;
	}

	
	public void FadeOut (float _timeToFade)
	{
		timeToFade = _timeToFade;
		alpha = 0f;
		fadeType = FadeType.fadeOut;
		fadeStartTime = Time.time;
	}
	
	
	public bool isFading ()
	{
		if (fadeType == FadeType.fadeOut && alpha < 1f)
		{
			return true;
		}
		else if (fadeType == FadeType.fadeIn && alpha > 0f)
		{
			return true;
		}

		return false;
	}

	
	public void OnDeserializing ()
	{
		FadeIn (0.5f);
	}
	
	
	public Vector3 PositionRelativeToCamera (Vector3 _position)
	{
		return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
	}
	
	
	public Vector3 RightVector ()
	{
		return (transform.right);
	}
	
	
	public Vector3 ForwardVector ()
	{
		Vector3 camForward;
		
		camForward = transform.forward;
		camForward.y = 0;
		
		return (camForward);
	}
	
}
