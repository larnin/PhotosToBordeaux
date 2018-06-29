using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

public class CameraLogic : MonoBehaviour
{
    public class PhotoInfos
    {
        public Vector2 pos;
        public Texture2D photo;
    }

    public static List<PhotoInfos> m_photos = new List<PhotoInfos>();

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
    [SerializeField] float m_maxPhotoDistance = 100;
    [SerializeField] int m_maxPhotoCount = 24;

    SubscriberList m_subscriberList = new SubscriberList();
    Transform map;
    int m_photoCount;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CameraStartEvent>.Subscriber(onCameraStart));
        m_subscriberList.Add(new Event<CameraEndEvent>.Subscriber(onCameraEnd));
        m_subscriberList.Subscribe();

        gameObject.SetActive(false);
        m_flashSurface.SetActive(false);
        map = GameObject.Find("GameMap").GetComponent<Transform>();

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
        if (!gameObject.activeSelf || m_photoCount > m_maxPhotoCount)
            return;

        m_flashSurface.SetActive(true);
        DOVirtual.DelayedCall(m_flashDuration, () => m_flashSurface.SetActive(false));

        const int width = 640;
        const int height = 360;

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

        PhotoInfos infos = new PhotoInfos();
        infos.photo = pic;
        infos.pos = getPhotoLocation();

        m_photos.Add(infos);

        m_photoCount++;

        Event<PhotoTakenEvent>.Broadcast(new PhotoTakenEvent(pic, m_photoCount, m_maxPhotoCount));
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

    Vector2 getPhotoLocation()
    {
        Plane p = new Plane(map.up, map.position);
        float d = 0;
        Ray r = new Ray(m_camera.transform.position, m_camera.transform.forward);
        bool ok = p.Raycast(r, out d);
        if (!ok)
            return new Vector2(-10000, -10000);
        var point = r.origin + r.direction * d;
        point.Scale(new Vector3(1/map.localScale.x, 1/map.localScale.y, 1/map.localScale.z));
        point /= 2.0f;

        return new Vector2(point.x, point.z);
    }
}
