using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    public class Invocant<TInput> : IInvoke<TInput>, IInvocant<TInput>
    {
        private IInvoke<TInput> _last;

        public void Invoke(TInput input)
        {
            _last?.Invoke(input);
        }

        public IDo<TInput> Invoked()
        {
            return new Transformation<TInput>(input => input, this);
        }

        class Transformation<TResult> : IDo<TResult>, IInvoke<TInput>
        {
            private readonly Func<TInput, TResult> _transform;
            private readonly Invocant<TInput> _origin;

            public Transformation(Func<TInput, TResult> transform, Invocant<TInput> origin)
            {
                _transform = transform;

                // Store origin so we can pass it on to the next transformation when the chain of invocations is extended.
                // Make this one the last invocant on the original creator so that all intermediate methods are included when invoked.
                (_origin = origin)._last = this;
            }

            public IDo<TResult> Then(Action action)
            {
                return Then(action.ToFunc<TResult>());
            }

            public IDo<TResult> Then(Action<TResult> action)
            {
                return Then(action.ToFunc());
            }

            public IDo<TNext> Then<TNext>(Func<TResult, TNext> transform)
            {
                // Make our output the input of the next transformation.
                return new Transformation<TNext>(input => transform(_transform(input)), _origin);
            }

            public void Invoke(TInput input)
            {
                _transform(input);
            }
        }
    }
}
