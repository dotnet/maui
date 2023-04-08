using System;

namespace Microsoft.Maui
{
	public class FocusRequest : RetrievePlatformValueRequest<bool>
	{
		public FocusRequest()
		{
		}

		[Obsolete("Use FocusRequest() instead.")]
		public FocusRequest(bool isFocused)
		{
			if (isFocused)
				IsFocused = isFocused;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this request set or not the focus.
		/// </summary>
		[Obsolete("Use SetResult(bool) or TrySetResult(bool) instead.")]
		public bool IsFocused
		{
			get => Result;
			set => SetResult(value);
		}
	}
}
