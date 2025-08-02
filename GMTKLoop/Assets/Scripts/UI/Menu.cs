using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    public string menuName;
    public bool open;

    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text highScoreText;

    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }

    public void SetRoundText(bool won)
    {
        if (roundText == null)
            return;

        if (won)
            roundText.text = $"Round: {GameManager.i.GetCurrentRound()} | won";
        else
            roundText.text = $"Round: {GameManager.i.GetCurrentRound()} | lost";
    }

    public void SetHighscoreText()
    {
        if (highScoreText == null)
            return;
        highScoreText.text = $"Highscore: {GameManager.i.GetHighRound()}";
    }
}
