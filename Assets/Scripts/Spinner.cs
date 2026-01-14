using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 45f; // Degrees per second
    public bool simulatePhysics = false;
    private Rigidbody rb;

    void Start()
    {
        float randomAngle = Random.Range(0f, 360f);
        transform.Rotate(Vector3.up.normalized * randomAngle, Space.Self);
        if (simulatePhysics) {
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = new Vector3(0f, Mathf.Deg2Rad * rotationSpeed, 0f);
            rb.isKinematic = false;
        }
    }
    
    void Update()
    {
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
    }
}
