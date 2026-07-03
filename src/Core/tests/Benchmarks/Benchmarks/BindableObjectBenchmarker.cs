using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class BindableObjectBenchmarker
	{
		BindableProperty[] _properties;

		[Params(1, 3, 8, 15, 30, 50)]
		public int PropertiesToSet { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			_properties = Enumerable.Range(0, PropertiesToSet)
				.Select(i => BindableProperty.Create($"Property{i}", typeof(int), typeof(BindableObject), -1))
				.ToArray();
		}
		
		private class Bindable : BindableObject {}

		[Benchmark]
		public void SetsAndReadsProperties()
		{
			var bindable = new Bindable();

			var count = _properties.Length;
			for (int i = 0; i < count; i++)
			{
				bindable.SetValue(_properties[i], i);
				_ = bindable.GetValue(_properties[i]);
			}
		}
	}
}