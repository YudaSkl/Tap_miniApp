using UnityEngine;

/// <summary>
/// Локальные сохранения (PlayerPrefs) + отправка на сервер через TelegramBridge.
/// </summary>
public class SaveManager : MonoBehaviour
{
    const string KeyTaps = "tapmini_taps";
    const string KeyCoins = "tapmini_coins";

    TelegramBridge _telegram;

    public void Init(TelegramBridge telegram)
    {
        _telegram = telegram;
    }

    public (int taps, int coins) LoadLocal()
    {
        int taps = PlayerPrefs.GetInt(KeyTaps, 0);
        int coins = PlayerPrefs.GetInt(KeyCoins, 0);
        return (taps, coins);
    }

    public void SaveLocal(int taps, int coins)
    {
        PlayerPrefs.SetInt(KeyTaps, taps);
        PlayerPrefs.SetInt(KeyCoins, coins);
        PlayerPrefs.Save();
    }

    public void PushToServer(int taps, int coins)
    {
        _telegram?.PostProgress(taps, coins);
    }
}
