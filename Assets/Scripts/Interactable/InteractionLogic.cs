using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InteractionLogic : MonoBehaviour
{
    const string interactionButton = "Fire1";

    [SerializeField] Text m_interactionText;
    [SerializeField] LayerMask m_layer;
    [SerializeField] float m_rayMaxDistance;

    BaseInteractableLogic m_hoveredObject;
    SubscriberList m_subscriberList = new SubscriberList();

    bool m_mapEnabled = false;

    private void Awake()
    {
        m_subscriberList.Add(new Event<MapStartEvent>.Subscriber(onMapStart));
        m_subscriberList.Add(new Event<MapEndEvent>.Subscriber(onMapEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        if(m_mapEnabled)
        {
            m_hoveredObject = null;
            m_interactionText.text = "";
            return;
        }

        RaycastHit hit = new RaycastHit();
        bool touched = Physics.Raycast(transform.position, transform.forward, out hit, m_rayMaxDistance, m_layer);
        if(!touched)
        {
            if(m_hoveredObject != null)
            {
                m_hoveredObject.onHoverEnd();
                m_hoveredObject = null;
            }
        }
        else
        {
            var interactable = hit.collider.GetComponent<BaseInteractableLogic>();
            if (interactable != m_hoveredObject)
            {
                if (m_hoveredObject != null)
                    m_hoveredObject.onHoverEnd();

                m_hoveredObject = interactable;

                if (m_hoveredObject != null)
                    m_hoveredObject.onHoverStart();
            }
        }

        if (m_hoveredObject != null && m_hoveredObject.isInteractable)
            m_interactionText.text = m_hoveredObject.interactiontName;
        else m_interactionText.text = "";

        if (m_hoveredObject != null && m_hoveredObject.isInteractable && Input.GetButtonDown(interactionButton))
            m_hoveredObject.onInteraction();
    }

    void onMapStart(MapStartEvent e)
    {
        m_mapEnabled = true;
    }

    void onMapEnd(MapEndEvent e)
    {
        m_mapEnabled = false;
    }
}
