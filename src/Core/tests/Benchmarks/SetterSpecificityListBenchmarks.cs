#nullable enable
using System;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Maui.Handlers.Benchmarks;

/// <summary>
/// Head-to-head comparison of SetterSpecificityList implementations:
/// - OldClassSSL: copy of the original class-based, parallel-array + binary-search implementation
/// - NewStructSSL: the new struct-based, inline _top/_second + lazy _rest implementation
///
/// Both use a self-contained copy of SetterSpecificity so the benchmark is standalone.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class SetterSpecificityListBenchmarks
{
	// --- Create contexts and set 2 entries (the 99% hot path) ---

	[Benchmark(Baseline = true)]
	public object OldClass_Create100_Set2()
	{
		OldContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new OldContext();
			ctx.Values[Specificity.DefaultValue] = "default";
			ctx.Values[Specificity.FromHandler] = "handler";
			last = ctx;
		}
		return last!;
	}

	[Benchmark]
	public object NewStruct_Create100_Set2()
	{
		NewContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new NewContext();
			ctx.Values.SetValue(Specificity.DefaultValue, "default");
			ctx.Values.SetValue(Specificity.FromHandler, "handler");
			last = ctx;
		}
		return last!;
	}

	// --- Create contexts and set 3 entries (the < 1% cold path) ---

	[Benchmark]
	public object OldClass_Create100_Set3()
	{
		OldContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new OldContext();
			ctx.Values[Specificity.DefaultValue] = "default";
			ctx.Values[Specificity.Style] = "style";
			ctx.Values[Specificity.ManualValueSetter] = "manual";
			last = ctx;
		}
		return last!;
	}

	[Benchmark]
	public object NewStruct_Create100_Set3()
	{
		NewContext? last = null;
		for (int i = 0; i < 100; i++)
		{
			var ctx = new NewContext();
			ctx.Values.SetValue(Specificity.DefaultValue, "default");
			ctx.Values.SetValue(Specificity.Style, "style");
			ctx.Values.SetValue(Specificity.ManualValueSetter, "manual");
			last = ctx;
		}
		return last!;
	}

	// --- Allocate 1000 empty contexts ---

	[Benchmark]
	public int OldClass_Alloc1000()
	{
		int sum = 0;
		for (int i = 0; i < 1000; i++)
			sum += new OldContext().Values.Count;
		return sum;
	}

	[Benchmark]
	public int NewStruct_Alloc1000()
	{
		int sum = 0;
		for (int i = 0; i < 1000; i++)
			sum += new NewContext().Values.Count;
		return sum;
	}

	// --- Update existing value 1000× ---

	[Benchmark]
	public object OldClass_Update1000()
	{
		var ctx = new OldContext();
		ctx.Values[Specificity.DefaultValue] = "default";
		ctx.Values[Specificity.ManualValueSetter] = "v0";
		for (int i = 0; i < 1000; i++)
			ctx.Values[Specificity.ManualValueSetter] = "v" + i;
		return ctx;
	}

	[Benchmark]
	public object NewStruct_Update1000()
	{
		var ctx = new NewContext();
		ctx.Values.SetValue(Specificity.DefaultValue, "default");
		ctx.Values.SetValue(Specificity.ManualValueSetter, "v0");
		for (int i = 0; i < 1000; i++)
			ctx.Values.SetValue(Specificity.ManualValueSetter, "v" + i);
		return ctx;
	}

	// ==================================================================
	// Minimal SetterSpecificity — just a ulong for ordering
	// ==================================================================
	readonly struct Specificity(ulong value)
	{
		readonly ulong _value = value;
		public static readonly Specificity DefaultValue = new(0);
		public static readonly Specificity FromHandler = new(0xFFFFFFFFFFFFFFFF);
		public static readonly Specificity ManualValueSetter = new(0x0000000010000000);
		public static readonly Specificity Style = new(0x0010000000000000);
		public static bool operator ==(Specificity a, Specificity b) => a._value == b._value;
		public static bool operator !=(Specificity a, Specificity b) => a._value != b._value;
		public static bool operator >(Specificity a, Specificity b) => a._value > b._value;
		public static bool operator <(Specificity a, Specificity b) => a._value < b._value;
		public static bool operator >=(Specificity a, Specificity b) => a._value >= b._value;
		public static bool operator <=(Specificity a, Specificity b) => a._value <= b._value;
		public override bool Equals(object? obj) => obj is Specificity s && s._value == _value;
		public override int GetHashCode() => _value.GetHashCode();
	}

	// ==================================================================
	// Context wrappers
	// ==================================================================
	sealed class OldContext
	{
		public readonly OldClassSSL<object> Values = new(3);
		public readonly OldClassSSL<object> Bindings = new();
	}

	sealed class NewContext
	{
		public NewStructSSL<object> Values;
#pragma warning disable CS0649 // benchmark only uses Values
		public NewStructSSL<object> Bindings;
#pragma warning restore CS0649
	}

	// ==================================================================
	// NEW: struct-based SetterSpecificityList (matches the PR implementation)
	// ==================================================================
	struct NewStructSSL<T>
	{
		Entry _top;
		Entry _second;
		RestList? _rest;
		int _count;

		public readonly int Count => _count;
		public readonly T? GetValue() => _count >= 1 ? _top.Value : default;

		public void SetValue(Specificity key, T value)
		{
			var e = new Entry(value, key);
			if (_count == 0) { _top = e; _count = 1; return; }
			if (_top.Key == key) { _top = e; return; }
			if (_count == 1) { if (key > _top.Key) { _second = _top; _top = e; } else { _second = e; } _count = 2; return; }
			if (_second.Key == key) { _second = e; return; }
			if (key > _top.Key) { Push(_second); _second = _top; _top = e; _count++; return; }
			if (key > _second.Key) { Push(_second); _second = e; _count++; return; }
			_rest ??= new RestList();
			if (_rest.Insert(e)) _count++;
		}

		public void Remove(Specificity key)
		{
			if (_count >= 1 && _top.Key == key) { if (_count == 1) { _top = default; _count = 0; } else if (_count == 2) { _top = _second; _second = default; _count = 1; } else { _top = _second; _second = _rest!.PopLast(); _count--; } }
			else if (_count >= 2 && _second.Key == key) { if (_count == 2) { _second = default; _count = 1; } else { _second = _rest!.PopLast(); _count--; } }
			else if (_count >= 3 && _rest?.Remove(key) == true) { _count--; }
		}

		void Push(Entry entry) { _rest ??= new RestList(); _rest.InsertSorted(entry); }

		struct Entry(T? value, Specificity key) { public T? Value = value; public readonly Specificity Key = key; }

		sealed class RestList
		{
			Entry[] _entries = new Entry[4];
			int _count;
			public bool Insert(Entry e)
			{
				for (int i = 0; i < _count; i++) { if (_entries[i].Key == e.Key) { _entries[i] = e; return false; } }
				InsertSorted(e);
				return true;
			}
			public void InsertSorted(Entry e)
			{
				if (_count == _entries.Length) Array.Resize(ref _entries, _count * 2);
				int i = _count;
				while (i > 0 && _entries[i - 1].Key > e.Key) { _entries[i] = _entries[i - 1]; i--; }
				_entries[i] = e;
				_count++;
			}
			public Entry PopLast() { var e = _entries[--_count]; _entries[_count] = default; return e; }
			public bool Remove(Specificity key)
			{
				for (int i = 0; i < _count; i++)
				{
					if (_entries[i].Key == key)
					{
						Array.Copy(_entries, i + 1, _entries, i, _count - i - 1);
						_entries[--_count] = default;
						return true;
					}
				}
				return false;
			}
		}
	}

	// ==================================================================
	// OLD: class-based SetterSpecificityList (copy of original implementation)
	// ==================================================================
	sealed class OldClassSSL<T> where T : class
	{
		const int Delta = 3;
		Specificity[] _keys;
		T[] _values;
		int _count;

		public int Count => _count;
		public T this[Specificity key] { set => Set(key, value); get => Get(key); }

		public OldClassSSL() { _keys = []; _values = []; }
		public OldClassSSL(int cap)
		{
			if (cap == 0) { _keys = []; _values = []; }
			else { _keys = new Specificity[cap]; _values = new T[cap]; }
		}

		public T GetValue() { var i = _count - 1; return i < 0 ? default! : _values[i]; }

		void Set(Specificity key, T value)
		{
			int lo = 0, hi = _count - 1;
			while (lo <= hi) { int m = lo + ((hi - lo) >> 1); if (_keys[m] == key) { _values[m] = value; return; } if (_keys[m] < key) lo = m + 1; else hi = m - 1; }
			if (_keys.Length == _count) { var nk = new Specificity[_count + Delta]; var nv = new T[_count + Delta]; if (_count > 0) { Array.Copy(_keys, nk, _count); Array.Copy(_values, nv, _count); } _keys = nk; _values = nv; }
			if (_count > lo) { Array.Copy(_keys, lo, _keys, lo + 1, _count - lo); Array.Copy(_values, lo, _values, lo + 1, _count - lo); }
			_keys[lo] = key; _values[lo] = value; _count++;
		}

		T Get(Specificity key)
		{
			int lo = 0, hi = _count - 1;
			while (lo <= hi) { int m = lo + ((hi - lo) >> 1); if (_keys[m] == key) return _values[m]; if (_keys[m] < key) lo = m + 1; else hi = m - 1; }
			return default!;
		}

		public void Remove(Specificity key)
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
