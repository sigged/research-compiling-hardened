using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sigged.CsC.NetFx.Wpf
{
    
    public class CrossDomainConsoleInputReader : TextReader
    {
        
        private InputAggregator inputAggregator;

        public CrossDomainConsoleInputReader(InputAggregator inputaggregator)
        {
            inputAggregator = inputaggregator;
        }
        
        public override int Read()
        {
            int input = inputAggregator.GetUserInputChar();
            Console.Write((char)input);
            return input;
        }

        public override string ReadLine()
        {
            string input = inputAggregator.GetUserInputString();
            Console.WriteLine(input);
            return input;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            buffer = new char[count];
            return 0;
        }
    }
}
