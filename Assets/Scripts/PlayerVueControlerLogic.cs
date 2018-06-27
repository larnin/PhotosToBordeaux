using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PlayerVueControlerLogic : MonoBehaviour
{
    public enum UpdateType
    {
        UPDATE,
        LATE_UPDATE,
        FIXED_UPDATE,
    }
    
    string mouseNameAxisCamX = "Mouse X";
    string mouseNameAxisCamY = "Mouse Y";
    
    [SerializeField] UpdateType m_updateType = UpdateType.UPDATE;
    [SerializeField] bool m_inverseVerticalAxis = false;
    [SerializeField] float m_mouseSensibility = 1;
    [SerializeField] float m_clampVerticalRotationTop = 85;
    [SerializeField] float m_clampVerticalRotationBottom = -85;
    [SerializeField] float m_startSpeedRotationDecreaseDistance = 20;
    [SerializeField] float m_powMoveSpeed = 1.5f;
    [SerializeField] float m_cameraRotSpeed = 2;

    float m_mouseX;
    float m_mouseY;

    float m_rotX = 0;
    float m_rotY = 0;

    float m_offsetRotX = 0;
    float m_offsetRotY = 0;

    private void Start()
    {
        var rot = transform.localRotation.eulerAngles;
        m_rotX = rot.y;
        m_rotY = rot.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        updateControls();

        if (Time.deltaTime > 0 && m_updateType == UpdateType.UPDATE)
            onUpdate();
    }

    private void FixedUpdate()
    {
        if (Time.deltaTime > 0 && m_updateType == UpdateType.FIXED_UPDATE)
            onUpdate();
    }

    private void LateUpdate()
    {
        if (Time.deltaTime > 0 && m_updateType == UpdateType.LATE_UPDATE)
            onUpdate();
    }

    void onUpdate()
    {
        m_offsetRotX += Mathf.Sign(m_mouseX) * Mathf.Pow(Mathf.Abs(m_mouseX * m_mouseSensibility), m_powMoveSpeed);
        m_offsetRotY += Mathf.Sign(m_mouseY) * Mathf.Pow(Mathf.Abs(m_mouseY * m_mouseSensibility), m_powMoveSpeed);

        float valueX = Mathf.Sign(m_offsetRotX) * Mathf.Pow(Mathf.Abs(m_offsetRotX) * m_cameraRotSpeed, m_powMoveSpeed) * Time.deltaTime;
        if (Mathf.Abs(valueX) > Mathf.Abs(m_offsetRotX))
            valueX = m_offsetRotX;
        m_offsetRotX -= valueX;
        m_rotX += valueX;

        float valueY = Mathf.Sign(m_offsetRotY) * Mathf.Pow(Mathf.Abs(m_offsetRotY) * m_cameraRotSpeed, m_powMoveSpeed) * Time.deltaTime;
        if (Mathf.Abs(valueY) > Mathf.Abs(m_offsetRotY))
            valueY = m_offsetRotY;
        m_offsetRotY -= valueY;

        float multiplierY = 1;
        if (m_rotY < m_clampVerticalRotationBottom + m_startSpeedRotationDecreaseDistance && valueY < 0)
            multiplierY = Mathf.Abs(m_rotY - m_clampVerticalRotationBottom) / m_startSpeedRotationDecreaseDistance;
        if (m_rotY > m_clampVerticalRotationTop - m_startSpeedRotationDecreaseDistance && valueY > 0)
            multiplierY = Mathf.Abs(m_rotY - m_clampVerticalRotationTop) / m_startSpeedRotationDecreaseDistance;
        m_rotY += valueY * multiplierY;
        m_rotY = Mathf.Clamp(m_rotY, m_clampVerticalRotationBottom, m_clampVerticalRotationTop);

        transform.localRotation = Quaternion.Euler(m_rotY, m_rotX, 0);
    }

    void updateControls()
    {
        m_mouseX = Input.GetAxisRaw(mouseNameAxisCamX);
        m_mouseY = Input.GetAxisRaw(mouseNameAxisCamY);
        if (m_inverseVerticalAxis)
            m_mouseY *= -1;
    }
}
