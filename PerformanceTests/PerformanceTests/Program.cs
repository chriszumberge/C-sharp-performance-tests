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

        static void Main(string[] args)
        {
            Test_ThrowException_Vs_HandlingCommonConditions(NUM_TEST_OPS / 20);
            Console.WriteLine();

            Test_ShortCircuitFileReading_LengthChecks(NUM_TEST_OPS);
            Console.WriteLine();

            Test_ChunkyCalls_vs_ChattyCalls(NUM_TEST_OPS);
            Console.WriteLine();

            Test_Object_Initialization_Methods(NUM_TEST_OPS);
            Console.WriteLine();

            Console.ReadLine();
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
                Console.WriteLine($"Reading empty filestream {numOps}: {watch.Elapsed.ToString()}ms");
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
                Console.WriteLine($"Checking length before reading empty filestream {numOps}: {watch.Elapsed.ToString()}ms");
            }
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
            Console.WriteLine($"Throwing Exception over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();

            watch.Start();
            for (int i = 0; i < numOps; i++)
            {
                var o = MethodReturnsNull();
            }
            watch.Stop();
            Console.WriteLine($"Handling Common Case over {numOps}: {watch.Elapsed.ToString()}ms");
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
            Console.WriteLine($"Handling Common Case Inside Try/Catch over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();
        }

        private static object MethodThrowsException()
        {
            throw new Exception();
        }

        private static object MethodReturnsNull()
        {
            return null;
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
            Console.WriteLine($"Chatty Calls over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                TestClass o = new TestClass();
                o.SetValues(i, f, s);
            }
            watch.Stop();
            Console.WriteLine($"Chunky Calls over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();
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
            Console.WriteLine($"Set autoproperties with constructor over {numOps}: {watch.Elapsed.ToString()}ms");
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
            Console.WriteLine($"Set autoproperties after constructor over {numOps}: {watch.Elapsed.ToString()}ms");
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
            Console.WriteLine($"Set public properties with constructor over {numOps}: {watch.Elapsed.ToString()}ms");
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
            Console.WriteLine($"Set public properties after constructor over {numOps}: {watch.Elapsed.ToString()}ms");
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
            Console.WriteLine($"Set properties with setter methods over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();

            // Set properties in constructor
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass(i, f, s);
            }
            watch.Stop();
            Console.WriteLine($"Set properties in constructor over {numOps}: {watch.Elapsed.ToString()}ms");
            watch.Reset();
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
}
