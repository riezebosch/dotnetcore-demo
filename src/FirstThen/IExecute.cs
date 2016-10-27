namespace FirstThen
{
    public interface IFinally<in TInput, out TResult>
    {
        TResult Execute(TInput input);
    }
}