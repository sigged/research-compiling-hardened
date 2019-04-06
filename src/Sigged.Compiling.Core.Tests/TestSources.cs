using System.Collections.Generic;

namespace Sigged.Compiling.Core.Tests
{
    internal class TestSources
    {
        public static IEnumerable<object[]> CompilingSources =>
            new List<object[]>
            {
                new object[]
                {
@"
using System;

namespace SimpleSource{
    public class SimpleClass
    {
        public static void SimpleMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}
"
                },
                new object[]
                {
@"
using System;

namespace SimpleSource{
    public class SimpleClass
    {
        public static void SimpleMethod()
        {
            int max = int.MaxValue;
            int inc = 1;
            int i = max + inc; //overflow
        }
    }
}
"
                }
            };


        public static IEnumerable<object[]> NonCompilingSources =>
            new List<object[]>
            {
                new object[]
                {
@"
namespace SimpleSource{
    public class SimpleClass
    {
        public static void SimpleMethod()
        {
            Console.WriteLine(); //bad call, lacking using System;
        }
    }
}
"
                },
new object[]
                {
@"
namespace SimpleSource{
    public class SimpleClass
    {
        public static void SimpleMethod()
        {
            int i = int.MaxValue + 1; //overflow
        }
    }
}
"
                }
            };

    }
}
