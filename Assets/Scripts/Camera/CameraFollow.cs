using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(8, 25, -15);
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + offset;

        if (followSpeed <= 0f)
            transform.position = desiredPos;
        else
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(47.5f, 0f, 0f);
    }
}
