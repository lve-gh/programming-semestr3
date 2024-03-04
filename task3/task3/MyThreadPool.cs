namespace MyThreadPool;

using System.Collections.Concurrent;

public class MyThreadPool
{
    private ThreadMy[] threads;
    private volatile bool running = true;
    private volatile int ToQueue = 0;
    ManualResetEvent empty = new(false);
    private ConcurrentQueue<IExecutable> tasks = new();
    private Semaphore amountOfTasks = new(0, int.MaxValue);
    public MyThreadPool(int threadsAmount)
    {
        this.threads = Enumerable.Range(0, threadsAmount).Select(_ => new ThreadMy(this.ProvideTask)).ToArray();
    }
    private interface IExecutable
    {
        public void Execute();
    }

    private void Submit(IExecutable task)
    {
        this.tasks.Enqueue(task);
        this.amountOfTasks.Release();
        Interlocked.Decrement(ref this.ToQueue);
    }

    private Action ProvideTask()
    {
        this.amountOfTasks.WaitOne();
        this.tasks.TryDequeue(out IExecutable? task);
        this.UpdateQueueIsEmpty();
        return task!.Execute;
    }

    private void UpdateQueueIsEmpty()
    {
        lock (this.tasks)
        {
            if (!this.running && this.tasks.IsEmpty && this.ToQueue == 0)
            {
                this.empty.Set();
            }
        }
    }

    private class EmptyExecutable : IExecutable
    {
        public void Execute() { }
    }

    public IMyTask<T> Submit<T>(Func<T> task)
    {
        lock (this.tasks)
        {
            if (!this.running)
            {
                throw new InvalidOperationException("The thread pool is shut down.");
            }

            Task<T> myTask = new(this, task);
            this.tasks.Enqueue(myTask);
            this.amountOfTasks.Release();
            return myTask;
        }
    }

    public IMyTask Submit(Action task)
    {
        Task myTask;
        lock (this.tasks)
        {
            if (!this.running)
            {
                throw new InvalidOperationException("The thread pool is shut down.");
            }

            myTask = new(this, task);
            this.tasks.Enqueue(myTask);
        }

        this.amountOfTasks.Release();
        return myTask;
    }

    public void Shutdown()
    {
        this.running = false;
        this.UpdateQueueIsEmpty();
        if (this.ToQueue != 0)
        {
            this.empty.WaitOne();
        }

        foreach (var thread in this.threads)
        {
            thread.ShutDown();
            this.tasks.Enqueue(new EmptyExecutable());
        }

        this.amountOfTasks.Release(this.threads.Length);

        foreach (var thread in this.threads)
        {
            thread.Wait();
        }
    }

    public void Dispose() => this.Shutdown();




    private class Task : Task<Monostate>, IMyTask
    {
        public Task(MyThreadPool threadPool, Action task)
            : base(
                threadPool,
                () =>
                {
                    task();
                    return Monostate.Instance;
                }
            )
        { }

        public IMyTask<TNew> ContinueWith<TNew>(Func<TNew> continuation) =>
            this.ContinueWith((Monostate _) => continuation());

        public IMyTask ContinueWith(Action continuation) =>
            this.ContinueWith((Monostate _) => continuation());
    }

    private class Task<T> : IMyTask<T>, IExecutable
    {
        private MyThreadPool threadPool;
        private volatile bool isCompleted = false;
        private Option<Result<T, Exception>> result = Option<Result<T, Exception>>.None;
        private ManualResetEvent executeCompleted = new(false);
        private Option<Func<T>> task;
        private List<IExecutable> continuations = new();

        public Task(MyThreadPool threadPool, Func<T> task)
        {
            this.task = task;
            this.threadPool = threadPool;
        }

        public bool IsCompleted => this.isCompleted;

        /// <inheritdoc cref="IMyTask{T}.Result"/>
        public T Result
        {
            get
            {
                if (this.isCompleted)
                {
                    return this.result.Unwrap().UnwrapOrElse(e => throw new AggregateException(e));
                }

                this.executeCompleted.WaitOne();
                return Volatile
                    .Read(ref this.result)
                    .Unwrap()
                    .UnwrapOrElse(e => throw new AggregateException(e));
            }
        }

        public void Execute()
        {
            if (!this.task.IsSome())
            {
                return;
            }

            this.result = Try<Exception>.Call(this.task.Unwrap());

            lock (this.result)
            {
                this.isCompleted = true;
                this.executeCompleted.Set();
                foreach (var continuation in this.continuations)
                {
                    this.threadPool.Submit(continuation);
                }
            }
        }

        public void Wait() => this.executeCompleted.WaitOne();

