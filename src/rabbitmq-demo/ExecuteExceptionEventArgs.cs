using System;

namespace rabbitmq_demo
{
    public class ExecuteExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public Type Receiver { get; internal set; }

        public override string ToString()
        {
            return $@"Receiver:   {Receiver}
Exception:   {Exception}";
        }
    }
}