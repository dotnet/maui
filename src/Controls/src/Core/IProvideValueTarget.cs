namespace Microsoft.Maui.Controls.Xaml
{
	public interface IProvideValueTarget
	{
		object TargetObject { get; }
		object TargetProperty { get; }
	}
}