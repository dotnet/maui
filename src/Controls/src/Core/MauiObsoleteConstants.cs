namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Diagnostic IDs for obsolete MAUI APIs.
	/// These IDs can be used to suppress specific obsolete warnings with <c>#pragma warning disable MAUIXXXX</c>.
	/// When an API is removed in a future release, grep for the corresponding ID to find all suppression sites.
	/// </summary>
	internal static class MauiObsoleteConstants
	{
		/// <summary>
		/// <see cref="VisualElement.BackgroundColor"/> and <see cref="VisualElement.BackgroundColorProperty"/> are obsolete.
		/// Use <see cref="VisualElement.Background"/> instead.
		/// This API will be removed in .NET 12.
		/// </summary>
		public const string BackgroundColorObsolete = "MAUI0003";
	}
}
