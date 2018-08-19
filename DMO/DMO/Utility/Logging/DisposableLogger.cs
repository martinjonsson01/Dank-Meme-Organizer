using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility.Logging
{
    public class DisposableLogger : IDisposable
    {
        private readonly Action<Stopwatch> _endLogFunction;
        private readonly Stopwatch _sw;

        public DisposableLogger(Action startLogFunction, Action<Stopwatch> endLogFunction)
        {
            _endLogFunction = endLogFunction;

            _sw = new Stopwatch();
            _sw.Start();
            startLogFunction?.Invoke();
        }

        public void Dispose()
        {
            _sw.Stop();
            _endLogFunction?.Invoke(_sw);
        }
    }
}
