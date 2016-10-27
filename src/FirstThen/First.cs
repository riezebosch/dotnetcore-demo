using System;
using System.Collections.Generic;
using System.Linq;


namespace FirstThen
{
    public static class First
    {
        public static IDo<T, TResult> Do<T, TResult>(Func<T, TResult> a)
        {
            return new Invoker<T, TResult>(a);
        }

        public static IDo<T, T> Do<T>(Action<T> a)
        {
            return new Invoker<T, T>(m => m).Then(a);
        }

        public static IDo<T, T> Do<T>(Action a)
        {
            return new Invoker<T, T>(m => m).Then(a);
        }
    }
}
