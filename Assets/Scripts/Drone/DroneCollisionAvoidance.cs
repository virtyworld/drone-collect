/// <summary>
/// Handles collision avoidance for drones by detecting nearby drones and calculating avoidance forces.
/// Uses prediction-based avoidance to prevent collisions and maintain safe distances between drones.
/// </summary>
using UnityEngine;
using System.Collections.Generic;

public class DroneCollisionAvoidance : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private float minSeparationDistance = 1f;
    [SerializeField] private float maxAvoidanceForce = 5f;
    [SerializeField] private LayerMask droneLayer;
    [SerializeField] private float predictionTime = 1f;

    private DroneMovement droneMovement;
    private DroneAI droneAI;
    private List<Transform> nearbyDrones = new List<Transform>();
    private Vector3 avoidanceForce;

    /// <summary>
    /// Initializes required components and validates their presence
    /// </summary>
    private void Start()
    {
        droneMovement = GetComponent<DroneMovement>();
        droneAI = GetComponent<DroneAI>();

        if (droneMovement == null)
        {
            Debug.LogError("DroneMovement component not found on " + gameObject.name);
        }
        if (droneAI == null)
        {
            Debug.LogError("DroneAI component not found on " + gameObject.name);
        }
    }

    /// <summary>
    /// Main update loop that handles collision avoidance
    /// </summary>
    private void Update()
    {
        FindNearbyDrones();
        CalculateAvoidanceForce();
        ApplyAvoidanceForce();
    }

    /// <summary>
    /// Detects and stores nearby drones within the detection radius
    /// </summary>
    private void FindNearbyDrones()
    {
        nearbyDrones.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, droneLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                nearbyDrones.Add(hitCollider.transform);
            }
        }
    }

    /// <summary>
    /// Calculates avoidance force based on nearby drones' positions and velocities
    /// </summary>
    private void CalculateAvoidanceForce()
    {
        avoidanceForce = Vector3.zero;

        foreach (var drone in nearbyDrones)
        {
            Vector3 relativePosition = drone.position - transform.position;
            float distance = relativePosition.magnitude;

            if (distance < minSeparationDistance)
            {
                // Calculate predicted position
                DroneMovement otherDroneMovement = drone.GetComponent<DroneMovement>();
                Vector3 otherVelocity = otherDroneMovement != null ? otherDroneMovement.GetVelocity() : Vector3.zero;
                Vector3 predictedPosition = drone.position + otherVelocity * predictionTime;

                // Calculate avoidance direction
                Vector3 avoidanceDirection = (transform.position - predictedPosition).normalized;
                float forceMagnitude = Mathf.Clamp01(1f - (distance / minSeparationDistance)) * maxAvoidanceForce;

                avoidanceForce += avoidanceDirection * forceMagnitude;
            }
        }
    }

    /// <summary>
    /// Applies the calculated avoidance force to the drone's movement
    /// </summary>
    private void ApplyAvoidanceForce()
    {
        if (avoidanceForce != Vector3.zero)
        {
            // Normalize the force if it's too strong
            if (avoidanceForce.magnitude > maxAvoidanceForce)
            {
                avoidanceForce = avoidanceForce.normalized * maxAvoidanceForce;
            }

            // Apply the avoidance force to the drone's movement
            if (droneMovement != null)
            {
                droneMovement.AddAvoidanceForce(avoidanceForce);
            }
        }
    }

    /// <summary>
    /// Visualizes the detection radius, minimum separation distance, and avoidance force in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualize minimum separation distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minSeparationDistance);

        // Visualize avoidance force
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, avoidanceForce);
        }
    }
}
