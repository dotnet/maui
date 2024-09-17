using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.BindingSourceGen;

// Original source:
// https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm.SourceGenerators/Helpers/EquatableArray%7BT%7D.cs

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
	where T : IEquatable<T>
{
	private readonly T[]? array;

	private EquatableArray(ImmutableArray<T> array)
	{
		this.array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);
	}

	public EquatableArray(T[] array) : this(array.ToImmutableArray())
	{
	}

	public ref readonly T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref AsImmutableArray().ItemRef(index);
	}

	public int Length
	{
		get => array?.Length ?? 0;
	}

	public T Last()
	{
		if (array is null || array.Length == 0)
		{
			throw new InvalidOperationException("Array is empty.");
		}

		return array[array.Length - 1];
	}
	public bool Equals(EquatableArray<T> array)
	{
		return AsSpan().SequenceEqual(array.AsSpan());
	}

	public override bool Equals(object? obj)
	{
		return obj is EquatableArray<T> array && Equals(this, array);
	}

	public override int GetHashCode()
	{
		if (this.array is not T[] array)
		{
			return 0;
		}

		HashCode hashCode = default;

		foreach (T item in array)
		{
			hashCode.Add(item);
		}

		return hashCode.ToHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ImmutableArray<T> AsImmutableArray()
	{
		return Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in this.array));
	}

	public ReadOnlySpan<T> AsSpan()
	{
		return AsImmutableArray().AsSpan();
	}

	public T[] ToArray()
	{
		return AsImmutableArray().ToArray();
	}

	public ImmutableArray<T>.Enumerator GetEnumerator()
	{
		return AsImmutableArray().GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)AsImmutableArray()).GetEnumerator();
	}

	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
	{
		return !left.Equals(right);
	}
}
