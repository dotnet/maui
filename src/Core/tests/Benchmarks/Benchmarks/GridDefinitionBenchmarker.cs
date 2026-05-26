using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks;

[MemoryDiagnoser]
public class GridDefinitionBenchmarker
{
	private ColumnDefinitionCollectionTypeConverter _columnDefinitionConverter;
	private RowDefinitionCollectionTypeConverter _rowDefinitionConverter;

	private ITypeDescriptorContext _context;
	private ColumnDefinitionCollection _columns;
	private RowDefinitionCollection _rows;

	private const int Iterations = 100;

	[GlobalSetup]
	public void Setup()
	{
		_columnDefinitionConverter = new();
		_rowDefinitionConverter = new();
		_context = null; // replace with actual context if needed
		_columns =
		[
			new ColumnDefinition(new GridLength(1, GridUnitType.Auto)),
			new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
			new ColumnDefinition(new GridLength(3, GridUnitType.Absolute))
		];

		_rows =
		[
			new RowDefinition(new GridLength(1, GridUnitType.Auto)),
			new RowDefinition(new GridLength(2, GridUnitType.Star)),
			new RowDefinition(new GridLength(3, GridUnitType.Absolute))
		];
	}

	[Benchmark]
	public void ColumnConvertFrom()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_columnDefinitionConverter.ConvertFromInvariantString(_context, "Auto, 2*, 3");
		}
	}

	[Benchmark]
	public void ColumnConvertTo()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_columnDefinitionConverter.ConvertToInvariantString(_context, _columns);
		}
	}

	[Benchmark]
	public void RowConvertFrom()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_rowDefinitionConverter.ConvertFromInvariantString(_context, "Auto, 2*, 3");
		}
	}

	[Benchmark]
	public void RowConvertTo()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_rowDefinitionConverter.ConvertToInvariantString(_context, _rows);
		}
	}
}
