#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font ITextStyle.Font => this.ToFont();
	}
}