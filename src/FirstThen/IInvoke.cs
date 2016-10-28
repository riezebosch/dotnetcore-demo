namespace FirstThen
{
    public interface IInvoke<in TInput>
    {
        void Invoke(TInput input);
    }
}