using System;
using System.Collections.Generic;
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
		NotifyingObject SourceTypedBinding2;
		NotifyingObject SourceSourceGenBinding;
		MyObject TargetSetValue;
		MyObject TargetBinding;
		MyObject TargetTypedBinding;
		MyObject TargetTypedBinding2;
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

			SourceTypedBinding2 = new NotifyingObject { Name = "Initial" };
			TargetTypedBinding2 = new MyObject();
			TargetTypedBinding2.SetBinding(MyObject.NameProperty, new TypedBinding<NotifyingObject, string>(
				o => (o.Name, true),
				null,
				handlersCount: 1,
				handlers: static o => GetHandlers(o)
			)
			{ Source = SourceTypedBinding2 });

			static IEnumerable<ValueTuple<INotifyPropertyChanged, string>> GetHandlers(NotifyingObject o)
			{
				yield return (o, "Name");
			}

			SourceSourceGenBinding = new NotifyingObject { Name = "Initial" };
			TargetSourceGenBinding = new MyObject();
			TargetSourceGenBinding.SetBinding(MyObject.NameProperty, static (NotifyingObject o) => o.Name, source: SourceSourceGenBinding, mode: BindingMode.OneWay);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			DispatcherProvider.SetCurrent(null);
		}

		static int _counter;

		[Benchmark(Baseline = true)]
		public void SetValue()
		{
			TargetSetValue.SetValue(MyObject.NameProperty, (++_counter).ToString());
		}

		[Benchmark]
		public void Binding()
		{
			SourceBinding.Name = (++_counter).ToString();
		}

		[Benchmark]
		public void TypedBinding()
		{
			SourceTypedBinding.Name = (++_counter).ToString();
		}

		[Benchmark]
		public void TypedBinding2()
		{
			SourceTypedBinding2.Name = (++_counter).ToString();
		}

		[Benchmark]
		public void SourceGeneratedBinding()
		{
			SourceSourceGenBinding.Name = (++_counter).ToString();
		}
	}
}
