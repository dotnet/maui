#nullable enable
using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Specifies a request for the retrieval of a platform value.
	/// </summary>
	public class RetrievePlatformValueRequest<T>
	{
		bool _hasResult;
		T? _result;

		/// <summary>
		/// The result of the retrieval operation.
		/// </summary>
		public T Result => _hasResult ? _result! : throw new InvalidOperationException("No result value was set.");

		/// <summary>
		/// Sets the result value of the retrieval operation.
		/// </summary>
		/// <param name="result">The result value to set on this request.</param>
		/// <exception cref="InvalidOperationException">The result has already been set.</exception>
		public void SetResult(T result)
		{
			if (_hasResult)
				throw new InvalidOperationException("Request already had a result value set.");

			_result = result;
			_hasResult = true;
		}

		/// <summary>
		/// Attempts to set the result value of the retrieval operation.
		/// </summary>
		/// <param name="result">The result value to set on this request.</param>
		/// <returns>true if the operation was successful; otherwise, false.</returns>
		public bool TrySetResult(T result)
		{
			if (_hasResult)
				return false;

			_result = result;
			_hasResult = true;

			return true;
		}
	}
}