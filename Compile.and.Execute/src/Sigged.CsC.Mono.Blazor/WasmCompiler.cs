using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Sigged.CsC.Mono.Blazor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sigged.CsC.Mono.Blazor
{
    public static class WasmCompiler
    {
        private const string blazorBootLocation = "_framework/blazor.boot.json";
        private const string clrBinLocation = "_framework/_bin/";
        private static Task initTask;
        private static IEnumerable<MetadataReference> MetaDataReferences;

        public static void InitializeCompiler(HttpClient client)
        {
            initTask = Init(client);
        }

        private static async Task Init(HttpClient client)
        {
            var response = await client.GetJsonAsync<BlazorBootModel>(blazorBootLocation);
            var assemblyDownloadTasks = response.assemblyReferences.Where(
                    assemblyName => assemblyName.EndsWith(".dll"))
                        .Select(assemblyName => client.GetAsync(clrBinLocation + assemblyName));
            var assemblies = await Task.WhenAll(assemblyDownloadTasks);

            var references = new List<MetadataReference>(assemblies.Length);
            foreach (var assembly in assemblies)
            {
                using (var readTask = await assembly.Content.ReadAsStreamAsync())
                {
                    references.Add(MetadataReference.CreateFromStream(readTask));
                }
            }

            MetaDataReferences = references;
        }

        public static void WhenReady(Func<Task> callback)
        {
            if (initTask.Status != TaskStatus.RanToCompletion)
            {
                initTask.ContinueWith(x => callback());
            }
            else
            {
                callback();
            }
        }

        public static SyntaxTree Parse(string text, CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text ?? "", Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options);
        }

        public static EmitResult Compile(string sourceCode, Stream outputStream)
        {
            //create c# parsing options
            var parserOptions = CSharpParseOptions.Default
                .WithLanguageVersion(LanguageVersion.Default);

            //create compiler options
            var compilerOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOverflowChecks(true)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithPlatform(Platform.AnyCpu);

            //parse source code
            var parsedSyntaxTree = Parse(sourceCode, options: parserOptions);

            //create assembly
            var compilation = CSharpCompilation
                .Create("UserAssembly", new SyntaxTree[] { parsedSyntaxTree }, MetaDataReferences, compilerOptions);

            return compilation.Emit(outputStream);
        }

    }
}
