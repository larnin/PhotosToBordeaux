using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverLogic : MonoBehaviour
{
    [SerializeField] string m_mainMenuScene;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void onContinuePress()
    {
        SceneManager.LoadScene(m_mainMenuScene);
    }
}
