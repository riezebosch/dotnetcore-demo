using System;

namespace rabbitmq_demo
{
    public interface IContinuation
    {
        void ContinueWith(Action<string> a);
    }
}