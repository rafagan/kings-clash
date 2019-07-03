﻿using UnityEngine;
using UnitySteer.Helpers;

/// <summary>
/// Post-processing beahavior that ensures that the vehicle always moves 
/// at at least a minimum speed
/// </summary>
/// <remarks>
/// This could also be done as a property of TickedVehicle, but adding it
/// as a post-processing behavior for now to have it be more modular.
/// </remarks>
[AddComponentMenu("UnitySteer/Steer/... for Minimum Speed (Post-process)")]
public class SteerForMinimumSpeed : Steering
{
	[SerializeField]
	float _minimumSpeed = 4;

	public override bool IsPostProcess 
	{
		get { return true; }
	}


	/// <summary>
	/// Calculates the force to apply to the vehicle to reach a point
	/// </summary>
	/// <returns>
	/// A <see cref="Vector3"/>
	/// </returns>
	protected override Vector3 CalculateForce()
	{
		Vector3 result = Vector3.zero;
		if (Vehicle.DesiredSpeed < _minimumSpeed)
		{
			result = Vehicle.DesiredVelocity.normalized * _minimumSpeed;
		}
		return result;
	}
}
