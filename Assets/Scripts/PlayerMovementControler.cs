using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class PlayerMovementControler : MonoBehaviour
{
    public enum UpdateType
    {
        UPDATE,
        LATE_UPDATE,
        FIXED_UPDATE,
    }

    const string horizontalAxis = "Horizontal";
    const string verticalAxis = "Vertical";

    [SerializeField] UpdateType m_updateType = UpdateType.UPDATE;
    [SerializeField] float m_forwardSpeed = 1;
    [SerializeField] float m_sideSpeed = 1;
    [SerializeField] float m_backSpeed = 1;

    Transform m_camera;
    Rigidbody m_rigidbody;

    float m_horizontal;
    float m_vertical;

    private void Start()
    {
        m_camera = GetComponentInChildren<Camera>().transform;
        m_rigidbody = GetComponent<Rigidbody>();
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
        var forward = m_camera.forward;
        forward.y = 0;
        forward.Normalize();
        var side = new Vector3(forward.z, 0, -forward.x);

        var dir = forward * m_vertical * (m_vertical > 0 ? m_forwardSpeed : m_backSpeed) + side * m_horizontal * m_sideSpeed;
        dir.y = m_rigidbody.velocity.y;
        m_rigidbody.velocity = dir;
    }

    void updateControls()
    {
        m_horizontal = Input.GetAxisRaw(horizontalAxis);
        m_vertical = Input.GetAxisRaw(verticalAxis);
    }
}
