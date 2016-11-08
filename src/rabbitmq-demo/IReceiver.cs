using System;

namespace rabbitmq_demo
{
    public interface IReceiver
    {
        void Subscribe<T>(Action<T> action);
    }
}