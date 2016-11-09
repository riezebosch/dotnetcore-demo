using ef_demo;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using rabbitmq_demo.tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
        public class ReceiveTask<T> : IReceive<T>
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            public void Execute(T item)
            {
                tcs.SetResult(item);
            }

            public TaskAwaiter<T> GetAwaiter()
            {
                return tcs.Task.GetAwaiter();
            }
        }
    
}
