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

        public static string Get_Main_With_StringArrayParms_Code()
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

        public static string Get_Main_With_ObjectParms_Code()
        {
            return @"
using System;
namespace Test {
    public class Program {
        public static void Main(object[] args) 
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

        public static string Get_NoMainMethod_Code()
        {
            return @"
using System;
namespace Test {
    public class Program {
        public static void BadMain(string[] args) { }
    }
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
