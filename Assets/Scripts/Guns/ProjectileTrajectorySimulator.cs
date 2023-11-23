using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ProjectileTrajectorySimulator : MonoBehaviour
{
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private GameObject ghostProjectilePrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxPhysicsFrameIterations = 50;
    //[SerializeField] private float simulationStep = 0.02f; // Similar to FixedUpdate time step


    private Scene simulationScene;
    private PhysicsScene physicsScene;
    private Dictionary<Transform, Transform> ghostObjectsMap = new Dictionary<Transform, Transform>();


    void Awake()
    {
        CreatePhysicsScene();
        ReplicateObstacles();
    }

    void CreatePhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulationScene.GetPhysicsScene();
    }

    void ReplicateObstacles()
    {
        foreach (Transform child in obstaclesParent)
        {
            GameObject ghostObject = Instantiate(child.gameObject, child.position, child.rotation);
            SceneManager.MoveGameObjectToScene(ghostObject, simulationScene);
            ghostObject.GetComponent<Renderer>().enabled = false;
            ghostObjectsMap.Add(child, ghostObject.transform);
        }
    }

    public void SimulateTrajectory(Vector3 startPosition)
    {
        TeleporterBulletBehavior_test ghostProjectile = Instantiate(ghostProjectilePrefab, startPosition, Camera.main.transform.rotation).GetComponent<TeleporterBulletBehavior_test>();
        SceneManager.MoveGameObjectToScene(ghostProjectile.gameObject, simulationScene);

        //ghostProjectile.GetComponent<Renderer>().enabled = false;
        ghostProjectile.Shoot_TP_Bullet();

        lineRenderer.positionCount = maxPhysicsFrameIterations;

        for (int i = 0; i < maxPhysicsFrameIterations; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime * 2);
            lineRenderer.SetPosition(i, ghostProjectile.transform.position);
        }

        Destroy(ghostProjectile.gameObject);
    }


    void Update()
    {
        // Update ghost objects position to reflect any movement in the main scene
        foreach (var item in ghostObjectsMap)
        {
            if (!item.Key.gameObject.isStatic)
            {
                item.Value.position = item.Key.position;
                item.Value.rotation = item.Key.rotation;
            }
        }
    }
}
