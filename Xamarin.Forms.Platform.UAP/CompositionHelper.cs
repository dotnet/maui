using System.Reflection;
using Windows.UI.Composition;

namespace Xamarin.Forms.Platform.UWP
{
	static class CompositionHelper
	{
		static bool SetTypePresent;
		static bool IsTypePresent;

		public static bool IsCompositionGeometryTypePresent
		{
			get
			{
				if (!SetTypePresent)
				{
					MethodInfo methodInfo = typeof(Compositor).GetMethod("CreateEllipseGeometry");
					IsTypePresent = methodInfo != null;
					SetTypePresent = true;
				}

				return IsTypePresent;
			}
		}
	}
}
