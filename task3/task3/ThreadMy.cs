namespace MyThreadPool;
internal class ThreadMy
{
    private Thread thread;
    private Func<Action> tasks;
    private volatile bool running = true;
    private void Loop()
    {
        while (running)
        {
            tasks()();
        }
    }
    public ThreadMy(Func<Action> _tasks)
    {
        tasks = _tasks;
        thread = new Thread(Loop);
        thread.Start();
    }
    public void ShutDown()
    {
        running = false;
    }
    public void Wait()
    {
        thread.Join();
    }
}

