using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapInteractableLogic : BaseInteractableLogic
{
    SubscriberList m_subscriberList = new SubscriberList();
    bool m_canInteract = true;
    bool m_cameraActive = false;
    AudioSource m_source;

    private void Awake()
    {
        m_subscriberList.Add(new Event<MapEndEvent>.Subscriber(onMapEnd));
        m_subscriberList.Add(new Event<CameraStartEvent>.Subscriber(onCameraStart));
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();

        m_source = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override string interactiontName { get { return "Carte"; } }
    public override bool isInteractable { get { return m_canInteract && !m_cameraActive; } }

    public override void onHoverEnd()
    {

    }

    public override void onHoverStart()
    {

    }

    public override void onInteraction()
    {
        Event<MapStartEvent>.Broadcast(new MapStartEvent());
        m_canInteract = false;
        m_source.Play();
    }

    void onMapEnd(MapEndEvent e)
    {
        m_canInteract = true;
        m_source.Play();
    }

    void onCameraStart(CameraStartEvent e)
    {
        m_cameraActive = true;
    }
    
    void onCameraEnd(CameraEndEvent e)
    {
        m_cameraActive = false;
    }
}
