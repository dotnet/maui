using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Maps.WPF;

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		static bool s_isInitialized;

		internal static string AuthenticationToken { get; set; }

		public static void Init(string authenticationToken)
		{
			AuthenticationToken = authenticationToken;
			Init();
		}

		public static void Init()
		{
			if (s_isInitialized)
				return;
			GeocoderBackend.Register();
			s_isInitialized = true;
		}
	}
}
