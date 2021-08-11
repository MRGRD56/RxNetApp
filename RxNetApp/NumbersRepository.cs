using System;
using System.Threading;
using System.Threading.Tasks;

namespace RxNetApp
{
    public static class NumbersRepository
    {
        private static readonly Random Random = new();

        public static async Task<int> GetNumberAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(Random.Next(50, 200), cancellationToken);
            return Random.Next(0, 10);
        }
    }
}