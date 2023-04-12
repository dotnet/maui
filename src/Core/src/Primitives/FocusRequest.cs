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
		public bool IsFocused
		{
			[Obsolete("Use Result instead.")]
			get => Result;
			[Obsolete("Use SetResult(bool) or TrySetResult(bool) instead.")]
			set => SetResult(value);
		}
	}
}
