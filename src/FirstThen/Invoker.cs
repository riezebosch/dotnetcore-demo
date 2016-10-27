using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstThen
{
    internal class Invoker<TInput, TResult> : IDo<TInput, TResult>,  IFinally<TInput, TResult>
    {
        private Func<TInput, TResult> _transform;
        private IExecute<TInput> _last;

        public Invoker(Func<TInput, TResult> transform)
        {
            _transform = transform;
        }

        public void Invoke(TInput input)
        {
            if (_last == null)
            {
                Finally().Execute(input);
            }
            else
            {
                _last.Invoke(input);
            }
        }

        public IFinally<TInput, TResult> Finally()
        {
            return this;
        }

        public IDo<TInput, TResult> Then(Action action)
        {
            return Then(input => { action(); return input; });
        }

        public IDo<TInput, TResult> Then(Action<TResult> action)
        {
            return Then(input => { action(input); return input; });
        }

        public IDo<TInput, TNext> Then<TNext>(Func<TResult, TNext> transform) 
        {
            var next = new Invoker<TInput, TNext>(input => transform(_transform(input)));
            _last = next;

            return next;
        }

        TResult IFinally<TInput, TResult>.Execute(TInput input)
        {
            return _transform(input);
        }
    }

    internal class Invoker<TResult> : IDo<TResult>, IFinally<TResult>
    {
        private Func<TResult> a;
        private IExecute _last;

        public Invoker(Func<TResult> a)
        {
            this.a = a;
        }

        public TResult Execute()
        {
            return a();
        }

        public IFinally<TResult> Finally()
        {
            return this;
        }

        public void Invoke()
        {
            if (_last == null)
            {
                Finally().Execute();
            }
            else
            {
                _last.Invoke();
            }
        }

        public IDo<TResult> Then(Action action)
        {
            return Then(input => { action(); return input; });
        }

        public IDo<TNext> Then<TNext>(Func<TResult, TNext> transform)
        {
            var next = new Invoker<TNext>(() => transform(a()));
            _last = next;

            return next;
        }
    }
}
