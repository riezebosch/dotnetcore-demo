using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    public interface IInvoke<T, TResult>
    {
        TResult Invoke(T input);
        IInvoke<T, TNext> Then<TNext>(Func<T, TNext> p) where TNext : T;
        IInvoke<T, T> Then(Action<T> p);
        IInvoke<T, T> Then(Action nothing);
    }
}
