using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PhotoCounterLogic : MonoBehaviour
{
    [SerializeField] int m_warningCount = 5;
    [SerializeField] float m_visibleTime = 2;
    [SerializeField] float m_hideTime = 1;
    [SerializeField] float m_visibleOffset = 200;

    Text m_CountLabel;
    Image m_image;
    SubscriberList m_subscriberList = new SubscriberList();
    float m_origine;
    Tween m_tween;

    private void Awake()
    {
        m_CountLabel = transform.Find("CountLabel").GetComponent<Text>();
        m_image = transform.Find("PhotoBack").Find("Photo").GetComponent<Image>();
        m_CountLabel.gameObject.SetActive(false);

        m_subscriberList.Add(new Event<PhotoTakenEvent>.Subscriber(onPhotoTaken));
        m_subscriberList.Subscribe();
    }

    private void Start()
    {
        m_origine = transform.localPosition.y;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void onPhotoTaken(PhotoTakenEvent e)
    {
        if (m_tween != null)
        {
            m_tween.Kill();
            m_tween = null;
        }

        m_image.sprite = Sprite.Create(e.texture, new Rect(0, 0, e.texture.width, e.texture.height), new Vector2(e.texture.width / 2, e.texture.height / 2));
        m_CountLabel.gameObject.SetActive(true);
        m_CountLabel.text = e.photoCount + "/" + e.maxPhotoCount;
        if (e.maxPhotoCount - e.photoCount < m_warningCount)
            m_CountLabel.color = Color.red;

        transform.localPosition = new Vector3(transform.localPosition.x, m_origine + m_visibleOffset, transform.localPosition.z);

        m_tween = DOVirtual.DelayedCall(m_visibleTime, () =>
        {
            m_tween = transform.DOLocalMoveY(m_origine, m_hideTime).OnComplete(() =>
            {
                if (e.maxPhotoCount - e.photoCount >= m_warningCount)
                    m_CountLabel.gameObject.SetActive(false);
                m_tween = null;
            });
        });
    }
}