using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

[RequireComponent(typeof(Animator))]
public class PlayerScript : MonoBehaviour
{
    private uint? m_RightHandId;
    private uint? m_LeftHandId;
    private Vector3 m_LastRightHandPosition = Vector3.zero;
    private Vector3 m_LastLeftHandPosition = Vector3.zero;
    private bool m_FoundFloor;
    private float m_MaxFloorDistance = 0.1f;
    private Vector3 m_LastHeadLocation = Vector3.zero;
    private float m_MovementThreshold = 0.01f;
    private Vector3 m_LastBodyRotationForward;
        
    private Animator m_Animator;

    [SerializeField]
    private AudioSource m_AudioSource;

    // Use this for initialization
    void Start()
    {
        m_Animator = GetComponent<Animator>();

        m_LastBodyRotationForward = Camera.main.transform.forward;

        InteractionManager.SourceDetected += InteractionManager_SourceDetected;
        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
        InteractionManager.SourceLost += InteractionManager_SourceLost;
        //SurfaceMeshesToPlanes.Instance.MakePlanesComplete += Instance_MakePlanesComplete;
    }

    private void InteractionManager_SourceLost(InteractionSourceState state)
    {
        m_LastRightHandPosition = Vector3.zero;
        m_LastLeftHandPosition = Vector3.zero;
    }

    private void InteractionManager_SourceUpdated(InteractionSourceState state)
    {
        updateHandPosition(state);
    }

    private void InteractionManager_SourceDetected(InteractionSourceState state)
    {
        //state.properties.sourceLossRisk
        //state.properties.location.TryGetVelocity
        if (state.source.kind == InteractionSourceKind.Hand)
        {
            determineHand(state);
        }
        else if (state.source.kind == InteractionSourceKind.Other)
        {
            m_AudioSource.Play();
        }
    }

    private void determineHand(InteractionSourceState state)
    {
        Vector3 handPosition;
        if (state.properties.location.TryGetPosition(out handPosition))
        {
            Vector3 handDirection = handPosition - Camera.main.transform.position;
            Vector3 facingDirection = Camera.main.transform.forward;

            float angle = angleDir(facingDirection, handDirection, Camera.main.transform.up);

            if (angle > 0)
            {
                m_RightHandId = state.source.id;
                updateHandPosition(state);
            }
            else
            {
                m_LeftHandId = state.source.id;
                updateHandPosition(state);
            }
        }
    }

    float angleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    private void updateHandPosition(InteractionSourceState state)
    {
        if (m_RightHandId != null && state.source.id == m_RightHandId)
        {
            state.properties.location.TryGetPosition(out m_LastRightHandPosition);
        }

        if (m_LeftHandId != null && state.source.id == m_LeftHandId)
        {
            state.properties.location.TryGetPosition(out m_LastLeftHandPosition);
        }
    }

    void OnAnimatorIK()
    {
        if (m_LastRightHandPosition != Vector3.zero)
        {
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            m_Animator.SetIKPosition(AvatarIKGoal.RightHand, m_LastRightHandPosition);
        }
        else
        {
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }

        if (m_LastLeftHandPosition != Vector3.zero)
        {
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            m_Animator.SetIKPosition(AvatarIKGoal.LeftHand, m_LastLeftHandPosition);
        }
        else
        {
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        }

        m_Animator.SetLookAtWeight(1);
        m_Animator.SetLookAtPosition(Camera.main.transform.position + Camera.main.transform.forward);
        
        m_Animator.SetBoneLocalRotation(HumanBodyBones.Head, Camera.main.transform.rotation);

        if (Vector3.Angle(m_LastBodyRotationForward, Camera.main.transform.forward) > 30)
        {
            transform.parent.forward = m_LastBodyRotationForward = Camera.main.transform.forward;
        }
        
        float movementDelta = Vector3.Distance(Camera.main.transform.position, m_LastHeadLocation);
        if (!m_Animator.GetBool("Walk") && movementDelta > m_MovementThreshold)
        {
            m_Animator.SetBool("Walk", true);
            m_Animator.SetBool("WalkSide", !m_Animator.GetBool("WalkSide"));
        }
        else if(movementDelta <= m_MovementThreshold)
        {
            m_Animator.SetBool("Walk", false);
        }
        m_LastHeadLocation = Camera.main.transform.position;

        if (Input.GetMouseButton(0))
        {
            Vector3 handPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            m_Animator.SetIKPosition(AvatarIKGoal.RightHand, handPosition);
        }
    }
}
