using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCreatorBulletBehavior : MonoBehaviour
{
    //SPHERE VERSION
    //public GameObject platformSpherePrefab;

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
    //    {
    //        ContactPoint contact = collision.contacts[0];
    //        Vector3 contactPoint = contact.point;
    //        Vector3 contactNormal = contact.normal.normalized;

    //        // Calculate the overlap adjustment, slightly less than half the diameter to ensure overlap
    //        float overlapAdjustment = platformSpherePrefab.transform.localScale.x * 0.49f;

    //        // Calculate the adjusted position for the platform sphere
    //        Vector3 adjustedPosition = contactPoint + (contactNormal * overlapAdjustment);

    //        // Instantiate the platform sphere with adjusted position
    //        Instantiate(platformSpherePrefab, adjustedPosition, Quaternion.identity);

    //        // Destroy the bullet
    //        Destroy(gameObject);
    //    }
    //}

    //CUBE VERSION
    //BEST VERSION YET
    public GameObject platformCubePrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 contactPoint = contact.point;
            Vector3 contactNormal = contact.normal;

            // Calculate the adjusted position for the platform cube
            Vector3 cubeSize = platformCubePrefab.transform.localScale;
            Vector3 adjustedPosition = CalculateAdjustedPosition(contactPoint, contactNormal, cubeSize);

            // Calculate the rotation to align the cube's face with the collided surface
            Quaternion rotation = Quaternion.Euler(0f, CalculateYRotation(contactNormal), 0f);

            // Instantiate the platform cube with adjusted position and rotation
            Instantiate(platformCubePrefab, adjustedPosition, rotation);

            Destroy(gameObject);
        }
    }

    private Vector3 CalculateAdjustedPosition(Vector3 contactPoint, Vector3 contactNormal, Vector3 cubeSize)
    {
        if (Mathf.Abs(contactNormal.y) > 0.5f)
        {
            float yOffset = (contactNormal.y > 0f) ? cubeSize.y / 2f : -cubeSize.y / 2f;
            return new Vector3(contactPoint.x, contactPoint.y + yOffset, contactPoint.z);
        }
        else
        {
            float xOffset = (Mathf.Abs(contactNormal.x) > 0.5f) ? (contactNormal.x > 0f ? cubeSize.x / 2f : -cubeSize.x / 2f) : 0f;
            float zOffset = (Mathf.Abs(contactNormal.z) > 0.5f) ? (contactNormal.z > 0f ? cubeSize.z / 2f : -cubeSize.z / 2f) : 0f;
            return new Vector3(contactPoint.x + xOffset, contactPoint.y, contactPoint.z + zOffset);
        }
    }

    private float CalculateYRotation(Vector3 contactNormal)
    {
        // Convert the contact normal to a rotation around the Y-axis
        Vector3 direction = new Vector3(contactNormal.x, 0f, contactNormal.z).normalized;
        return Quaternion.LookRotation(direction).eulerAngles.y;
    }
}
