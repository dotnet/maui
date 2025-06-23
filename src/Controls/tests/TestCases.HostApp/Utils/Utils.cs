namespace Maui.Controls.Sample
{
	public static class Utils
	{
		public static T With<T>(this T that, Action<T> action)
		{
			action(that);
			return that;
		}
	}
}
