using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using NSubstitute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class BlockingReceiverTests
    {
        [Fact]
        public void WaitForNextValue()
        {
            var items = Enumerable.Range(0, 10).ToList();
            using (var service = new BlockingReceiver<int>())
            {
                var task = Task.Run(() =>
                    {
                        foreach (var item in items)
                        {
                            service.Execute(item);
                            Thread.Sleep(100);
                        }
                    });

                foreach (var item in items)
                {
                    var result = service.Next();
                    Assert.Equal(item, result);
                }
            }
        }

        [Fact]
        public void TimeoutWhenMessageDoesNotArrive()
        {
            using (var service = new BlockingReceiver<int>())
            {
                Assert.Throws<TimeoutException>(() => service.Next());
            }
        }
    }
}
