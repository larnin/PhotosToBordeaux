using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class EndGameLogic : MonoBehaviour
{
    [SerializeField] Transform m_plane;
    [SerializeField] float m_bordeauxRadius;
    [SerializeField] Image m_fadePlane;
    [SerializeField] float m_fadeTime;
    [SerializeField] string m_winScene;
    [SerializeField] string m_looseScene;

    GameObject m_gameMap;

    SubscriberList m_subscriberList = new SubscriberList();
    bool m_ended = false;

    void Start()
    {
        m_subscriberList.Add(new Event<ButtonTimeoutEvent>.Subscriber(onButtonTimeout));
        m_subscriberList.Subscribe();
        m_gameMap = GameObject.Find("GameMap");
        m_fadePlane.gameObject.SetActive(false);

        LevelMap.instance.time = 0;
    }
    
    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        if (m_ended)
            return;
        var planePos = new Vector2(m_plane.position.x / m_gameMap.transform.localScale.x, m_plane.position.z / m_gameMap.transform.localScale.x) / 2;
        var distance = (new Vector2(LevelMap.instance.bordeaux.x, LevelMap.instance.bordeaux.y) - planePos).magnitude;

        if (distance < m_bordeauxRadius)
            onWin();

        LevelMap.instance.time += Time.deltaTime;
    }

    void onWin()
    {
        m_ended = true;

        fade(Color.white);
        DOVirtual.DelayedCall(m_fadeTime, () => SceneManager.LoadScene(m_winScene));
    }

    void onButtonTimeout(ButtonTimeoutEvent e)
    {
        if (m_ended)
            return;

        m_ended = true;

        fade(Color.black);
        DOVirtual.DelayedCall(m_fadeTime, () => SceneManager.LoadScene(m_looseScene));
    }

    void fade(Color color)
    {
        m_fadePlane.gameObject.SetActive(true);
        m_fadePlane.color = new Color(color.r, color.g, color.b, 0);
        m_fadePlane.DOColor(color, m_fadeTime);
    }
}
