using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, -10f);
    private float cameraZ;

    void Start()
    {
        cameraZ = transform.position.z;

        // 自动寻找 Player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        Vector3 targetPos = player.position + offset;
        targetPos.z = cameraZ;
        transform.position = targetPos;
    }
}
