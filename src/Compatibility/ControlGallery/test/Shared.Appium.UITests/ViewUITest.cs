using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public abstract class ViewUITest : UITest
	{
		public ViewUITest(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void Focus()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.Focus);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void UnFocus()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.Unfocus);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void InputTransparent()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.InputTransparent);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public virtual void IsEnabled()
		{
			var remote = new StateViewContainerRemote(App, Test.VisualElement.IsEnabled);
			remote.GoTo();

			var isEnabled = remote.GetStateLabel().GetText();
			ClassicAssert.AreEqual("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().GetText();
			ClassicAssert.AreEqual("False", isDisabled);
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void Opacity()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.Opacity);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void Rotation()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.Rotation);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void RotationX()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.RotationX);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void RotationY()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.RotationY);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void Scale()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.Scale);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void TranslationX()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.TranslationX);
			remote.GoTo();
		}

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		[Category(UITestCategories.ManualReview)]
		public virtual void TranslationY()
		{
			var remote = new ViewContainerRemote(App, Test.VisualElement.TranslationY);
			remote.GoTo();
		}

		protected override void FixtureSetup()
		{
			int retries = 0;
			while (true)
			{
				try
				{
					base.FixtureSetup();
					NavigateToGallery();
					break;
				}
				catch (Exception e)
				{
					TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureSetup threw an exception. Attempt {retries}/{SetupMaxRetries}.{Environment.NewLine}Exception details: {e}");
					if (retries++ < SetupMaxRetries)
					{
						Reset();
					}
					else
					{
						throw;
					}
				}
			}
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();

			try
			{
				this.Back();
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		protected abstract void NavigateToGallery();
	}
}