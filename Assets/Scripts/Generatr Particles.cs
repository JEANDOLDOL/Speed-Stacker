using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GenerateParticles : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> particleList = new List<ParticleSystem>();
    [SerializeField] GameObject parent;
    
    public void GenerateParticle()
    {
        int particleIndex = Random.Range(0, particleList.Count);
        ParticleSystem particleSystem = particleList[particleIndex];
        Instantiate(particleSystem,parent.transform);
    }
}
