/// <summary>
/// Represents a base/headquarters in the game that can collect and store resources.
/// Handles resource collection tracking and visual effects when resources are unloaded.
/// </summary>
using UnityEngine;
using UnityEngine.Events;
public class Base : MonoBehaviour
{
    [SerializeField] private ParticleSystem unloadParticles;

    public int factionId;
    public int collectedResources = 0;

    /// <summary>
    /// Initializes the base with a faction ID and sets up resource unloading event handling
    /// </summary>
    /// <param name="factionId">The ID of the faction this base belongs to</param>
    /// <param name="onResourceUnloaded">Event that triggers when resources are unloaded</param>
    public void Setup(int factionId, UnityEvent<int, int> onResourceUnloaded)
    {
        this.factionId = factionId;
        onResourceUnloaded.AddListener(OnResourceUnloaded);
    }

    /// <summary>
    /// Handles the resource unloading event, playing particle effects if the resources belong to this base's faction
    /// </summary>
    /// <param name="factionId">The ID of the faction unloading resources</param>
    /// <param name="amount">The amount of resources being unloaded</param>
    public void OnResourceUnloaded(int factionId, int amount)
    {
        if (factionId == this.factionId)
        {
            // Play particle effect
            if (unloadParticles != null)
            {
                unloadParticles.Play();
            }
        }
    }
}