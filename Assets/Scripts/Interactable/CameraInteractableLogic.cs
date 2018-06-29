using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CameraInteractableLogic : BaseInteractableLogic
{
    SubscriberList m_subscriberList = new SubscriberList();

    AudioSource m_source;

    bool m_interactable = true;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();

        m_source = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override string interactiontName { get { return "Appareil photo"; } }
    public override bool isInteractable { get { return m_interactable; } }

    public override void onHoverEnd()
    {

    }

    public override void onHoverStart()
    {

    }

    public override void onInteraction()
    {
        Event<CameraStartEvent>.Broadcast(new CameraStartEvent());
        m_source.Play();
        m_interactable = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

    }

    void onCameraEnd(CameraEndEvent e)
    {
        m_interactable = true;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        m_source.Play();
    }
}
