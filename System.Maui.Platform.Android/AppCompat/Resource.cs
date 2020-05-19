using Android.Runtime;

[assembly: ResourceDesigner("Xamarin.Forms.Platform.Android.Resource", IsApplication = false)]

namespace Xamarin.Forms.Platform.Android
{
	public class Resource
	{
		static Resource()
		{
			ResourceIdManager.UpdateIdValues();
		}

		public class Attribute
		{
			// aapt resource value: 0x7f0100a5
			// ReSharper disable once InconsistentNaming
			// Android is pretty insistent about this casing
			public static int actionBarSize = 2130772133;

			static Attribute()
			{
				ResourceIdManager.UpdateIdValues();
			}

			Attribute()
			{
			}
		}

		public class Id
		{
			public static int main_appbar = 2131165336;

			public static int main_backdrop = 2131165338;

			public static int main_collapsing = 2131165337;

			public static int main_scrollview = 2131165341;

			public static int main_tablayout = 2131165340;

			public static int main_toolbar = 2131165339;

			static Id()
			{
				ResourceIdManager.UpdateIdValues();
			}

			private Id()
			{
			}
		}


		public class Layout
		{
			public static int RootLayout = 2130903096;

			static Layout()
			{
				ResourceIdManager.UpdateIdValues();
			}

			private Layout()
			{
			}
		}

	}
}