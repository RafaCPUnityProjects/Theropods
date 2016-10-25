/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Camera.cs"
 * 
 *	This is the base class for GameCamera and FirstPersonCamera.
 * 
 */


using UnityEngine;
using System.Collections;

public class _Camera : MonoBehaviour
{
	
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
	
	
	public virtual void MoveCameraInstant ()
	{ 	}
	
	
	protected float ConstrainAxis (float desired, Vector2 range)
	{
		if (range.x < range.y)
		{
			desired = Mathf.Clamp (desired, range.x, range.y);
		}
		
		else if (range.x > range.y)
		{
			desired = Mathf.Clamp (desired, range.y, range.x);
		}
		
		else
		{
			desired = range.x;
		}
			
		return desired;
	}
	
}
