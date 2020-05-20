namespace System.Maui
{
	public interface ICheckBox : IView
	{
		bool IsChecked { get; set; }
		Color Color { get; }
	}
}
