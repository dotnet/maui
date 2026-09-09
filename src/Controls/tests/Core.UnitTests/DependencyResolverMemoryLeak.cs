using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public sealed class DependencyResolverMemoryLeak
{
    const int N = 30;

    [Fact]
    public void DependencyResolver_ResolveUsing_Leaks()
    {
        DependencyResolver.ResetResolver();

        try
        {
            var control = CreateControl();
            var mitigation = CreateMitigation();
            var leaky = CreateLeaky();

            ForceGc();

            var controlAlive = CountAlive(control);
            var leakyAlive = CountAlive(leaky);
            var mitigationAlive = CountAlive(mitigation);

            Assert.Equal(0, controlAlive);
            Assert.Equal(N, leakyAlive);
            Assert.Equal(0, mitigationAlive);
        }
        finally
        {
            DependencyResolver.ResetResolver();
        }
    }

    [Fact]
    public void ResetResolver_ReleasesCapturedTarget()
    {
        DependencyResolver.ResetResolver();

        try
        {
            var leaky = CreateReleasedViaReset();

            ForceGc();

            var leakyAlive = CountAlive(leaky);
            Assert.Equal(0, leakyAlive);
        }
        finally
        {
            DependencyResolver.ResetResolver();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] CreateReleasedViaReset()
    {
        var batch = new ResolverBatch();
        var references = batch.PayloadReferences;
        DependencyResolver.ResolveUsing(batch.Resolve);
        DependencyResolver.ResetResolver();
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] CreateControl()
    {
        var batch = new ResolverBatch();
        return batch.PayloadReferences;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] CreateLeaky()
    {
        var batch = new ResolverBatch();
        var references = batch.PayloadReferences;
        DependencyResolver.ResolveUsing(batch.Resolve);
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] CreateMitigation()
    {
        var batch = new ResolverBatch();
        var references = batch.PayloadReferences;
        DependencyResolver.ResolveUsing(batch.Resolve);
        DependencyResolver.ResolveUsing(static (_, _) => null!);
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ForceGc()
    {
        for (var i = 0; i < 7; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    static int CountAlive(IEnumerable<WeakReference> references) =>
        references.Count(reference => reference.IsAlive);

    sealed class ResolverBatch
    {
        readonly PayloadOwner[] _owners =
            Enumerable.Range(0, N).Select(static _ => new PayloadOwner()).ToArray();

        public WeakReference[] PayloadReferences =>
            _owners.Select(static owner => new WeakReference(owner.Payload)).ToArray();

        public object Resolve(Type type, object[] arguments) => null;
    }

    sealed class PayloadOwner
    {
        public byte[] Payload { get; } = new byte[1024 * 1024];
    }
}
