namespace Microsoft.Maui
{
	/// <summary>
	/// Marker interface for containers (like ListView, TableView, ViewCell) that manage their own
	/// layout and should NOT have safe area insets applied to their content.
	/// Views inside these containers will not receive safe area padding.
	/// </summary>
	public interface ISafeAreaIgnoredContainer
	{
	}
}
