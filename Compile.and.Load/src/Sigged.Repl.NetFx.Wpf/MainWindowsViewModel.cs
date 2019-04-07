using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sigged.Repl.NetFx.Wpf
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


        public MainWindowsViewModel()
        {
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

                runThread = new Thread(new ThreadStart(async () =>
                {
                    IsRunning = true;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        Status = "Building...";
                        IsBuilding = true;

                        var result = await compiler.Compile(sourceCode, "REPLAssembly", stream);
                        var assemly = Assembly.Load(stream.ToArray());
                        var type = assemly.GetType("Test.Program");
                        //var test = type.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public, null, null);
                        try
                        {
                            IsBuilding = false;
                            Status = "Running";

                            type.InvokeMember("Main",
                                                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                                                null, null,
                                                new object[] { new string[] { } });

                            Status = "Application stopped";
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
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
