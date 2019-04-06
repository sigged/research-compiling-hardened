using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sigged.Compiling.Core
{
    public class Compiler
    {
        public Compiler(string metaReferencesPath)
        {
            MetaReferencesPath = metaReferencesPath;
        }

        public string MetaReferencesPath { get; set; }

        protected IEnumerable<MetadataReference> GetMetadataReferences()
        {
            DirectoryInfo di = new DirectoryInfo(MetaReferencesPath);
            foreach (FileInfo fi in di.GetFiles("*.dll"))
            {
                yield return MetadataReference.CreateFromFile(Path.Combine(fi.FullName));
            }
        }

        public SyntaxTree Parse(string text, CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text ?? "", Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options);
        }

        public EmitResult Compile(
            string source,
            string assemblyName,
            Stream outputStream,
            Stream outputPdbStream = null,
            LanguageVersion languageVersion = LanguageVersion.Default,
            ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default,
            OptimizationLevel optimizationLevel = OptimizationLevel.Release,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
            Platform cpuPlatform = Platform.AnyCpu
        )
        {
            //create c# parsing options
            var parserOptions = CSharpParseOptions.Default
                .WithLanguageVersion(languageVersion);

            //create compiler options
            var compilerOptions = new CSharpCompilationOptions(outputKind)
                .WithOverflowChecks(true)
                .WithOptimizationLevel(optimizationLevel)
                .WithPlatform(cpuPlatform);

            //parse source code
            var parsedSyntaxTree = Parse(source, options: parserOptions);

            //create assembly
            var metaDataRefs = GetMetadataReferences().ToList();
            var compilation = CSharpCompilation
                .Create(assemblyName, new SyntaxTree[] { parsedSyntaxTree }, metaDataRefs, compilerOptions);

            //compile and return results
            return (outputPdbStream != null) ?
                compilation.Emit(outputStream, outputPdbStream) :
                compilation.Emit(outputStream);
        }

        public EmitResult Compile(
            string source,
            string outputFilepath,
            string outputPdbFilePath = null,
            LanguageVersion languageVersion = LanguageVersion.Default,
            ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default,
            OptimizationLevel optimizationLevel = OptimizationLevel.Release,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
            Platform cpuPlatform = Platform.AnyCpu
        )
        {
            string assemblyName = Path.GetFileNameWithoutExtension(outputFilepath);
            Stream fileStream = null;
            Stream pdbFileStream = null;
            using (fileStream = new FileStream(outputFilepath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (outputPdbFilePath != null)
                {
                    using (pdbFileStream = new FileStream(outputPdbFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        return Compile(source, assemblyName, fileStream, pdbFileStream, languageVersion, generalDiagnosticOption, optimizationLevel, outputKind, cpuPlatform);
                    }
                }
                return Compile(source, assemblyName, fileStream, null, languageVersion, generalDiagnosticOption, optimizationLevel, outputKind, cpuPlatform);
            }
        }
    }
}
