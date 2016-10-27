namespace FirstThen
{
    public interface IExecute
    {
        void Invoke();
    }

    public interface IExecute<TInput>
    {
        void Invoke(TInput input);
    }
}