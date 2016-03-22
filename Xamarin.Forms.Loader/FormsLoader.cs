using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Core.UITests;

namespace Xamarin.Forms.Loader
{
	internal sealed class FormsLoader
	{
		static IEnumerable<FormsType> formsTypes;
		static IEnumerable<TestType> iOSTestTypes;
		static IEnumerable<TestType> androidTestTypes;

		public FormsLoader()
		{
			var formsCoreAssembly = typeof (View).Assembly;
			var iOSUITestAssembly = typeof (iOSLoaderIdentifier).Assembly;
			var androidUITestAssembly = typeof (AndroidLoaderIdentifier).Assembly;

			// skip interfaces, delegates, classes that inherit from attribute
			formsTypes =
				from o in formsCoreAssembly.GetTypes()
				where o.IsVisible && o.IsClass && !o.IsDelegate() && !o.InheritsFromAttribute() && !o.InheritsFromException()
				select new FormsType(this, o);

			iOSTestTypes =
				from o in iOSUITestAssembly.GetTypes()
				where o.IsTestFixture() && o.HasCategoryAttribute()
				select new TestType(this, o);

			androidTestTypes =
				from o in androidUITestAssembly.GetTypes()
				where o.IsTestFixture() && o.HasCategoryAttribute()
				select new TestType(this, o);
		}

		public IEnumerable<FormsType> FormsTypes()
		{
			return formsTypes;
		}

		public IEnumerable<TestType> IOSTestTypes()
		{
			return iOSTestTypes;
		}

		public IEnumerable<TestType> AndroidTestTypes()
		{
			return androidTestTypes;
		}
	}
}