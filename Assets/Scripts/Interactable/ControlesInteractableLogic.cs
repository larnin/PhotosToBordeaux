using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ControlesInteractableLogic : BaseInteractableLogic
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraStartEvent>.Subscriber(onCameraStart));
        m_subscriberList.Subscribe();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override string interactiontName { get { return "Reprendre les controles"; } }
    public override bool isInteractable { get { return true; } }

    public override void onHoverEnd()
    {

    }

    public override void onHoverStart()
    {

    }

    public override void onInteraction()
    {
        gameObject.SetActive(false);
        Event<CameraEndEvent>.Broadcast(new CameraEndEvent());
    }

    void onCameraStart(CameraStartEvent e)
    {
        gameObject.SetActive(true);
    }
}
