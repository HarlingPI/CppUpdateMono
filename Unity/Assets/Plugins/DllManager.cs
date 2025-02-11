using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DllUtils
{
    /// <summary>
    /// 创建者:   Harling
    /// 创建时间: 2025-02-11 11:24:45
    /// 备注:     由PIToolKit工具生成
    /// </summary>
    /// <remarks></remarks>
    public static class DllManager
    {
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr dllptr, string symbolName);

        [DllImport("kernel32")]
        public static extern bool FreeLibrary(IntPtr dllptr);

        public static T GetDelegate<T>(IntPtr dllptr, string functionName) where T : class
        {
            IntPtr symbol = GetProcAddress(dllptr, functionName);
            if (symbol == IntPtr.Zero)
            {
                throw new Exception("Couldn't get function:" + functionName);
            }
            return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
        }
    }
}
