using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class TypeConversionBenchmarker
	{
		XamlServiceProvider _xamlServiceProvider;
		Setter _setter;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_xamlServiceProvider = new XamlServiceProvider();

			_setter = new Setter();
			_setter.Property = Shell.BackgroundColorProperty;
		}

		[Benchmark]
		public object ProvideValue()
		{
			_setter.Value = "Black";
			return ((IValueProvider)_setter).ProvideValue(_xamlServiceProvider);
		}
	}
}
