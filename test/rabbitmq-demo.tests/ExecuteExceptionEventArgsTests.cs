using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class ExecuteExceptionEventArgsTests
    {
        [Fact]
        public void ToStringOfEventArgs()
        {
            var e = new ExecuteExceptionEventArgs { Receiver = typeof(int), Exception = new TimeoutException("too long") };
            Assert.Equal(@"Receiver:   System.Int32
Exception:   System.TimeoutException: too long", e.ToString());
        }
    }
}
