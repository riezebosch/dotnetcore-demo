using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace rabbitmq_demo
{
    public class ReceiveAsync<T> : IReceive<T>
    {
        TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

        public virtual void Execute(T item)
        {
            tcs.SetResult(item);
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return tcs.Task.GetAwaiter();
        }

        public async Task<T> WithTimeout(TimeSpan timeout)
        {
            await Task.WhenAny(tcs.Task, Task.Delay(timeout));
            if (!tcs.Task.IsCompleted)
            {
                throw new TimeoutException();
            }

            return tcs.Task.Result;
        }

        public Task<T> WithTimeout()
        {
            return WithTimeout(TimeSpan.FromSeconds(5));
        }
    }

}
