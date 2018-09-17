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
        const int PCT_PRNT_LENGTH = 11;

        static int startPrintWidth = Console.BufferWidth - PRINT_LENGTH - PCT_PRNT_LENGTH;
        static void Main(string[] args)
        {
            //Test_ThrowException_Vs_HandlingCommonConditions(NUM_TEST_OPS / 200);
            //Console.WriteLine();

            //Test_ShortCircuitFileReading_LengthChecks(NUM_TEST_OPS * 1);
            //Console.WriteLine();

            //Test_ChunkyCalls_vs_ChattyCalls(NUM_TEST_OPS * 50);
            //Console.WriteLine();

            //Test_Object_Initialization_Methods(NUM_TEST_OPS * 50);
            //Console.WriteLine();

            //Test_Design_with_ValueTypes(NUM_TEST_OPS * 50);
            //Console.WriteLine();

            //Test_Add_vs_AddRange(NUM_TEST_OPS * 1);
            //Console.WriteLine();

            Test_Foreach_vs_For(NUM_TEST_OPS * 1);
            Console.WriteLine();

            Console.ReadLine();
        }

        private static void Test_ThrowException_Vs_HandlingCommonConditions(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();

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
            results.Add(new Tuple<string, TimeSpan>("Throwing Exception", watch.Elapsed));
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
            results.Add(new Tuple<string, TimeSpan>("Handling Common Case Inside Try/Catch", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int i = 0; i < numOps; i++)
            {
                var o = MethodReturnsNull();
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>("Handling Common Case", watch.Elapsed));
            watch.Reset();

            TimeSpan baselineTime = results.Select(x => x.Item2).Min();
            results = results.OrderByDescending(x => x.Item2).ToList();
            foreach (var result in results)
            {
                WriteTimingString(result.Item1, numOps, result.Item2, baselineTime);
            }
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
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();

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
                results.Add(new Tuple<string, TimeSpan>("Reading empty filestream", watch.Elapsed));
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
                results.Add(new Tuple<string, TimeSpan>("Checking length before", watch.Elapsed));
                watch.Reset();

                TimeSpan baselineTime = results.Select(x => x.Item2).Max();
                results = results.OrderByDescending(x => x.Item2).ToList();
                foreach (var result in results)
                {
                    WriteTimingString(result.Item1, numOps, result.Item2, baselineTime);
                }
            }
        }

        private static void Test_ChunkyCalls_vs_ChattyCalls(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();
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
            results.Add(new Tuple<string, TimeSpan>("Chatty Calls", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                TestClass o = new TestClass();
                o.SetValues(i, f, s);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>("Chunky Calls", watch.Elapsed));
            watch.Reset();

            TimeSpan baselineTime = results.Select(x => x.Item2).Max();
            results = results.OrderByDescending(x => x.Item2).ToList();
            foreach (var result in results)
            {
                WriteTimingString(result.Item1, numOps, result.Item2, baselineTime);
            }
        }

        private static void Test_Object_Initialization_Methods(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();
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
            results.Add(new Tuple<string, TimeSpan>("Set autoproperties with ctor", watch.Elapsed));
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
            results.Add(new Tuple<string, TimeSpan>("Set autoproperties after ctor", watch.Elapsed));
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
            results.Add(new Tuple<string, TimeSpan>("Set public properties with ctor", watch.Elapsed));
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
            results.Add(new Tuple<string, TimeSpan>("Set public properties after ctor", watch.Elapsed));
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
            results.Add(new Tuple<string, TimeSpan>("Set properties with setter methods", watch.Elapsed));
            watch.Reset();

            // Set properties in constructor
            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var o = new TestClass(i, f, s);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>("Set properties as constructor args", watch.Elapsed));
            watch.Reset();

            TimeSpan baselineTime = results.Select(x => x.Item2).Max();
            results = results.OrderByDescending(x => x.Item2).ToList();
            foreach (var result in results)
            {
                WriteTimingString(result.Item1, numOps, result.Item2, baselineTime);
            }
        }

        private static void Test_Design_with_ValueTypes(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();
            double d = 3.14;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var test = new SimpleTestClass(d);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>("Using classes", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                var test2 = new SimpleTestStruct(d);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>("Using structs", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
        }

        private static void Test_Add_vs_AddRange(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();
            Stopwatch watch = new Stopwatch();

            int tinyListSize = 10;
            int smallListSize = 100;
            int mediumListSize = 1000;
            int largeListSize = 10000;
            int hugeListSize = 10000000;

            List<int> tinyList = new List<int>();
            List<int> smallList = new List<int>();
            List<int> mediumList = new List<int>();
            List<int> largeList = new List<int>();
            List<int> hugeList = new List<int>();
            for (int i = 0; i < hugeListSize; i++)
            {
                if (i < tinyListSize)
                {
                    tinyList.Add(i);
                }
                if (i < smallListSize)
                {
                    smallList.Add(i);
                }
                if (i < mediumListSize)
                {
                    mediumList.Add(i);
                }
                if (i < largeListSize)
                {
                    largeList.Add(i);
                }
                hugeList.Add(i);
            }

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                for (int i = 0; i < tinyListSize; i++)
                {
                    l.Add(i);
                }
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Iterative add, {tinyListSize} list", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                l.AddRange(tinyList);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Add range, {tinyListSize} list", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
            results.Clear();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                for (int i = 0; i < smallListSize; i++)
                {
                    l.Add(i);
                }
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Iterative add, {smallListSize} list", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                l.AddRange(smallList);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Add range, {smallListSize} list", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
            results.Clear();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                for (int i = 0; i < mediumListSize; i++)
                {
                    l.Add(i);
                }
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Iterative add, {mediumListSize} list", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                l.AddRange(mediumList);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Add range, {mediumListSize} list", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
            results.Clear();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                for (int i = 0; i < largeListSize; i++)
                {
                    l.Add(i);
                }
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Iterative add, {largeListSize} list", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                l.AddRange(largeList);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Add range, {largeListSize} list", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
            results.Clear();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                for (int i = 0; i < hugeListSize; i++)
                {
                    l.Add(i);
                }
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Iterative add, {hugeListSize} list", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
            {
                List<int> l = new List<int>();
                l.AddRange(hugeList);
            }
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Add range, {hugeListSize} list", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
            results.Clear();
        }

        private static void Test_Foreach_vs_For(int numOps)
        {
            IList<Tuple<string, TimeSpan>> results = new List<Tuple<string, TimeSpan>>();
            Stopwatch watch = new Stopwatch();
            string s = "monkeys!";
            int dummy = 0;
            // make the string long
            System.Text.StringBuilder sb = new System.Text.StringBuilder(s);
            for (int i = 0; i < 1000000; i++)
                sb.Append(s);
            s = sb.ToString();
            ////

            watch.Start();
            for (int x = 0; x < numOps; x++)
                foreach (char c in s) dummy++;
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"Foreach", watch.Elapsed));
            watch.Reset();

            watch.Start();
            for (int x = 0; x < numOps; x++)
                for (int i = 0; i < s.Length; i++)
                    dummy++;
            watch.Stop();
            results.Add(new Tuple<string, TimeSpan>($"For", watch.Elapsed));
            watch.Reset();

            ShowResults(numOps, results);
        }

        private static void ShowResults(int numOps, IList<Tuple<string, TimeSpan>> results)
        {
            TimeSpan baselineTime = results.Select(x => x.Item2).Max();
            results = results.OrderByDescending(x => x.Item2).ToList();
            foreach (var result in results)
            {
                WriteTimingString(result.Item1, numOps, result.Item2, baselineTime);
            }
        }

        private static void WriteTimingString(string prefix, int numOps, TimeSpan time, TimeSpan? baselineTime = null)
        {
            Console.Write($"{prefix} over {numOps} ops:");
            Console.SetCursorPosition(startPrintWidth, Console.CursorTop);
            Console.Write($" {time.ToString().Substring(3)}");

            if (baselineTime.HasValue)
            {
                double percent = (double)time.Ticks / (double)baselineTime.Value.Ticks;
                string pctString = $" {Math.Round(percent, 2):0.00}x";

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
