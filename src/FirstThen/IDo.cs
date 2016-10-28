using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    public interface IDo<out TResult>
    {
        IDo<TNext> Then<TNext>(Func<TResult, TNext> transform);
        IDo<TResult> Then(Action<TResult> action);
        IDo<TResult> Then(Action action);
    }
}