        public IMyTask<TNew> ContinueWith<TNew>(Func<T, TNew> continuation)
        {
            this.IncrementTaskAmount();
            Task<TNew> newTask = new(this.threadPool, () => continuation(this.Result));
            this.SubmitContinuation(newTask);
            return newTask;
        }

        public IMyTask ContinueWith(Action<T> continuation)
        {
            this.IncrementTaskAmount();
            Task newTask = new(this.threadPool, () => continuation(this.Result));
            this.SubmitContinuation(newTask);
            return newTask;
        }

        private void IncrementTaskAmount()
        {
            Interlocked.Increment(ref this.threadPool.ToQueue);
            if (!this.threadPool.running)
            {
                Interlocked.Decrement(ref this.threadPool.ToQueue);
                this.threadPool.UpdateQueueIsEmpty();
                throw new InvalidOperationException("The thread pool is shut down.");
            }
        }

        private void SubmitContinuation(IExecutable task)
        {
            lock (this.result)
            {
                if (this.isCompleted)
                {
                    this.threadPool.Submit(task);
                }
                else
                {
                    this.continuations.Add(task);
                }
            }
        }
    }
}

public interface IMyTask<T>
{
    public bool IsCompleted { get; }

    public T Result { get; }

    public void Wait();

    public IMyTask<TNew> ContinueWith<TNew>(Func<T, TNew> continuation);

    public IMyTask ContinueWith(Action<T> continuation);
}

public interface IMyTask : IMyTask<Monostate>
{

    public IMyTask<TNew> ContinueWith<TNew>(Func<TNew> continuation);
    public IMyTask ContinueWith(Action continuation);
}

public class Monostate
{
    /// <summary>
    /// Instance of the class.
    /// </summary>
    public static Monostate Instance { get; } = new();

    private Monostate() { }
}

public record Option<T>
{
    public sealed record Some(T value) : Option<T>;

    public sealed record _None : Option<T>;

    public static readonly _None None = new _None();

    public static Option<T> From(T? value) => value is null ? None : new Some(value);

    public static implicit operator Option<T>(T value) => new Some(value);

    public bool IsSome() => this is Some;

    public bool IsNone() => this is _None;
    public Option<TNew> Map<TNew>(Func<T, TNew> func) =>
        this is Some some ? new Option<TNew>.Some(func(some.value)) : Option<TNew>.None;
    public void Map(Action<T> action)
    {
        if (this is Some some)
        {
            action(some.value);
        }
    }
    public T Unwrap() =>
        this is Some some
            ? some.value
            : throw new InvalidOperationException("Invalid option access");

    private Option() { }
}

public static class Try<TException>
where TException : Exception
{
    public static Result<TResult, TException> Call<TResult>(Func<TResult> func)
    {
        try
        {
            return new Result<TResult, TException>.Ok(func());
        }
        catch (TException exception)
        {
            return new Result<TResult, TException>.Err(exception);
        }
    }

    public static async Task<Result<TResult, TException>> CallAsync<TResult>(
        Func<Task<TResult>> func
    )
    {
        try
        {
            return new Result<TResult, TException>.Ok(await func());
        }
        catch (TException exception)
        {
            return new Result<TResult, TException>.Err(exception);
        }
    }
    public static async Task<Option<TException>> CallAsync(
        Func<Task> func
    )
    {
        try
        {
            await func();
            return Option<TException>.None;
        }
        catch (TException exception)
        {
            return new Option<TException>.Some(exception);
        }
    }
}


public record Result<TOk, TErr>
{
    public sealed record Ok(TOk value) : Result<TOk, TErr>;

    public sealed record Err(TErr value) : Result<TOk, TErr>;

    public TOk UnwrapOrElse(Func<TErr, TOk> func) =>
        this is Ok ok ? ok.value : func((this as Err)!.value);

    public bool IsOk() => this is Ok;

    public bool IsErr() => this is Err;

    public Result<TNew, TErr> Map<TNew>(Func<TOk, TNew> func) =>
        this is Ok ok
            ? new Result<TNew, TErr>.Ok(func(ok.value))
            : new Result<TNew, TErr>.Err((this as Err)!.value);

    public TNew MapOrElse<TNew>(Func<TOk, TNew> funcOk, Func<TErr, TNew> funcErr) =>
        this is Ok ok ? funcOk(ok.value) : funcErr((this as Err)!.value);

    public Option<TOk> TryUnwrap() => this is Ok ok ? ok.value : Option<TOk>.None;

    public TErr UnwrapErr() =>
        this is Err err ? err.value : throw new InvalidOperationException("Invalid result access");

    private Result() { }
}
