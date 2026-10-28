using UnityEngine;

public class Portal : MonoBehaviour
{
    public Renderer ring;
    void Update()
    {
        if (ring && GameManager.I)
            ring.material.color = GameManager.I.PortalOpen ? new Color(0.2f, 1f, 0.4f) : new Color(0.8f, 0.15f, 0.15f);
    }
    void OnTriggerStay(Collider other)
    {
        var gm = GameManager.I;
        if (gm == null || other.GetComponent<PlayerController>() == null) return;
        if (gm.PortalOpen) gm.Win();
        else gm.Toast("Collect all crystals first! (" + gm.crystals + "/" + gm.crystalsNeeded + ")");
    }
}
