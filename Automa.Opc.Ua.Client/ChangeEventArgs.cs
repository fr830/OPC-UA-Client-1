using System;
using System.Collections.Generic;

namespace Automa.Opc.Ua.Client
{
    public class ChangeEventArgs : EventArgs
    {
        public string Tag { get; set; }

        public IEnumerable<object> Values { get; set; }
    }
}