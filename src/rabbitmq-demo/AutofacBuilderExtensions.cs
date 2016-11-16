using System;
using Autofac;
using Autofac.Builder;

namespace rabbitmq_demo
{
    public static class AutofacBuilderExtensions
    {
        public static IRegistrationBuilder<TReceiver, 
            ConcreteReflectionActivatorData, 
            SingleRegistrationStyle> 
            RegisterReceiverFor<TReceiver, TMessage>(this ContainerBuilder builder)
            where TReceiver: IReceive<TMessage>
        {
             return builder
                .RegisterType<TReceiver>()
                .As<IReceive<TMessage>>();
        }
    }
}