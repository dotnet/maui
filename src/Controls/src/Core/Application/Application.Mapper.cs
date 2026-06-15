#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(Application)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.Application legacy behaviors
#if ANDROID
				// There is also a mapper on Window for this property since this property is relevant at the window level for
				// Android not the application level
				ApplicationHandler.Mapper.ReplaceMapping<Application, ApplicationHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif
			}
		}
	}
}
