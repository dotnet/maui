using System;
using ElmSharp;
using Tizen.NET.MaterialComponents;

namespace Xamarin.Forms.Platform.Tizen
{
	public class MaterialShellTabs : MTabs, IShellTabs
	{
		public MaterialShellTabs(EvasObject parent) : base(parent)
		{
		}

		ShellTabsType IShellTabs.Type
		{
			get => (ShellTabsType)Type;
			set
			{
				Type = (MTabsType)value;
			}
		}

		public EvasObject TargetView
		{
			get
			{
				return this;
			}
		}
	}
}
