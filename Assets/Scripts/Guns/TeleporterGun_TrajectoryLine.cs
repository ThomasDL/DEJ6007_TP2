using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterGun_TrajectoryLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public Transform bulletSpawnPoint; // The point from which the bullet will be fired
    [SerializeField] private float launchForce = 32f; // The initial force that will be applied to the bullet
    [SerializeField] private int resolution = 30; // How many points will be calculated for the line

    private PlayerController_test _playerController;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        lineRenderer.enabled = false;
        _playerController = GameObject.Find("Player_test_achraf").GetComponent<PlayerController_test>();
    }

    void Update()
    {
        //Only draw trajectory if CanShoot is true
        if (_playerController.CanShoot)
        {
            lineRenderer.enabled = true;
            DrawTrajectory();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawTrajectory()
    {
        Vector3[] points = new Vector3[resolution];
        lineRenderer.positionCount = resolution;

        Vector3 startingPoint = bulletSpawnPoint.position;
        Vector3 startingVelocity = bulletSpawnPoint.forward * launchForce;

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
}