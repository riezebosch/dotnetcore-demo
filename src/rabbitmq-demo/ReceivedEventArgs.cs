using System;
using System.Collections.Generic;

namespace rabbitmq_demo
{
    public class ReceivedEventArgs
    {
        public string Message { get; set; }
        public string Topic { get; set; }
        public Type HandledBy { get; set; }

        public override string ToString()
        {
            return $@"Receiver:   {HandledBy}
  Topic:    {Topic}
  Message:  {Message}";
        }
    }
}