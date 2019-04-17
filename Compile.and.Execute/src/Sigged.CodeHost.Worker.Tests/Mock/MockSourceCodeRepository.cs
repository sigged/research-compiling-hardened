using System.Collections.Generic;

namespace Sigged.CodeHost.Worker.Tests.Mock
{
    internal class MockSourceCodeRepository
    {
        public static string Get_Working_SimpleOutput_Code()
        {
            return @"
using System;
namespace Test {
    public class Program {
        public static void Main(string[] args) 
        {
            Console.WriteLine(""All your base are belong to us."");
        }
    }
}
";
        }

        public static IEnumerable<object[]> Get_Varying_MainParms_Codes =>
            new List<object[]>
            {
                new object[] { Get_Main_With_NoParms_Code() },
                new object[] { Get_Main_With_StringArrayParms_Code() },
            };
        

        public static string Get_Main_With_StringArrayParms_Code()
        {
            return @"
//Main with string params 
using System;
namespace Test {
    public class Program {
        public static void Main(string[] args) 
        {
            Console.WriteLine(""All your base are belong to us."");
        }
    }
}
";
        }

        public static string Get_Main_With_NoParms_Code()
        {
            return @"
//Main with NO params 
using System;
namespace Test {
    public class Program {
        public static void Main() 
        {
            Console.WriteLine(""All your base are belong to us."");
        }
    }
}
";
        }

        public static IEnumerable<object[]> Get_Bad_MainMethod_Codes =>
            new List<object[]>
            {
                //CS5001 = Program does not contain a static 'Main' method suitable for an entry point
                //CS0028 = 'Main' has the wrong signature to be an entry point
                new object[] { Get_Bad_MainMethod_None_In_Code(), "CS5001" },
                new object[] { Get_Bad_MainMethod_ObjectsParam_Code(), "CS0028" },
                new object[] { Get_Bad_MainMethod_FloatParam_Code(), "CS0028" },
            };

        public static string Get_Bad_MainMethod_None_In_Code()
        {
            return @"
//NO main method
namespace Test { public class Program { } }
";
        }


        public static string Get_Bad_MainMethod_ObjectsParam_Code()
        {
            return @"
//bad args main method (object[])
namespace Test {
    public class Program { public static void Main(object[] args) { } }
}
";
        }

        public static string Get_Bad_MainMethod_FloatParam_Code()
        {
            return @"
//bad args main method (float)
namespace Test {
    public class Program { public static void Main(float args) { } }
}
";
        }

        public static string Get_AmbiguousMain_Code()
        {
            return @"
using System;
namespace Test {
    public class ProgramA {
        public static void Main(string[] args) { }
    }
    public class ProgramB {
        public static void Main(string[] args) { }
    }
}
";
        }


    }
}
