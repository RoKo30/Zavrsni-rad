using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuInputHandler : MonoBehaviour
{
    public TMP_InputField seedInputField;
    public TMP_InputField levelsInputField;
    public RoomGeneratorSO levelData;

    public string nextSceneName = "GameScene";

    public void StartGame()
    {
        if (int.TryParse(seedInputField.text, out int seed))
            levelData.seed = seed;
        else
            levelData.seed = Random.Range(0, 99999);

        if (int.TryParse(levelsInputField.text, out int levels))
            levelData.numberOfLevels = levels;
        else
            levelData.numberOfLevels = 3;

        SceneManager.LoadScene(nextSceneName);
    }
    
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}