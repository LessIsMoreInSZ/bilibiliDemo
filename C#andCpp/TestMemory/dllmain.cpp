// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include <cstdlib>   
#include <windows.h>
#define _CRTDBG_MAP_ALLOC
#include <crtdbg.h>

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

// 导出内存分配函数（无释放接口）
extern "C" __declspec(dllexport) void* AllocateMemory(int size) {
    return malloc(size * 1024); // 分配1KB块（实际按需调整）
}

// 导出释放函数（C#中不会调用）
extern "C" __declspec(dllexport) void FreeMemory(void* ptr) {
    free(ptr);
}

