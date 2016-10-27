using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    public interface IInvoke<TInput, TResult>
    {
        TResult Invoke(TInput input);
        IInvoke<TInput, TNext> Then<TNext>(Func<TResult, TNext> p) ;
        IInvoke<TInput, TResult> Then(Action<TResult> p);
        IInvoke<TInput, TResult> Then(Action nothing);
    }
}
