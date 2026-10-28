using UnityEngine;

/// Put on any trigger (enemy body, lava zone). Damage rate is limited by GameManager i-frames.
public class Hazard : MonoBehaviour
{
    public int damage = 10;
    void OnTriggerStay(Collider other)
    {
        if (GameManager.I != null && other.GetComponent<PlayerController>() != null)
            GameManager.I.Damage(damage);
    }
}
