using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

public class ButtonInteractableLogic : BaseInteractableLogic
{
    Color lightColor = new Color(1, 0, 0);
    Color pressedColor = new Color(1, 1, 1);
    const float pressTime = 0.5f;
    const float blinkTime = 1.0f;

    [SerializeField] bool m_bigButton = false;

    bool m_cameraActive = false;
    bool m_enabled = false;
    float m_remainingTime;
    SubscriberList m_subscriberList = new SubscriberList();

    GameObject m_offButton;
    GameObject m_redButton;
    GameObject m_pressedButton;
    Light m_light;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraStartEvent>.Subscriber(onCameraStart));
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();

        ButtonManagerLogic.addButton(this);

        m_offButton = transform.Find("bouton").gameObject;
        m_redButton = transform.Find("boutonlight").gameObject;
        m_pressedButton = transform.Find("boutonpressed").gameObject;
        m_light = transform.Find("Point Light").GetComponent<Light>();

        setState(LightState.Off);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
        ButtonManagerLogic.removeButton(this);
    }

    public void startButton(float maxTime)
    {
        if (m_enabled)
            return;

        m_enabled = true;
        m_remainingTime = maxTime;

        setState(LightState.Red);
    }

    private void Update()
    {
        if(m_enabled)
        {
            bool lastState = Mathf.FloorToInt(m_remainingTime / blinkTime) % 2 == 0;
            m_remainingTime -= Time.deltaTime;
            bool currentState = Mathf.FloorToInt(m_remainingTime / blinkTime) % 2 == 0;
            if(lastState != currentState)
                setState(currentState ? LightState.Off : LightState.Red);

            if (m_remainingTime <= 0)
                onEndTimer();
        }
    }

    enum LightState
    {
        Off,
        Red,
        Pressed
    }

    void setState(LightState s)
    {
        switch(s)
        {
            case LightState.Off:
                m_pressedButton.SetActive(false);
                m_redButton.SetActive(false);
                m_offButton.SetActive(true);
                m_light.gameObject.SetActive(false);
                break;
            case LightState.Pressed:
                m_pressedButton.SetActive(true);
                m_redButton.SetActive(false);
                m_offButton.SetActive(false);
                m_light.gameObject.SetActive(true);
                m_light.color = pressedColor;
                break;
            case LightState.Red:
                m_pressedButton.SetActive(false);
                m_redButton.SetActive(true);
                m_offButton.SetActive(false);
                m_light.gameObject.SetActive(true);
                m_light.color = lightColor;
                break;
        }
    }

    void onEndTimer()
    {
        Event<ButtonTimeoutEvent>.Broadcast(new ButtonTimeoutEvent());
    }

    public override string interactiontName { get { return m_bigButton ? "Big fat bouton" : "Bouton"; } }

    public override bool isInteractable { get { return !m_cameraActive; } }

    public override void onHoverEnd()
    {
        
    }

    public override void onHoverStart()
    {

    }

    public override void onInteraction()
    {
        m_enabled = false;
        setState(LightState.Pressed);
        DOVirtual.DelayedCall(pressTime, () =>
        {
            if (!m_enabled)
                setState(LightState.Off);
        });
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