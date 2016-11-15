using Autofac;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace rabbitmq_demo
{
    public class BlockingReceiver<TContract> : IReceive<TContract>, IDisposable
    {
        private BlockingCollection<TContract> _collection = new BlockingCollection<TContract>();

        public void Dispose()
        {
            _collection.Dispose();
        }

        public virtual void Execute(TContract item)
        {
            _collection.Add(item);
        }

        public virtual TContract Next()
        {
            return Next(TimeSpan.FromSeconds(5));
        }

        public TContract Next(TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    return _collection.Take(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
            }
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
