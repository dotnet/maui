using System.Collections.Generic;

namespace Xamarin.Forms.Platform.iOS
{
	public static class ViewExtensions
	{
		public static IEnumerable<Page> GetParentPages(this Page target)
		{
			var result = new List<Page>();
			var parent = target.RealParent as Page;
			while (!Application.IsApplicationOrNull(parent))
			{
				result.Add(parent);
				parent = parent.RealParent as Page;
			}

			return result;
		}
	}
}