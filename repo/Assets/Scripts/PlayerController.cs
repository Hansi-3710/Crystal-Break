using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 7f, dashSpeed = 20f, dashTime = 0.18f, dashCooldown = 1f, accel = 45f, gravity = -25f;
    public Transform model;
    CharacterController cc; Vector3 vel, dashDir; float vy, dashEnd, nextDash;

    void Awake() { cc = GetComponent<CharacterController>(); }
    public float DashCooldown01 { get { return Mathf.Clamp01((nextDash - Time.time) / dashCooldown); } }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || gm.state != GameState.Playing) return;
        Vector3 input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1f);

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextDash)
        {
            dashDir = input.sqrMagnitude > 0.01f ? input.normalized : (model ? model.forward : transform.forward);
            dashEnd = Time.time + dashTime; nextDash = Time.time + dashCooldown;
            gm.GrantInvuln(dashTime + 0.1f); AudioManager.Play("dash");
        }
        bool dashing = Time.time < dashEnd;
        Vector3 target = dashing ? dashDir * dashSpeed : input * speed;
        vel = Vector3.MoveTowards(vel, target, accel * (dashing ? 10f : 1f) * Time.deltaTime);
        vy = cc.isGrounded ? -2f : vy + gravity * Time.deltaTime;
        cc.Move((vel + Vector3.up * vy) * Time.deltaTime);
        if (vel.sqrMagnitude > 0.5f && model)
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(vel.normalized), 15f * Time.deltaTime);
    }
}
