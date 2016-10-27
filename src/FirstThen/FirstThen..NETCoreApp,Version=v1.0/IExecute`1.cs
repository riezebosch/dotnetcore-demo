namespace FirstThen
{

    public interface IExecute<TInput>
    {
        void Invoke(TInput input);
    }
}