using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CallNativeDllCSharp
{
    class Program
    {
        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Test1();

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "TestLog",ExactSpelling = false)]
        public static extern void TestLogNative(string log);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_BasicData(char d1, short d2, int d3, long d4, float d5, double d6);


        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_BasicDataRef(ref sbyte d1, ref short d2, ref int d3, ref long d4, ref float d5, ref double d6);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_BasicDataPointer(ref sbyte d1, ref short d2, ref int d3, ref long d4, ref float d5, ref double d6);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float Test_Add(float num1, float num2);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float Test_BasicDataArr(int[] arr1, float[] arr2);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Test_BasicDataRet();

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_BasicDataString(string str);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern string Test_BasicDataChar(string str);
        public static extern IntPtr Test_BasicDataChar(string str);


        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_BasicDataByteArr(byte[] str);

        [StructLayout(LayoutKind.Sequential)]
        public struct ChildStruct
        {
            public int num;
            public double pi;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct StructA
        {
            public short id;
            public ChildStruct cs;
            public IntPtr pcs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =5)]
            public int[] nums;
        };

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test_Struct(ref StructA param);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Test_StructRet();

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ConvertChildStruct(ref ChildStruct cs);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float Sum(int length, __arglist);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Log(int level, IntPtr ptr);

        [DllImport("NativeDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogFuncPointer(Log logptr);

        public static Log logfuncptr = LogCallback;

        static int LogCallback(int level, IntPtr ptr)
        {

            return 0;
        }
        static void Main(string[] args)
        {

            //SetLogFuncPointer(logfuncptr);
            //Test_BasicDataString("Test");
            IntPtr intPtr = Test_BasicDataChar("testchar");
            string stringB = Marshal.PtrToStringAnsi(intPtr);
            float s = Test_Add(5, 6);
            Console.Read();
        }
    }
}
