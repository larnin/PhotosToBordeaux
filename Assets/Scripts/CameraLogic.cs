using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

public class CameraLogic : MonoBehaviour
{
    static List<Texture2D> m_photos = new List<Texture2D>();

    const string photoKey = "Fire1";
    const string fovKey = "Mouse ScrollWheel";

    [SerializeField] Camera m_camera;
    [SerializeField] Camera m_uiCamera;
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

        m_photos.Clear();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        if (Input.GetButtonDown(photoKey))
            DOVirtual.DelayedCall(0.01f, takePhoto);
        var offset = Input.GetAxisRaw(fovKey);
        if(offset != 0)
            zoom(Math.Sign(offset));
    }

    void takePhoto()
    {
        if (!gameObject.activeSelf)
            return;

        m_flashSurface.SetActive(true);
        DOVirtual.DelayedCall(m_flashDuration, () => m_flashSurface.SetActive(false));

        const int width = 320;
        const int height = 180;

        RenderTexture rt = new RenderTexture(width, height, 24);
        m_camera.targetTexture = rt;
        Texture2D pic = new Texture2D(width, height, TextureFormat.RGB24, false);
        m_camera.Render();
        RenderTexture.active = rt;
        pic.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        pic.Apply();
        m_camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        m_photos.Add(pic);
    }

    void onCameraStart(CameraStartEvent e)
    {
        gameObject.SetActive(true);
    }

    void onCameraEnd(CameraEndEvent e)
    {
        gameObject.SetActive(false);
        resetZoom();
    }

    void resetZoom()
    {
        float d = m_minFOV - m_camera.fieldOfView;
        m_camera.DOFieldOfView(m_minFOV, Mathf.Abs(d * m_FOVStepTime / m_FOVStep));
    }

    void zoom(int stepCount)
    {
        float target = m_camera.fieldOfView + stepCount * m_FOVStep;
        target = Mathf.Clamp(target, Mathf.Min(m_minFOV, m_maxFOV), Mathf.Max(m_minFOV, m_maxFOV));
        m_camera.DOFieldOfView(target, Mathf.Abs(stepCount * m_FOVStepTime));
    }
}
