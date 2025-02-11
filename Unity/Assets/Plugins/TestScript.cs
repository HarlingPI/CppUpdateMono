using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using static TestScript;

class TestScript : MonoBehaviour
{
#if UNITY_EDITOR
    // pointer handle to the C++ DLL
    public IntPtr libarayHandle;
    public delegate void InitDelegate(IntPtr gameObjectNew,
    IntPtr gameObjectGetTransform, IntPtr transformSetPosition);
#endif

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
    //OSX 和Linux下的导入
    [DLLImport("__Internal")]
    public static extern IntPtr dlopen(string path, int flag);
    [DllImport("__Internal")]
    public static extern IntPtr dlsym(IntPtr handle, string symbolName);
    [DllImport("__Internal")]
    public static extern int dlclose(IntPtr handle);

    public static IntPtr OpenLibrary(string path)
    {
        IntPtr handle = dlopen(path, 0);
        if(handle == IntPtr.Zero)
        {
           throw new Exception("Couldn't open native library: "+ path);
        }
        return handle;
    }
    
    public static void CloseLibrary(IntPtr libraryHandle)
    {
         dlclose(libraryHandle);
    }
    
    public static T GetDelegate<T>(IntPtr libraryHandle, string functionName)
    where T: class
    {
         IntPtr symbol = dlsym(libraryHandle, functionName);
         if(symbol == IntPtr.Zero)
         {
            throw new Exception("Couldn't get function:" + functionName); 
         }
         return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
    }
#elif UNITY_EDITOR_WIN
    // win 编辑器下
    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(IntPtr libraryHandle,
    string symbolName);

    [DllImport("kernel32")]
    public static extern bool FreeLibrary(IntPtr libraryHandle);

    public static IntPtr OpenLibrary(string path)
    {
        IntPtr handle = LoadLibrary(path);
        if (handle == IntPtr.Zero)
        {
            throw new Exception("Couldn't open native library: " + path);
        }
        return handle;
    }

    public static void CloseLibrary(IntPtr libraryHandle)
    {
        FreeLibrary(libraryHandle);
    }

    public static T GetDelegate<T>(IntPtr libraryHandle, string functionName)
    where T : class
    {
        IntPtr symbol = GetProcAddress(libraryHandle, functionName);
        if (symbol == IntPtr.Zero)
        {
            throw new Exception("Couldn't get function:" + functionName);
        }
        return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
    }
#else
    //本地加载
    [DllImport("NativeScript")]
    static extern void Init(IntPtr gameObjectNew,
    IntPtr gameObjectGetTransform, IntPtr transformSetPosition);

    [DllImport("NativeScript")]
    static extern void MonoBehaviourUpdate();
#endif

    delegate int GameObjectNewDelegate();
    delegate int GameObjectGetTransformDelegate(int thisHandle);
    delegate void TransformSetPositionDelegate(int thisHandle, Vector3 position);

#if UNITY_EDITOR_OSX
    const string LIB_PATH = "/NativeScript.bundle/Contents/MacOS/NativeScript";
#elif UNITY_EDITOR_LINUX
    const string LIB_PATH = "/NativeScript.so";
#elif UNITY_EDITOR_WIN
    const string LIB_PATH = "/NativeScript.dll";
#endif

    void Awake()
    {
#if UNITY_EDITOR
        //open the native library
        libarayHandle = OpenLibrary(Application.dataPath + LIB_PATH);
        InitDelegate Init = GetDelegate<InitDelegate>(libarayHandle, "Init");
        //MonoBehaviourUpdate = GetDelegate<MonoBehaviourUpdateDelegate>(
        //libarayHandle, "MonoBehaviourUpdate");
#endif

        //init the C++ Library
        //ObjectStore.Init(1024);
        Init(
        Marshal.GetFunctionPointerForDelegate(new GameObjectNewDelegate(GameObjectNew)),
        Marshal.GetFunctionPointerForDelegate(new GameObjectGetTransformDelegate(GameObjectGetTransform)),
        Marshal.GetFunctionPointerForDelegate(new TransformSetPositionDelegate(TransformSetPosition))
       );

    }

    void Update()
    {
        //MonoBehaviourUpdate();
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        CloseLibrary(libarayHandle);
        libarayHandle = IntPtr.Zero;
#endif
    }

    //c# function for c++ call
    static int GameObjectNew()
    {
        //GameObject go = new GameObject();
        //return ObjectStore.Store(go);
        return -1;
    }

    static int GameObjectGetTransform(int thisHandle)
    {
        //GameObject go = (GameObject)ObjectStore.Get(thisHandle);
        //Transform transform = go.transform;
        //return ObjectStore.Store(transform);
        return -1;
    }

    static void TransformSetPosition(int handle, Vector3 position)
    {
        //Transform t = (Transform)ObjectStore.Get(handle);
        //t.position = position;
    }
}