using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigged.CsC.NetFx.Wpf
{

    public delegate void ConsoleOutputHandler(object sender, string output);

    internal  class AssemblyLoaderProxy: AssemblyLoader
    {
        public readonly AssemblyLoader Instance;
        protected InputAggregator inputAggregator;

        public AssemblyLoaderProxy(AssemblyLoader loaderInstance, InputAggregator inputaggregator)
        {
            inputAggregator = inputaggregator;
            Instance = loaderInstance;
            Instance.OnConsoleOutput += new ConsoleOutputHandler((sender, output) => RaiseOnConsoleOutput(Instance, output));
        }

        public override void LoadAndRun(byte[] assemblyBytes, InputAggregator inputAggregator)
        {
            this.Instance.LoadAndRun(assemblyBytes, inputAggregator);
        }
    }

    internal class AssemblyLoader : MarshalByRefObject
    {
        public event ConsoleOutputHandler OnConsoleOutput;

        protected void RaiseOnConsoleOutput(AssemblyLoader loader, string output)
        {
            OnConsoleOutput?.Invoke(loader, output);
        }

        public virtual void LoadAndRun(byte[] assemblyBytes, InputAggregator inputAggregator)
        {
            CrossDomainConsoleOutputWriter outputRedirector = new CrossDomainConsoleOutputWriter();
            outputRedirector.OnConsoleOutput += OutputRedirector_OnConsoleOutput;

            CrossDomainConsoleInputReader inputRedirector = new CrossDomainConsoleInputReader(inputAggregator);
            
            Console.SetOut(outputRedirector);
            Console.SetIn(inputRedirector);


            var assembly = AppDomain.CurrentDomain.Load(assemblyBytes);

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

        }

        protected void OutputRedirector_OnConsoleOutput(object sender, string output)
        {
            OnConsoleOutput?.Invoke(this, output);
        }
    }
}
