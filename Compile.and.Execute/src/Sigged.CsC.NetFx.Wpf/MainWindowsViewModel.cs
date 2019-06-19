using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;
using Sigged.CsC.CodeSamples.Parser;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Sigged.CsC.NetFx.Wpf
{
    public class MainWindowsViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Compiler compiler;
        private Thread runThread;
        private InputAggregator inputAggregator;

        public MainWindowsViewModel(InputAggregator inputaggregator)
        {
            inputAggregator = inputaggregator;

            string netstandardRefsDirectory = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName, "libs", "netstandard2.0");
            compiler = new Compiler(netstandardRefsDirectory);

            runThread = null;

            SourceCode =
@"using System;

namespace Test {

    public class Program {
    
        public static void Main(string[] args) 
        {
            Console.Write(""What is your name ? "");
            //char input = (char)Console.Read();
            string input = Console.ReadLine();
            Console.WriteLine($""Hello { input }"");
        }

    }

}";
            consoleOutput =
@"============================================
          .NET Framework C# REPL
                       by Sigged
============================================
1. Enter C# code in the editor on the right
2. Press Build and Run
3. Watch Roslyn in action!
============================================

Ready.

";
        }

        private string sourceCode;
        public string SourceCode
        {
            get { return sourceCode; }
            set {
                sourceCode = value;
                RaisePropertyChanged();
            }
        }

        private string consoleOutput;
        public string ConsoleOutput
        {
            get { return consoleOutput; }
            set
            {
                consoleOutput = value;
                RaisePropertyChanged();
            }
        }

        private bool isBuilding;
        public bool IsBuilding
        {
            get { return isBuilding; }
            set
            {
                isBuilding = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(Build));
                RaisePropertyChanged(nameof(BuildAndRun));
                RaisePropertyChanged(nameof(Stop));
            }
        }

        private bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                isRunning = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(Build));
                RaisePropertyChanged(nameof(BuildAndRun));
                RaisePropertyChanged(nameof(Stop));
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged();
            }
        }

        private DiagnosticViewModel selectedDiagnostic;
        public DiagnosticViewModel SelectedDiagnostic
        {
            get { return selectedDiagnostic; }
            set
            {
                selectedDiagnostic = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<DiagnosticViewModel> diagnostics;
        public ObservableCollection<DiagnosticViewModel> Diagnostics
        {
            get { return diagnostics; }
            set {
                diagnostics = value;
                RaisePropertyChanged();
            }
        }

        private CodeSample selectedCodeSample;
        public CodeSample SelectedCodeSample
        {
            get { return selectedCodeSample; }
            set
            {
                selectedCodeSample = value;
                RaisePropertyChanged();
                SourceCode = selectedCodeSample.Contents;
            }
        }

        private ObservableCollection<CodeSample> codeSamples;
        public ObservableCollection<CodeSample> CodeSamples
        {
            get { return codeSamples; }
            set
            {
                codeSamples = value;
                RaisePropertyChanged();
            }
        }
        

        public ICommand LoadSamples => new RelayCommand(
            async () => {
                CodeSamples = new ObservableCollection<CodeSample>(
                    await SampleParser.GetSamples());
            }
        );

        public ICommand Build => new RelayCommand(
            async () => {
                using (var stream = new MemoryStream())
                {
                    Status = "Building...";
                    IsBuilding = true;
                    
                    EmitResult results = await compiler.Compile(sourceCode, "REPLAssembly", stream);
                    Diagnostics = new ObservableCollection<DiagnosticViewModel>(results.Diagnostics
                                                    .Select(diag => new DiagnosticViewModel(diag)));
                    IsBuilding = false;
                    
                    Status = results.Success ? "Build Success" : "Build Failed";
                }
            },
            () =>
            {
                return !isRunning && !IsBuilding;
            }
        );

        public ICommand BuildAndRun => new RelayCommand(
            async () => {

                await Task.Delay(0);

                //string tmpAssemblyPath = Path.GetTempFileName();
                //string tmpAssemblyDir = Path.GetDirectoryName(tmpAssemblyPath);
                //string tmpAssemblyName = Path.GetDirectoryName(tmpAssemblyPath);

                var permissions = new PermissionSet(PermissionState.Unrestricted);
                //var permissions = new PermissionSet(null);
                //var permissions = new PermissionSet(PermissionState.None);

                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.UnmanagedCode));
                permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
                //permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.AllFlags));
                permissions.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
                permissions.AddPermission(new WebPermission(PermissionState.None));

                AppDomainSetup appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    //ShadowCopyFiles = "true",
                    //LoaderOptimization = LoaderOptimization.MultiDomainHost
                };
                AppDomain domain = AppDomain.CreateDomain("HardenedDomain", null, appDomainSetup, permissions);

                runThread = new Thread(new ThreadStart(async () =>
                {
                    IsRunning = true;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        Status = "Building...";
                        IsBuilding = true;
                        try
                        {
                            //var result = await compiler.Compile(sourceCode, tmpAssemblyName, stream, outputKind: OutputKind.ConsoleApplication);

                            //using (var fs = new FileStream(tmpAssemblyPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                            //{
                            //    stream.CopyTo(fs);
                            //}

                            ////var assembly = AppDomain.CurrentDomain.Load(stream.ToArray());
                            ////Type entryClass = assembly.EntryPoint.DeclaringType;


                            //var handle = Activator.CreateInstance(domain, tmpAssemblyPath, "Harmless.HelloWorld.Program");
                            //var o = handle.Unwrap();

                            var result = await compiler.Compile(sourceCode, "REPLAssembly", stream, outputKind: OutputKind.ConsoleApplication);

                            Type assemblyLoaderType = typeof(AssemblyLoader);
                            AssemblyLoader loader = (AssemblyLoader)domain.CreateInstanceFromAndUnwrap(assemblyLoaderType.Assembly.Location, assemblyLoaderType.FullName);

                            AssemblyLoaderProxy loaderProxy = new AssemblyLoaderProxy(loader, inputAggregator);
                            loaderProxy.OnConsoleOutput += LoaderProxy_OnConsoleOutput;

                            IsBuilding = false;
                            Status = "Running";

                            try
                            {
                                //load and run assembly in remote AppDomain, using the proxy object
                                loaderProxy.LoadAndRun(stream.ToArray(), inputAggregator);
                            }
                            catch(Exception ex)
                            {
                                throw;
                            }
                            finally
                            {
                                Status = "Application stopped";
                                
                                //todo: find a way to safely unload Appdomain (thread problems)
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            //File.Delete(tmpAssemblyPath);
                        }
                    }

                    IsRunning = false;
                }));
                runThread.Start();
            }, 
            () => {
                return !isRunning && !IsBuilding;
            }
        );

        private void LoaderProxy_OnConsoleOutput(object sender, string output)
        {
            Application.Current.Dispatcher.Invoke(() => {
                ConsoleOutput += output;
            });
        }
        
        public ICommand Stop => new RelayCommand(
            () => {
                if(runThread?.IsAlive == true)
                {
                    try
                    {
                        runThread.Abort();
                    }
                    catch(ThreadAbortException)
                    {
                    }
                    finally
                    {
                        IsRunning = false;
                        Console.WriteLine("\n== Execution cancelled by user ==");
                    }
                    
                }
            },
            () =>
            {
                return isRunning;
            }
        );
    }
}
