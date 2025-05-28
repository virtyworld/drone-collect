using UnityEngine;
using System.Collections.Generic;

public class SpawnedResource : MonoBehaviour
{
    public bool isFree = true;
    private Queue<DroneAI> waitingDrones = new Queue<DroneAI>();
    private DroneAI currentDrone = null;

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

    public bool IsInQueue(DroneAI drone)
    {
        return waitingDrones.Contains(drone);
    }

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

    public bool IsCurrentDrone(DroneAI drone)
    {
        return currentDrone == drone;
    }

}
