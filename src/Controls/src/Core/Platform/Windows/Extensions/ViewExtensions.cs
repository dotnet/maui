using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ViewExtensions
	{
		public static IEnumerable<IPage> GetParentPages(this IView target)
		{
			var result = new List<IPage>();
			var parent = target.Parent as IPage;
			while (!Application.IsApplicationOrNull(parent))
			{
				result.Add(parent);
				parent = parent.Parent as IPage;
			}

			return result;
		}
	}
}