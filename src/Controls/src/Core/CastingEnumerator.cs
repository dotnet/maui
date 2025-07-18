#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	internal sealed class CastingEnumerator<T, TFrom> : IEnumerator<T> where T : class where TFrom : class
	{
		readonly IEnumerator<TFrom> _enumerator;

		bool _disposed;

		public CastingEnumerator(IEnumerator<TFrom> enumerator)
		{
			_enumerator = enumerator ?? throw new System.ArgumentNullException(nameof(enumerator));
		}

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			_enumerator.Dispose();
		}

		object? IEnumerator.Current
		{
			get { return Current; }
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator.Reset();
		}

		public T Current
		{
			get { return (_enumerator.Current as T)!; }
		}
	}
}