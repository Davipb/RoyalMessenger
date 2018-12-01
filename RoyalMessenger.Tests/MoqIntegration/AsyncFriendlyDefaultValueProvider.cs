using Moq;
using System;
using System.Threading.Tasks;

namespace RoyalMessenger.Tests.MoqIntegration
{
    /// <summary>
    /// A <see cref="DefaultValueProvider"/> for Moq that properly returns non-null tasks
    /// that can be awaited.
    /// </summary>
    public class AsyncFriendlyDefaultValueProvider : LookupOrFallbackDefaultValueProvider
    {
        public AsyncFriendlyDefaultValueProvider()
        {
            Register(typeof(Task<>), (type, mock) =>
            {
                var argument = type.GetGenericArguments()[0];
                var result = GetDefaultValue(argument, mock);

                // return Task.FromResult<T>(result)
                return typeof(Task)
                    .GetMethod("FromResult", Type.EmptyTypes)
                    .MakeGenericMethod(argument)
                    .Invoke(null, new[] { result });
            });

            Register(typeof(ValueTask<>), (type, mock) =>
            {
                var argument = type.GetGenericArguments()[0];
                var result = GetDefaultValue(argument, mock);

                // return new ValueTask<T>(result)
                return type
                    .GetConstructor(new[] { argument })
                    .Invoke(new[] { result });
            });

            Register(typeof(Task), (_, __) => Task.CompletedTask);
            Register(typeof(ValueTask), (_, __) => new ValueTask(Task.CompletedTask));
        }


    }
}
