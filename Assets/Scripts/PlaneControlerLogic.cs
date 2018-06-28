using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

class PlaneControlerLogic : MonoBehaviour
{
    const string horizontalAxis = "Horizontal";
    const string verticalAxis = "Vertical";

    [SerializeField] float m_speed = 1;
    [SerializeField] float m_rotSpeed = 1;
    [SerializeField] float m_rollMax = 1;

    [SerializeField] float m_verticalSpeed = 1;
    [SerializeField] float m_verticalMax = 1;
    [SerializeField] float m_minHeight = 1;
    [SerializeField] float m_maxHeight = 1;
    [SerializeField] Ease m_verticalEase = Ease.Linear;

    [SerializeField] float m_accelerationTime = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector3 m_angle = Vector3.zero;

    float m_leftPressTime = 0;
    float m_rightPressTime = 0;
    float m_upPressTime = 0;
    float m_downPressTime = 0;

    bool m_left = false;
    bool m_right = false;
    bool m_up = false;
    bool m_down = false;

    private void Awake()
    {
        m_angle = transform.rotation.eulerAngles;

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(onGenerationEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        updateControls();
    }

    private void FixedUpdate()
    {
        float leftValue = Mathf.Clamp(m_leftPressTime / m_accelerationTime, -1, 1);
        if (leftValue < 0)
            leftValue = 1 + leftValue;
        float rightValue = Mathf.Clamp(m_rightPressTime / m_accelerationTime, -1, 1);
        if (rightValue < 0)
            rightValue = 1 + rightValue;
        float upValue = Mathf.Clamp(m_upPressTime / m_accelerationTime, -1, 1);
        if (upValue < 0)
            upValue = 1 + upValue;
        float downValue = Mathf.Clamp(m_downPressTime / m_accelerationTime, -1, 1);
        if (downValue < 0)
            downValue = 1 + downValue;

        m_angle.y += (rightValue - leftValue) * m_rotSpeed * Time.deltaTime;
        m_angle.z = -(rightValue - leftValue) * m_rollMax;
        m_angle.x = -(DOVirtual.EasedValue(0, 1, upValue, m_verticalEase) - DOVirtual.EasedValue(0, 1, downValue, m_verticalEase)) * m_verticalMax;

        transform.rotation = Quaternion.Euler(m_angle);

        var pos = transform.position;
        pos.y = Mathf.Clamp(pos.y + (upValue - downValue) * m_verticalSpeed * Time.deltaTime, m_minHeight, m_maxHeight);

        var dir = transform.forward;
        dir.y = 0;
        pos += dir * Time.deltaTime * m_speed;

        transform.position = pos;
    }

    void updateControls()
    {
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);

        m_left = x < 0;
        m_right = x > 0;
        m_up = y > 0;
        m_down = y < 0;

        Func<float, bool, float> lambdaPressTime = (float value, bool pressed) =>
            {
                bool oldPressed = value > 0;
                if (oldPressed != pressed)
                {
                    if(Mathf.Abs(value) < m_accelerationTime)
                    {
                        if (value > 0)
                            value -= m_accelerationTime;
                        else value += m_accelerationTime;
                    }
                    else value = 0;
                }
                value += Time.deltaTime * (pressed ? 1 : -1);
                return value;
            };

        m_leftPressTime = lambdaPressTime(m_leftPressTime, m_left);
        m_rightPressTime = lambdaPressTime(m_rightPressTime, m_right);
        m_upPressTime = lambdaPressTime(m_upPressTime, m_up);
        m_downPressTime = lambdaPressTime(m_downPressTime, m_down);
    }

    void onGenerationEnd(GenerationFinishedEvent e)
    {
        m_angle.y = LevelMap.instance.startRotation;
        var map = GameObject.Find("GameMap");
        Debug.Log(map.transform.localScale);
        transform.position = new Vector3(LevelMap.instance.startPos.x * 2 * map.transform.localScale.x, transform.position.y, LevelMap.instance.startPos.y * 2 * map.transform.localScale.z);

        Debug.Log(LevelMap.instance.startPos);
    }
}
