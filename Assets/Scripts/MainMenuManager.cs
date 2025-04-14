using UnityEngine;
using UnityEngine.SceneManagement; // Import scene management

public class MainMenuManager : MonoBehaviour
{
    // Public method for button click
    public void OnStartButtonPressed()
    {
        LoadIntroScene(); // 调用无参数方法
    }

    // Private method that actually loads the scene
    private void LoadIntroScene()
    {
        SceneManager.LoadScene("IntroScene"); // 这里切换场景
    }
}
