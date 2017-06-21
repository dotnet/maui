using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
    [TestFixture]
	public class TemplatedViewUnitTests : BaseTestFixture 
	{
        [Test]
        public void TemplatedView_should_have_the_InternalChildren_correctly_when_ControlTemplate_changed()
        {
            var sut = new TemplatedView();
            IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
            internalChildren.Add(new VisualElement());
            internalChildren.Add(new VisualElement());
            internalChildren.Add(new VisualElement());

            sut.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

            Assert.AreEqual(1, internalChildren.Count);
            Assert.IsInstanceOf<ExpectedView>(internalChildren[0]);
        }

        private class ExpectedView : View
        {
            public ExpectedView()
            {
            }
        }

		public class MyTemplate : StackLayout
		{
			public MyTemplate()
			{
				Children.Add(new ContentPresenter());
			}
		}

		[Test]
		public void BindingsShouldBeAppliedOnTemplateChange()
		{
			var template0 = new ControlTemplate(typeof(MyTemplate));
			var template1 = new ControlTemplate(typeof(MyTemplate));
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			var cv = new ContentView {
				ControlTemplate = template0,
				Content = label
			};
			cv.BindingContext = "Foo";

			Assume.That(label.Text, Is.EqualTo("Foo"));
			cv.ControlTemplate = template1;
			Assert.That(label.Text, Is.EqualTo("Foo"));
		}
	}
}