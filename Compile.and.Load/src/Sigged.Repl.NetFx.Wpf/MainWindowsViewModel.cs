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

        private ObservableCollection<DiagnosticViewModel> diagnostics;
        public ObservableCollection<DiagnosticViewModel> Diagnostics
        {
            get { return diagnostics; }
            set {
                diagnostics = value;
                RaisePropertyChanged();
            }
        }

        public ICommand Clear => new RelayCommand(
            () => {
                SourceCode = "";
            }, 
            () => {
                return SourceCode?.Length > 0;
            }
        );

        public ICommand Build => new RelayCommand(() => {
            using (var stream = new MemoryStream())
            {
                EmitResult results = compiler.Compile(sourceCode, "REPLAssembly", stream);
                Diagnostics = new ObservableCollection<DiagnosticViewModel>(results.Diagnostics
                                                .Select(diag => new DiagnosticViewModel(diag)));
            }
        });

        public ICommand BuildAndRun => new RelayCommand(() => {

        });

    }
}
