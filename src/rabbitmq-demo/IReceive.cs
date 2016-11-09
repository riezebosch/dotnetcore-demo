namespace rabbitmq_demo
{
    public interface IReceive<T>
    {
        void Execute(T item);
    }
}