namespace System.Maui
{
	public interface IExpander : IView
	{
		double Spacing { get; }
		IView Header { get; }
		IView Content { get; }
		bool IsExpanded { get; set; }
	}
}
