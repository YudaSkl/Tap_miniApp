using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Собирает сцену: Main Camera, EventSystem, App + Canvas/UI (иерархия для GameBootstrap).
/// </summary>
static class TapMiniAppSceneCompose
{
    const string MenuComposeCurrent = "Tools/Tap miniApp/Compose Scene";
    const string MenuComposeMain = "Tools/Tap miniApp/Compose Main Scene";
    const string MainScenePath = "Assets/Scenes/Main.unity";

    [MenuItem(MenuComposeCurrent)]
    static void ComposeCurrentScene()
    {
        ComposeCore();
    }

    [MenuItem(MenuComposeMain)]
    static void ComposeMainScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (!System.IO.File.Exists(MainScenePath))
        {
            Debug.LogError($"Tap miniApp: сцена не найдена: {MainScenePath}");
            return;
        }

        EditorSceneManager.OpenScene(MainScenePath);
        ComposeCore();
    }

    static void ComposeCore()
    {
        var scene = EditorSceneManager.GetActiveScene();

        EnsureMainCamera();
        var app = GetOrCreateAppWithBootstrap(scene);
        EnsureEventSystem();
        RebuildUiUnderApp(app);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Tap miniApp: сцена скомпонована (Camera, EventSystem, App/Canvas/UI).");
    }

    static void EnsureMainCamera()
    {
        if (GameObject.FindGameObjectWithTag("MainCamera") != null)
            return;

        var go = new GameObject("Main Camera");
        Undo.RegisterCreatedObjectUndo(go, "Compose Scene — Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        go.AddComponent<AudioListener>();
        go.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
    }

    static GameObject GetOrCreateAppWithBootstrap(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject app = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "App")
            {
                app = root;
                break;
            }
        }

        if (app == null)
        {
            app = new GameObject("App");
            Undo.RegisterCreatedObjectUndo(app, "Compose Scene — App");
            Undo.AddComponent<GameBootstrap>(app);
        }
        else if (app.GetComponent<GameBootstrap>() == null)
        {
            Undo.AddComponent<GameBootstrap>(app);
        }

        return app;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            return;

        var es = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(es, "Compose Scene — EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static void RebuildUiUnderApp(GameObject app)
    {
        var old = app.transform.Find("Canvas");
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        var canvasGo = new GameObject("Canvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Compose Scene — Canvas");
        canvasGo.transform.SetParent(app.transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        CreateText(canvasGo.transform, "UserText", "…", 28, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(800, 60));
        CreateText(canvasGo.transform, "ScoreText", "Тапы: 0", 36, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(800, 80));

        var tapPanel = new GameObject("TapPanel");
        Undo.RegisterCreatedObjectUndo(tapPanel, "Compose Scene — TapPanel");
        tapPanel.transform.SetParent(canvasGo.transform, false);
        var rt = tapPanel.AddComponent<RectTransform>();
        StretchFull(rt);
        tapPanel.AddComponent<Image>();

        CreateText(canvasGo.transform, "LeaderboardText", "", 22, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20, 120), new Vector2(400, 220));

        var btnRow = new GameObject("Buttons");
        Undo.RegisterCreatedObjectUndo(btnRow, "Compose Scene — Buttons");
        btnRow.transform.SetParent(canvasGo.transform, false);
        var rowRt = btnRow.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0f);
        rowRt.anchorMax = new Vector2(0.5f, 0f);
        rowRt.pivot = new Vector2(0.5f, 0f);
        rowRt.anchoredPosition = new Vector2(0, 16);
        rowRt.sizeDelta = new Vector2(900, 56);

        var h = btnRow.AddComponent<HorizontalLayoutGroup>();
        h.childAlignment = TextAnchor.MiddleCenter;
        h.spacing = 12;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;

        CreateButton(btnRow.transform, "Sync");
        CreateButton(btnRow.transform, "Топ");
        CreateButton(btnRow.transform, "Поддержать");
    }

    static void CreateButton(Transform parent, string label)
    {
        var go = new GameObject(label + "Button");
        Undo.RegisterCreatedObjectUndo(go, "Compose Scene — Button");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.45f, 0.85f, 0.95f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 120;
        le.preferredHeight = 48;

        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        var t = txtGo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 20;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = label;
        StretchFull(txtGo.GetComponent<RectTransform>());
    }

    static Text CreateText(Transform parent, string name, string text, int size, TextAnchor align, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Compose Scene — Text");
        go.transform.SetParent(parent, false);
        var rtt = go.AddComponent<RectTransform>();
        rtt.anchorMin = anchorMin;
        rtt.anchorMax = anchorMax;
        rtt.pivot = anchorMin;
        rtt.anchoredPosition = anchoredPos;
        rtt.sizeDelta = sizeDelta;
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.text = text;
        return t;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
