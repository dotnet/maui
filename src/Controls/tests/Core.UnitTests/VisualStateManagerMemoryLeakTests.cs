#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class VisualStateManagerMemoryLeakTests : BaseTestFixture
    {
        const int N = 30;
        enum Scenario { Control, Leaky, Mitigation }

        [Fact, Category(TestCategory.Memory)]
        public void VisualStateGroup_States_Clear_Leaks()
        {
            var control = Create(Scenario.Control);
            var leaky = Create(Scenario.Leaky);
            var mitigation = Create(Scenario.Mitigation);
            for (var i = 0; i < 7; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            Assert.Equal(0, Alive(control));
            Assert.Equal(0, Alive(leaky));
            Assert.Equal(0, Alive(mitigation));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static List<WeakReference<byte[]>> Create(Scenario scenario)
        {
            var result = new List<WeakReference<byte[]>>();
            var attach = typeof(StateTriggerBase).GetMethod(
                "SendAttached", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var detach = typeof(StateTriggerBase).GetMethod(
                "SendDetached", BindingFlags.Instance | BindingFlags.NonPublic)!;

            for (var i = 0; i < N; i++)
            {
                var payload = new byte[1024 * 1024];
                result.Add(new(payload));
                if (scenario == Scenario.Control)
                    continue;

                var trigger = new ProbeTrigger(payload);
                var state = new VisualState { Name = "Probe" };
                state.StateTriggers.Add(trigger);
                var group = new VisualStateGroup();
                group.States.Add(state);
                VisualStateManager.SetVisualStateGroups(
                    new Label(), new VisualStateGroupList { group });
                attach.Invoke(trigger, null);
                if (scenario == Scenario.Mitigation)
                    detach.Invoke(trigger, null);
                group.States.Clear();
            }
            return result;
        }

        static int Alive(IEnumerable<WeakReference<byte[]>> items)
            => items.Count(item => item.TryGetTarget(out _));

        sealed class ProbeTrigger(byte[] payload) : StateTriggerBase
        {
            protected override void OnAttached() => Publisher.Changed += Changed;
            protected override void OnDetached() => Publisher.Changed -= Changed;
            void Changed(object? sender, EventArgs e) => GC.KeepAlive(payload);
        }

        static class Publisher
        {
#pragma warning disable CS0414
            public static event EventHandler? Changed;
#pragma warning restore CS0414
        }
    }
}