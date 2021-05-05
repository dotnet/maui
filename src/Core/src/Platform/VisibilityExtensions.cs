namespace Microsoft.Maui
{
	public static class VisibilityExtensions
	{
		public static Visibility ToVisibility(this bool isVisible)
		{
			if (isVisible)
				return Visibility.Visible;

			return Visibility.Hidden;
		}
	}
}
