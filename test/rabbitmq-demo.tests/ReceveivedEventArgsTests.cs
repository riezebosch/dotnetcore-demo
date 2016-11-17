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
            var e = new ReceivedEventArgs { HandledBy = typeof(int), Message = "hallo", MessageType = typeof(int) };
            Assert.Equal(@"Receiver:   System.Int32
  Type:     System.Int32
  Message:  hallo", e.ToString());
        }
    }
}
