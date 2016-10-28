using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FirstThen.tests
{

    public class ActionExtensionTests
    { 
        [Fact]
        public void ActionToFunc()
        {
            var executed = false;
            Action action = () => executed = true;
            Func<int, int> func = action.ToFunc<int>();

            func(3);
            Assert.True(executed);
        }

        [Fact]
        public void ActionToFuncInputIsOutput()
        {
            Action action = () => { };
            Func<int, int> func = action.ToFunc<int>();

            var result = func(3);
            Assert.Equal(3, result);
        }

        [Fact]
        public void ActionWithParameterToFunc()
        {
            var result = 0;
            Action<int> action = input =>  result = input;
            Func<int, int> func = action.ToFunc<int>();

            func(3);
            Assert.Equal(3, result);
        }
    }
}
