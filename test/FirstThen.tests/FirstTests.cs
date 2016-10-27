using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FirstThen.tests
{
    public class FirstTests
    {
        [Fact]
        public void FirstFuncThenFuncWithTransformation()
        {
            var result = First.Do<string, StringBuilder>(new StringBuilder().Append)
                .Then(builder => builder.Append("r"))
                .Then(builder => builder.ToString())
                .Then(s => s.ToUpper())
                .Invoke("m");

            Assert.Equal("MR", result);
        }

        [Fact]
        public void FirstFuncThenActionWithParameter()
        {
            bool executed = false;
            Action<string> nothing = m => executed = true;
            var result = First
                .Do<string, string>(m => m)
                .Then(nothing)
                .Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstAction()
        {
            bool executed = false;
            Action<string> nothing = m => executed = true;
            var result = First
                .Do(nothing)
                .Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstActionWithoutParametes()
        {
            bool executed = false;
            Action nothing = () => executed = true;
            var result = First
                .Do<string>(nothing)
                .Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncThenActionWithoutParameters()
        {
            int executed = 0;
            Action nothing = () => executed++;
            var result = First
                .Do<string, string>(m => m)
                .Then(nothing)
                .Invoke("m");

            Assert.Equal(1, executed);
        }
    }
}
