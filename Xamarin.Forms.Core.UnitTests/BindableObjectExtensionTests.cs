using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BindableObjectExtensionTests : BaseTestFixture
	{
		[Test]
		public void SetBindingNull()
		{
			Assert.That(() => BindableObjectExtensions.SetBinding(null, MockBindable.TextProperty, "Name"),
				Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => BindableObjectExtensions.SetBinding(new MockBindable(), null, "Name"),
				Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => BindableObjectExtensions.SetBinding(new MockBindable(), MockBindable.TextProperty, null),
				Throws.InstanceOf<ArgumentNullException>());

			Assert.That(() => BindableObjectExtensions.SetBinding<MockViewModel>(null, MockBindable.TextProperty, vm => vm.Text),
				Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => BindableObjectExtensions.SetBinding<MockViewModel>(new MockBindable(), null, vm => vm.Text),
				Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => BindableObjectExtensions.SetBinding<MockViewModel>(new MockBindable(), MockBindable.TextProperty, null),
				Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public void Issue2643()
		{
			Label labelTempoDiStampa = new Label();
			labelTempoDiStampa.BindingContext = new { Name = "1", Company = "Xamarin" };
			labelTempoDiStampa.SetBinding(Label.TextProperty, "Name", stringFormat: "Hi: {0}");

			Assert.That(labelTempoDiStampa.Text, Is.EqualTo("Hi: 1"));
		}

		class Bz27229ViewModel
		{
			public object Member { get; private set; }
			public Bz27229ViewModel()
			{
				Member = new Generic<Label>(new Label { Text = "foo" });
			}
		}

		class Generic<TResult>
		{
			public Generic(TResult result)
			{
				Result = result;
			}

			public TResult Result { get; set; }
		}

		[Test]
		public void Bz27229()
		{
			var totalCheckTime = new TextCell { Text = "Total Check Time" };
			totalCheckTime.BindingContext = new Bz27229ViewModel();
			totalCheckTime.SetBinding(TextCell.DetailProperty, "Member.Result.Text");
			Assert.AreEqual("foo", totalCheckTime.Detail);

			totalCheckTime = new TextCell { Text = "Total Check Time" };
			totalCheckTime.BindingContext = new Bz27229ViewModel();
			totalCheckTime.SetBinding<Bz27229ViewModel>(TextCell.DetailProperty, vm =>
				((Generic<Label>)vm.Member).Result.Text);

			Assert.AreEqual("foo", totalCheckTime.Detail);
		}
	}
}