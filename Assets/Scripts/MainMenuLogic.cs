using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NRand;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{
    [SerializeField] GameObject m_howToPlay;
    [SerializeField] InputField m_seed;
    [SerializeField] string m_gameScene;

    GameObject instancingMessage;

    private void Start()
    {
        m_howToPlay.SetActive(false);
        Cursor.lockState = CursorLockMode.None;

        LevelMap.instance.canUseInstancing = SystemInfo.supportsInstancing;
        instancingMessage = transform.Find("Instancing").gameObject;
        instancingMessage.SetActive(!LevelMap.instance.canUseInstancing);
    }

    public void onPlayPress()
    {
        if (m_seed.text == "")
            LevelMap.instance.seed = (int)new StaticRandomGenerator<DefaultRandomGenerator>().Next();
        else LevelMap.instance.seed = m_seed.text.GetHashCode();

        SceneManager.LoadScene(m_gameScene);
    }

    public void onOptionPress()
    {
        m_howToPlay.SetActive(true);
    }

    public void onQuit()
    {
        Application.Quit();
    }
}
