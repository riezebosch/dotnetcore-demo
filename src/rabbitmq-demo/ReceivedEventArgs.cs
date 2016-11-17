using System;
using System.Collections.Generic;

namespace rabbitmq_demo
{
    public class ReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Type MessageType { get; set; }
        public Type HandledBy { get; set; }

        public override string ToString()
        {
            return $@"Receiver:   {HandledBy}
  Type:     {MessageType}
  Message:  {Message}";
        }
    }
}