namespace Xamarin.Forms.UITest.Validator
{
	internal class FormsUiTest
	{
		public FormsUiTest(string memberName, string testName)
		{
			MemberName = memberName;
			TestName = testName;
		}

		public string MemberName { get; private set; }

		public string TestName { get; private set; }
	}
}