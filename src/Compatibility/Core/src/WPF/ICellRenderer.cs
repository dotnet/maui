using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public interface ICellRenderer : IRegisterable
	{
		System.Windows.DataTemplate GetTemplate(Cell cell);
	}
}
