using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    public interface IDo<out TResult> : IExecute
    {
        IDo<TNext> Then<TNext>(Func<TResult, TNext> transform);
        IDo<TResult> Then(Action action);
        IFinally<TResult> Finally();
    }

    public interface IDo<TInput, out TResult> : IExecute<TInput>
    {
        IDo<TInput, TNext> Then<TNext>(Func<TResult, TNext> transform);
        IDo<TInput, TResult> Then(Action<TResult> action);
        IDo<TInput, TResult> Then(Action action);
        IFinally<TInput, TResult> Finally();
    }
}
