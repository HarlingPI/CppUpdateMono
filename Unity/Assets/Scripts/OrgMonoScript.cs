using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 创建者:   Harling
/// 创建时间: 2025-02-11 10:36:02
/// 备注:     由PIToolKit工具生成
/// </summary>
/// <remarks></remarks>
public class OrgMonoScript : MonoBehaviour
{
    public float height = 1f;
    private float offset = 0;
    private Vector3 orgPos;
    private void Start()
    {
        orgPos = transform.position;
        offset = orgPos.magnitude;
    }
    private void Update()
    {
        transform.position = orgPos + MathF.Sin(offset + Time.timeSinceLevelLoad) * height * Vector3.up;
    }
}
