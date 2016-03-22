namespace Xamarin.Forms.Loader
{
	public sealed class MissingCategoryOnTestFixtureException : LoaderException
	{
		readonly string fixtureName;

		public MissingCategoryOnTestFixtureException(string fixtureName)
		{
			this.fixtureName = fixtureName;
		}

		public override string Message
		{
			get { return "Missing Category attribute: " + fixtureName; }
		}
	}
}