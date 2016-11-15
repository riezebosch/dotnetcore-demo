using Autofac;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace rabbitmq_demo
{
    public class ReceiveAsync<TContract> : IReceive<TContract>
    {
        TaskCompletionSource<TContract> tcs = new TaskCompletionSource<TContract>();

        public virtual void Execute(TContract item)
        {
            tcs.SetResult(item);
        }

        public async Task<TContract> WithTimeout(TimeSpan timeout)
        {
            await Task.WhenAny(tcs.Task, Task.Delay(timeout));
            if (!tcs.Task.IsCompleted)
            {
                throw new TimeoutException();
            }

            return tcs.Task.Result;
        }

        public Task<TContract> WithTimeout()
        {
            return WithTimeout(TimeSpan.FromSeconds(5));
        }


        public void SubscribeToEvents(Listener listener)
        {
            listener.SubscribeEvents<TContract>(WrapInContainer());
        }

        public void SubscribeToCommand(Listener listener)
        {
            listener.SubscribeCommands<TContract>(WrapInContainer());
        }

        private IContainer WrapInContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IReceive<TContract>>(this);

            return builder.Build();
        }

    }

}
