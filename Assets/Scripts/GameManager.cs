using UnityEngine;

public enum GameState { Title, Playing, Won, Lost }

/// Owns the Reward / Damage / End triad state, HUD, and end screens.
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public int maxHealth = 100, health, crystals, crystalsNeeded, score;
    public GameState state = GameState.Playing;
    public bool startInTitle = true;
    public Transform player; public PlayerController playerController;
    public float shake;

    float startTime, endElapsed, invulnUntil, flash, toastUntil;
    string toast = "";

    void Awake()
    {
        I = this; health = maxHealth;
        state = startInTitle ? GameState.Title : GameState.Playing;
        startTime = Time.time;
    }
    void Start() { AudioManager.StartMusic(); }

    public void SetNeeded(int n) { crystalsNeeded = n; }
    public bool PortalOpen { get { return crystals >= crystalsNeeded; } }
    public float Elapsed { get { return (state == GameState.Playing || state == GameState.Title) ? Time.time - startTime : endElapsed; } }
    public void GrantInvuln(float t) { invulnUntil = Mathf.Max(invulnUntil, Time.time + t); }
    public void Toast(string m) { toast = m; toastUntil = Time.time + 1.5f; }

    // REWARD
    public void AddCrystal()
    {
        if (state != GameState.Playing) return;
        crystals++; score += 100;
        AudioManager.Play("collect");
        Toast(PortalOpen ? "Portal OPEN! Reach the green ring!" : "Crystal " + crystals + "/" + crystalsNeeded);
    }

    // DAMAGE
    public void Damage(int amount)
    {
        if (state != GameState.Playing || Time.time < invulnUntil) return;
        health -= amount; invulnUntil = Time.time + 0.6f; shake = 0.4f; flash = 0.5f;
        AudioManager.Play("damage");
        if (health <= 0) { health = 0; Lose(); }
    }

    // END
    public void Win()
    {
        if (state != GameState.Playing) return;
        endElapsed = Time.time - startTime;
        score += health * 5 + Mathf.Max(0, (int)(120 - endElapsed)) * 10;
        state = GameState.Won; AudioManager.StopMusic(); AudioManager.Play("win");
    }
    void Lose()
    {
        endElapsed = Time.time - startTime;
        state = GameState.Lost; AudioManager.StopMusic(); AudioManager.Play("lose");
    }
    public void Restart() { GameBootstrap.Rebuild(); }

    void Update()
    {
        shake = Mathf.MoveTowards(shake, 0, Time.deltaTime);
        flash = Mathf.MoveTowards(flash, 0, Time.deltaTime);
        if (state == GameState.Title && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        { state = GameState.Playing; startTime = Time.time; }
        if ((state == GameState.Won || state == GameState.Lost) && Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    void Box(Rect r, Color c) { GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }

    void OnGUI()
    {
        int fs = Mathf.Max(14, Screen.height / 32);
        var lab = new GUIStyle(GUI.skin.label) { fontSize = fs, fontStyle = FontStyle.Bold };
        lab.normal.textColor = Color.white;
        var center = new GUIStyle(lab) { alignment = TextAnchor.MiddleCenter };
        var big = new GUIStyle(center) { fontSize = fs * 3 };
        float W = Screen.width, H = Screen.height;

        if (flash > 0) Box(new Rect(0, 0, W, H), new Color(1, 0, 0, flash * 0.5f));

        if (state != GameState.Title)
        {
            Box(new Rect(20, 20, 300, fs), new Color(0, 0, 0, 0.6f));
            Box(new Rect(20, 20, 300f * health / maxHealth, fs), Color.Lerp(Color.red, Color.green, health / (float)maxHealth));
            GUI.Label(new Rect(330, 14, 400, fs * 2), "HP " + health, lab);
            GUI.Label(new Rect(20, 25 + fs, 600, fs * 2), "Crystals " + crystals + "/" + crystalsNeeded + "   Score " + score, lab);
            GUI.Label(new Rect(20, 25 + fs * 2.2f, 500, fs * 2), "Time " + Elapsed.ToString("0.0") + "s", lab);
            if (playerController != null && playerController.DashCooldown01 > 0)
                GUI.Label(new Rect(20, 25 + fs * 3.4f, 500, fs * 2), "Dash recharging...", lab);
            if (Time.time < toastUntil) GUI.Label(new Rect(0, H * 0.15f, W, fs * 2), toast, center);
        }

        if (state == GameState.Title)
        {
            Box(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.7f));
            GUI.Label(new Rect(0, H * 0.2f, W, fs * 3), "CRYSTAL BREAK", big);
            GUI.Label(new Rect(0, H * 0.4f, W, H * 0.3f),
                "REWARD: collect all 8 crystals\nDAMAGE: avoid red drones and lava\nEND: reach the portal alive\n\nWASD / Arrows = move     SPACE = dash (brief invulnerability)\n\nPress ENTER to start", center);
        }
        else if (state == GameState.Won || state == GameState.Lost)
        {
            bool w = state == GameState.Won;
            Box(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.75f));
            GUI.Label(new Rect(0, H * 0.2f, W, fs * 3), w ? "YOU ESCAPED!" : "GAME OVER", big);
            GUI.Label(new Rect(0, H * 0.4f, W, fs * 5),
                "Score " + score + "\nCrystals " + crystals + "/" + crystalsNeeded + "\nTime " + endElapsed.ToString("0.0") + "s", center);
            if (GUI.Button(new Rect(W / 2 - 150, H * 0.65f, 300, fs * 2.5f), "Play Again (R)")) Restart();
        }
    }
}
