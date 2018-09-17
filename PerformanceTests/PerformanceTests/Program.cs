using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTests
{
    class Program
    {
        const int NUM_TEST_OPS = 1000000;
        const int PRINT_LENGTH = 13;
        const int PCT_PRNT_LENGTH = 13;

        static int startPrintWidth = Console.BufferWidth - PRINT_LENGTH - PCT_PRNT_LENGTH;
        static void Main(string[] args)
        {
            Test_ThrowException_Vs_HandlingCommonConditions(NUM_TEST_OPS / 20);
            Console.WriteLine();

            Test_ShortCircuitFileReading_LengthChecks(NUM_TEST_OPS * 50);
            Console.WriteLine();

            Test_ChunkyCalls_vs_ChattyCalls(NUM_TEST_OPS * 50);
            Console.WriteLine();

            Test_Object_Initialization_Methods(NUM_TEST_OPS * 50);
            Console.WriteLine();

            Test_Design_with_ValueTypes(NUM_TEST_OPS * 50);
            Console.WriteLine();

            Console.ReadLine();
        }

        private static void Test_ThrowException_Vs_HandlingCommonConditions(int numOps)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numOps; i++)
            {
                try
                {
                    var o = MethodThrowsException();
                }
                catch (Exception ex) { }
            }
            watch.Stop();
            TimeSpan exceptionTime = watch.Elapsed;
            watch.Reset();

            watch.Start();
            for (int i = 0; i < numOps; i++)
            {
                try
                {
                    var o = MethodReturnsNull();
                }
                catch (Exception ex) { }
            }
            watch.Stop();
            TimeSpan handleInsideTryCatchTime = watch.Elapsed;
            watch.Reset();

            watch.Start();
            for (int i = 0; i < numOps; i++)
            {
                var o = MethodReturnsNull();
            }
            watch.Stop();
            TimeSpan commonCaseTime = watch.Elapsed;
            watch.Reset();

            TimeSpan baselineTime = new List<TimeSpan> { exceptionTime, commonCaseTime, handleInsideTryCatchTime }.Min();

            WriteTimingString("Throwing Exception", numOps, exceptionTime, baselineTime);
            WriteTimingString("Handling Common Case", numOps, commonCaseTime, baselineTime);
            WriteTimingString("Handling Common Case Inside Try/Catch", numOps, handleInsideTryCatchTime, baselineTime);
        }

        private static object MethodThrowsException()
        {
            throw new Exception();
        }

        private static object MethodReturnsNull()
        {
            return null;
        }

        private static void Test_ShortCircuitFileReading_LengthChecks(int numOps)
        {
            string filePath = Path.Combine(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.ToString(), "EmptyFile.txt");

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                int b;

                Stopwatch watch = new Stopwatch();
                watch.Start();
                for (int i = 0; i < numOps; i++)
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    b = fileStream.ReadByte();
                }
                watch.Stop();
                TimeSpan readingEmptyTime = watch.Elapsed;
                watch.Reset();

                watch.Start();
                for (int i = 0; i < numOps; i++)
                {
                    for (int x = 0; x < fileStream.Length; x++)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        b = fileStream.ReadByte();
                    }
                }
                watch.Stop();
                TimeSpan checkLengthTime = watch.Elapsed;

                TimeSpan baselineTime = new List<TimeSpan> { readingEmptyTime, checkLengthTime }.Max();

                WriteTimingString("Reading empty filestream", numOps, readingEmptyTime, baselineTime);
                WriteTimingString("Checking length before", numOps, checkLengthTime, baselineTime);
            }
        }

        private static void Test_ChunkyCalls_vs_ChattyCalls(int numOps)
        {
            int i = Helpers.GetInt();
            float f = Helpers.GetFloat();
            string s = Helpers.GetString();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                TestClass o = new TestClass();
                o.SetTestInt(i);
                o.SetTestFloat(f);
                o.SetTestString(s);
            }
            watch.Stop();
            TimeSpan chattyTime = watch.Elapsed;
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                TestClass o = new TestClass();
                o.SetValues(i, f, s);
            }
            watch.Stop();
            TimeSpan chunkyTime = watch.Elapsed;
            watch.Reset();

            TimeSpan baselineTime = new List<TimeSpan> { chattyTime, chunkyTime }.Max();

            WriteTimingString("Chatty Calls", numOps, chattyTime, baselineTime);
            WriteTimingString("Chunky Calls", numOps, chunkyTime, baselineTime);
        }

        private static void Test_Object_Initialization_Methods(int numOps)
        {
            int i = Helpers.GetInt();
            float f = Helpers.GetFloat();
            string s = Helpers.GetString();

            Stopwatch watch = new Stopwatch();

            // Set auto properties in constructor
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass()
                {
                    AutoTestInt = i,
                    AutoTestFloat = f,
                    AutoTestString = s
                };
            }
            watch.Stop();
            TimeSpan autoPropertiesInCtorTime = watch.Elapsed;
            watch.Reset();

            // Set auto properties after construction
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass();
                o.AutoTestInt = i;
                o.AutoTestFloat = f;
                o.AutoTestString = s;
            }
            watch.Stop();
            TimeSpan autoPropertiesAfterCtorTime = watch.Elapsed;            
            watch.Reset();

            // Set public properties in constructor
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass()
                {
                    TestInt = i,
                    TestFloat = f,
                    TestString = s
                };
            }
            watch.Stop();
            TimeSpan setPropertiesInCtorTime = watch.Elapsed;
            watch.Reset();

            // Set public properties after construction
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass();
                o.TestInt = i;
                o.TestFloat = f;
                o.TestString = s;
            }
            watch.Stop();
            TimeSpan setPropertiesAfterCtorTime = watch.Elapsed;
            watch.Reset();

            // Set properties using setter methods
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass();
                o.SetTestInt(i);
                o.SetTestFloat(f);
                o.SetTestString(s);
            }
            watch.Stop();
            TimeSpan setterMethodsTime = watch.Elapsed;
            watch.Reset();

            // Set properties in constructor
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass(i, f, s);
            }
            watch.Stop();
            TimeSpan ctorArgTime = watch.Elapsed;
            watch.Reset();

            TimeSpan baselineTime = new List<TimeSpan> { autoPropertiesInCtorTime, autoPropertiesAfterCtorTime, setPropertiesInCtorTime,
                                                         setPropertiesAfterCtorTime, setterMethodsTime, ctorArgTime}.Max();

            WriteTimingString("Set autoproperties with ctor", numOps, autoPropertiesInCtorTime, baselineTime);
            WriteTimingString("Set autoproperties after ctor", numOps, autoPropertiesAfterCtorTime, baselineTime);
            WriteTimingString("Set public properties with ctor", numOps, setPropertiesInCtorTime, baselineTime);
            WriteTimingString("Set public properties after ctor", numOps, setPropertiesAfterCtorTime, baselineTime);
            WriteTimingString("Set properties with setter methods", numOps, setterMethodsTime, baselineTime);
            WriteTimingString("Set properties as constructor args", numOps, ctorArgTime, baselineTime);
        }

        private static void Test_Design_with_ValueTypes(int numOps)
        {
            double d = 3.14;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            for (int x= 0; x < numOps; x++)
            {
                var test = new SimpleTestClass(d);
            }
            watch.Stop();
            TimeSpan classTime = watch.Elapsed;
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var test2 = new SimpleTestStruct(d);
            }
            watch.Stop();
            TimeSpan structTime = watch.Elapsed;
            watch.Reset();

            TimeSpan baselineTime = new List<TimeSpan> { classTime, structTime }.Max();

            WriteTimingString("Using classes", numOps, classTime, baselineTime);
            WriteTimingString("Using structs", numOps, structTime, baselineTime);
        }

        private static void WriteTimingString(string prefix, int numOps, TimeSpan time, TimeSpan? baselineTime = null)
        {
            Console.Write($"{prefix} over {numOps} ops:");
            Console.SetCursorPosition(startPrintWidth, Console.CursorTop);
            Console.Write($" {time.ToString().Substring(3)}");

            if (baselineTime.HasValue)
            {
                double percent = (double)time.Ticks / (double)baselineTime.Value.Ticks;
                string pctString = $" {Math.Round(percent, 4):0.0000}x";

                int x = 0;
                for (int i = Console.BufferWidth; i > Console.BufferWidth - PCT_PRNT_LENGTH; i--)
                {
                    Console.SetCursorPosition(i - 2, Console.CursorTop);
                    if (x < pctString.Length)
                    {
                        Console.Write(pctString[pctString.Length - 1 - x]);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    x++;
                }
                //if (percent < 10)
                //{
                //    Console.Write($" {Math.Round(percent, 4):0.0000}x");
                //}
                //else if (percent < 100)
                //{
                //    Console.Write($" {Math.Round(percent, 4):0.000}x");
                //}
                //else if (percent < 1000)
                //{
                //    Console.Write($" {Math.Round(percent, 4):0.00}x");
                //}
                //else
                //{
                //    Console.Write($" {percent.ToEngineering()}x");
                //}
            }

            Console.WriteLine();
        }
    }

    public static class Helpers
    {
        public static int GetInt() => 5;
        public static float GetFloat() => 2.5f;
        public static string GetString() => "Test String";
    }

    public class TestClass
    {
        private int testInt;
        public int TestInt
        {
            get { return testInt; }
            set { testInt = value; }
        }
        public int AutoTestInt { get; set; }

        private float testFloat;
        public float TestFloat
        {
            get { return testFloat; }
            set { testFloat = value; }
        }
        public float AutoTestFloat { get; set; }

        private string testString;
        public string TestString
        {
            get { return testString; }
            set { testString = value; }
        }
        public string AutoTestString { get; set; }

        public TestClass() { }
        public TestClass(int testInt, float testFloat, string testString)
        {

        }

        public void SetTestInt(int i) { testInt = i; }
        public void SetTestFloat(float f) { testFloat = f; }
        public void SetTestString(string s) { testString = s; }

        public void SetValues(int i, float f, string s)
        {
            testInt = i;
            testFloat = f;
            testString = s;
        }
    }

    public class SimpleTestClass
    {
        public double y;
        public SimpleTestClass(double arg)
        {
            this.y = arg;
        }
    }

    public struct SimpleTestStruct
    {
        public double y;
        public SimpleTestStruct(double arg)
        {
            this.y = arg;
        }
    }

    public static class FormatExtensions
    {
        public static string ToEngineering(this double value)
        {
            int exp = (int)(Math.Floor(Math.Log10(value) / 3.0) * 3.0);
            double newValue = value * Math.Pow(10.0, -exp);
            if (newValue >= 1000.0)
            {
                newValue = newValue / 1000.0;
                exp = exp + 3;
            }
            return string.Format("{0:##0}e{1}", newValue, exp);
        }
    }
}
