#nullable enable
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Handlers.Benchmarks;

[MemoryDiagnoser]
public class ElementHandlerUpdateBenchmarks
{
	static readonly string[] Properties = ["First", "Second", "Third", "Fourth"];

	readonly QueuedDispatcher _dispatcher = new();
	BenchmarkHandler _disabledHandler = null!;
	BenchmarkHandler _automaticHandler = null!;
	BenchmarkHandler _explicitHandler = null!;
	ExplicitBatchingButton _explicitView = null!;

	[Params(10, 100, 1000)]
	public int UpdateCount { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_disabledHandler = CreateHandler(new DisabledBatchingButton());
		_automaticHandler = CreateHandler(new AutomaticBatchingButton());
		_explicitView = new ExplicitBatchingButton();
		_explicitHandler = CreateHandler(_explicitView);
	}

	[Benchmark(Baseline = true)]
	public void FeatureDisabledBurst()
	{
		for (var i = 0; i < UpdateCount; i++)
			_disabledHandler.UpdateValue(Properties[i & 3]);
	}

	[Benchmark]
	public void AutomaticBurst()
	{
		for (var i = 0; i < UpdateCount; i++)
			_automaticHandler.UpdateValue(Properties[i & 3]);

		_dispatcher.RunNext();
	}

	[Benchmark]
	public void ExplicitBurst()
	{
		_explicitView.BatchBegin();

		for (var i = 0; i < UpdateCount; i++)
			_explicitHandler.UpdateValue(Properties[i & 3]);

		_explicitView.BatchCommit();
	}

	[Benchmark]
	public void FeatureDisabledSparse() =>
		_disabledHandler.UpdateValue(Properties[0]);

	[Benchmark]
	public void AutomaticSparse()
	{
		_automaticHandler.UpdateValue(Properties[0]);
		_dispatcher.RunNext();
	}

	BenchmarkHandler CreateHandler(BenchmarkButton view)
	{
		var services = new ServiceCollection();
		services.AddSingleton<IDispatcher>(_dispatcher);

		var handler = new BenchmarkHandler();
		handler.SetMauiContext(new MauiContext(services.BuildServiceProvider()));
		handler.SetVirtualView(view);
		_dispatcher.Clear();
		return handler;
	}

	sealed class BenchmarkHandler : ViewHandler<Button, object>
	{
		static readonly PropertyMapper<IView, BenchmarkHandler> Mapper = new()
		{
			["First"] = Map,
			["Second"] = Map,
			["Third"] = Map,
			["Fourth"] = Map,
		};

		public BenchmarkHandler()
			: base(Mapper)
		{
		}

		public int MapCount { get; private set; }

		protected override object CreatePlatformView() =>
			new();

		static void Map(BenchmarkHandler handler, IView view) =>
			handler.MapCount++;
	}

	abstract class BenchmarkButton : Button, IPropertyUpdateBatchingElement
	{
		protected abstract bool IsBatchingEnabled { get; }

		protected abstract bool IsAutomaticBatchingEnabled { get; }

		protected abstract bool IsExplicitlyScoped { get; }

		bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingEnabled => IsBatchingEnabled;

		bool IPropertyUpdateBatchingElement.IsAutomaticPropertyUpdateBatchingEnabled => IsAutomaticBatchingEnabled;

		bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingExplicitlyScoped => IsExplicitlyScoped;
	}

	sealed class DisabledBatchingButton : BenchmarkButton
	{
		protected override bool IsBatchingEnabled => false;

		protected override bool IsAutomaticBatchingEnabled => false;

		protected override bool IsExplicitlyScoped => false;
	}

	sealed class AutomaticBatchingButton : BenchmarkButton
	{
		protected override bool IsBatchingEnabled => true;

		protected override bool IsAutomaticBatchingEnabled => true;

		protected override bool IsExplicitlyScoped => false;
	}

	sealed class ExplicitBatchingButton : BenchmarkButton
	{
		protected override bool IsBatchingEnabled => true;

		protected override bool IsAutomaticBatchingEnabled => false;

		protected override bool IsExplicitlyScoped => Batched;
	}

	sealed class QueuedDispatcher : IDispatcher
	{
		readonly Queue<Action> _pending = new();

		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			_pending.Enqueue(action);
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action) =>
			Dispatch(action);

		public IDispatcherTimer CreateTimer() =>
			throw new NotSupportedException();

		public void RunNext() =>
			_pending.Dequeue().Invoke();

		public void Clear() =>
			_pending.Clear();
	}
}
