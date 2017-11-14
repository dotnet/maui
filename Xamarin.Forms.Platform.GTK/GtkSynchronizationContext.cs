using GLib;
using System;
using System.Threading;

namespace Xamarin.Forms.Platform.GTK
{
    public class GtkSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            Idle.Add(() =>
            {
                d(state);
                return false;
            });
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == FormsWindow.MainThreadID)
            {
                d(state);
            }
            else
            {
                var evt = new ManualResetEvent(false);
                Exception exception = null;

                Idle.Add(() =>
                {
                    try
                    {
                        d(state);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    finally
                    {
                        evt.Set();
                    }
                    return false;
                });

                evt.WaitOne();

                if (exception != null)
                    throw exception;
            }
        }
    }
}