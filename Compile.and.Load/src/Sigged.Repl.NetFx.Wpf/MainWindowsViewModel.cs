using System.ComponentModel;
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

        public ICommand Clear => new RelayCommand(
            () => {
                SourceCode = "";
            }, 
            () => {
                return SourceCode?.Length > 0;
            }
        );

        public ICommand Build => new RelayCommand(() => {

        });

        public ICommand BuildAndRun => new RelayCommand(() => {

        });

    }
}
