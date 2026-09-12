using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	/// <summary>
	/// Measures the property notification path: setting a bindable property with subscribers attached, and raising
	/// a notification by name for something that is not a bindable property.
	/// </summary>
	/// <remarks>
	/// Each benchmark loops internally so the args resolution is measured above the harness noise floor; a single
	/// set is only a handful of nanoseconds and the run-to-run spread swallows the difference.
	/// </remarks>
	[MemoryDiagnoser]
	public class PropertyNotificationBenchmarker
	{
		const int Iterations = 100;

		// A real MAUI BindableObject (Label) rather than a synthetic stand-in, so the
		// benchmark reflects the actual property/notification machinery used in the framework.
		class Notifier : Label
		{
			public void RaiseByName(string propertyName) => OnPropertyChanged(propertyName);
		}

		Notifier _notifier;
		string[] _values;
		string[] _names;
		uint _index;

		[GlobalSetup]
		public void Setup()
		{
			_notifier = new Notifier();
			_notifier.PropertyChanged += (_, _) => { };
			_notifier.PropertyChanging += (_, _) => { };

			// Alternated so every set is a real change and actually raises.
			_values = new[] { "a", "b" };

			// Stands in for the framework's own OnPropertyChanged(nameof(X)) sites and for user code.
			_names = Enumerable.Range(0, 512).Select(i => "PlainProperty" + i).ToArray();
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public void SetBindableProperty()
		{
			for (var i = 0; i < Iterations; i++)
			{
				_notifier.Text = _values[(_index++) & 1];
			}
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public void RaiseByName()
		{
			for (var i = 0; i < Iterations; i++)
			{
				_notifier.RaiseByName(_names[(_index++) & 511]);
			}
		}
	}
}
