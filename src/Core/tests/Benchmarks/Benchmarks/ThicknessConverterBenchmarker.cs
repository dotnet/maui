using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Converters;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class ThicknessConverterBenchmarker
	{
		ThicknessTypeConverter _converter;

		const int Iterations = 1000;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_converter = new();
		}

		[Benchmark]
		public void ConvertFrom()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_converter.ConvertFromInvariantString("1,2,3,4");
				_converter.ConvertFromInvariantString("1, 2,3, 4");
				_converter.ConvertFromInvariantString("1,2 ");
				_converter.ConvertFromInvariantString("1.1,2.2,3.3,4.4");
				_converter.ConvertFromInvariantString("1.1, 2.2,3.3, 4.4");
				_converter.ConvertFromInvariantString("1.1,2.2 ");

				_converter.ConvertFromInvariantString("1 2 3 4");
				_converter.ConvertFromInvariantString("1 2 3 4");
				_converter.ConvertFromInvariantString("1 2 ");
				_converter.ConvertFromInvariantString("1.1 2.2 3.3 4.4");
				_converter.ConvertFromInvariantString("1.1 2.2 3.3 4.4");
				_converter.ConvertFromInvariantString("1.1 2.2");

				try
				{
					// These throw exceptions
					_converter.ConvertFromInvariantString("");
					_converter.ConvertFromInvariantString("1,invalid");
					_converter.ConvertFromInvariantString("1.1 invalid");
					_converter.ConvertFromInvariantString("invalid");
				}
				catch
				{
				}
			}
		}

		[Benchmark]
		public void ConvertTo()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_converter.ConvertToInvariantString(new Thickness());
				_converter.ConvertToInvariantString(new Thickness(0));
				_converter.ConvertToInvariantString(new Thickness(1));
				_converter.ConvertToInvariantString(new Thickness(1.1));
				_converter.ConvertToInvariantString(new Thickness(0, 0));
				_converter.ConvertToInvariantString(new Thickness(1, 2));
				_converter.ConvertToInvariantString(new Thickness(1.1, 2.2));
				_converter.ConvertToInvariantString(new Thickness(0, 0, 0, 0));
				_converter.ConvertToInvariantString(new Thickness(1, 2, 3, 4));
				_converter.ConvertToInvariantString(new Thickness(1.1, 2.2, 3.3, 4.4));
			}
		}
	}
}
