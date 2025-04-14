using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Transform of the player object  

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z; // Keep the camera's Z position unchanged  
            transform.position = newPosition;
        }
    }
}