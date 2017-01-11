
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using HoloToolkit.Unity.SpatialMapping;

public class PlaceMirror : MonoBehaviour
{
    private Vector3 m_InitialGazeDirection;
    private MeshRenderer[] m_MeshRenderers;
    private bool m_Placed = false;

    void Start()
    {
        m_InitialGazeDirection = Camera.main.transform.forward;
    }

    // Use this for initialization
    void Update()
    {
        if (!m_Placed && Time.timeSinceLevelLoad > 10)
        {
            // Grab the mesh renderer that's on the same object as this script.
            m_MeshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();

            // Do a raycast into the world based on the user's
            // head position and orientation.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                    30.0f, SpatialMappingManager.Instance.LayerMask))
            {
                Vector3 pointToMe = (headPosition - hitInfo.point).normalized;

                if (Vector3.Dot(m_InitialGazeDirection, hitInfo.normal) < -0.9f &&
                    Vector3.Dot(m_InitialGazeDirection, pointToMe) < -0.9f)
                {
                    // If the raycast hit a hologram...
                    // Display the cursor mesh.
                    foreach (MeshRenderer renderer in m_MeshRenderers)
                    {
                        renderer.enabled = true;
                    }

                    // Move the cursor to the point where the raycast hit.
                    this.transform.position = hitInfo.point + hitInfo.normal * 0.1f;
                    transform.position.Set(transform.position.x, Camera.main.transform.position.y, transform.position.z);

                    // Rotate the cursor to hug the surface of the hologram.
                    this.transform.rotation = Quaternion.FromToRotation(-transform.forward, hitInfo.normal);
                    transform.LookAt(Camera.main.transform.position);
                    transform.forward = Camera.main.transform.forward;
                    transform.up = Vector3.up;

                    m_Placed = true;
                }
            }
        }
    }
}
