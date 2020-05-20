namespace System.Maui
{
	public interface IProgress : IView
	{
		double Progress { get; }
		Color ProgressColor { get; }
	}
}