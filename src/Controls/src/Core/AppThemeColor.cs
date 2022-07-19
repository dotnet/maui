using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class AppThemeColor
	{
		Color _light;
		Color _dark;
		Color _default;
		bool _isLightSet;
		bool _isDarkSet;
		bool _isDefaultSet;

		public Color Light
		{
			get => _light;
			set
			{
				_light = value;
				_isLightSet = true;
			}
		}

		public Color Dark
		{
			get => _dark;
			set
			{
				_dark = value;
				_isDarkSet = true;
			}
		}

		public Color Default
		{
			get => _default;
			set
			{
				_default = value;
				_isDefaultSet = true;
			}
		}

		internal AppThemeBinding GetBinding()
		{
			var binding = new AppThemeBinding();
			if (_isDarkSet)
				binding.Dark = Dark;
			if (_isLightSet)
				binding.Light = Light;
			if (_isDefaultSet)
				binding.Default = Default;

			return binding;
		}
	}
}
