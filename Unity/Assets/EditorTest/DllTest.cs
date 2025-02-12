using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using DllUtils;
using NUnit.Framework;
using PIToolKit.Public.Native;
using UnityEngine;
using Debug = UnityEngine.Debug;

public unsafe class DllTest
{
    public delegate void InitDelegate(IntPtr logptr);
    public delegate void LogDelegate(IntPtr msg);
    [Test]
    public void StaticTest()
    {
        var dllptr = DllManager.LoadLibrary(Application.dataPath + "/Plugins/x86_64/NativeSDK.dll");
        try
        {
            var setfunc = DllManager.GetDelegate<InitDelegate>(dllptr, "SetStaticFunc");
            var method = typeof(DllTest).GetMethod("Log", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var logptr = method.MethodHandle.GetFunctionPointer();
            setfunc(logptr);

            var invoke = DllManager.GetDelegate<LogDelegate>(dllptr, "StaticInvoke");
            var strptr = Marshal.StringToHGlobalAnsi("StaticTest");
            invoke(strptr);
            Marshal.FreeHGlobal(strptr);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            DllManager.FreeLibrary(dllptr);
        }
    }
    private static void Log(IntPtr msg)
    {
        var str = Marshal.PtrToStringAnsi(msg);
        File.WriteAllText("Assets/Log.txt", str + "\n");
        Debug.Log(str);
    }

    public delegate void SetArray(IntPtr array);
    public delegate void IndexInvoke(int index);
    public delegate void ItemInvoke(void* item);
    [Test]
    public void InstanceTest()
    {
        var dllptr = DllManager.LoadLibrary(Application.dataPath + "/Plugins/x86_64/NativeSDK.dll");
        try
        {
            var method = typeof(TestClass).GetMethod("TestFunc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var funcptr = method.MethodHandle.GetFunctionPointer();
            var setfunc = DllManager.GetDelegate<InitDelegate>(dllptr, "SetInstanceFunc");
            setfunc(funcptr);

            //将引用数组设置到C++再通过索引调用方法
            {
                TestClass[] objs = new TestClass[] { new TestClass(42), new TestClass(43) };
                Debug.Log("InvokeByIndex-----------------------");
                var setarray = DllManager.GetDelegate<SetArray>(dllptr, "SetArray");
                var handle = GCHandle.Alloc(objs, GCHandleType.Pinned);
                IntPtr address = handle.AddrOfPinnedObject();
                setarray(address);

                var invoke = DllManager.GetDelegate<IndexInvoke>(dllptr, "InstanceInvokeByIndex");
                for (int i = 0; i < objs.Length; i++)
                {
                    invoke(i);
                }

                handle.Free();
            }
            //直接通过数组元素调用
            {
                Debug.Log("InvokeByItem-----------------------");
                var invoke = DllManager.GetDelegate<ItemInvoke>(dllptr, "InstanceInvokeByItem");
                TestClass[] objs = new TestClass[] { new TestClass(42), new TestClass(43) };

                //IntPtr* arrayPtr = (IntPtr*)Marshal.UnsafeAddrOfPinnedArrayElement(objs, 0);

                //for (int i = 0; i < objs.Length; i++)
                //{
                //    invoke(arrayPtr[i].ToPointer());
                //}
                for (int i = 0; i < objs.Length; i++)
                {
                    invoke(objs[i].GetPointer());
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            DllManager.FreeLibrary(dllptr);
        }

    }

    public delegate void SwapInvoke(int start, int count);
    [Test]
    public void SwapItemTest()
    {
        TestClass[] objs = new TestClass[] { new TestClass(42), new TestClass(43) };
        Debug.Log($"交换前-------------");
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].TestFunc();
        }


        var dllptr = DllManager.LoadLibrary(Application.dataPath + "/Plugins/x86_64/NativeSDK.dll");
        var setarray = DllManager.GetDelegate<SetArray>(dllptr, "SetArray");
        var handle = GCHandle.Alloc(objs, GCHandleType.Pinned);
        IntPtr address = handle.AddrOfPinnedObject();
        setarray(address);

        var swapfunc = DllManager.GetDelegate<SwapInvoke>(dllptr, "SwapItem");
        swapfunc(0, 2);
        handle.Free();
        DllManager.FreeLibrary(dllptr);

        Debug.Log($"交换后-------------");
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].TestFunc();
        }
    }

    [Test]
    public void PointerAccessTimeTest()
    {
        int count = 100000;

        TestClass[] objs = new TestClass[] { new TestClass(42), new TestClass(43) };

        var time = ReckonTime(() =>
        {
            for (int i = 0; i < count; i++)
            {
                objs[0].GetPointer();
            }
        });
        Debug.Log($"GetPointer耗时：{time}ms");

        IntPtr* arrayPtr = (IntPtr*)Marshal.UnsafeAddrOfPinnedArrayElement(objs, 0);
        time = ReckonTime(() =>
        {
            for (int i = 0; i < count; i++)
            {
                arrayPtr[0].ToPointer();
            }
        });
        Debug.Log($"MarshalArray耗时：{time}ms");
    }


    public delegate void InitialProperty(IntPtr getter, IntPtr setter);
    public delegate void TestFunc(void* ptr);
    [Test]
    public void PropertyTest()
    {
        var property = typeof(TestClass).GetProperty("Property", BindingFlags.Instance | BindingFlags.Public);
        IntPtr getter = property.GetMethod.MethodHandle.GetFunctionPointer();
        IntPtr setter = property.SetMethod.MethodHandle.GetFunctionPointer();

        var dllptr = DllManager.LoadLibrary(Application.dataPath + "/Plugins/x86_64/NativeSDK.dll");
        //设置属性的方法
        DllManager.GetDelegate<InitialProperty>(dllptr, "PropertyTest")(getter, setter);

        var obj = new TestClass(21) { Property = 1024 };

        Debug.Log($"Property:{obj.Property}");

        DllManager.GetDelegate<TestFunc>(dllptr, "GetTest")(obj.GetPointer());
        DllManager.GetDelegate<TestFunc>(dllptr, "SetTest")(obj.GetPointer());

        Debug.Log($"Property:{obj.Property}");

        DllManager.FreeLibrary(dllptr);
    }

    public static double ReckonTime(Action action)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        action();
        watch.Stop();
        TimeSpan timespan = watch.Elapsed;
        return timespan.TotalMilliseconds;
    }


    public class TestClass
    {
        public int ID;

        public float Property { get; set; }

        public TestClass(int id)
        {
            ID = id;
        }

        public void TestFunc()
        {
            File.AppendAllText("Assets/Log.txt", ID.ToString() + "\n");
            Debug.Log(ID);
        }
    }
}
