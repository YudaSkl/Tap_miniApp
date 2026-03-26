using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Вызовы JavaScript через WebGL jslib (Telegram Mini App).
/// </summary>
public class TelegramBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Tg_RequestUser();

    [DllImport("__Internal")]
    private static extern void Tg_PostProgress(int taps, int coins);

    [DllImport("__Internal")]
    private static extern void Tg_OpenDonate(int stars);
#else
    private static void Tg_RequestUser() { }

    private static void Tg_PostProgress(int taps, int coins)
    {
        Debug.Log($"[Editor] Tg_PostProgress taps={taps} coins={coins}");
    }

    private static void Tg_OpenDonate(int stars)
    {
        Debug.Log($"[Editor] Tg_OpenDonate stars={stars}");
    }
#endif

    public void RequestUser()
    {
        Tg_RequestUser();
    }

    public void PostProgress(int taps, int coins)
    {
        Tg_PostProgress(taps, coins);
    }

    public void OpenDonate(int stars)
    {
        Tg_OpenDonate(stars);
    }
}
