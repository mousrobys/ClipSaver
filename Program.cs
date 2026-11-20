using System.Threading;

namespace ClipSaver;

static class Program
{
    private static Mutex? _mutex;
    internal static EventWaitHandle? ShowEvent;

    [STAThread]
    static void Main()
    {
        const string mutexName = "Local\\ClipSaver_SingleInstance_Mutex";
        const string eventName = "Local\\ClipSaver_ShowWindow_Event";

        bool createdNew;
        _mutex = new Mutex(true, mutexName, out createdNew);

        if (!createdNew)
        {
            try
            {
                using var ev = EventWaitHandle.OpenExisting(eventName);
                ev.Set();
            }
            catch
            {
                // ignore
            }
            return;
        }

        try
        {
            ShowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => LogCrash(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash(e.ExceptionObject as Exception);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                LogCrash(ex);
                throw;
            }
        }
        finally
        {
            ShowEvent?.Dispose();
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }

    private static void LogCrash(Exception? ex)
    {
        if (ex is null) return;
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipSaver");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "crash.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\r\n\r\n");
        }
        catch
        {
            // ignore logging errors
        }
    }
}
