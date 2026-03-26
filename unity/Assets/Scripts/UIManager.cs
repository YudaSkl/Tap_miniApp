using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Тексты счёта, пользователя и лидерборда.
/// </summary>
public class UIManager : MonoBehaviour
{
    Text _userText;
    Text _scoreText;
    Text _leaderboardText;

    public void Bind(Text userText, Text scoreText, Text leaderboardText)
    {
        _userText = userText;
        _scoreText = scoreText;
        _leaderboardText = leaderboardText;
    }

    public void SetUserLabel(string firstName, string username)
    {
        if (_userText == null) return;
        var u = string.IsNullOrEmpty(username) ? "" : $"@{username}";
        var name = string.IsNullOrEmpty(firstName) ? "Player" : firstName;
        _userText.text = string.IsNullOrEmpty(u) ? name : $"{name} {u}";
    }

    public void SetScore(int taps, int coins)
    {
        if (_scoreText == null) return;
        _scoreText.text = $"Тапы: {taps}   Монеты: {coins}";
    }

    public void SetLeaderboard(string message)
    {
        if (_leaderboardText == null) return;
        _leaderboardText.text = message;
    }

    /// <summary>
    /// Формат строк из API: name|taps построчно.
    /// </summary>
    public void SetLeaderboardRows(System.Collections.Generic.List<(string name, int taps)> rows)
    {
        if (_leaderboardText == null) return;
        var sb = new StringBuilder();
        sb.AppendLine("Топ:");
        int rank = 1;
        foreach (var r in rows)
        {
            sb.AppendLine($"{rank}. {r.name} — {r.taps}");
            rank++;
        }
        _leaderboardText.text = sb.ToString();
    }
}
