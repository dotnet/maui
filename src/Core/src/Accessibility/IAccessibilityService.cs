namespace Microsoft.Maui.Accessibility
{
	interface IAccessibilityService
	{
		/// <summary>
		/// Tell screen reader to read out the text specified
		/// </summary>
		/// <param name="text"></param>
		void SetAnnouncement(string text);
	}
}
