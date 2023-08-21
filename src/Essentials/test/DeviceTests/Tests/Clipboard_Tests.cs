// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Clipboard")]
	public class Clipboard_Tests
	{
		[Theory]
		[InlineData("text")]
		[InlineData("some really long test text")]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task Set_Clipboard_Values(string text)
		{
			return Utils.OnMainThread(async () =>
			{
				await Clipboard.SetTextAsync(text);
				Assert.True(Clipboard.HasText);
			});
		}

		[Theory]
		[InlineData("text")]
		[InlineData("some really long test text")]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task Get_Clipboard_Values(string text)
		{
			return Utils.OnMainThread(async () =>
			{
				await Clipboard.SetTextAsync(text);
				var clipText = await Clipboard.GetTextAsync();

				Assert.NotNull(clipText);
				Assert.Equal(text, clipText);
			});
		}
	}
}
