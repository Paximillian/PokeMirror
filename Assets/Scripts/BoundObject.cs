using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundObject : MonoBehaviour
{
    [SerializeField]
    private Transform m_BoundTo;

    [SerializeField]
    private bool m_PositionBound;
    [SerializeField]
    private bool m_RotationBound;

    // Update is called once per frame
    void Update()
    {
        if (m_PositionBound)
        {
            transform.position = m_BoundTo.position;
        }
        if (m_RotationBound)
        {
            transform.rotation = m_BoundTo.rotation;
            transform.Rotate(0, 0, -90);
        }
    }
}
