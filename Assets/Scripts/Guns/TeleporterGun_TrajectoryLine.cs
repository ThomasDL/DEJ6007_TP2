using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterGun_TrajectoryLine : MonoBehaviour
{
    [SerializeField] private PlayerController_test _player;
    LineRenderer lineRenderer;

    public Transform bulletSpawnPoint; // The point from which the bullet will be fired
    [HideInInspector] public float bulletSpeed; // The initial force that will be applied to the bullet
    public int resolution = 30; // How many points will be calculated for the line

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        bulletSpeed = _player.gunList[_player.currentGun].bulletSpeed;
        timeStep = Time.fixedDeltaTime;
    }

    void Update()
    {
        DrawTrajectory();
    }

    void DrawTrajectory()
    {
        Vector3[] points = new Vector3[resolution];
        lineRenderer.positionCount = resolution;

        Vector3 startingPoint = bulletSpawnPoint.position;
        Vector3 startingVelocity = bulletSpawnPoint.up * bulletSpeed;

        for (int i = 0; i < resolution; i++)
        {
            float time = i * 0.1f;
            points[i] = startingPoint + startingVelocity * time + Physics.gravity * time * time / 2f;
            if (points[i].y < 0)
            {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public float simulationTime = 2.0f; // Duration of the simulation
    public float timeStep; // Time interval between each simulation step

    private Vector3[] trajectoryPoints;
    private int numberOfPoints;

    public void SimulateTrajectory(float launchSpeed)
    {
        Vector3 initialVelocity = bulletSpawnPoint.up * launchSpeed;
        Vector3 currentPosition = bulletSpawnPoint.position;
        Vector3 currentVelocity = initialVelocity;

        numberOfPoints = Mathf.CeilToInt(simulationTime / timeStep);
        trajectoryPoints = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            trajectoryPoints[i] = currentPosition;

            // Apply gravity
            currentVelocity += Physics.gravity * timeStep;
            // Update position
            currentPosition += currentVelocity * timeStep;
        }
    }

    // Call this method to get the calculated trajectory points
    public Vector3[] GetTrajectoryPoints()
    {
        return trajectoryPoints;
    }
}