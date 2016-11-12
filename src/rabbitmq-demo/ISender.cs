using System;

namespace rabbitmq_demo
{
    public interface ISender : IDisposable
    {
        void Publish<T>(T personCreated);
    }
}