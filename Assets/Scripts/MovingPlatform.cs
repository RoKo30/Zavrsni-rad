using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 1f;

    private Vector3 target;
    private Vector3 lastPosition;
    public Vector3 DeltaMovement { get; private set; }

    private bool waiting = false;

    void Start()
    {
        target = pointB.position;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (waiting) return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        DeltaMovement = transform.position - lastPosition;

        if (Vector3.Distance(transform.position, target) < 0.01f)
            StartCoroutine(WaitAndSwitch());

        lastPosition = transform.position;
    }

    IEnumerator WaitAndSwitch()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTime);
        target = (target == pointA.position) ? pointB.position : pointA.position;
        waiting = false;
    }
}