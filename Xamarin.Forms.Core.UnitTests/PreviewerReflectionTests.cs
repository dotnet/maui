using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class PreviewerReflectionTests
	{
		class FakePlatform : IPlatform
		{
			public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void PageHasPlatformProperty()
		{
			var page = new Page();

			var setPlatform = page.GetType().GetProperty("Platform", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.That(setPlatform, Is.Not.Null, "Previewer requires that Page have a property called 'Platform'");

			TestDelegate setValue = () => setPlatform.SetValue(page, new FakePlatform(), null);
			Assert.That(setValue, Throws.Nothing, "'Page.Platform' must have a setter");
		}

		[Test]
		public void RegisterAllExists()
		{
			var type = typeof(Registrar);

			var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			var method = methods.Single(t =>
			{
				var parameters = t.GetParameters();

				return t.Name == "RegisterAll"
						&& parameters.Length == 1
						&& parameters[0].ParameterType == typeof(Type[]);
			});

			Assert.That(method, Is.Not.Null, "Previewer requires that Registrar have a static non-public method "
											+ "'RegisterAll' which takes a single parameter of Type[]");
		}
	}
}