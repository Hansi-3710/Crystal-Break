using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; public Vector3 offset = new Vector3(0, 14, -10);
    void LateUpdate()
    {
        if (!target) return;
        Vector3 goal = target.position + offset;
        float s = GameManager.I ? GameManager.I.shake : 0f;
        transform.position = Vector3.Lerp(transform.position, goal, 8f * Time.deltaTime) + Random.insideUnitSphere * s;
        transform.rotation = Quaternion.LookRotation(target.position + Vector3.up - transform.position);
    }
}
