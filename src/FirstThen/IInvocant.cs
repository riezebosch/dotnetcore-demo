namespace FirstThen
{
    public interface IInvocant<out TResult>
    {
        IDo<TResult> Invoked();
    }
}