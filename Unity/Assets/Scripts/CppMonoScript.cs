using DllUtils;
using PIToolKit.Public.Native;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public unsafe class CppMonoScript : MonoBehaviour
{

    public delegate void InitialProperty(IntPtr getter, IntPtr setter);
    public delegate void InitialPool(int size);
    public delegate void ReleasePool();
    public delegate void AddTransform(void* transform, Vector3 position);
    public delegate void UpdateFunc(float time);

    private IntPtr dllptr;
    private UpdateFunc update;
    private void Awake()
    {
        Type transformtype = typeof(Transform);
        PropertyInfo property = transformtype.GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
        IntPtr getter = property.GetMethod.MethodHandle.GetFunctionPointer();
        IntPtr setter = property.SetMethod.MethodHandle.GetFunctionPointer();

        dllptr = DllManager.LoadLibrary(Application.dataPath + "/Plugins/x86_64/NativeSDK.dll");
        //设置属性的方法
        DllManager.GetDelegate<InitialProperty>(dllptr, "InitialProperty")(getter, setter);
        //初始化池
        DllManager.GetDelegate<InitialPool>(dllptr, "InitialPool")(transform.childCount);

        var addfunc = DllManager.GetDelegate<AddTransform>(dllptr, "AddTransform");

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            addfunc(child.GetPointer(), child.position);
        }

        update = DllManager.GetDelegate<UpdateFunc>(dllptr, "Update");
    }

    private void Update()
    {
        update?.Invoke(Time.timeSinceLevelLoad);
    }

    private void OnDisable()
    {
        DllManager.GetDelegate<ReleasePool>(dllptr, "ReleasePool")();

        DllManager.FreeLibrary(dllptr);
    }
}