using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Runs a provided delegate function or sub repeatedly and asynchronously in the style of a For statement.
    /// </summary>
    public class AsyncFor : IReportProgress
    {
        public AsyncFor()
        {
            BatchSize = int.MaxValue;
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
        /// The number of tasks to run at once.
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
        private int CompletedTasks
        {
            get
            {
                return _completedTasks;
            }
            set
            {
                _completedTasks = value;
                ProgressChanged?.Invoke(this, new ProgressReportedEventArgs() { Progress = Progress, IsIndeterminate = false });
                if (CompletedTasks == TotalTasks)
                {
                    IsCompleted = true;
                    Completed?.Invoke(this, new EventArgs());
                }
            }
        }
        int _completedTasks;
        object _completedTasksLock = new object();

        public float Progress => CompletedTasks / TotalTasks;

        public string Message => string.Empty;

        public bool IsIndeterminate => false;

        public bool IsCompleted { get; protected set; }

        #endregion

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
                if (RunningTasks.Count < BatchSize && taskItemQueue.Count > 0)
                {
                    // We can run more tasks

                    // Get the next task item to run
                    var item = taskItemQueue.Dequeue(); // The item in Collection to process

                    // Start the task
                    var tTask = Task.Run(async () =>
                    {
                        await delegateFunction(item);
                        lock (_completedTasksLock)
                        {
                            CompletedTasks += 1;
                        }
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
                if (nextI <= endValue && RunningTasks.Count < BatchSize)
                {
                    // We can run more tasks

                    var item = nextI; //To avoid async weirdness with having this in the below lambda

                    // Start the task
                    var tTask = Task.Run(async () =>
                    {
                        await delegateFunction(item);
                        lock (_completedTasksLock)
                        {
                            CompletedTasks += 1;
                        }
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
        /// Asynchronously runs <paramref name="delegateFunction"/> for every item in the given collection
        /// </summary>
        /// <typeparam name="T">Type of the collection item</typeparam>
        /// <param name="delegateFunction">The function to asynchronously run</param>
        /// <param name="collection">The collection to be enumerated</param>
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
        public async Task RunForEach<T>(IEnumerable<T> collection, ForEachItem<T> delegateSub)
        {
            await RunForEach(collection, i =>
            {
                delegateSub(i);
                return Task.CompletedTask;
            });
        }
        #endregion

    }
}