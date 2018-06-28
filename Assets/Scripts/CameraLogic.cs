using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

public class CameraLogic : MonoBehaviour
{
    const string photoKey = "Fire1";
    const string fovKey = "Mouse ScrollWheel";

    [SerializeField] Camera m_camera;
    [SerializeField] GameObject m_flashSurface;
    [SerializeField] float m_flashDuration;
    [SerializeField] float m_minFOV = 60;
    [SerializeField] float m_maxFOV = 20;
    [SerializeField] float m_FOVStep = 2;
    [SerializeField] float m_FOVStepTime = 0.2f;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraStartEvent>.Subscriber(onCameraStart));
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();

        gameObject.SetActive(false);
        m_flashSurface.SetActive(false);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        if (Input.GetButtonDown(photoKey))
            DOVirtual.DelayedCall(0.01f, takePhoto);
    }

    void takePhoto()
    {
        if (!gameObject.activeSelf)
            return;

        m_flashSurface.SetActive(true);
        DOVirtual.DelayedCall(m_flashDuration, () => m_flashSurface.SetActive(false));
    }

    void onCameraStart(CameraStartEvent e)
    {
        gameObject.SetActive(true);
    }

    void onCameraEnd(CameraEndEvent e)
    {
        gameObject.SetActive(false);
    }
}
