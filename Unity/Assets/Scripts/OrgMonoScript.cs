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
    private float[] offsets;
    private Vector3[] positions;
    private Transform[] transforms;
    private void Awake()
    {
        offsets = new float[transform.childCount];
        positions = new Vector3[transform.childCount];
        transforms = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            transforms[i] = transform.GetChild(i);
            positions[i] = child.position;
            offsets[i] = positions[i].magnitude;
        }
    }
    private void Update()
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            var transform = transforms[i];
            var offset = new Vector3(0, Mathf.Sin(offsets[i] + Time.timeSinceLevelLoad), 0);
            transform.position = positions[i] + offset;
        }
    }
}
