using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PlatformTestsGallery
{
	[Preserve(AllMembers = true)]
	public class CategoryFilter : TestFilter
	{
		string _category;

		public CategoryFilter(string category) => _category = category;

		public override TNode AddToXml(TNode parentNode, bool recursive)
		{
			TNode result = parentNode.AddElement("category", _category);
			return result;
		}

		public override bool Match(ITest test)
		{
			return test.Properties[PropertyNames.Category].Contains(_category);
		}
	}
}