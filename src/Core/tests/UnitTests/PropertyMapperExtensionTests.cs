using System;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.PropertyMapping)]
	public class PropertyMapperExtensionTests
	{
		[Fact]
		public void AddAfterMapping()
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.AppendToMapping(nameof(IView.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			Assert.True(originalIndex < additionalIndex);
		}

		[Fact]
		public void AddBeforeMapping()
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.PrependToMapping(nameof(IView.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			Assert.True(additionalIndex < originalIndex);
		}

		[Fact]
		public void ModifyMapping()
		{
			string log = string.Empty;

			var msg1 = "original";
			var msg2 = "modification";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.ModifyMapping(nameof(IView.Background), (h, v, a) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.DoesNotContain(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);
		}
	}
}
