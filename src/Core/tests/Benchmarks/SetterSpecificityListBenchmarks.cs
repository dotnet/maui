#nullable enable
using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks;

/// <summary>
/// Head-to-head comparison of SetterSpecificityList implementations:
/// - Legacy: copy of the original class-based, parallel-array + binary-search implementation
/// - Current: the real struct-based SetterSpecificityList from the project
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class SetterSpecificityListBenchmarks
{
	static readonly SetterSpecificity DefaultValue = default;
	static readonly SetterSpecificity FromHandler = SetterSpecificity.FromHandler;
	static readonly SetterSpecificity ManualValueSetter = new(0, 0, 0, 1, 0, 0, 0, 0);
	static readonly SetterSpecificity Style = new(0, 1, 0, 0, 0, 0, 0, 0);

	// --- Create contexts and set 2 entries (the 99% hot path) ---

	[Benchmark(Baseline = true)]
	public object Legacy_Create100_Set2()
	{
		LegacyContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new LegacyContext();
			ctx.Values[DefaultValue] = "default";
			ctx.Values[FromHandler] = "handler";
			last = ctx;
		}
		return last!;
	}

	[Benchmark]
	public object Current_Create100_Set2()
	{
		CurrentContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new CurrentContext();
			ctx.Values.SetValue(DefaultValue, "default");
			ctx.Values.SetValue(FromHandler, "handler");
			last = ctx;
		}
		return last!;
	}

	// --- Create contexts and set 3 entries (the < 1% cold path) ---

	[Benchmark]
	public object Legacy_Create100_Set3()
	{
		LegacyContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new LegacyContext();
			ctx.Values[DefaultValue] = "default";
			ctx.Values[Style] = "style";
			ctx.Values[ManualValueSetter] = "manual";
			last = ctx;
		}
		return last!;
	}

	[Benchmark]
	public object Current_Create100_Set3()
	{
		CurrentContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new CurrentContext();
			ctx.Values.SetValue(DefaultValue, "default");
			ctx.Values.SetValue(Style, "style");
			ctx.Values.SetValue(ManualValueSetter, "manual");
			last = ctx;
		}
		return last!;
	}

	// --- Allocate 1000 empty contexts ---

	[Benchmark]
	public int Legacy_Alloc1000()
	{
		int sum = 0;
		for (int i = 0; i < 1000; i++)
			sum += new LegacyContext().Values.Count;
		return sum;
	}

	[Benchmark]
	public int Current_Alloc1000()
	{
		int sum = 0;
		for (int i = 0; i < 1000; i++)
			sum += new CurrentContext().Values.Count;
		return sum;
	}

	// --- Update existing value 1000× ---

	[Benchmark]
	public object Legacy_Update1000()
	{
		var ctx = new LegacyContext();
		ctx.Values[DefaultValue] = "default";
		ctx.Values[ManualValueSetter] = "v0";
		for (int i = 0; i < 1000; i++)
			ctx.Values[ManualValueSetter] = "v" + i;
		return ctx;
	}

	[Benchmark]
	public object Current_Update1000()
	{
		var ctx = new CurrentContext();
		ctx.Values.SetValue(DefaultValue, "default");
		ctx.Values.SetValue(ManualValueSetter, "v0");
		for (int i = 0; i < 1000; i++)
			ctx.Values.SetValue(ManualValueSetter, "v" + i);
		return ctx;
	}

	// ==================================================================
	// Context wrappers
	// ==================================================================

	sealed class LegacyContext
	{
		public readonly LegacySetterSpecificityList<object> Values = new(3);
	}

	sealed class CurrentContext
	{
		public SetterSpecificityList<object> Values;
	}

	// ==================================================================
	// Legacy: copy of the original class-based SetterSpecificityList
	// ==================================================================
	sealed class LegacySetterSpecificityList<T> where T : class
	{
		const int Delta = 3;
		SetterSpecificity[] _keys;
		T[] _values;
		int _count;

		public int Count => _count;
		public T this[SetterSpecificity key] { set => Set(key, value); get => Get(key); }

		public LegacySetterSpecificityList() { _keys = []; _values = []; }
		public LegacySetterSpecificityList(int cap)
		{
			if (cap == 0) { _keys = []; _values = []; }
			else { _keys = new SetterSpecificity[cap]; _values = new T[cap]; }
		}

		public T GetValue() { var i = _count - 1; return i < 0 ? default! : _values[i]; }

		void Set(SetterSpecificity key, T value)
		{
			int lo = 0, hi = _count - 1;
			while (lo <= hi) { int m = lo + ((hi - lo) >> 1); if (_keys[m] == key) { _values[m] = value; return; } if (_keys[m] < key) lo = m + 1; else hi = m - 1; }
			if (_keys.Length == _count) { var nk = new SetterSpecificity[_count + Delta]; var nv = new T[_count + Delta]; if (_count > 0) { Array.Copy(_keys, nk, _count); Array.Copy(_values, nv, _count); } _keys = nk; _values = nv; }
			if (_count > lo) { Array.Copy(_keys, lo, _keys, lo + 1, _count - lo); Array.Copy(_values, lo, _values, lo + 1, _count - lo); }
			_keys[lo] = key; _values[lo] = value; _count++;
		}

		T Get(SetterSpecificity key)
		{
			int lo = 0, hi = _count - 1;
			while (lo <= hi) { int m = lo + ((hi - lo) >> 1); if (_keys[m] == key) return _values[m]; if (_keys[m] < key) lo = m + 1; else hi = m - 1; }
			return default!;
		}

		public void Remove(SetterSpecificity key)
		{
			int lo = 0, hi = _count - 1;
			while (lo <= hi)
			{
				int m = lo + ((hi - lo) >> 1);
				if (_keys[m] == key) { int n = m + 1; if (n < _count) { Array.Copy(_keys, n, _keys, m, _count - n); Array.Copy(_values, n, _values, m, _count - n); _values[_count - 1] = null!; } else { _values[m] = null!; } _count--; return; }
				if (_keys[m] < key) lo = m + 1; else hi = m - 1;
			}
		}
	}
}
