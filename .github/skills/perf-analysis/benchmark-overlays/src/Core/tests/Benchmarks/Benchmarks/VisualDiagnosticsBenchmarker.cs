#nullable enable
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Benchmarks;

[MemoryDiagnoser]
public class VisualDiagnosticsBenchmarker
{
	readonly VisualElementStub _parent = new();
	readonly VisualElementStub _child = new();
	int _notificationCount;
	bool _diagnosticsInitiallyEnabled;

	[GlobalSetup]
	public void Setup()
	{
		_diagnosticsInitiallyEnabled = RuntimeFeature.EnableMauiDiagnostics;
		RuntimeFeature.EnableMauiDiagnostics = true;
		VisualDiagnostics.VisualTreeChanged += OnVisualTreeChanged;
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		VisualDiagnostics.VisualTreeChanged -= OnVisualTreeChanged;
		RuntimeFeature.EnableMauiDiagnostics = _diagnosticsInitiallyEnabled;
	}

	[Benchmark(OperationsPerInvoke = 1000)]
	public int DispatchVisualTreeChanged()
	{
		for (var i = 0; i < 1000; i++)
			VisualDiagnostics.OnChildAdded(_parent, _child, 0);

		return _notificationCount;
	}

	void OnVisualTreeChanged(object? sender, VisualTreeChangeEventArgs e)
	{
		_notificationCount++;
	}

	sealed class VisualElementStub : IVisualTreeElement
	{
		public IReadOnlyList<IVisualTreeElement> GetVisualChildren() => Array.Empty<IVisualTreeElement>();

		public IVisualTreeElement? GetVisualParent() => null;
	}
}
