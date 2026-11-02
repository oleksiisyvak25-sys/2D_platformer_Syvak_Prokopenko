using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuNavigation : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public string sceneName;
        public Button button;
        public GameObject lockIcon;
    }

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Levels")]
    [SerializeField] private LevelEntry[] levels;
    [SerializeField] private bool unlockAllLevels = true;

    private void Awake()
    {
        if (levels == null) return;

        foreach (var entry in levels)
        {
            if (entry == null || entry.button == null) continue;

            string sceneName = entry.sceneName;
            entry.button.onClick.AddListener(() => LoadLevel(sceneName));
        }
    }

    private void Start()
    {
        ShowMain();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && mainPanel != null && !mainPanel.activeSelf)
        {
            ShowMain();
        }
    }

    public void ShowMain()
    {
        SetPanels(true, false, false);
    }

    public void ShowLevels()
    {
        RefreshLevels();
        SetPanels(false, true, false);
    }

    public void ShowSettings()
    {
        SetPanels(false, false, true);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        Time.timeScale = 1f;
        SceneLoader.Load(sceneName);
    }

    private void RefreshLevels()
    {
        if (levels == null) return;

        for (int i = 0; i < levels.Length; i++)
        {
            LevelEntry entry = levels[i];
            if (entry == null) continue;

            bool unlocked = unlockAllLevels || i == 0 || LevelProgress.IsReached(entry.sceneName);

            if (entry.button != null) entry.button.interactable = unlocked;
            if (entry.lockIcon != null) entry.lockIcon.SetActive(!unlocked);
        }
    }

    private void SetPanels(bool main, bool levelsVisible, bool settingsVisible)
    {
        if (mainPanel != null) mainPanel.SetActive(main);
        if (levelsPanel != null) levelsPanel.SetActive(levelsVisible);
        if (settingsPanel != null) settingsPanel.SetActive(settingsVisible);
    }
}