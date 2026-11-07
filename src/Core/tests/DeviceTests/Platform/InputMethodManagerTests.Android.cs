using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public class InputMethodManagerTests : TestBase
	{
		[Theory]
		[InlineData("Entry")]
		[InlineData("SearchBar")]
		[InlineData("HideSoftInput")]
		[InlineData("ShowSoftInput")]
		public async Task KeyboardOperationsPreserveBorrowedService(string operation)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var context = MauiProgram.DefaultContext;
				var manager = context.GetSystemService(Context.InputMethodService).JavaCast<InputMethodManager>();
				Assert.NotNull(manager);
				var handle = manager.Handle;
				Assert.NotEqual(IntPtr.Zero, handle);
				using var editText = new EditText(context);

				switch (operation)
				{
					case "Entry":
						editText.UpdateReturnType(new EntryStub { ReturnType = ReturnType.Done });
						break;
					case "SearchBar":
						editText.UpdateReturnType(new SearchBarStub { ReturnType = ReturnType.Search });
						break;
					case "HideSoftInput":
						editText.HideSoftInput();
						break;
					case "ShowSoftInput":
						editText.ShowSoftInput();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(operation));
				}

				Assert.Equal(handle, manager.Handle);
				_ = manager.IsActive;
			});
		}
	}
}
