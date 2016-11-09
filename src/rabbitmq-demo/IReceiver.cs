using System;

namespace rabbitmq_demo
{
    [Obsolete("Implement the IReceive message on this class so it can be subscribed to the receiver.")]
    public interface IReceiver
    {
        void Subscribe<T>(Action<T> action);
    }
}