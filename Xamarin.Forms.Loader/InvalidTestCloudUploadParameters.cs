namespace Xamarin.Forms.Loader
{
	public sealed class InvalidTestCloudUploadParameters : LoaderException
	{
		readonly string parameters;

		public InvalidTestCloudUploadParameters(string parameters)
		{
			this.parameters = parameters;
		}

		public override string Message
		{
			get { return "Missing Category attribute: " + parameters; }
		}
	}
}