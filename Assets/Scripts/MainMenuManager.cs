using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstLevel = "Level1";

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Load(firstLevel);
    }
}