namespace Microsoft.Maui
{
	public class FocusRequest
	{
		public FocusRequest(bool isFocused)
		{
			IsFocused = isFocused;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this request set or not the focus.
		/// </summary>
		public bool IsFocused { get; set; }
	}
}
