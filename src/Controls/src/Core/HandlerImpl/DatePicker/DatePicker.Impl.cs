#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		Font ITextStyle.Font => this.ToFont();
	}
}