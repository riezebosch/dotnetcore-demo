using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    internal class Invoker<T, TResult> : IInvoke<T, TResult>
    {
        private Func<T, TResult> a;

        public Invoker(Func<T, TResult> a)
        {
            this.a = a;
        }

        public TResult Invoke(T input)
        {
            return a(input);
        }

        public IInvoke<T, T> Then(Action p)
        {
            return Then(m => { p(); return m; });
        }

        public IInvoke<T, T> Then(Action<T> p)
        {
            return Then(m => { p(m); return m; });
        }

        public IInvoke<T, TNext> Then<TNext>(Func<T, TNext> p) where TNext : T
        {
            return new Invoker<T, TNext>(p);
        }
    }
}
