/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera2D.cs"
 * 
 *	This GameCamera allows scrolling horizontally and vertically without altering perspective.
 *	Based on the work by Eric Haines (Eric5h5) at http://wiki.unity3d.com/index.php?title=OffsetVanishingPoint
 * 
 */

using UnityEngine;
using System.Collections;

public class GameCamera2D : _Camera
{
	
	public bool targetIsPlayer = true;
	public Transform target;

	public bool lockHorizontal = true;
	public bool lockVertical = true;

	public bool limitHorizontal;
	public bool limitVertical;

	public Vector2 constrainHorizontal;
	public Vector2 constrainVertical;
	
	public Vector2 freedom = new Vector2 (0f, 0f);
	public float dampSpeed = 0.9f;

	public Vector2 afterOffset = new Vector2 (0f, 0f);
	
	public Vector2 perspectiveOffset = new Vector2 (0f, 0f);
	private Vector2 desiredOffset = new Vector2 (0f, 0f);
	private SettingsManager settingsManager;

	
	private void Awake ()
	{
		this.GetComponent<Camera>().enabled = false;
		settingsManager = AdvGame.GetReferences ().settingsManager;
	}
	
	
	private void Start ()
	{
		if (targetIsPlayer && GameObject.FindWithTag (Tags.player))
		{
			target = GameObject.FindWithTag (Tags.player).transform;
		}
		
		if (target)
		{
			MoveCameraInstant ();
		}
	}

	
	public void SwitchTarget (Transform _target)
	{
		target = _target;
	}
	
	
	private void Update ()
	{
		if (target)
		{
			MoveCamera ();
		}
	}
	
	
	private void SetDesired ()
	{
		Vector2 targetOffset = GetOffsetForPosition (target.transform.position);
		
		if (targetOffset.x < (perspectiveOffset.x - freedom.x))
		{
			desiredOffset.x = targetOffset.x + freedom.x;
		}
		else if (targetOffset.x > (perspectiveOffset.x + freedom.x))
		{
			desiredOffset.x = targetOffset.x - freedom.x;
		}

		desiredOffset.x += afterOffset.x;
		if (limitHorizontal)
		{
			desiredOffset.x = ConstrainAxis (desiredOffset.x, constrainHorizontal);
		}
		
		if (targetOffset.y < (perspectiveOffset.y - freedom.y))
		{
			desiredOffset.y = targetOffset.y + freedom.y;
		}
		else if (targetOffset.y > (perspectiveOffset.y + freedom.y))
		{
			desiredOffset.y = targetOffset.y - freedom.y;
		}
		
		desiredOffset.y += afterOffset.y;
		if (limitVertical)
		{
			desiredOffset.y = ConstrainAxis (desiredOffset.y, constrainVertical);
		}
	}	
	
	
	public void MoveCamera ()
	{
		if (targetIsPlayer && GameObject.FindWithTag (Tags.player))
		{
			target = GameObject.FindWithTag (Tags.player).transform;
		}
		
		if (target && (!lockHorizontal || !lockVertical))
		{
			SetDesired ();
		
			if (!lockHorizontal)
			{
				perspectiveOffset.x = Mathf.Lerp (perspectiveOffset.x, desiredOffset.x, Time.deltaTime * dampSpeed);
			}
			
			if (!lockVertical)
			{
				perspectiveOffset.y = Mathf.Lerp (perspectiveOffset.y, desiredOffset.y, Time.deltaTime * dampSpeed);
			}
		}
		
		SetProjection ();
	}
	
	
	public override void MoveCameraInstant ()
	{
		if (targetIsPlayer && GameObject.FindWithTag (Tags.player))
		{
			target = GameObject.FindWithTag (Tags.player).transform;
		}
		
		if (target && (!lockHorizontal || !lockVertical))
		{
			SetDesired ();
		
			if (!lockHorizontal)
			{
				perspectiveOffset.x = desiredOffset.x;
			}
			
			if (!lockVertical)
			{
				perspectiveOffset.y = desiredOffset.y;
			}
		}
		
		SetProjection ();
	}


	private void SetProjection ()
	{
		GetComponent<Camera>().projectionMatrix = AdvGame.SetVanishingPoint (this.GetComponent<Camera>(), perspectiveOffset);
	}


	public void SnapToOffset ()
	{
		perspectiveOffset = afterOffset;
		SetProjection ();
	}
	
	
	public IEnumerator ResetProjection ()
	{
		yield return new WaitForFixedUpdate ();
		GetComponent<Camera>().ResetProjectionMatrix ();
	}


	private Vector2 GetOffsetForPosition (Vector3 targetPosition)
	{
		Vector2 targetOffset = new Vector2 ();
		float forwardOffsetScale = 93 - (299 * this.GetComponent<Camera>().nearClipPlane);

		if (settingsManager && settingsManager.IsTopDown ())
		{

			targetOffset.x = - (targetPosition.x - transform.position.x) / (forwardOffsetScale * (targetPosition.y - transform.position.y));
			targetOffset.y = - (targetPosition.z - transform.position.z) / (forwardOffsetScale * (targetPosition.y - transform.position.y));
		}
		else
		{
			targetOffset.x = (targetPosition.x - transform.position.x) / (forwardOffsetScale * (targetPosition.z - transform.position.z));
			targetOffset.y = (targetPosition.y - transform.position.y) / (forwardOffsetScale * (targetPosition.z - transform.position.z));
		}
		
		return targetOffset;
	}


	public void SetCorrectRotation ()
	{
		if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsTopDown ())
		{
			transform.rotation = Quaternion.Euler (90f, 0, 0);
			return;
		}

		transform.rotation = Quaternion.Euler (0, 0, 0);
	}


	public bool IsCorrectRotation ()
	{
		if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsTopDown ())
		{
			if (transform.rotation == Quaternion.Euler (90f, 0, 0))
			{
				return true;
			}

			return false;
		}

		if (transform.rotation == Quaternion.Euler (0, 0, 0))
		{
			return true;
		}

		return false;
	}


	private void OnDestroy ()
	{
		settingsManager = null;
	}
	
}