using System;

namespace rabbitmq_demo
{
    public interface ISender
    {
        void Publish<T>(T personCreated);
    }
}