using UnityEngine;

public class PlayerPlatformFollower : MonoBehaviour
{
    private MovingPlatform currentPlatform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MovingPlatform platform))
            currentPlatform = platform;
        Debug.LogWarning("ENTERED");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out MovingPlatform platform) && platform == currentPlatform)
            currentPlatform = null;
        Debug.LogWarning("EXITED");

    }

    void LateUpdate()
    {
        if (currentPlatform != null)
        {
            transform.position += currentPlatform.DeltaMovement;
        }
    }
}