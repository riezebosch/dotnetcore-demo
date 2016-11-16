using System;

namespace rabbitmq_demo
{
    public interface ISender
    {
        void PublishEvent<TMessage>(TMessage message);
        void PublishCommand<TMessage>(TMessage message);
    }
}