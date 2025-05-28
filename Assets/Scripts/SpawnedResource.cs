/// <summary>
/// Manages a resource that can be claimed by drones. Implements a queue system for drones waiting to access the resource.
/// Handles resource allocation, release, and queue management for multiple drones.
/// </summary>
using UnityEngine;
using System.Collections.Generic;

public class SpawnedResource : MonoBehaviour
{
    public bool isFree = true;
    private Queue<DroneAI> waitingDrones = new Queue<DroneAI>();
    private DroneAI currentDrone = null;

    /// <summary>
    /// Attempts to claim the resource for a drone. If resource is free, claims it immediately.
    /// Otherwise, adds the drone to the waiting queue.
    /// </summary>
    /// <param name="drone">The drone attempting to claim the resource</param>
    /// <returns>True if resource was claimed successfully, false otherwise</returns>
    public bool TryClaimResource(DroneAI drone)
    {
        if (isFree && currentDrone == null)
        {
            isFree = false;
            currentDrone = drone;
            return true;
        }
        else if (!waitingDrones.Contains(drone))
        {
            waitingDrones.Enqueue(drone);
        }
        return false;
    }

    /// <summary>
    /// Releases the resource and assigns it to the next drone in the waiting queue if any exist.
    /// </summary>
    public void ReleaseResource()
    {
        isFree = true;
        currentDrone = null;

        // If there are waiting drones, give the resource to the next one
        if (waitingDrones.Count > 0)
        {
            DroneAI nextDrone = waitingDrones.Dequeue();
            isFree = false;
            currentDrone = nextDrone;
        }
    }

    /// <summary>
    /// Checks if a specific drone is currently in the waiting queue.
    /// </summary>
    /// <param name="drone">The drone to check</param>
    /// <returns>True if the drone is in the queue, false otherwise</returns>
    public bool IsInQueue(DroneAI drone)
    {
        return waitingDrones.Contains(drone);
    }

    /// <summary>
    /// Removes a specific drone from the waiting queue while preserving the order of other drones.
    /// </summary>
    /// <param name="drone">The drone to remove from the queue</param>
    public void RemoveFromQueue(DroneAI drone)
    {
        if (waitingDrones.Contains(drone))
        {
            var tempQueue = new Queue<DroneAI>();
            while (waitingDrones.Count > 0)
            {
                var d = waitingDrones.Dequeue();
                if (d != drone)
                {
                    tempQueue.Enqueue(d);
                }
            }
            waitingDrones = tempQueue;
        }
    }

    /// <summary>
    /// Checks if a specific drone is currently using the resource.
    /// </summary>
    /// <param name="drone">The drone to check</param>
    /// <returns>True if the drone is currently using the resource, false otherwise</returns>
    public bool IsCurrentDrone(DroneAI drone)
    {
        return currentDrone == drone;
    }

}
