﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Destructible2D.Examples
{
	/// <summary>This component allows you to create moving targets that randomly flip around to become indestructible.</summary>
	[ExecuteInEditMode]
	[HelpURL(D2dHelper.HelpUrlPrefix + "D2dGalleryTarget")]
	[AddComponentMenu(D2dHelper.ComponentMenuPrefix + "Gallery Target")]
	public class D2dGalleryTarget : MonoBehaviour
	{
		[Tooltip("Is the target facing forward?")]
		public bool FrontShowing;

		[Tooltip("How fast the target can flip sides")]
		public float FlipSpeed = 10.0f;

		[Tooltip("The minimum time the target can face forward in seconds")]
		public float FrontTimeMin = 1.0f;

		[Tooltip("The maximum time the target can face forward in seconds")]
		public float FrontTimeMax = 2.0f;

		[Tooltip("The minimum time the target can be hidden in seconds")]
		public float BackTimeMin = 1.0f;

		[Tooltip("The maximum time the target can be hidden in seconds")]
		public float BackTimeMax = 10.0f;

		[Tooltip("The start position of the target in local space")]
		public Vector3 StartPosition;

		[Tooltip("The end position of the target in local space")]
		public Vector3 EndPosition;

		[Tooltip("The current movement progress in local space")]
		public float MoveProgress;

		[Tooltip("The maximum speed this target can move in local space")]
		public float MoveSpeed;

		[Tooltip("The destructible of this target")]
		public D2dDestructible Destructible;

		// Seconds until a flip
		private float cooldown;

		// Current angle of the flipping
		private float angle;

		protected virtual void Awake()
		{
			ResetCooldown();
		}

		protected virtual void Update()
		{
			// Update flipping if the game is playing
			if (Application.isPlaying == true)
			{
				cooldown -= Time.deltaTime;

				// Flip?
				if (cooldown <= 0.0f)
				{
					FrontShowing = !FrontShowing;

					ResetCooldown();
				}
			}

			// Get target angle based on flip state
			var targetAngle = FrontShowing == true ? 0.0f : 180.0f;

			// Slowly rotate to the target angle if the game is playing
			if (Application.isPlaying == true)
			{
				var factor = D2dHelper.DampenFactor(FlipSpeed, Time.deltaTime);

				angle = Mathf.Lerp(angle, targetAngle, factor);
			}
			// Instantly rotate if it's not
			else
			{
				angle = targetAngle;
			}

			transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);

			// Make the destructible indestructible if it's past 90 degrees
			if (Destructible != null)
			{
				Destructible.Indestructible = targetAngle >= 90.0f;
			}

			// Update movement
			if (Application.isPlaying == true)
			{
				MoveProgress += MoveSpeed * Time.deltaTime;
			}

			var moveDistance = (EndPosition - StartPosition).magnitude;

			if (moveDistance > 0.0f)
			{
				var progress01 = Mathf.PingPong(MoveProgress / moveDistance, 1.0f);

				transform.localPosition = Vector3.Lerp(StartPosition, EndPosition, Mathf.SmoothStep(0.0f, 1.0f, progress01));
			}

		}

		protected virtual void OnDrawGizmosSelected()
		{
			if (transform.parent != null)
			{
				Gizmos.matrix = transform.parent.localToWorldMatrix;
			}

			Gizmos.DrawLine(StartPosition, EndPosition);
		}

		private void ResetCooldown()
		{
			if (FrontShowing == true)
			{
				cooldown = Random.Range(FrontTimeMin, FrontTimeMax);
			}
			else
			{
				cooldown = Random.Range(BackTimeMin, BackTimeMax);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Destructible2D.Examples
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(D2dGalleryTarget))]
	public class D2dMovingTarget_Editor : D2dEditor<D2dGalleryTarget>
	{
		protected override void OnInspector()
		{
			Draw("FrontShowing");
			Draw("FlipSpeed");
			BeginError(Any(t => t.FrontTimeMin < 0.0f || (t.FrontTimeMin > t.FrontTimeMax)));
				Draw("FrontTimeMin");
				Draw("FrontTimeMax");
			EndError();
			BeginError(Any(t => t.BackTimeMin < 0.0f || (t.BackTimeMin > t.BackTimeMax)));
				Draw("BackTimeMin");
				Draw("BackTimeMax");
			EndError();
			BeginError(Any(t => t.StartPosition == t.EndPosition));
				Draw("StartPosition");
				Draw("EndPosition");
			EndError();
			Draw("MoveProgress");
			Draw("MoveSpeed");
			BeginError(Any(t => t.Destructible == null));
				Draw("Destructible");
			EndError();
		}
	}
}
#endif