using UnityEngine;
using UnityEngine.Events;
public class Base : MonoBehaviour
{
    [SerializeField] private ParticleSystem unloadParticles;
    [SerializeField] private float scaleEffectDuration = 0.5f;
    [SerializeField] private float scaleEffectMultiplier = 1.2f;

    public int factionId;
    public int collectedResources = 0;

    public void Setup(int factionId, UnityEvent<int, int> onResourceUnloaded)
    {
        this.factionId = factionId;
        onResourceUnloaded.AddListener(OnResourceUnloaded);
    }

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