using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.JSInterop;
using Sigged.CsC.CodeSamples.Parser;
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
        public string StatusCode = "";
        public bool IsRunning = false;
        public bool IsBuilding = false;
        public IEnumerable<DiagnosticViewModel> Diagnostics = new List<DiagnosticViewModel>();
        public IEnumerable<Exception> Exceptions = new List<Exception>();

        private string selectedCodeSampleId;
        public string SelectedCodeSampleId {
            get => selectedCodeSampleId;
            set
            {
                var codeSample = CodeSample.GetSamples().FirstOrDefault(s => s.Id == value);
                if(codeSample != null)
                {
                    JsInterop.InvokeAsync<string>("setCodeSample", codeSample.Contents);
                }
                selectedCodeSampleId = value;
            }
        }

        public IEnumerable<CodeSample> CodeSamples = new List<CodeSample>();

        protected override Task OnInitAsync()
        {
            CodeSamples = CodeSample.GetSamples();

            WasmCompiler.InitializeCompiler(Client);

            return base.OnInitAsync();
        }

        protected override Task OnAfterRenderAsync()
        {
            JsInterop.InvokeAsync<string>("appLoaded");
            return base.OnAfterRenderAsync();
        }


        public async Task Build()
        {
            string code = await JsInterop.InvokeAsync<string>("getSourceCode");
            WasmCompiler.WhenReady(() => { return StartInternal(code, false); });
        }

        public async Task BuildAndRun()
        {
            string code = await JsInterop.InvokeAsync<string>("getSourceCode");
            WasmCompiler.WhenReady(() => { return StartInternal(code, true); });
        }

        public void Stop()
        {
        }

        Task StartInternal(string code, bool runOnSuccess)
        {
            return Task.Run(() => {

                ConsoleOutput = "";
                Status = "Building...";
                StatusCode = "busy";
                IsBuilding = true;
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
                        emitResult = WasmCompiler.Compile(code, assemblyStream);
                        assemblyBytes = assemblyStream.ToArray();
                    }

                    sw.Stop();
                    Console.WriteLine($"Build time: {sw.ElapsedMilliseconds} ms");

                    Status = (emitResult?.Success == true) ? "Build succeeded" : "Build failed";
                    StatusCode = (emitResult?.Success == true) ? "success" : "error";
                    IsBuilding = false;
                    base.StateHasChanged();

                    Diagnostics = new List<DiagnosticViewModel>(emitResult.Diagnostics.Select(diag => new DiagnosticViewModel(diag)));

                    if (emitResult?.Success == true && runOnSuccess)
                    {
                        IsRunning = true;
                        StatusCode = "busy";
                        base.StateHasChanged();

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
                            StatusCode = "";
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

                            IsRunning = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Status = "Application crashed";
                    StatusCode = "error";
                    exception = ex;
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }

                if (exception != null)
                {
                    Exceptions = new List<Exception> { exception };
                }

                base.StateHasChanged();
            });
            
        }
    }
}
