using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CameraInteractableLogic : BaseInteractableLogic
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override string interactiontName { get { return "Appareil photo"; } }
    public override bool isInteractable { get { return true; } }

    public override void onHoverEnd()
    {

    }

    public override void onHoverStart()
    {

    }

    public override void onInteraction()
    {
        Event<CameraStartEvent>.Broadcast(new CameraStartEvent());
        gameObject.SetActive(false);
    }

    void onCameraEnd(CameraEndEvent e)
    {
        gameObject.SetActive(true);
    }
}
