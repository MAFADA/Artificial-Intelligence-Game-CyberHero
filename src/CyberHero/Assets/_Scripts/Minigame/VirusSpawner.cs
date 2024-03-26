using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawner : MonoBehaviour
{
    [SerializeField] Virus virusPrefab;
    [SerializeField] float minSpawnInterval;
    [SerializeField] float maxSpawnInterval;
    [SerializeField] Wayponts waypointsSpawner;

    float timer;
    Vector2 virusSpawnPos;
    Quaternion virusRotation;


    void Start()
    {
        virusSpawnPos = this.transform.position;
        virusRotation = Quaternion.Euler(0, 0, 0);

    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0)
        {
            timer = Random.Range(minSpawnInterval, maxSpawnInterval);

            var virus = Instantiate(
                virusPrefab,
                virusSpawnPos,
                virusRotation);
            virus.SetWaypoint(waypointsSpawner);
            virus.SetUpDistanceLimit(30);

            return;
        }

        timer -= Time.deltaTime;
    }
}
