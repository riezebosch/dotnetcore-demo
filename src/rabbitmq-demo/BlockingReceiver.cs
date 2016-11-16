using Autofac;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace rabbitmq_demo
{
    public class BlockingReceiver<TMessage> : IReceive<TMessage>, IDisposable
    {
        private BlockingCollection<TMessage> _collection = new BlockingCollection<TMessage>();

        public void Dispose()
        {
            _collection.Dispose();
        }

        public virtual void Execute(TMessage item)
        {
            _collection.Add(item);
        }

        public virtual TMessage Next()
        {
            return Next(TimeSpan.FromSeconds(5));
        }

        public TMessage Next(TimeSpan timeout)
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
            listener.SubscribeEvents<TMessage>(WrapInContainer());
        }

        public void SubscribeToCommand(Listener listener)
        {
            listener.SubscribeCommands<TMessage>(WrapInContainer());
        }

        private IContainer WrapInContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IReceive<TMessage>>(this);

            return builder.Build();
        }
    }
}
