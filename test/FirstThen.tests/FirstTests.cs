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
                .Finally()
                .Execute("m");

            Assert.Equal("MR", result);
        }

        [Fact]
        public void FirstFuncThenActionWithParameter()
        {
            bool executed = false;
            Action<string> nothing = m => executed = true;
            First
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
            First
                .Do(nothing)
                .Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstActionWithoutParameters()
        {
            bool executed = false;
            Action nothing = () => executed = true;
            First
                .Do<string>(nothing)
                .Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncThenActionWithoutParameters()
        {
            int executed = 0;
            Action nothing = () => executed++;
            First
                .Do<string, string>(m => m)
                .Then(nothing)
                .Invoke("m");

            Assert.Equal(1, executed);
        }

        [Fact]
        public void InvokeAlwaysExecutesOnLastActionEvenWhenPerformedHalfwayThroughToIntegrateTheWholeChainToBeExensible()
        {
            // Arrange
            bool executed = false;
            var first = First.Do<int, int>(i => i * 2);
            first.Then(() => executed = true);

            // Act
            first.Invoke(4);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void ExcuteOnFinallyYieldsResult()
        {
            var result = First.Do<int, int>(i => i * 2)
                .Then(i => $"transform the result into {i}")
                .Finally()
                .Execute(4);

            Assert.Equal("transform the result into 8", result);
        }

        [Fact]
        public void FirstFuncWithoutParameters()
        {
            var result = First
                .Do(() => "input")
                .Then(m => m + m)
                .Finally()
                .Execute();
            Assert.Equal("inputinput", result);
        }

        [Fact]
        public void FirstFuncInvokeOnLast()
        {
            bool executed = false;
            var first = First
                .Do(() => "input");

            first.Then(() => executed = true);

            first.Invoke();
            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncWithoutParametersThenActionIsExecuted()
        {
            bool executed = false;
            Action nothing = () => executed = true;
            var result = First
                .Do(() => "input")
                .Then(nothing)
                .Finally()
                .Execute();

            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncWithoutParametersThenResultIsTransient()
        {
            Action nothing = () => { };
            var result = First
                .Do(() => "input")
                .Then(nothing)
                .Finally()
                .Execute();


            Assert.Equal("input", result);
        }
    }
}
