namespace Microsoft.Maui
{
	public interface IChildGestureRecognizer : IGestureRecognizer
	{
		public IGestureRecognizer GestureRecognizer { get; set; }
	}
}
