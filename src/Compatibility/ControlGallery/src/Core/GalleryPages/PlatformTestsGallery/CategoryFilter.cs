//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.Internals;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PlatformTestsGallery
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