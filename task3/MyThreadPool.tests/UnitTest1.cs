namespace MyThreadPool.tests;

public class MyThreadPoolTests
{
    [Test]
    public void TestTheTask()
    {
        int n = 5;
        MyThreadPool pool = new(2);
        var task = pool.Submit(() => Volatile.Write(ref n, 1));
        task.Wait();
        Assert.That(Volatile.Read(ref n), Is.EqualTo(1));
    }

    [Test]
    public void TestTheReturn()
    {
        int n = 5;
        MyThreadPool pool = new(2);
        var task = pool.Submit(() => n);
        Assert.That(task.Result, Is.EqualTo(n));
    }

    [Test]
    public void TestPlus()
    {
        int n = 5;
        MyThreadPool pool = new(2);
        var task = pool.Submit(() => n).ContinueWith(n => n + 1);
        Assert.That(task.Result, Is.EqualTo(6));
    }

    [Test]

    public void IsCompleted()
    {
        MyThreadPool pool = new(2);
        ManualResetEvent mre = new(false);
        var task = pool.Submit(() => mre.WaitOne());
        Assert.That(task.IsCompleted, Is.False);
        mre.Set();
        task.Wait();
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void ThreadsCountCheck()
    {
        int n = 5;
        var initialProcessesAmount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
        MyThreadPool pool = new(n);
        Assert.That(
            System.Diagnostics.Process.GetCurrentProcess().Threads.Count,
            Is.EqualTo(initialProcessesAmount + n)
        );
    }

    [Test]
    public void TestShutodwn()
    {
        int n = 5;
        var initialProcessesAmount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
        MyThreadPool pool = new(n);
        pool.Shutdown();
        Assert.That(
            System.Diagnostics.Process.GetCurrentProcess().Threads.Count,
            Is.EqualTo(initialProcessesAmount)
        );
    }

}
