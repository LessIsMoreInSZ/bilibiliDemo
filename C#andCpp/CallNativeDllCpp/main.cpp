#include <stdio.h>
#include <stdarg.h>
#define DLL_IMPORT
#include "../NativeDll/Native.h"
#pragma comment(lib, "../../bin/NativeDll.lib")

void test(int length, ...)
{
	va_list ap;
	va_start(ap, length);


	int num1 = va_arg(ap, int);
	int num2 = va_arg(ap, int);
	double num3 = va_arg(ap, double);
	va_end(ap);
}
int main(int argc, char* argv[])
{
	test(3, 123, 456, 12.3);
	return 0;
}