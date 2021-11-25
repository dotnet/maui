using AAttribute = Android.Resource.Attribute;

namespace Microsoft.Maui
{
	internal static class ColorStates
	{
		public static readonly int[][] Default =
		{
			new int[] { },
		};

		public static readonly int[][] EditText =
		{
			new[] {  AAttribute.StateEnabled },
			new[] { -AAttribute.StateEnabled },
		};

		public static readonly int[][] CheckBox =
		{
			new int[] {  AAttribute.StateEnabled,  AAttribute.StateChecked },
			new int[] {  AAttribute.StateEnabled, -AAttribute.StateChecked },
			new int[] { -AAttribute.StateEnabled,  AAttribute.StateChecked },
			new int[] { -AAttribute.StateEnabled, -AAttribute.StatePressed },
		};

		public static readonly int[][] Switch =
		{
			new int[] { -AAttribute.StateEnabled },
			new int[] {  AAttribute.StateChecked },
			new int[] { },
		};

		public static readonly int[][] Button =
		{
			new int[] { AAttribute.StateEnabled },
			new int[] {-AAttribute.StateEnabled },
			new int[] {-AAttribute.StateChecked },
			new int[] { AAttribute.StatePressed }
		};
	}
}