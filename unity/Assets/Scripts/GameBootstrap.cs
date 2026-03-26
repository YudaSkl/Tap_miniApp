using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Связывает менеджеры с UI из сцены (собирается через Tools → Tap miniApp → Compose Scene), принимает данные из JS (SendMessage).
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Tooltip("Базовый URL API для GET /api/leaderboard (без слэша в конце).")]
    [SerializeField]
    string _apiBase = "PLACEHOLDER_API_BASE";

    [SerializeField]
    int _donateStars = 10;

    UIManager _ui;
    SaveManager _save;
    TelegramBridge _telegram;
    TapManager _tap;

    Text _userText;
    Text _scoreText;
    Text _leaderboardText;
    GameObject _tapPanel;

    void Awake()
    {
        ResolveUiFromHierarchy();
        if (_userText == null || _scoreText == null || _leaderboardText == null || _tapPanel == null)
        {
            Debug.LogError(
                "GameBootstrap: не найдено UI (Canvas / UserText / …). Убедитесь, что GameBootstrap висит на объекте **App**, под ним **Canvas** (как после Tools → Tap miniApp → Compose Scene), и сцена сохранена.");
            return;
        }

        WireUiButtons();

        _ui = gameObject.AddComponent<UIManager>();
        _save = gameObject.AddComponent<SaveManager>();
        _telegram = gameObject.AddComponent<TelegramBridge>();
        _save.Init(_telegram);

        var (taps, coins) = _save.LoadLocal();
        _ui.Bind(_userText, _scoreText, _leaderboardText);
        _ui.SetUserLabel("", "");
        _ui.SetScore(taps, coins);

        _tap = _tapPanel.GetComponent<TapManager>();
        if (_tap == null)
            _tap = _tapPanel.AddComponent<TapManager>();

        var img = _tapPanel.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0.02f);
            img.raycastTarget = true;
        }

        _tap.Init(_ui, _save, taps, coins);
    }

    void Start()
    {
        _telegram?.RequestUser();
    }

    void ResolveUiFromHierarchy()
    {
        var canvas = ResolveTapMiniAppCanvas();
        if (canvas == null)
            return;

        var root = canvas.transform;
        _userText = FindDeepChild(root, "UserText")?.GetComponent<Text>();
        _scoreText = FindDeepChild(root, "ScoreText")?.GetComponent<Text>();
        _leaderboardText = FindDeepChild(root, "LeaderboardText")?.GetComponent<Text>();
        _tapPanel = FindDeepChild(root, "TapPanel")?.gameObject;
    }

    void WireUiButtons()
    {
        var canvas = ResolveTapMiniAppCanvas();
        if (canvas == null)
            return;

        var root = canvas.transform;
        BindButtonByPath(root, "Buttons/SyncButton", OnSyncClicked);
        BindButtonByPath(root, "Buttons/ТопButton", OnLeaderboardClicked);
        BindButtonByPath(root, "Buttons/ПоддержатьButton", OnDonateClicked);
    }

    /// <summary>
    /// Transform.Find пропускает неактивные объекты; Canvas может быть не прямым ребёнком App — ищем надёжнее.
    /// </summary>
    Canvas ResolveTapMiniAppCanvas()
    {
        var local = GetComponentInChildren<Canvas>(true);
        if (local != null && LooksLikeComposedUi(local.transform))
            return local;

        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (LooksLikeComposedUi(c.transform))
                return c;
        }

        return null;
    }

    static bool LooksLikeComposedUi(Transform canvasRoot)
    {
        return FindDeepChild(canvasRoot, "UserText") != null
               && FindDeepChild(canvasRoot, "TapPanel") != null
               && FindDeepChild(canvasRoot, "Buttons") != null;
    }

    /// <summary>
    /// Обход дерева с GetChild — находит и неактивные объекты (в отличие от Transform.Find).
    /// </summary>
    static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
                return child;
            var nested = FindDeepChild(child, name);
            if (nested != null)
                return nested;
        }

        return null;
    }

    static Transform FindChildPath(Transform root, string path)
    {
        if (string.IsNullOrEmpty(path))
            return root;
        var parts = path.Split('/');
        var current = root;
        foreach (var part in parts)
        {
            Transform next = null;
            for (var i = 0; i < current.childCount; i++)
            {
                var ch = current.GetChild(i);
                if (ch.name == part)
                {
                    next = ch;
                    break;
                }
            }

            if (next == null)
                return null;
            current = next;
        }

        return current;
    }

    static void BindButtonByPath(Transform canvasRoot, string path, UnityEngine.Events.UnityAction handler)
    {
        var t = FindChildPath(canvasRoot, path);
        if (t == null)
            return;
        var btn = t.GetComponent<Button>();
        if (btn == null)
            return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(handler);
    }

    void OnSyncClicked()
    {
        if (_tap == null || _save == null) return;
        _save.PushToServer(_tap.Taps, _tap.Coins);
    }

    void OnLeaderboardClicked()
    {
        StartCoroutine(FetchLeaderboard());
    }

    void OnDonateClicked()
    {
        _telegram?.OpenDonate(_donateStars);
    }

    IEnumerator FetchLeaderboard()
    {
        _ui.SetLeaderboard("Загрузка…");
        var baseUrl = _apiBase.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl) || baseUrl.Contains("PLACEHOLDER"))
        {
            _ui.SetLeaderboard("Укажите API URL (Inspector → GameBootstrap).");
            yield break;
        }

        var url = baseUrl + "/api/leaderboard?limit=10";
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                _ui.SetLeaderboard("Нет связи с сервером.");
                yield break;
            }

            var text = req.downloadHandler.text;
            var rows = ParseLeaderboardJson(text);
            if (rows.Count == 0)
            {
                _ui.SetLeaderboard("Пока пусто.");
                yield break;
            }

            _ui.SetLeaderboardRows(rows);
        }
    }

    static List<(string name, int taps)> ParseLeaderboardJson(string json)
    {
        var list = new List<(string, int)>();
        var re = new Regex("\"username\"\\s*:\\s*\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"\\s*,\\s*\"taps\"\\s*:\\s*(\\d+)");
        foreach (Match m in re.Matches(json))
        {
            if (m.Groups.Count >= 3)
            {
                var name = m.Groups[1].Value.Replace("\\\"", "\"");
                int taps = int.Parse(m.Groups[2].Value);
                list.Add((name, taps));
            }
        }

        return list;
    }

    /// <summary>
    /// Вызывается из bridge.js: SendMessage('GameBootstrap', 'OnTelegramUser', json).
    /// </summary>
    public void OnTelegramUser(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var dto = JsonUtility.FromJson<TelegramUserPayload>(json);
            _ui?.SetUserLabel(dto.first_name ?? "", dto.username ?? "");
        }
        catch
        {
            _ui?.SetUserLabel("Player", "");
        }
    }

    [System.Serializable]
    class TelegramUserPayload
    {
        public string username;
        public string first_name;
    }
}
