﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RxNetApp
{
    internal static class Program
    {
        #region Asynchronous Stream

        private static async IAsyncEnumerable<int> GetNumbersAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var random = new Random();
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(random.Next(50, 200), cancellationToken);
                yield return i;
            }
        }

        private static async Task PrintNumbers1(CancellationToken cancellationToken = default)
        {
            await foreach (var number in GetNumbersAsync(cancellationToken))
            {
                if (number % 2 == 1)
                {
                    Console.Write(number + " ");
                }
            }

            Console.Write("Completed");
        }

        #endregion

        #region ReactiveX

        private static IObservable<int> GetNumbersObservable(CancellationToken cancellationToken = default) =>
            Observable.Create<int>(async observer =>
            {
                var random = new Random();

                for (var i = 0; i < 10; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        observer.OnCompleted();
                        break;
                    }

                    await Task.Delay(random.Next(50, 200), cancellationToken);
                    observer.OnNext(i);
                }
            });

        private static async Task PrintNumbers2(CancellationToken cancellationToken = default)
        {
            var observable = GetNumbersObservable(cancellationToken);
            await observable
                .Where(number => number % 2 == 1)
                .ForEachAsync(number =>
                {
                    Console.Write($"{number} ");
                }, cancellationToken);

            Console.Write("Completed");
        }

        #endregion

        private static async Task Main(string[] args)
        {
            await PrintNumbers1();
            Console.WriteLine();
            await PrintNumbers2();
        }
    }
}