using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    public Transform[] spawnPoints;

    private Texture2D _softParticleTexture;

    private void Awake()
    {
        _softParticleTexture = CreateSoftParticleTexture();
    }

    public void SpawnZombies(int waveNumber)
    {
        int bossCount = 0;
        int miniBossCount = 0;

        if (waveNumber == 5)
        {
            miniBossCount = 1;
        }
        else if (waveNumber == 10)
        {
            bossCount = 1;
            miniBossCount = 2;
        }
        else if (waveNumber == 15)
        {
            miniBossCount = 2;
        }
        else if (waveNumber == 20)
        {
            bossCount = 1;
            miniBossCount = 4;
        }
        else if (waveNumber == 25)
        {
            miniBossCount = 4;
        }
        else if (waveNumber == 30)
        {
            bossCount = 1;
            miniBossCount = 6;
        }

        int normalZombies = 5 + (waveNumber * 2);
        int totalZombies = normalZombies + bossCount + miniBossCount;

        WaveManager.Instance.currentZombiesAlive = totalZombies;

        float baseHealth = 100f + (waveNumber * 5f);
        float baseDamage = 15f + (waveNumber * 1.5f);
        float baseSpeed = 1.5f + (waveNumber * 0.1f);

        int spawnedBosses = 0;
        int spawnedMiniBosses = 0;

        for (int i = 0; i < totalZombies; i++)
        {
            Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject selectedZombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            GameObject spawnedZombie = Instantiate(selectedZombiePrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

            float finalHealth = baseHealth;
            float finalDamage = baseDamage;
            float finalSpeed = baseSpeed;

            if (spawnedBosses < bossCount)
            {
                finalHealth *= 4f;
                finalDamage *= 2.5f;
                finalSpeed *= 1.2f;
                spawnedZombie.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);

                SetAgentColor(spawnedZombie, Color.red);
                AddAura(spawnedZombie, Color.red, 2.5f);

                spawnedBosses++;
            }
            else if (spawnedMiniBosses < miniBossCount)
            {
                finalHealth *= 2f;
                finalDamage *= 1.5f;
                finalSpeed *= 1.1f;
                spawnedZombie.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

                Color orange = new Color(1f, 0.5f, 0f);
                SetAgentColor(spawnedZombie, orange);
                AddAura(spawnedZombie, orange, 1.8f);

                spawnedMiniBosses++;
            }
            else
            {
                float randomScale = Random.Range(0.85f, 1.15f);
                spawnedZombie.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                finalHealth *= Random.Range(0.8f, 1.2f);
                finalDamage *= Random.Range(0.8f, 1.2f);
                finalSpeed *= Random.Range(0.8f, 1.2f);

                if (Random.value > 0.5f)
                {
                    Color[] mutedColors = new Color[] {
                        new Color(0.4f, 0.6f, 0.4f, 1f),
                        new Color(0.5f, 0.5f, 0.6f, 1f),
                        new Color(0.6f, 0.5f, 0.4f, 1f)
                    };
                    SetAgentColor(spawnedZombie, mutedColors[Random.Range(0, mutedColors.Length)]);
                }
            }

            ZombieHealth health = spawnedZombie.GetComponent<ZombieHealth>();
            if (health != null) health.Initialize(finalHealth);

            ZombieAI ai = spawnedZombie.GetComponent<ZombieAI>();
            if (ai != null) ai.Initialize(finalDamage, finalSpeed);
        }
    }

    private void SetAgentColor(GameObject target, Color color)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer is ParticleSystemRenderer) continue;

            renderer.material.color = color;
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
    }

    private void AddAura(GameObject target, Color auraColor, float radius)
    {
        GameObject auraObj = new GameObject("AuraEffect");
        auraObj.transform.SetParent(target.transform);
        auraObj.transform.localPosition = new Vector3(0, 1f, 0); 

        ParticleSystem ps = auraObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startColor = new Color(auraColor.r, auraColor.g, auraColor.b, 0.4f); 
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 2.5f); 
        main.startSpeed = 0.02f; 
        main.startLifetime = 4f; 
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.loop = true;

        var emission = ps.emission;
        emission.rateOverTime = 8f; 

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere; 
        shape.radius = radius;
        shape.radiusThickness = 0.1f; 

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.2f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.2f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer psRenderer = auraObj.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                Material particleMat = new Material(shader);
                if (_softParticleTexture != null)
                {
                    particleMat.mainTexture = _softParticleTexture;
                }
                psRenderer.material = particleMat;
            }
        }
    }

    private Texture2D CreateSoftParticleTexture()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                alpha = alpha * alpha; 
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();
        return texture;
    }
}
