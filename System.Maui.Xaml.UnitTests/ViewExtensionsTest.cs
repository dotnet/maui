using NUnit.Framework;
using System;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class ViewExtensionsTest : BaseTestFixture
	{
		[Test]
		public void TestGetResource ()
		{
			var resource = new object ();
			var view = new View {
				Resources = new ResourceDictionary {
					{ "foo", resource }
				},
			};
			var found = view.Resources ["foo"];
			Assert.AreSame (resource, found);
		}

		[Test]
		public void TestResourceNotFound ()
		{
			var view = new View ();
			var resource = ((IResourcesProvider)view).IsResourcesCreated ? view.Resources ["foo"] : null;
			Assert.Null (resource);
		}

		[Test]
		public void TestGetResourceInParents ()
		{
			var resource = new object ();
			var nestedView = new View ();
			var stack = new StackLayout {
				Children = {
					new StackLayout {
						Children = {
							new StackLayout {
								Children = {
									nestedView
								}
							}
						}
					}
				}
			};
			stack.Resources = new ResourceDictionary {
				{ "foo", resource }
			};

			var found = stack.Resources ["foo"];
			Assert.AreSame (resource, found);
		}
	}
}

