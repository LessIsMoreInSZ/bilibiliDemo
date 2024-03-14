#pragma once

#ifdef __cplusplus
#define EXTERNC extern "C"
#else
#define EXTERNC
#endif

#ifdef DLL_IMPORT
#define HEAD EXTERNC __declspec(dllimport)
#else
#define HEAD EXTERNC __declspec(dllexport)
#endif
#define CallingConvention _cdecl

HEAD void CallingConvention Test1();

HEAD void CallingConvention TestLogA(const char *log);

//HEAD void CallingConvention TestLog(const char *log);

HEAD void CallingConvention Test_BasicData(char d1, short d2, int d3, long long d4, float d5, double d6);

HEAD void CallingConvention Test_BasicDataRef(char &d1, short &d2, int &d3, long long &d4, float &d5, double &d6);

HEAD void CallingConvention Test_BasicDataPointer(char *d1, short *d2, int *d3, long long *d4, float *d5, double *d6);

HEAD float CallingConvention Test_Add(float num1, float num2);

HEAD void CallingConvention Test_BasicDataArr(int *arr1, float *arr2);

HEAD void* CallingConvention Test_BasicDataRet();

HEAD void CallingConvention Test_BasicDataString(char *str);

HEAD char* CallingConvention Test_BasicDataChar(char* str);


HEAD void CallingConvention Test_BasicDataByteArr(char *str);


struct ChildStruct
{
	int num;
	double pi;
};
struct StructA
{
	short id;
	ChildStruct cs;
	ChildStruct *pcs;
	int nums[5];
};
HEAD void CallingConvention Test_Struct(StructA param);

HEAD void* CallingConvention Test_StructRet();

HEAD void* CallingConvention ConvertChildStruct(ChildStruct *cs);

HEAD float CallingConvention Sum(int length, ...);

//int Log(int level, IntPtr ptr)
typedef int(*pfun)(int level, void* ptr);

//void SetLogFuncPointer(Log logptr);

HEAD void CallingConvention SetLogFuncPointer(pfun p);