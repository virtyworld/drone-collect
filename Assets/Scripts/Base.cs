using UnityEngine;

public class Base : MonoBehaviour
{
    [SerializeField] private ParticleSystem unloadParticles;
    [SerializeField] private float scaleEffectDuration = 0.5f;
    [SerializeField] private float scaleEffectMultiplier = 1.2f;

    private int collectedResources = 0;
    private Vector3 originalScale;
    private float scaleEffectTimer = 0f;
    private bool isScaling = false;

    private void Start()
    {
        originalScale = transform.localScale;

        // Create particle system if not assigned
        if (unloadParticles == null)
        {
            GameObject particlesObj = new GameObject("UnloadParticles");
            particlesObj.transform.SetParent(transform);
            particlesObj.transform.localPosition = Vector3.zero;

            unloadParticles = particlesObj.AddComponent<ParticleSystem>();
            var main = unloadParticles.main;
            main.startColor = GetComponent<MeshRenderer>().material.color;
            main.startSize = 0.2f;
            main.startSpeed = 2f;
            main.duration = 1f;
            main.loop = false;

            var emission = unloadParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shape = unloadParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
        }
    }

    private void Update()
    {
        if (isScaling)
        {
            scaleEffectTimer += Time.deltaTime;
            float progress = scaleEffectTimer / scaleEffectDuration;

            if (progress >= 1f)
            {
                transform.localScale = originalScale;
                isScaling = false;
            }
            else
            {
                float scale = Mathf.Lerp(scaleEffectMultiplier, 1f, progress);
                transform.localScale = originalScale * scale;
            }
        }
    }

    public void OnResourceUnloaded()
    {
        collectedResources++;

        // Play particle effect
        if (unloadParticles != null)
        {
            unloadParticles.Play();
        }

        // Start scale effect
        isScaling = true;
        scaleEffectTimer = 0f;
    }

    public int GetCollectedResources()
    {
        return collectedResources;
    }
}