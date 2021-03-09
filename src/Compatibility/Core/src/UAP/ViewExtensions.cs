using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class ViewExtensions
	{
		public static IEnumerable<Page> GetParentPages(this Page target)
		{
			var result = new List<Page>();
			var parent = target.Parent as Page;
			while (!Application.IsApplicationOrNull(parent))
			{
				result.Add(parent);
				parent = parent.Parent as Page;
			}

			return result;
		}
	}
}