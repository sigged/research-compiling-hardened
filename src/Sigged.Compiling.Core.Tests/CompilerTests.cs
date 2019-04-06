using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Sigged.Compiling.Core.Tests
{
    public class CompilerTests
    {
        private readonly Compiler compiler;

        public CompilerTests()
        {
            string netstandardRefsDirectory = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.Parent.FullName, "libs", "netstandard2.0");
            compiler = new Compiler(netstandardRefsDirectory);
        }

        [Theory]
        [MemberData(nameof(TestSources.CompilingSources), MemberType = typeof(TestSources))]
        public void Compiles_To_Dll(string source)
        {
            string dllPath = "";
            try
            {
                //arrange
                dllPath = Path.GetTempFileName().Replace(".tmp", ".dll");

                //act
                var result = compiler.Compile(source, dllPath);

                //assert
                Assert.True(result.Success);

            }
            finally
            {
                //clean up
                if (File.Exists(dllPath))
                    File.Delete(dllPath);
            }
            
        }


        [Theory]
        [MemberData(nameof(TestSources.CompilingSources), MemberType = typeof(TestSources))]
        public void Compiles_To_Stream(string source)
        {
            //arrange
            using (Stream stream = new MemoryStream())
            {

                //act
                var result = compiler.Compile(source, "testAssemblyName", stream);

                //assert
                Assert.True(result.Success);
                Assert.NotEqual(0L, stream.Length);
            }
        }


        [Theory]
        [MemberData(nameof(TestSources.NonCompilingSources), MemberType = typeof(TestSources))]
        public void Compile_Fails_On_Badcode(string source)
        {
            string dllPath = "";
            try
            {
                //arrange
                dllPath = Path.GetTempFileName().Replace(".tmp", ".dll");

                //act
                var result = compiler.Compile(source, dllPath);

                //assert
                Assert.False(result.Success);

            }
            finally
            {
                //clean up
                if (File.Exists(dllPath))
                    File.Delete(dllPath);
            }
        }
    }
    
}
