using System.Threading.Tasks;
using Microsoft.Maui.Animations;

using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BasicVisualElement : VisualElement
	{
	}

	public class BasicVisualElementHandler : ViewHandler<BasicVisualElement, object>
	{
		public BasicVisualElementHandler(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override object CreatePlatformView()
		{
			return new object();
		}
	}
}