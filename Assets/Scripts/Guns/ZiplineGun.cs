using UnityEngine;

public class ZiplineGun : MonoBehaviour
{
    public LayerMask groundLayer; // Set the layer mask for the ground in the Inspector
    public Material lineMaterial; // Material for the line renderer
    public float maxZiplineDistance = 100f; // Max distance for zipline
    public float poleHeight = 2f; // Height of the poles

    private LineRenderer lineRenderer;
    private bool isZiplineReady;

    [SerializeField] private Transform gunPoint;

    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        ActivateZiplineGun();
        //lineRenderer.material = lineMaterial;
        //lineRenderer.startWidth = 0.1f;
        //lineRenderer.endWidth = 0.1f;
        //lineRenderer.enabled = false;
    }

    void Update()
    {
        ShowZiplinePreview();

        if (Input.GetButtonDown("Fire1") && isZiplineReady)
        {
            DeployZipline();
        }
    }

    public void ActivateZiplineGun()
    {
        isZiplineReady = true;
        lineRenderer.enabled = true;
    }

    private void ShowZiplinePreview()
    {
        RaycastHit hit;
        if (Physics.Raycast(gunPoint.position, Vector3.forward, out hit, maxZiplineDistance, groundLayer))
        {
            Debug.Log("hit.transform.position = " + hit.transform.position);
            Debug.DrawRay(gunPoint.position, hit.transform.position, Color.red, 2f);
            Debug.DrawLine(gunPoint.position, hit.transform.position, Color.red);
            lineRenderer.SetPositions(new Vector3[] { gunPoint.position, hit.point });
        }
        else
        {
            lineRenderer.SetPositions(new Vector3[] { gunPoint.position, gunPoint.position + gunPoint.forward * maxZiplineDistance * -1f });
        }
    }

    private void DeployZipline()
    {
        lineRenderer.enabled = false;
        Vector3 startPoint = PlayerController_test.Instance.gameObject.transform.position + PlayerController_test.Instance.gameObject.transform.forward; // 1 meter in front of the player
        Vector3 endPoint = lineRenderer.GetPosition(1);

        CreatePole(startPoint);
        CreatePole(endPoint);

        CreateRope(startPoint, endPoint);

        //isZiplineReady = false;
    }

    private void CreatePole(Vector3 position)
    {
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.position = position + Vector3.up * poleHeight / 2;
        pole.transform.localScale = new Vector3(0.2f, poleHeight / 2, 0.2f);
        pole.AddComponent<Rigidbody>().isKinematic = true; // Make the pole static
    }

    private void CreateRope(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Vector3 direction = (endPoint - startPoint).normalized;
        float distance = Vector3.Distance(startPoint, endPoint);

        rope.transform.position = (startPoint + endPoint) / 2;
        rope.transform.position += new Vector3(0f, 1f, 0f);
        rope.transform.up = direction;
        rope.transform.localScale = new Vector3(0.05f, distance / 2, 0.05f);
    }
}
