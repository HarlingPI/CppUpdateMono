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
	void WriteToFile(string str)
	{
		ofstream os = ofstream("Assets/CppAccetped.txt", ios::out | ios::binary | ios::ate);
		if (os.is_open())
		{
			os.write(str.data(), str.length());
		}
		os.close();
	}
	void AppendToFile(string str)
	{
		ofstream os = ofstream("Assets/CppAccetped.txt", ios::out | ios::binary | ios::app);
		if (os.is_open())
		{
			os.write(str.data(), str.length());
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
			WriteToFile(message);
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
		AppendToFile("ArrayStart：" + to_string((long long)arrayptr) + "\n");
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
			AppendToFile("Index：" + to_string(index) + "  " + to_string((long long)item) + "\n");
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


#pragma region SwapTest
	DllExport void __stdcall SwapItem(int start, int count)
	{
		if (::array)
		{
			AppendToFile("Start：" + to_string(start) + "\n");
			AppendToFile("Count：" + to_string(count) + "\n");

			auto length = count / 2;

			AppendToFile("Length：" + to_string(length) + "\n");

			for (size_t i = 0; i < length; i++)
			{
				auto& cur = ::array[start + i];
				auto& tal = ::array[start + count - 1 - i];

				auto tmp = cur;
				cur = tal;
				tal = tmp;
			}

		}
	}
#pragma endregion


#pragma region UpdateTest
	struct Vector3
	{
		float x;
		float y;
		float z;

		Vector3 operator+(const Vector3& other)const
		{
			return Vector3(this->x + other.x, this->y + other.y, this->z + other.z);
		}
	};

	typedef Vector3(*PositionGet)(void*);

	typedef void(*PositionSet)(void*, Vector3);

	PositionGet getter = NULL;
	PositionSet setter = NULL;

	int poolsize = 0;
	int count = 0;

	void** transforms = NULL;
	Vector3* positions = NULL;
	float* offsets = NULL;

	DllExport void __stdcall InitialPool(int count)
	{
		if (!transforms)
		{
			transforms = new void* [count];
		}
		if (!positions)
		{
			positions = new Vector3[count];
		}
		if (!offsets)
		{
			offsets = new float[count];
		}
		poolsize = count;

		AppendToFile("InitSize：" + to_string(count) + "\n");
	}

	DllExport void __stdcall InitialProperty(PositionGet getter, PositionSet setter)
	{
		::getter = getter;
		::setter = setter;

		AppendToFile("Getter：" + to_string((UINT)getter) + "\n");

		AppendToFile("Setter：" + to_string((UINT)setter) + "\n");
	}

	DllExport void __stdcall AddTransform(void* transform, Vector3 position)
	{
		if (::count < ::poolsize)
		{
			int index = ::count;
			transforms[index] = transform;

			AppendToFile("Address：" + to_string((unsigned long long)transform) + "\n");

			positions[index] = position;
			offsets[index] = sqrt(position.x * position.x + position.y * position.y + position.z * position.z);

			::count++;

			AppendToFile("Count：" + to_string(::count) + "\n");
		}
	}

	DllExport void __stdcall Update(float time)
	{
		for (size_t i = 0; i < ::count; i++)
		{
			auto transform = transforms[i];
			auto offset = Vector3(0, sin(offsets[i] + time), 0);
			setter(transform, positions[i] + offset);
		}
	}

	DllExport void __stdcall ReleasePool()
	{
		if (transforms)
		{
			delete[] transforms;
		}
		if (positions)
		{
			delete[] positions;
		}
		if (offsets)
		{
			delete[] offsets;
		}
		AppendToFile("ReleasePool\n");
	}
#pragma endregion
}