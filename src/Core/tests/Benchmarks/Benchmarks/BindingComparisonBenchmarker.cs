using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class BindingComparisonBenchmarker
	{
		const int Iterations = 100;

		public class NotifyingObject : INotifyPropertyChanged
		{
			string _name = "Initial";
			public string Name
			{
				get => _name;
				set { _name = value; OnPropertyChanged(); }
			}

			public event PropertyChangedEventHandler PropertyChanged;
			void OnPropertyChanged([CallerMemberName] string name = null) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public class MyObject : BindableObject
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create(nameof(Name), typeof(string), typeof(MyObject));

			public string Name
			{
				get => (string)GetValue(NameProperty);
				set => SetValue(NameProperty, value);
			}
		}

		class BenchmarkDispatcher : IDispatcher
		{
			public bool IsDispatchRequired => false;
			public int ManagedThreadId => Environment.CurrentManagedThreadId;
			public bool Dispatch(Action action) { action(); return true; }
			public bool DispatchDelayed(TimeSpan delay, Action action) { action(); return true; }
			public IDispatcherTimer CreateTimer() => throw new NotImplementedException();
		}

		class BenchmarkDispatcherProvider : IDispatcherProvider
		{
			readonly BenchmarkDispatcher _dispatcher = new();
			public IDispatcher GetForCurrentThread() => _dispatcher;
		}

		NotifyingObject SourceBinding;
		NotifyingObject SourceTypedBinding;
		NotifyingObject SourceSourceGenBinding;
		MyObject TargetSetValue;
		MyObject TargetBinding;
		MyObject TargetTypedBinding;
		MyObject TargetSourceGenBinding;

		[GlobalSetup]
		public void Setup()
		{
			DispatcherProvider.SetCurrent(new BenchmarkDispatcherProvider());

			TargetSetValue = new MyObject();

			SourceBinding = new NotifyingObject { Name = "Initial" };
			TargetBinding = new MyObject();
			TargetBinding.SetBinding(MyObject.NameProperty, new Microsoft.Maui.Controls.Binding("Name", source: SourceBinding));

			SourceTypedBinding = new NotifyingObject { Name = "Initial" };
			TargetTypedBinding = new MyObject();
			TargetTypedBinding.SetBinding(MyObject.NameProperty, new TypedBinding<NotifyingObject, string>(
				o => (o.Name, true),
				null,
				handlers: new[] { Tuple.Create<Func<NotifyingObject, object>, string>(o => o, "Name") }
			)
			{ Source = SourceTypedBinding });

			SourceSourceGenBinding = new NotifyingObject { Name = "Initial" };
			TargetSourceGenBinding = new MyObject();
			TargetSourceGenBinding.SetBinding(MyObject.NameProperty, static (NotifyingObject o) => o.Name, source: SourceSourceGenBinding, mode: BindingMode.OneWay);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			DispatcherProvider.SetCurrent(null);
		}

		[Benchmark(Baseline = true)]
		public void SetValue()
		{
			for (int i = 0; i < Iterations; i++)
			{
				TargetSetValue.SetValue(MyObject.NameProperty, $"Value{i}");
			}
		}

		[Benchmark]
		public void Binding()
		{
			for (int i = 0; i < Iterations; i++)
			{
				SourceBinding.Name = $"Value{i}";
			}
		}

		[Benchmark]
		public void TypedBinding()
		{
			for (int i = 0; i < Iterations; i++)
			{
				SourceTypedBinding.Name = $"Value{i}";
			}
		}

		[Benchmark]
		public void SourceGeneratedBinding()
		{
			for (int i = 0; i < Iterations; i++)
			{
				SourceSourceGenBinding.Name = $"Value{i}";
			}
		}
	}
}
