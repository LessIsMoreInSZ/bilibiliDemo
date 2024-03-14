#include "Native.h"
#include <iostream>
#include <windows.h>
HEAD void CallingConvention Test1()
{
	SetLastError(2);
	printf("call success\n");
}

HEAD void CallingConvention TestLogA(const char *log)
{
	printf("logA:%s\n", log);
}

HEAD void CallingConvention Test_BasicData(char d1, short d2, int d3, long long d4, float d5, double d6)
{

}

HEAD void CallingConvention Test_BasicDataRef(char &d1, short &d2, int &d3, long long &d4, float &d5, double &d6)
{
	d1 = 1;
	d2 = 2;
	d3 = 3;
	d4 = 4;
	d5 = 5.5f;
	d6 = 6.6;
}

HEAD void CallingConvention Test_BasicDataPointer(char *d1, short *d2, int *d3, long long *d4, float *d5, double *d6)
{
	*d1 = 10;
	*d2 = 20;
	*d3 = 30;
	*d4 = 40;
	*d5 = 15.5f;
	*d6 = 16.6;
}

HEAD float CallingConvention Test_Add(float num1, float num2)
{
	return num1 + num2;
}

HEAD void CallingConvention Test_BasicDataArr(int *arr1, float *arr2)
{
	arr1[0] = 11;
	arr1[1] = 22;
	arr1[2] = 33;

	arr2[0] = 22.2f;
	arr2[1] = 22.2f;
	arr2[2] = 22.2f;
	arr2[3] = 22.2f;
}

int intarr[5] = {1,2,3,4,5};
HEAD void* CallingConvention Test_BasicDataRet()
{
	return intarr;
}

HEAD void CallingConvention Test_BasicDataString(char *str)
{
	printf("%s\n", str);
}

HEAD char* CallingConvention Test_BasicDataChar(char* str)
{
	*str = '66';
	printf("%s\n", str);
	//printf("666");
	return str;
}

HEAD void CallingConvention Test_BasicDataByteArr(char *str)
{
	printf("%s\n", str);
}

HEAD void CallingConvention Test_Struct(StructA param)
{
	param.id = 123;
}

StructA structa;

struct FrameInfo
{
	char username[20];
	double pts;
};

struct Frame
{
	int width; //0-4
	int height;//4-8
	int format;//8-12
	int linesize[4]; //12-28
	unsigned char* data[4];//32-64
	FrameInfo *info; //64-72
};
Frame frame;
FrameInfo info;
HEAD void* CallingConvention Test_StructRet()
{
	frame.width = 1920;
	frame.height = 1080;
	frame.format = 0;
	for (int i = 0; i < 4; i++)
	{
		frame.linesize[i] = 100 * i;
		frame.data[i] = new unsigned char[10];
		for (int j = 0; j < 10; j++)
		{
			frame.data[i][j] = i;
		}
	}
	info.pts = 12.5;
	memset(info.username, 0, 20);
	memcpy(info.username, "hello world", strlen("hello world"));
	frame.info = &info;
	return &frame;
}

HEAD void* CallingConvention ConvertChildStruct(ChildStruct *cs)
{
	return cs;
}

HEAD float CallingConvention Sum(int length, ...)
{
	char* head = (char*)&length;
	int num1 = *(long long*)(head + 8);
	int num2 = *(long long*)(head + 16);
	int num3 = *(long long*)(head + 24);
	int num4 = *(long long*)(head + 32);
	double num5 = *(double*)(head + 40);

	return (num1 + num2 + num3 + num4 + num5) / length;
}

HEAD void CallingConvention SetLogFuncPointer(pfun p)
{
	int ret = p(0, NULL);
	printf("");
}

//HEAD void CallingConvention TestLog(const char *log)
//{
//	printf("log:%s\n", log);
//}
