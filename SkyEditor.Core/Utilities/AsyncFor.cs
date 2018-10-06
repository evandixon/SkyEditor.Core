using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SkyEditor.Core.Utilities.AsyncFor;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Runs a provided delegate function or sub repeatedly and asynchronously in the style of a For statement.
    /// </summary>
    public class AsyncFor : IReportProgress
    {
        public AsyncFor()
        {
            BatchSize = 0; // Unlimited
            RunningTasks = new List<Task>();
        }

        public delegate void ForItem(int i);
        public delegate void ForEachItem<T>(T i);
        public delegate Task ForEachItemAsync<T>(T i);
        public delegate Task ForItemAsync(int i);

        /// <summary>
        /// Raised when the progress of the batch operation changes
        /// </summary>
        public event EventHandler<ProgressReportedEventArgs> ProgressChanged;
        public event EventHandler Completed;

        #region Properties

        /// <summary>
        /// Whether or not to run each task sequentially.
        /// </summary>
        public bool RunSynchronously { get; set; }

        /// <summary>
        /// The number of tasks to run at once, or 0 or negative for no limit
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// The currently running tasks.
        /// </summary>
        private List<Task> RunningTasks { get; set; }

        /// <summary>
        /// The total number of tasks to run.
        /// </summary>
        private int TotalTasks { get; set; }

        /// <summary>
        /// The number of tasks that have been completed.
        /// </summary>
        /// <returns></returns>
        public int CompletedTasks
        {
            get
            {
                return _completedTasks;
            }
            private set
            {
                _completedTasks = value;                
            }
        }
        int _completedTasks;

        public float Progress => CompletedTasks / TotalTasks;

        public string Message => string.Empty;

        public bool IsIndeterminate => false;

        public bool IsCompleted { get; protected set; }

        #endregion

        protected void IncrementCompletedTasks()
        {
            Interlocked.Increment(ref _completedTasks);

            ProgressChanged?.Invoke(this, new ProgressReportedEventArgs() { Progress = Progress, IsIndeterminate = false });
            if (CompletedTasks == TotalTasks)
            {
                IsCompleted = true;
                Completed?.Invoke(this, new EventArgs());
            }
        }


        #region Core Functions

        /// <summary>
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="collection">The collection to be enumerated</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunForEach<T>(IEnumerable<T> collection, ForEachItemAsync<T> delegateFunction)
        {
            if (RunningTasks.Count > 0)
            {
                throw new InvalidOperationException(Properties.Resources.Utilities_AsyncFor_ErrorNoConcurrentExecution);
            }

            var exceptions = new List<Exception>();
            var taskItemQueue = new Queue<T>();
            foreach (var item in collection)
            {
                taskItemQueue.Enqueue(item);
            }

            TotalTasks = taskItemQueue.Count;

            // While there's either more tasks to start or while there's still tasks running
            while (taskItemQueue.Count > 0 || (taskItemQueue.Count == 0 && RunningTasks.Count > 0))
            {
                if ((BatchSize < 1 || RunningTasks.Count < BatchSize) && taskItemQueue.Count > 0)
                {
                    // We can run more tasks

                    // Get the next task item to run
                    var item = taskItemQueue.Dequeue(); // The item in Collection to process

                    // Start the task
                    var tTask = Task.Run(async () =>
                    {
                        await delegateFunction(item);
                        IncrementCompletedTasks();
                    });

                    // Either wait for it or move on
                    if (RunSynchronously)
                    {
                        await tTask;
                    }
                    else
                    {
                        RunningTasks.Add(tTask);
                    }
                }
                else
                {
                    if (RunningTasks.Count > 0)
                    {
                        // We can't start any more tasks, so we have to wait on one.
                        await Task.WhenAny(RunningTasks);

                        // Remove completed tasks
                        for (var count = RunningTasks.Count - 1; count >= 0; count--)
                        {
                            if (RunningTasks[count].Exception != null)
                            {
                                exceptions.Add(RunningTasks[count].Exception);
                                RunningTasks.RemoveAt(count);
                            }
                            else if (RunningTasks[count].IsCompleted)
                            {
                                RunningTasks.RemoveAt(count);
                            }
                        }
                    }
                    else
                    {
                        // We're finished.  Nothing else to do.
                        break;
                    }
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions.ToArray());
            }
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="DelegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunForEach<T>(IEnumerable<T> collection, ForEachItem<T> delegateSub)
        {
            await RunForEach(collection, i =>
            {
                delegateSub(i);
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="DelegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunFor(ForItemAsync delegateFunction, int startValue, int endValue, int stepCount = 1)
        {
            if (RunningTasks.Count > 0)
            {
                throw new InvalidOperationException(Properties.Resources.Utilities_AsyncFor_ErrorNoConcurrentExecution);
            }

            if (stepCount == 0)
            {
                throw (new ArgumentException(Properties.Resources.Utilities_AsyncFor_ErrorStepCount0, nameof(stepCount)));
            }

            var exceptions = new List<Exception>();

            // Find how many tasks there are to run
            // The +1 here makes the behavior "For i = 0 to 10" have 11 loops
            TotalTasks = (endValue - startValue + 1) / stepCount;

            if (TotalTasks < 0)
            {
                // Then in a normal For statement, the body would never be called
                TotalTasks = 0;
                CompletedTasks = 0;
                return;
            }

            int nextI = startValue;

            // While there's either more tasks to start or while there's still tasks running
            while (nextI <= endValue || RunningTasks.Count > 0)
            {
                // Add tasks if possible
                if (nextI <= endValue && (BatchSize < 1 || RunningTasks.Count < BatchSize))
                {
                    // We can run more tasks

                    var item = nextI; //To avoid async weirdness with having this in the below lambda

                    // Start the task
                    var tTask = Task.Run(async () =>
                    {
                        await delegateFunction(item);
                        IncrementCompletedTasks();
                    });

                    // Increment for the next run
                    nextI += stepCount;

                    // Either wait for it or move on
                    if (RunSynchronously)
                    {
                        await tTask;
                    }
                    else
                    {
                        RunningTasks.Add(tTask);
                    }
                }
                else
                {
                    // Otherwise, wait for one of them
                    await Task.WhenAny(RunningTasks);
                }

                // Remove completed tasks
                for (var count = RunningTasks.Count - 1; count >= 0; count--)
                {
                    if (RunningTasks[count].Exception != null)
                    {
                        exceptions.Add(RunningTasks[count].Exception);
                        RunningTasks.RemoveAt(count);
                    }
                    else if (RunningTasks[count].IsCompleted)
                    {
                        RunningTasks.RemoveAt(count);
                    }
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions.ToArray());
            }
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="DelegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunFor(int startValue, int endValue, ForItemAsync delegateSub, int stepCount = 1) => await RunFor(delegateSub, startValue, endValue, stepCount);

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="DelegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunFor(ForItem delegateSub, int startValue, int endValue, int stepCount = 1)
        {
            await RunFor(i =>
            {
                delegateSub(i);
                return Task.CompletedTask;
            }, startValue, endValue, stepCount);
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="DelegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public async Task RunFor(int startValue, int endValue, ForItem delegateSub, int stepCount = 1) => await RunFor(delegateSub, startValue, endValue, stepCount);

        #endregion

        #region Static Functions

        /// <summary>
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="collection">The collection to be enumerated</param>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task ForEach<T>(IEnumerable<T> collection, ForEachItemAsync<T> delegateFunction, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            void onProgressed(object sender, ProgressReportedEventArgs e)
            {
                progressReportToken.IsIndeterminate = e.IsIndeterminate;
                progressReportToken.Progress = e.Progress;
            }

            void onComplete(object sender, EventArgs e)
            {
                progressReportToken.IsCompleted = true;
            }

            var a = new AsyncFor();
            a.RunSynchronously = runSynchronously;
            a.BatchSize = batchSize;

            if (progressReportToken != null)
            {
                a.ProgressChanged += onProgressed;
                a.Completed += onComplete;
            }

            await a.RunForEach(collection, delegateFunction);

            if (progressReportToken != null)
            {
                a.ProgressChanged -= onProgressed;
                a.Completed -= onComplete;
            }
        }

        /// <summary>
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="collection">The collection to be enumerated</param>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task ForEach<T>(IEnumerable<T> collection, ForEachItem<T> delegateSub, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            await ForEach(collection, i =>
            {
                delegateSub(i);
                return Task.CompletedTask;
            }, runSynchronously, batchSize, progressReportToken);
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task For(int startValue, int endValue, ForItemAsync delegateFunction, int stepCount = 1, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            void onProgressed(object sender, ProgressReportedEventArgs e)
            {
                progressReportToken.IsIndeterminate = e.IsIndeterminate;
                progressReportToken.Progress = e.Progress;
            }

            void onComplete(object sender, EventArgs e)
            {
                progressReportToken.IsCompleted = true;
            }

            var a = new AsyncFor();
            a.RunSynchronously = runSynchronously;
            a.BatchSize = batchSize;

            if (progressReportToken != null)
            {
                a.ProgressChanged += onProgressed;
                a.Completed += onComplete;
            }

            await a.RunFor(delegateFunction, startValue, endValue, stepCount);

            if (progressReportToken != null)
            {
                a.ProgressChanged -= onProgressed;
                a.Completed -= onComplete;
            }
        }

        /// <summary>
        /// Asynchronously runs <see cref="delegateFunction"/> as a For statement would
        /// </summary>
        /// <param name="delegateSub">The function to asynchronously run</param>
        /// <param name="StartValue">The start value of the logical For statement</param>
        /// <param name="EndValue">The end value of the logical For statement</param>
        /// <param name="StepCount">How much to increment (or decrement if negative) the iterator of the logical For statement</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task For(int startValue, int endValue, ForItem delegateSub, int stepCount = 1, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            await For(startValue, endValue, i =>
            {
                delegateSub(i);
                return Task.CompletedTask;
            }, stepCount, runSynchronously, batchSize, progressReportToken);
        }

        #endregion

    }

    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="collection">The collection to be enumerated</param>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task RunAsyncForEach<T>(this IEnumerable<T> collection, ForEachItemAsync<T> delegateFunction, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            await AsyncFor.ForEach(collection, delegateFunction, runSynchronously, batchSize, progressReportToken);
        }

        /// <summary>
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="collection">The collection to be enumerated</param>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="runSynchronously">Whether or not to allow running multiple tasks at once. Defaults to false.</param>
        /// <param name="batchSize">The maximum number of tasks to run at once, or 0 or negative for no limit.</param>
        /// <param name="progressReportToken">Optional token to receive progress updates</param>
        /// <exception cref="InvalidOperationException">Thrown if execution starts before the end of another operation</exception>
        public static async Task RunAsyncForEach<T>(this IEnumerable<T> collection, ForEachItem<T> delegateFunction, bool runSynchronously = false, int batchSize = 0, ProgressReportToken progressReportToken = null)
        {
            await AsyncFor.ForEach(collection, delegateFunction, runSynchronously, batchSize, progressReportToken);
        }
    }
}