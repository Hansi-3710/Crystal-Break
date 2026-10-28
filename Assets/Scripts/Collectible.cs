using UnityEngine;

public class Collectible : MonoBehaviour
{
    public Transform model; float baseY;
    void Start() { if (model) baseY = model.localPosition.y; }
    void Update()
    {
        if (!model) return;
        model.Rotate(0, 90f * Time.deltaTime, 0, Space.World);
        var p = model.localPosition; p.y = baseY + 0.15f * Mathf.Sin(Time.time * 3f); model.localPosition = p;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || GameManager.I == null) return;
        GameManager.I.AddCrystal();
        Destroy(gameObject);
    }
}
