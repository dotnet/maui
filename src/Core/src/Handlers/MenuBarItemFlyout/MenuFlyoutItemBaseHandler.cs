using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class MenuFlyoutItemBaseHandler<T, TPlatform> : 
		ElementHandler<T, TPlatform>,
		IMenuFlyoutItemBaseHandler<T>
		where T : class, IMenuFlyoutItemBase
		where TPlatform : class
	{
		protected MenuFlyoutItemBaseHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}
	}
}
