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
            var invocant = new Invocant<string>();
            string result = string.Empty;
            invocant.Then(new StringBuilder().Append)
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
            invocant.Then(action);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstActionWithoutParameters()
        {
            bool executed = false;
            Action action = () => executed = true;

            var invocant = new Invocant<string>();
            invocant.Then(action);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void FirstFuncThenActionWithoutParameters()
        {
            bool executed = false;
            Action nothing = () => executed = true;

            var invocant = new Invocant<string>();
            invocant.Then(m => m).Then(nothing);

            invocant.Invoke("m");

            Assert.True(executed);
        }

        [Fact]
        public void ExtendOnFuncItself()
        {
            Func<int> invocant = () => 5;
            var next = invocant.Then(i => i * 2);

            var result = next();
            Assert.Equal(10, result);
        }

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

    public static class LambdaExtensions
    {
        public static Func<TNext> Then<T, TNext>(this Func<T> first, Func<T, TNext> then)
        {
            return new Func<TNext>(() => then(first()));
        }

       
    }

}
