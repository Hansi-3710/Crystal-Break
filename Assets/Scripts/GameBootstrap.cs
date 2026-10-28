using System.Collections.Generic;
using UnityEngine;

/// Builds the whole game at runtime so the project works in ANY empty scene.
/// Tripo models placed in Assets/Resources/Tripo/{Player,Enemy,Crystal,Pillar}
/// are used automatically; otherwise primitive stand-ins appear.
public static class GameBootstrap
{
    static GameObject root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart() { Build(false); }

    public static void Rebuild() { if (root) Object.Destroy(root); Build(true); }

    static GameObject Make(string n, Vector3 p)
    {
        var g = new GameObject(n);
        g.transform.SetParent(root.transform, false);
        g.transform.position = p;
        return g;
    }

    static void Tint(GameObject g, Color c)
    {
        foreach (var r in g.GetComponentsInChildren<Renderer>()) r.material.color = c;
    }

    /// Loads Resources/Tripo/<res> (or a primitive), scales to 'height', sits it on the parent's origin.
    static Transform Visual(string res, PrimitiveType fallback, float height, Color tint, Transform parent)
    {
        var pivot = new GameObject("Model").transform;
        pivot.SetParent(parent, false);
        var prefab = Resources.Load<GameObject>("Tripo/" + res);
        GameObject v;
        if (prefab != null) v = Object.Instantiate(prefab);
        else { v = GameObject.CreatePrimitive(fallback); Tint(v, tint); }
        foreach (var c in v.GetComponentsInChildren<Collider>()) Object.Destroy(c);
        v.transform.SetParent(pivot, false);
        var rs = v.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            Bounds b = Bnd(rs);
            v.transform.localScale = Vector3.one * (height / Mathf.Max(0.0001f, b.size.y));
            b = Bnd(rs);
            v.transform.position += new Vector3(pivot.position.x - b.center.x, pivot.position.y - b.min.y, pivot.position.z - b.center.z);
        }
        return pivot;
    }

    static Bounds Bnd(Renderer[] rs)
    {
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    static GameObject Block(string n, Vector3 pos, Vector3 scale, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = n; g.transform.SetParent(root.transform, false);
        g.transform.position = pos; g.transform.localScale = scale; Tint(g, c);
        return g;
    }

    static Vector3 RandPos(System.Random rng, List<Vector3> used, float minDist)
    {
        for (int i = 0; i < 200; i++)
        {
            var p = new Vector3((float)(rng.NextDouble() * 34 - 17), 0, (float)(rng.NextDouble() * 34 - 17));
            if (Vector3.Distance(p, new Vector3(0, 0, -16)) < 6) continue;
            if (Vector3.Distance(p, new Vector3(0, 0, 17)) < 5) continue;
            bool ok = true;
            foreach (var u in used) if (Vector3.Distance(p, u) < minDist) { ok = false; break; }
            if (ok) { used.Add(p); return p; }
        }
        return Vector3.zero;
    }

    public static void Build(bool skipTitle)
    {
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) c.enabled = false;
        foreach (var a in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) a.enabled = false;

        root = new GameObject("GameRoot");
        AudioManager.Init(root);
        var gm = root.AddComponent<GameManager>();
        gm.startInTitle = !skipTitle;
        var rng = new System.Random(7);
        var used = new List<Vector3>();

        RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.55f);
        var lg = new GameObject("Sun"); lg.transform.SetParent(root.transform);
        var l = lg.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.1f;
        lg.transform.rotation = Quaternion.Euler(50, -30, 0);

        Block("Ground", new Vector3(0, -0.5f, 0), new Vector3(40, 1, 40), new Color(0.16f, 0.18f, 0.24f));
        var wc = new Color(0.3f, 0.3f, 0.38f);
        Block("WallN", new Vector3(0, 1.5f, 20.5f), new Vector3(42, 3, 1), wc);
        Block("WallS", new Vector3(0, 1.5f, -20.5f), new Vector3(42, 3, 1), wc);
        Block("WallE", new Vector3(20.5f, 1.5f, 0), new Vector3(1, 3, 42), wc);
        Block("WallW", new Vector3(-20.5f, 1.5f, 0), new Vector3(1, 3, 42), wc);

        var player = Make("Player", new Vector3(0, 0.05f, -16));
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.4f; cc.center = new Vector3(0, 0.9f, 0);
        var prb = player.AddComponent<Rigidbody>(); prb.isKinematic = true; prb.sleepMode = RigidbodySleepMode.NeverSleep;
        var pc = player.AddComponent<PlayerController>();
        pc.model = Visual("Player", PrimitiveType.Capsule, 1.8f, new Color(0.2f, 0.85f, 1f), player.transform);
        gm.player = player.transform; gm.playerController = pc;

        for (int i = 0; i < 10; i++)
        {
            var p = Make("Pillar", RandPos(rng, used, 4f));
            var bc = p.AddComponent<BoxCollider>(); bc.size = new Vector3(1.8f, 2.8f, 1.8f); bc.center = new Vector3(0, 1.4f, 0);
            Visual("Pillar", PrimitiveType.Cylinder, 2.8f, new Color(0.5f, 0.5f, 0.6f), p.transform);
        }

        int crystals = 8;
        for (int i = 0; i < crystals; i++)
        {
            var c = Make("Crystal", RandPos(rng, used, 3f));
            var sc = c.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.9f; sc.center = new Vector3(0, 0.8f, 0);
            var col = c.AddComponent<Collectible>();
            col.model = Visual("Crystal", PrimitiveType.Sphere, 1.2f, new Color(1f, 0.85f, 0.2f), c.transform);
            col.model.localPosition = new Vector3(0, 0.3f, 0);
        }
        gm.SetNeeded(crystals);

        for (int i = 0; i < 5; i++)
        {
            var z = Make("LavaZone", RandPos(rng, used, 5f));
            var bc = z.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = new Vector3(4, 1, 4); bc.center = new Vector3(0, 0.5f, 0);
            z.AddComponent<Hazard>().damage = 10;
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(vis.GetComponent<Collider>());
            vis.transform.SetParent(z.transform, false); vis.transform.localPosition = new Vector3(0, 0.03f, 0);
            vis.transform.localScale = new Vector3(4, 0.06f, 4); Tint(vis, new Color(0.95f, 0.25f, 0.1f));
        }

        for (int i = 0; i < 3; i++)
        {
            var e = Make("Enemy", RandPos(rng, used, 6f));
            var cap = e.AddComponent<CapsuleCollider>(); cap.isTrigger = true; cap.radius = 0.8f; cap.height = 1.6f; cap.center = new Vector3(0, 0.8f, 0);
            var erb = e.AddComponent<Rigidbody>(); erb.isKinematic = true; erb.sleepMode = RigidbodySleepMode.NeverSleep;
            e.AddComponent<Hazard>().damage = 20;
            var ec = e.AddComponent<EnemyChaser>(); ec.target = player.transform;
            // Tripo drone is wide + flat (0.98 x 0.47): scale by height 0.9 -> ~1.9 m wingspan, hover 0.6 m above ground
            ec.model = Visual("Enemy", PrimitiveType.Capsule, 0.9f, new Color(0.9f, 0.1f, 0.2f), e.transform);
            ec.model.localPosition = new Vector3(0, 0.6f, 0);
        }

        var por = Make("Portal", new Vector3(0, 0, 17));
        var pbc = por.AddComponent<BoxCollider>(); pbc.isTrigger = true; pbc.size = new Vector3(3, 3, 3); pbc.center = new Vector3(0, 1.5f, 0);
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.Destroy(ring.GetComponent<Collider>());
        ring.transform.SetParent(por.transform, false); ring.transform.localPosition = new Vector3(0, 0.1f, 0);
        ring.transform.localScale = new Vector3(3, 0.1f, 3);
        por.AddComponent<Portal>().ring = ring.GetComponent<Renderer>();

        var cam = new GameObject("MainCamera"); cam.tag = "MainCamera"; cam.transform.SetParent(root.transform);
        cam.AddComponent<Camera>().fieldOfView = 55; cam.AddComponent<AudioListener>();
        var cf = cam.AddComponent<CameraFollow>(); cf.target = player.transform;
        cam.transform.position = player.transform.position + cf.offset;
        cam.transform.LookAt(player.transform.position);
    }
}
