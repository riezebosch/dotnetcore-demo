using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{

    internal class ReceiverWithException<T> : ReceiveAsync<T>
    {
        public override void Execute(T item)
        {
            base.Execute(item);
            throw new NotImplementedException();
        }
    }
}
