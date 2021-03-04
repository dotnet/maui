namespace Microsoft.Maui
{
	public interface IProgress : IView
	{
		double Progress { get; }
		Color ProgressColor { get; }
	}
}