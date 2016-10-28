namespace FirstThen
{
    public interface IInvoke<TInput>
    {
        void Invoke(TInput input);
    }
}