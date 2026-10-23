using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text jumpPadText;
    [SerializeField] TMP_Text wallPadText;
    [SerializeField] TMP_Text gravityText;

    PlayerPowers powers;
    GravitySwitchPower gravity;

    void Start()
    {
        powers = FindFirstObjectByType<PlayerPowers>();
        gravity = FindFirstObjectByType<GravitySwitchPower>();
    }

    // Update the text every frame
    void Update()
    {
        if (livesText) livesText.text = $"Lives: {GameLives.Lives}";
        ShowTimer(jumpPadText, "Square", powers ? powers.JumpPadTimeLeft : 0f);
        ShowTimer(wallPadText, "Triangle", powers ? powers.WallPadTimeLeft : 0f);
        ShowTimer(gravityText, "Gravity", gravity ? gravity.TimeLeft : 0f);
    }

    // Show the timer if time is left, hide it if not
    static void ShowTimer(TMP_Text text, string label, float timeLeft)
    {
        if (text == null) return;
        bool on = timeLeft > 0f;
        if (text.gameObject.activeSelf != on) text.gameObject.SetActive(on);
        if (on) text.text = $"{label}: {Mathf.CeilToInt(timeLeft)}s";
    }
}