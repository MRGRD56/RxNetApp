using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Random Random = new();
        
        #region Asynchronous Stream: IAsyncEnumberable

        private static async IAsyncEnumerable<int> GetNumbers1(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < 15; i++)
            {
                var number = await NumbersRepository.GetNumberAsync(cancellationToken);
                yield return number;
            }
        }

        private static async Task PrintNumbers1(CancellationToken cancellationToken = default)
        {
            #region Vanilla C#

            // await foreach (var number in GetNumbersAsync(cancellationToken))
            // {
            //     if (number % 2 == 1)
            //     {
            //         Console.Write(number + " ");   
            //     }
            // }

            #endregion

            #region System.Linq.AsyncEnumerable

            await GetNumbers1(cancellationToken)
                .Where(number => number % 2 == 1)
                .ForEachAsync(number =>
                {
                    Console.Write(number + " ");
                }, cancellationToken);

            #endregion

            Console.Write("Completed\n");
        }

        #endregion

        #region ReactiveX: IObservable

        private static IObservable<int> GetNumbers2(CancellationToken cancellationToken = default) =>
            Observable.Create<int>(async observer =>
            {
                for (var i = 0; i < 15; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        observer.OnCompleted();
                        break;
                    }

                    var number = await NumbersRepository.GetNumberAsync(cancellationToken);
                    observer.OnNext(number);
                }
            });

        private static async Task PrintNumbers2(CancellationToken cancellationToken = default)
        {
            var observable = GetNumbers2(cancellationToken);
            await observable
                .Where(number => number % 2 == 1)
                .ForEachAsync(number =>
                {
                    Console.Write($"{number} ");
                }, cancellationToken);

            Console.Write("Completed\n");
        }

        #endregion

        #region IAsyncEnumerable -> IObservable

        private static async Task PrintNumbers3(CancellationToken cancellationToken = default)
        {
            var observable = GetNumbers1(cancellationToken).ToObservable();
            await observable
                .Where(number => number % 2 == 1)
                .ForEachAsync(number =>
            {
                Console.Write(number + " ");
            }, cancellationToken);
            
            Console.Write("Completed\n");
        }

        #endregion
        
        #region Callback

        public static async Task GetNumbers4(Action<int> onNext, CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < 15; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var number = await NumbersRepository.GetNumberAsync(cancellationToken);
                onNext(number);
            }
        }

        public static async Task PrintNumbers4(CancellationToken cancellationToken = default)
        {
            await GetNumbers4(number =>
            {
                if (number % 2 == 1)
                {
                    Console.Write(number + " ");
                }
            }, cancellationToken);
            
            Console.Write("Completed\n");
        }
        
        #endregion

        private static async Task Main(string[] args)
        {
            await PrintNumbers1();
            await PrintNumbers2();
            await PrintNumbers3();
            await PrintNumbers4();
        }
    }
}