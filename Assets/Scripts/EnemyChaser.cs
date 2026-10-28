using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public Transform target, model;
    public float speed = 3.4f, chaseRange = 9f, patrolRadius = 4f;
    Vector3 home, patrol;

    void Start() { home = transform.position; NewPatrol(); }
    void NewPatrol()
    {
        var o = Random.insideUnitCircle * patrolRadius;
        patrol = new Vector3(Mathf.Clamp(home.x + o.x, -18, 18), home.y, Mathf.Clamp(home.z + o.y, -18, 18));
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || gm.state != GameState.Playing) return;
        Vector3 dest;
        bool chasing = target && Vector3.Distance(transform.position, target.position) < chaseRange;
        if (chasing) dest = new Vector3(target.position.x, transform.position.y, target.position.z);
        else { dest = patrol; if (Vector3.Distance(transform.position, patrol) < 0.5f) NewPatrol(); }
        Vector3 dir = dest - transform.position;
        if (dir.sqrMagnitude > 0.01f && model) model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(dir.normalized), 10f * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, dest, (chasing ? speed : speed * 0.5f) * Time.deltaTime);
    }
}
