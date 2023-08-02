using System.Runtime.CompilerServices;

namespace WPFMvvM.Extensions;

public static class AwaitableExtensions
{
    public static ConfiguredTaskAwaitable<TResult> FreeContext<TResult>(this Task<TResult> task)
        => task.ConfigureAwait(false);

    public static ConfiguredTaskAwaitable FreeContext(this Task task)
        => task.ConfigureAwait(false);
}
