using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class BindableObjectExtensionTests : BaseTestFixture
	{
		[Fact]
		public void SetBindingNull()
		{
			Assert.Throws<ArgumentNullException>(() => BindableObjectExtensions.SetBinding(null, MockBindable.TextProperty, "Name"));
			Assert.Throws<ArgumentNullException>(() => BindableObjectExtensions.SetBinding(new MockBindable(), null, "Name"));
			Assert.Throws<ArgumentNullException>(() => BindableObjectExtensions.SetBinding(new MockBindable(), MockBindable.TextProperty, null));
		}

		[Fact]
		public void Issue2643()
		{
			Label labelTempoDiStampa = new Label();
			labelTempoDiStampa.BindingContext = new { Name = "1", Company = "Microsoft.Maui.Controls" };
			labelTempoDiStampa.SetBinding(Label.TextProperty, "Name", stringFormat: "Hi: {0}");

			Assert.Equal("Hi: 1", labelTempoDiStampa.Text);
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

		[Fact]
		public void Bz27229()
		{
			var totalCheckTime = new TextCell { Text = "Total Check Time" };
			totalCheckTime.BindingContext = new Bz27229ViewModel();
			totalCheckTime.SetBinding(TextCell.DetailProperty, "Member.Result.Text");
			Assert.Equal("foo", totalCheckTime.Detail);
		}
	}
}
