using System;
using System.Collections.Generic;

namespace Automa.Opc.Ua.Client
{
    public class ChangeEventArgs : EventArgs
    {
        public string Tag { get; internal set; }

        public IEnumerable<object> Values { get; internal set; }
    }
}