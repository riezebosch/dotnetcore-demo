namespace rabbitmq_demo
{
    public interface IReceive<TMessage>
    {
        void Execute(TMessage item);
    }
}