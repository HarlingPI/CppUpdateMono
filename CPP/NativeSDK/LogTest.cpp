#include "framework.h"
#include <fstream>
#include <iostream>
#include <string>

using namespace std;
//#ifdef _WIN32
//#define DLLEXPORT __declspec(dllexport)
//#else
//#define DLLEXPORT
//#endif
//
//extern "C"
//{
//	//C# VECTOR STRUCT
//	struct Vector3
//	{
//		float x;
//		float y;
//		float z;
//	};
//	//c# function for c++ to call
//	int(*GameObjectNew)();
//	int(*GameObjectGetTransform)(int thisHandle);
//	void(*TransformSetPosition)(int thisHandle, Vector3 position);
//
//	//c++ functions for c# to call
//	int numCreated;
//
//	DLLEXPORT void Init(int(*gameObjectNew)(),int(*gameObjectGetTrasform)(int),void(*transformSetPosition)(int, Vector3))
//	{
//		GameObjectNew = gameObjectNew;
//		GameObjectGetTransform = gameObjectGetTrasform;
//		TransformSetPosition = transformSetPosition;
//
//		numCreated = 0;
//	}
//	//
//	DLLEXPORT void MonoBehaviourUpdate(int thisHandle)
//	{
//		if (numCreated < 10)
//		{
//			//获取函数handle,然后操作
//			int goHandle = GameObjectNew();
//			int transformHandle = GameObjectGetTransform(goHandle);
//			float comp = 10.0f * (float)numCreated;
//			Vector3 position = { comp, comp, comp };
//			TransformSetPosition(transformHandle, position);
//			numCreated++;
//		}
//	}
//
//
//
//
//}

#define DllExport _declspec(dllexport)

extern "C"
{
	void WriteToFile(const char* ptr, int len)
	{
		ofstream os = ofstream("Assets/CppSccetped.txt", ios::out | ios::binary | ios::ate);
		if (os.is_open())
		{
			os.write(ptr, len);
		}
		os.close();
	}
	void AppendToFile(const char* ptr, int len)
	{
		ofstream os = ofstream("Assets/CppSccetped.txt", ios::out | ios::binary | ios::app);
		if (os.is_open())
		{
			os.write(ptr, len);
		}
		os.close();
	}
#pragma region StaticTest
	//定义Log函数指针
	typedef void (*StaticFunc)(const char* message);

	StaticFunc staticFunc = nullptr;

	DllExport void __stdcall SetStaticFunc(StaticFunc callback)
	{
		staticFunc = callback;
	}

	DllExport void __stdcall StaticInvoke(const char* message)
	{
		if (staticFunc)
		{
			WriteToFile(message, strlen(message));
			staticFunc(message);
		}
	}
#pragma endregion

#pragma region InstanceTest
	typedef void(*InstanceFunc)(void*);

	InstanceFunc instanceFunc = NULL;

	void** array = NULL;

	DllExport void __stdcall SetArray(void** arrayptr)
	{
		::array = arrayptr;
		auto str = "Start：" + to_string((long long)arrayptr) + "\n";
		AppendToFile(str.data(), str.length());
	}

	DllExport void __stdcall SetInstanceFunc(InstanceFunc callback)
	{
		instanceFunc = callback;
	}

	DllExport void __stdcall InstanceInvokeByIndex(int index)
	{
		if (instanceFunc)
		{
			auto item = ::array[index];
			auto str = "Index：" + to_string(index) + "  " + to_string((long long)item) + "\n";
			AppendToFile(str.data(), str.length());
			instanceFunc(item);
		}
	}

	DllExport void __stdcall InstanceInvokeByItem(void* item)
	{
		if (instanceFunc)
		{
			instanceFunc(item);
		}
	}
#pragma endregion

}