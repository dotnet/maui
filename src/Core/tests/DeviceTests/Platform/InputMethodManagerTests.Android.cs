using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public class InputMethodManagerTests : CoreHandlerTestBase
	{
		[Theory]
		[InlineData("Entry")]
		[InlineData("Search")]
		[InlineData("Hide")]
		[InlineData("Show")]
		public Task InputMethodServiceRemainsUsable(string operation) =>
			InvokeOnMainThreadAsync(() =>
			{
				using var editText = new AppCompatEditText(Application.Context);
				var service = editText.Context.GetSystemService(Context.InputMethodService);
				Assert.NotNull(service);
				Assert.True(service.PeerReference.IsValid);

				switch (operation)
				{
					case "Entry":
						editText.UpdateReturnType(new EntryStub());
						break;
					case "Search":
						editText.UpdateReturnType(new SearchBarStub());
						break;
					case "Hide":
						editText.HideSoftInput();
						break;
					case "Show":
						editText.ShowSoftInput();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(operation));
				}

				Assert.True(service.PeerReference.IsValid);
			});
	}
}
