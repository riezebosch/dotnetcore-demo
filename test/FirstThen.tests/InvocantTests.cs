using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FirstThen.tests
{
    public class InvocantTests
    {
        [Fact]
        public void FirstFuncThenFuncWithTransformation()
        {
            var invocant = new Invocant<string>();
            string result = string.Empty;
            invocant
                .Invoked()
                .Then(new StringBuilder().Append)
                .Then(builder => builder.Append("r"))
                .Then(builder => builder.ToString())
                .Then(s => s.ToUpper())
                .Then(s => result = s);

            invocant.Invoke("m");

            Assert.Equal("MR", result);
        }

        [Fact]
        public void FirstFuncThenActionWithParameter()
        {
            bool executed = false;
            Action<string> action = m => executed = true;

            var invocant = new Invocant<string>();
            invocant
                .Invoked()
                .Then(m => m)
                .Then(action);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstAction()
        {
            bool executed = false;
            Action<string> action = m => executed = true;

            var invocant = new Invocant<string>();
            invocant
                .Invoked()
                .Then(action);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstActionWithoutParameters()
        {
            bool executed = false;
            Action action = () => executed = true;

            var invocant = new Invocant<string>();
            invocant
                .Invoked()
                .Then(action);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncThenActionWithoutParameters()
        {
            bool executed = false;
            Action nothing = () => executed = true;

            var invocant = new Invocant<string>();
            invocant
                .Invoked()
                .Then(m => m).Then(nothing);

            invocant.Invoke("m");

            Assert.True(executed);
        }
    }
}
