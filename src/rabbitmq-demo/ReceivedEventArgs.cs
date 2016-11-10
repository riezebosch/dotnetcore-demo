using System;
using System.Collections.Generic;

namespace rabbitmq_demo
{
    public class ReceivedEventArgs
    {
        public IEnumerable<char> Content { get; set; }
        public string Topic { get; set; }
        public Type HandledBy { get; set; }
    }
}