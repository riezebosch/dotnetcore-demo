using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class ReceveivedEventArgsTests
    {
        [Fact]
        public void ToStringOfEventArgs()
        {
            var e = new ReceivedEventArgs { HandledBy = typeof(int), Message = "hallo", Topic = "Int32" };
            Assert.Equal(@"Receiver:   System.Int32
  Topic:    Int32
  Message:  hallo", e.ToString());
        }
    }
}
