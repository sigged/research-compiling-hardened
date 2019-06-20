using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.JSInterop;
using Sigged.CsC.Mono.Blazor.Models;

namespace Sigged.CsC.Mono.Blazor.Pages
{
    public class IndexModel : ComponentBase
    {
        [Inject]
        private HttpClient Client { get; set; }

        [Inject]
        private IJSRuntime JsInterop { get; set; }

        public string ConsoleOutput = "";
        public string Status = "Ready";
        public IEnumerable<DiagnosticViewModel> Diagnostics = new List<DiagnosticViewModel>();

        public string SourceCode = @"using System;

class Program
{
    public static void Main()
    {
        Console.WriteLine(""Hello World"");
    }
}";

        protected override Task OnInitAsync()
        {
            WasmCompiler.InitializeCompiler(Client);

            JsInterop.InvokeAsync<string>("appLoaded");

            return base.OnInitAsync();
        }

        public void Build()
        {
            WasmCompiler.WhenReady(async () => { await StartInternal(false); });
        }

        public void BuildAndRun()
        {
            WasmCompiler.WhenReady(async () => { await StartInternal(true); });
        }

        async Task StartInternal(bool runOnSuccess)
        {
            await Task.Run(() => {

                ConsoleOutput = "";
                Status = "Building...";
                base.StateHasChanged();

                var sw = Stopwatch.StartNew();

                var originalOutput = Console.Out;
                var consoleOutputWriter = new StringWriter();

                Exception exception = null;

                EmitResult emitResult = null;
                byte[] assemblyBytes = null;

                try
                {

                    using (var assemblyStream = new MemoryStream())
                    {
                        emitResult = WasmCompiler.Compile(SourceCode, assemblyStream);
                        assemblyBytes = assemblyStream.ToArray();
                    }

                    sw.Stop();
                    Console.WriteLine($"Build time: {sw.ElapsedMilliseconds} ms");

                    Status = (emitResult?.Success == true) ? "Build succeeded" : "Build failed";
                    base.StateHasChanged();

                    Diagnostics = new List<DiagnosticViewModel>(emitResult.Diagnostics.Select(diag => new DiagnosticViewModel(diag)));

                    if (emitResult?.Success == true && runOnSuccess)
                    {
                        Console.WriteLine("Running...");
                        Console.SetOut(consoleOutputWriter);

                        try
                        {
                            //load assembly 
                            var assembly = Assembly.Load(assemblyBytes);

                            //invoke main method
                            var mainParms = assembly.EntryPoint.GetParameters();
                            if (mainParms.Count() == 0)
                            {
                                assembly.EntryPoint.Invoke(null, null);
                            }
                            else
                            {
                                if (mainParms[0].ParameterType == typeof(string[]))
                                    assembly.EntryPoint.Invoke(null, new string[] { null });
                                else
                                    assembly.EntryPoint.Invoke(null, null);
                            }

                            //output console writes
                            ConsoleOutput = consoleOutputWriter.ToString();
                            base.StateHasChanged();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            Console.SetOut(originalOutput);
                            Console.WriteLine("Run complete");
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }

                if (exception != null)
                {
                    ConsoleOutput += "\r\n" + exception.ToString();
                }

                base.StateHasChanged();
            });
            
        }
    }
}
