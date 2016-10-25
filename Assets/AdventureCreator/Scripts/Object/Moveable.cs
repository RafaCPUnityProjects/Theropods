/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Moveable.cs"
 * 
 *	This script is attached to any gameObject that is to be transformed
 *	during gameplay via the action ActionTransform.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class Moveable : MonoBehaviour
{

	public bool isMoving { get; set; }
	
	private float moveChangeTime;
	private float moveStartTime;
	
	private MoveMethod moveMethod;
	private TransformType transformType;

	private Vector3 targetVector;
	private	Vector3 startVector;

	
	private void FixedUpdate ()
	{
		if (isMoving)
		{
			if (Time.time < moveStartTime + moveChangeTime)
			{
				if (transformType == TransformType.Translate)
				{
					if (moveMethod == MoveMethod.Linear)
					{
						transform.localPosition = Vector3.Lerp (startVector, targetVector, AdvGame.LinearTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else if (moveMethod == MoveMethod.Smooth)
					{
						transform.localPosition = Vector3.Lerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else
					{
						transform.localPosition = Vector3.Slerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
				}

				else if (transformType == TransformType.Rotate)
				{
					if (moveMethod == MoveMethod.Linear)
					{
						transform.localEulerAngles = Vector3.Lerp (startVector, targetVector, AdvGame.LinearTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else if (moveMethod == MoveMethod.Smooth)
					{
						transform.localEulerAngles = Vector3.Lerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else
					{
						transform.localEulerAngles = Vector3.Slerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
				}
				
				else
				{
					if (moveMethod == MoveMethod.Linear)
					{
						transform.localScale = Vector3.Lerp (startVector, targetVector, AdvGame.LinearTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else if (moveMethod == MoveMethod.Smooth)
					{
						transform.localScale = Vector3.Lerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
					else
					{
						transform.localScale = Vector3.Slerp (startVector, targetVector, AdvGame.SmoothTimeFactor (moveStartTime, moveChangeTime)); 
					}
				}
				
			}
			else
			{
				isMoving = false;
			}
		}
	}
	
	
	public void Move (Vector3 _newVector, MoveMethod _moveMethod, float _transitionTime, TransformType _transformType)
	{
		isMoving = true;
		
		targetVector = _newVector;
		moveMethod = _moveMethod;
		transformType = _transformType;
		
		if (_transformType == TransformType.Translate)
		{
			startVector = transform.localPosition;
		}
		
		else if (_transformType == TransformType.Rotate)
		{
			startVector = transform.localEulerAngles;
		}
		
		else
		{
			startVector = transform.localScale;
		}
		
		moveChangeTime = _transitionTime;
		moveStartTime = Time.time;
	}


	public void Kill ()
	{
		isMoving = false;
	}
	
}
