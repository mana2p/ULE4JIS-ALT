using System;
using System.Threading;
using System.Windows.Forms;

namespace Ule4JisAlt
{
    internal static class Program
    {
        private const string MutexName = "Local\\ULE4JIS-ALT-SingleInstance-Mutex";

        [STAThread]
        static void Main()
        {
            Mutex? mutex = null;
            bool hasHandle = false;

            try
            {
                try
                {
                    mutex = new Mutex(false, MutexName);
                }
                catch (UnauthorizedAccessException)
                {
                    // 既に管理者権限等で別インスタンスが起動中の場合は静かに終了する
                    return;
                }

                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    // 前回のプロセスが異常終了していた場合はロックを引き継いで起動を続行する
                    hasHandle = true;
                }

                if (!hasHandle)
                {
                    return;
                }

                ApplicationConfiguration.Initialize();
                Application.Run(new TrayApplicationContext());
            }
            finally
            {
                if (mutex != null)
                {
                    if (hasHandle)
                    {
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        catch { }
                    }
                    mutex.Dispose();
                    GC.KeepAlive(mutex);
                }
            }
        }
    }
}
