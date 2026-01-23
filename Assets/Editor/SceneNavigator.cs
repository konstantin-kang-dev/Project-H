
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneNavigator : EditorWindow
{
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private List<SceneAsset> allScenes = new List<SceneAsset>();
    private List<SceneAsset> filteredScenes = new List<SceneAsset>();
    private bool showBuildScenes = true;
    private bool showAllScenes = true;

    [MenuItem("Tools/Scene Navigator")]
    public static void ShowWindow()
    {
        GetWindow<SceneNavigator>("Scene Navigator");
    }

    private void OnEnable()
    {
        RefreshSceneList();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        // Панель поиска
        DrawSearchPanel();

        // Кнопки быстрого доступа
        DrawQuickActions();

        EditorGUILayout.Space(5);

        // Фильтры
        DrawFilters();

        EditorGUILayout.Space(5);

        // Список сцен
        DrawSceneList();

        EditorGUILayout.EndVertical();
    }

    private void DrawSearchPanel()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.SetNextControlName("SearchField");
        string newSearchFilter = EditorGUILayout.TextField("Поиск:", searchFilter);

        if (newSearchFilter != searchFilter)
        {
            searchFilter = newSearchFilter;
            FilterScenes();
        }

        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            searchFilter = "";
            FilterScenes();
            GUI.FocusControl(null);
        }

        if (GUILayout.Button("↻", GUILayout.Width(25)))
        {
            RefreshSceneList();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Сохранить текущую"))
        {
            EditorSceneManager.SaveOpenScenes();
        }

        if (GUILayout.Button("Build Settings"))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFilters()
    {
        EditorGUILayout.BeginHorizontal();

        bool newShowBuildScenes = EditorGUILayout.Toggle("Build сцены", showBuildScenes);
        bool newShowAllScenes = EditorGUILayout.Toggle("Все сцены", showAllScenes);

        if (newShowBuildScenes != showBuildScenes || newShowAllScenes != showAllScenes)
        {
            showBuildScenes = newShowBuildScenes;
            showAllScenes = newShowAllScenes;
            FilterScenes();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSceneList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Build сцены
        if (showBuildScenes)
        {
            DrawSceneCategory("Build сцены", GetBuildScenes());
        }

        // Все остальные сцены
        if (showAllScenes)
        {
            var buildScenePaths = GetBuildScenePaths();
            var nonBuildScenes = filteredScenes.Where(s => !buildScenePaths.Contains(AssetDatabase.GetAssetPath(s))).ToList();
            DrawSceneCategory("Остальные сцены", nonBuildScenes);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSceneCategory(string categoryName, List<SceneAsset> scenes)
    {
        if (scenes.Count == 0) return;

        EditorGUILayout.LabelField(categoryName, EditorStyles.boldLabel);

        foreach (var scene in scenes)
        {
            DrawSceneItem(scene);
        }

        EditorGUILayout.Space(10);
    }

    private void DrawSceneItem(SceneAsset scene)
    {
        if (scene == null) return;

        EditorGUILayout.BeginHorizontal();

        string scenePath = AssetDatabase.GetAssetPath(scene);
        string sceneName = scene.name;
        bool isCurrentScene = EditorSceneManager.GetActiveScene().path == scenePath;

        // Подсветка текущей сцены
        if (isCurrentScene)
        {
            GUI.backgroundColor = Color.green;
        }

        if (GUILayout.Button(sceneName, EditorStyles.miniButton))
        {
            OpenScene(scenePath);
        }

        GUI.backgroundColor = Color.white;

        // Кнопка "добавить"
        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        // Показать в проводнике
        if (GUILayout.Button("→", GUILayout.Width(25)))
        {
            EditorGUIUtility.PingObject(scene);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RefreshSceneList()
    {
        allScenes.Clear();

        // Найти все .unity файлы в проекте
        string[] guids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene != null)
            {
                allScenes.Add(scene);
            }
        }

        // Сортировка по имени
        allScenes = allScenes.OrderBy(s => s.name).ToList();

        FilterScenes();
    }

    private void FilterScenes()
    {
        if (string.IsNullOrEmpty(searchFilter))
        {
            filteredScenes = new List<SceneAsset>(allScenes);
        }
        else
        {
            filteredScenes = allScenes.Where(s =>
                s.name.ToLower().Contains(searchFilter.ToLower())).ToList();
        }
    }

    private List<SceneAsset> GetBuildScenes()
    {
        var buildScenes = new List<SceneAsset>();
        var buildScenePaths = GetBuildScenePaths();

        foreach (var scene in filteredScenes)
        {
            if (buildScenePaths.Contains(AssetDatabase.GetAssetPath(scene)))
            {
                buildScenes.Add(scene);
            }
        }

        return buildScenes;
    }

    private HashSet<string> GetBuildScenePaths()
    {
        var paths = new HashSet<string>();

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                paths.Add(scene.path);
            }
        }

        return paths;
    }

    private void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}