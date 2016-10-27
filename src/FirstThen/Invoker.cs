using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    internal class Invoker<TInput, TResult> : IInvoke<TInput, TResult>
    {
        private Func<TInput, TResult> a;

        public Invoker(Func<TInput, TResult> a)
        {
            this.a = a;
        }

        public TResult Invoke(TInput input)
        {
            return a(input);
        }

        public IInvoke<TInput, TResult> Then(Action p)
        {
            return Then(m => { p(); return m; });
        }

        public IInvoke<TInput, TResult> Then(Action<TResult> p)
        {
            return Then(m => { p(m); return m; });
        }

        public IInvoke<TInput, TNext> Then<TNext>(Func<TResult, TNext> p) 
        {
            return new Invoker<TInput, TNext>(m => p(a(m)));
        }
    }
}
