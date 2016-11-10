namespace rabbitmq_demo
{
    public interface IReceive<TContract>
    {
        void Execute(TContract item);
    }
}