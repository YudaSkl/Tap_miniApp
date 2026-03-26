using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Область тапа (прозрачная панель на весь экран).
/// </summary>
[RequireComponent(typeof(Image))]
public class TapManager : MonoBehaviour, IPointerClickHandler
{
    UIManager _ui;
    SaveManager _save;
    int _taps;
    int _coins;

    public int Taps => _taps;
    public int Coins => _coins;

    public void Init(UIManager ui, SaveManager save, int startTaps, int startCoins)
    {
        _ui = ui;
        _save = save;
        _taps = startTaps;
        _coins = startCoins;
        _ui?.SetScore(_taps, _coins);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _taps++;
        _coins++;
        _ui?.SetScore(_taps, _coins);
        _save?.SaveLocal(_taps, _coins);
    }
}
