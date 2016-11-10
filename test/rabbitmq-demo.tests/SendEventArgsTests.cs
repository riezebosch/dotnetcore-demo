using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class SendEventArgsTests
    {
        [Fact]
        public void ToStringOfEventArgs()
        {
            var e = new SendEventArgs { Message = "hallo", Topic = "Int32" };
            Assert.Equal(@"Topic:    Int32
  Message:  hallo", e.ToString());
        }
    }
}
