using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public MainWindowsViewModel()
        {
            string netstandardRefsDirectory = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName, "libs", "netstandard2.0");
            compiler = new Compiler(netstandardRefsDirectory);

            SourceCode = "dfsdf";
        }

        private string sourceCode;
        public string SourceCode
        {
            get { return sourceCode; }
            set {
                sourceCode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Clear));
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


        public ICommand JumpToDiagnosticSource => new RelayCommand(
            () => {
                
                

            }
        );

        public ICommand Clear => new RelayCommand(
            () => {
                SourceCode = "";
            }, 
            () => {
                return SourceCode?.Length > 0;
            }
        );

        public ICommand Build => new RelayCommand(async  () => {
            using (var stream = new MemoryStream())
            {
                Status = "Building...";
                IsBuilding = true;
                EmitResult results = await compiler.Compile(sourceCode, "REPLAssembly", stream);
                Diagnostics = new ObservableCollection<DiagnosticViewModel>(results.Diagnostics
                                                .Select(diag => new DiagnosticViewModel(diag)));
                //Diagnostics[0].Diagnostic.Id
                IsBuilding = false;
                Status = results.Success ? "Build Success" : "Build Failed";
            }
        });

        public ICommand BuildAndRun => new RelayCommand(() => {

        });

    }
}
