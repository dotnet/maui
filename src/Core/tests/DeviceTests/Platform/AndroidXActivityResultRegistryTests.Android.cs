using AndroidX.Activity;
using Xunit;
using JavaClass = Java.Lang.Class;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Application)]
	public class AndroidXActivityResultRegistryTests
	{
		const string ExpectedActivityResultRegistryKey = "android:support:activity-result";

		[Fact]
		public void ComponentActivity_ActivityResultSavedStateKey_MatchesMauiRecoveryKey()
		{
			using var componentActivityClass = JavaClass.FromType(typeof(ComponentActivity));
			using var activityResultTagField = componentActivityClass.GetDeclaredField("ACTIVITY_RESULT_TAG");
			activityResultTagField.Accessible = true;

			var activityResultTag = activityResultTagField.Get(null)?.ToString();

			Assert.Equal(ExpectedActivityResultRegistryKey, activityResultTag);
		}
	}
}
