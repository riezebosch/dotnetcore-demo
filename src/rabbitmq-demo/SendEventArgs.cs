using System;

namespace rabbitmq_demo
{
    public class SendEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Topic { get; set; }

        public override string ToString()
        {
            return $@"Topic:    {Topic}
  Message:  {Message}";
        }
    }
}