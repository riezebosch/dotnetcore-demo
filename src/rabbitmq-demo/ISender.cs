public interface ISender
{
    void Publish<T>(T personCreated);
}