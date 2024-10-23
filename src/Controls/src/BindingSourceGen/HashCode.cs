using System.ComponentModel;
using System.Runtime.CompilerServices;

// Original source:
// https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Helpers/HashCode.cs

// Mimics System.HashCode, which is missing in NetStandard2.0.
// Placed in root namespace to avoid ambiguous reference with System.HashCode

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal struct HashCode
{
	private int hashCode;

	public void Add<T>(T value)
	{
		hashCode = Hash(hashCode, value);
	}

	public void Add<T>(T value, IEqualityComparer<T> comparer)
	{
		hashCode = Hash(hashCode, value, comparer);
	}

	public readonly int ToHashCode() => hashCode;

	public static int Combine<T1>(T1 value1)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		return hashCode;
	}

	public static int Combine<T1, T2>(T1 value1, T2 value2)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		return hashCode;
	}

	public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		hashCode = Hash(hashCode, value5);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		hashCode = Hash(hashCode, value5);
		hashCode = Hash(hashCode, value6);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		hashCode = Hash(hashCode, value5);
		hashCode = Hash(hashCode, value6);
		hashCode = Hash(hashCode, value7);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		hashCode = Hash(hashCode, value5);
		hashCode = Hash(hashCode, value6);
		hashCode = Hash(hashCode, value7);
		hashCode = Hash(hashCode, value8);
		return hashCode;
	}

	public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7,
		T8 value8, T9 value9, T10 value10)
	{
		int hashCode = 0;
		hashCode = Hash(hashCode, value1);
		hashCode = Hash(hashCode, value2);
		hashCode = Hash(hashCode, value3);
		hashCode = Hash(hashCode, value4);
		hashCode = Hash(hashCode, value5);
		hashCode = Hash(hashCode, value6);
		hashCode = Hash(hashCode, value7);
		hashCode = Hash(hashCode, value8);
		hashCode = Hash(hashCode, value9);
		hashCode = Hash(hashCode, value10);
		return hashCode;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash<T>(int hashCode, T value)
	{
		unchecked
		{
			return (hashCode * 397) ^ (value?.GetHashCode() ?? 0);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash<T>(int hashCode, T value, IEqualityComparer<T> comparer)
	{
		unchecked
		{
			return (hashCode * 397) ^ (value is null ? 0 : (comparer?.GetHashCode(value) ?? value.GetHashCode()));
		}
	}

#pragma warning disable CS0809 // Obsolete member 'HashCode.GetHashCode()' overrides non-obsolete member 'object.GetHashCode()'
	[Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes. Use ToHashCode to retrieve the computed hash code.",
		error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode() => throw new NotSupportedException();

	[Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj) => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member 'HashCode.GetHashCode()' overrides non-obsolete member 'object.GetHashCode()'
}
