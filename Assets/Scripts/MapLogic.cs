using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapLogic : MonoBehaviour
{
    const string closeButton = "Fire1";

    [SerializeField] GameObject m_pointPrefab;
    [SerializeField] Transform m_plane;

    SubscriberList m_subscriberList = new SubscriberList();
    Image m_surface;
    Vector2 m_mapOffset;
    Vector2 m_mapScale;
    bool m_set = false;
    GameObject m_gameMap;

    Transform m_planeIco;

    void Awake()
    {
        m_surface = transform.Find("Image").GetComponent<Image>();
        m_gameMap = GameObject.Find("GameMap");
        m_planeIco = transform.Find("Plane").GetComponent<Transform>();

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(onGenerationEnd));
        m_subscriberList.Add(new Event<MapStartEvent>.Subscriber(onOpenMap));
        m_subscriberList.Subscribe();

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        if(Input.GetButtonDown(closeButton))
        {
            Event<MapEndEvent>.Broadcast(new MapEndEvent());
            gameObject.SetActive(false);
        }

        if (!m_set)
            return;

        m_planeIco.localRotation = Quaternion.Euler(0, 0, -m_plane.rotation.eulerAngles.y);
        m_planeIco.localPosition = new Vector3(m_plane.position.x * m_mapScale.x + m_mapOffset.x, m_plane.position.z * m_mapScale.y + m_mapOffset.y, 0);

    }

    void onGenerationEnd(GenerationFinishedEvent e)
    {
        var map = LevelMap.instance.minimap;
        m_surface.sprite = Sprite.Create(map, new Rect(0, 0, map.width, map.height), new Vector2(map.width / 2, map.height / 2));

        Vector2 mapSize = new Vector2(900, 900);

        m_mapScale = new Vector2(mapSize.x / map.width, mapSize.y / map.height);
        m_mapOffset = -mapSize / 2;

        foreach(var p in LevelMap.instance.importantPoints)
        {
            var cross = Instantiate(m_pointPrefab, transform);
            cross.transform.localPosition = new Vector3(p.x * m_mapScale.x + m_mapOffset.x, p.y * m_mapScale.y + m_mapOffset.y, 0);
            cross.transform.Find("Label").GetComponent<Text>().text = p.name;
        }

        var bdxP = LevelMap.instance.bordeaux;
        var bdx = Instantiate(m_pointPrefab, transform);
        bdx.transform.localPosition = new Vector3(bdxP.x * m_mapScale.x + m_mapOffset.x, bdxP.y * m_mapScale.y + m_mapOffset.y, 0);
        var text = bdx.transform.Find("Label").GetComponent<Text>();
        text.text = bdxP.name;
        text.color = Color.red;

        m_mapScale.x /= m_gameMap.transform.localScale.x * 2;
        m_mapScale.y /= m_gameMap.transform.localScale.z * 2;

        m_set = true;
    }

    void onOpenMap(MapStartEvent e)
    {
        gameObject.SetActive(true);
    }
}
