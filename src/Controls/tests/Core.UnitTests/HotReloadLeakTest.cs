using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.HotReload;
using Xunit;

public sealed class HotReloadLeakTest
{
    const int N = 30;

    static readonly BindableProperty PayloadProperty =
        BindableProperty.CreateAttached("Payload", typeof(byte[]), typeof(HotReloadLeakTest), null);

    enum Mode
    {
        Control,
        Leaky,
        Mitigation
    }

    [Fact]
    public void MauiHotReloadHelper_CurrentViews_Leaks()
    {
        var wasEnabled = MauiHotReloadHelper.IsEnabled;

        try
        {
            var control = Create(Mode.Control);
            var leaky = Create(Mode.Leaky);
            var mitigation = Create(Mode.Mitigation);

            ForceGc();

            var controlAlive = Alive(control);
            var mitigationAlive = Alive(mitigation);
            var leakyAlive = Alive(leaky);

            Assert.Equal(0, controlAlive);
            Assert.Equal(0, mitigationAlive);
            Assert.Equal(0, leakyAlive);
        }
        finally
        {
            MauiHotReloadHelper.IsEnabled = wasEnabled;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] Create(Mode mode)
    {
        var references = new WeakReference[N];
        MauiHotReloadHelper.IsEnabled = true;

        for (var i = 0; i < N; i++)
        {
            var payload = new byte[1024 * 1024];
            var view = new ContentView();
            view.SetValue(PayloadProperty, payload);

            if (mode != Mode.Control)
                MauiHotReloadHelper.Register(view);

            if (mode == Mode.Leaky)
            {
                MauiHotReloadHelper.IsEnabled = false;
                MauiHotReloadHelper.UnRegister(view); // Returns without removing the view.
            }
            else if (mode == Mode.Mitigation)
            {
                MauiHotReloadHelper.UnRegister(view);
            }

            references[i] = new WeakReference(payload);
            MauiHotReloadHelper.IsEnabled = true;
        }

        return references;
    }

    static int Alive(WeakReference[] references) =>
        references.Count(reference => reference.IsAlive);

    static void ForceGc()
    {
        for (var i = 0; i < 7; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
