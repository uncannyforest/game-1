using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Cameras;

/// <summary>FreeLookCam, minus follow player and not storing orientation state<summary>
[RequireComponent(typeof(MixedAutoCam))]
public class OrthoFreeLookCamMovement : MonoBehaviour
{
    // This script is designed to be placed on the root object of a camera rig,
    // comprising 3 gameobjects, each parented to the next:

    // 	Camera Rig
    // 		Pivot
    // 			Camera

    [Range(0f, 10f)] [SerializeField] private float m_HTurnSpeed = 1.5f;  // How fast the rig will rotate left-right from user input.
    [Range(0f, 1f)] [SerializeField] private float m_VTurnSpeed = .03f;   // How fast the rig will rotate forward-back from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    [SerializeField] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.
    [SerializeField] private float m_MinCameraSize = 1f;
    [SerializeField] private float m_DefaultCameraSize = 2f;
    [SerializeField] private float m_MaxCameraSize = 4f;
    [Range(0f, 1f)] [SerializeField] private float m_ResetVTurnSpeed = .15f;
    [SerializeField] private float m_HoldForIsoTime = .2f;
    
    public float m_CameraSize {
        get => m_CamConfig.orthographicSize;
        set {
            m_CamConfig.orthographicSize = value;
        }
    }

    private bool m_IsResettingCamera = false;
    private bool m_RotDirPos;
    private int m_AimingForIso = 0;
    private float m_XKeyHoldTime = 0;
    private MixedAutoCam m_AutoScript;

    protected Camera m_CamConfig;
    protected Transform m_Cam; // the transform of the camera
    protected Transform m_Pivot; // the point at which the camera pivots around

    protected void Awake()
    {
        // find the camera in the object hierarchy
        m_CamConfig = GetComponentInChildren<Camera>();
        m_Cam = m_CamConfig.transform;
        m_Pivot = m_Cam.parent;

        // Lock or unlock the cursor.
        Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !m_LockCursor;
        
        m_AutoScript = GetComponent<MixedAutoCam>();
    }


    protected void Update()
    {
        if (SimpleInput.GetButtonDown("Reset Camera") && m_AutoScript.IsDoubleResetReady) {
            m_IsResettingCamera = true;
        }
        HandleRotationMovement();
        if (m_LockCursor && Input.GetMouseButtonUp(0))
        {
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_LockCursor;
        }
    }


    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleRotationMovement()
    {
        if(Time.timeScale < float.Epsilon)
        return;

        // Read the user input
        var x = SimpleInput.GetAxis("Mouse X");
        var y = SimpleInput.GetAxis("Mouse Y");

        // get current angle
        Vector3 m_TransformEulers = transform.localRotation.eulerAngles;
        float m_LookAngle = m_TransformEulers.y;
        if (m_LookAngle < -45) {
            m_LookAngle += 360;
        } else if (m_LookAngle > 315) {
            m_LookAngle -= 360;
        }

        // Force iso
        if (x != 0) {
            if (m_XKeyHoldTime == 0) { // key down
                m_XKeyHoldTime = Time.time + m_HoldForIsoTime;
                if (m_AimingForIso != 0) { // reset
                    m_AimingForIso = 0;
                }
            }
            m_RotDirPos = x > 0;
        } else if (m_XKeyHoldTime != 0) { // release
            if (Time.time > m_XKeyHoldTime) { // key was held
                if (m_RotDirPos) {
                    m_AimingForIso = Mathf.CeilToInt((m_LookAngle - 45) / 90) * 90 + 45;
                } else {
                    m_AimingForIso = Mathf.FloorToInt((m_LookAngle - 45) / 90) * 90 + 45;
                }
            }
            m_XKeyHoldTime = 0;
        }

        if (m_AimingForIso == 0) {
            if (x != 0) {
                m_LookAngle += m_RotDirPos ? m_HTurnSpeed : -m_HTurnSpeed;
            }
        } else {
            if (m_RotDirPos) {
                m_LookAngle += m_HTurnSpeed;
                if (m_LookAngle > m_AimingForIso) {
                    m_LookAngle = m_AimingForIso;
                    m_AimingForIso = 0;
                }
            } else {
                m_LookAngle -= m_HTurnSpeed;
                if (m_LookAngle < m_AimingForIso) {
                    m_LookAngle = m_AimingForIso;
                    m_AimingForIso = 0;
                }
            }
        }

        // Rotate the rig (the root object) around Y axis only:
        Quaternion m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

        // we adjust the current angle based on Y mouse input and turn speed
        if (!m_IsResettingCamera) {
            m_CameraSize -= y*m_VTurnSpeed*m_CameraSize; // multiply by m_CameraSize for more natural, log movement
        } else {
            if (Mathf.Abs(m_CameraSize - m_DefaultCameraSize) < m_ResetVTurnSpeed) {
                m_IsResettingCamera = false;
            }
            m_CameraSize = Mathf.MoveTowards(m_CameraSize, m_DefaultCameraSize, m_ResetVTurnSpeed);
        }
        // and make sure the new value is within the size range
        m_CameraSize = Mathf.Clamp(m_CameraSize, m_MinCameraSize, m_MaxCameraSize);


        if (m_TurnSmoothing > 0)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
        }
        else
        {
            transform.localRotation = m_TransformTargetRot;
        }
    }
}