using System;
using System.Threading.Tasks;

namespace ClusterClient.Utils
{
    public static class TaskExtensions
    {
        public static async Task<T> WithTimeoutAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
            if (!task.IsCompleted)
                throw new TimeoutException();
            return task.Result;
        }

        public static async Task<T> WithTimeoutAsync<T>(this Task<Task<T>> whenAnyTask, TimeSpan timeout)
        {
            await Task.WhenAny(whenAnyTask, Task.Delay(timeout));
            if (whenAnyTask.IsCompleted && whenAnyTask.Status == TaskStatus.RanToCompletion)
                return whenAnyTask.Result.Result;
            throw new TimeoutException();
        }
    }
}