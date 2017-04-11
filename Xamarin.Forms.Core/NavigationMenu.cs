using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms.Internals
{
	// Mark as internal until renderers are ready for release after 1.0
	[RenderWith(typeof(_NavigationMenuRenderer))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationMenu : View, INavigationMenuController, IElementConfiguration<NavigationMenu>
	{
		readonly List<Page> _targets = new List<Page>();

		readonly Lazy<PlatformConfigurationRegistry<NavigationMenu>> _platformConfigurationRegistry;

		public NavigationMenu()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NavigationMenu>>(() => new PlatformConfigurationRegistry<NavigationMenu>(this));
		}

		public IEnumerable<Page> Targets
		{
			get { return _targets; }
			set
			{
				if (_targets.AsEnumerable().SequenceEqual(value))
					return;

				foreach (Page page in value)
				{
					VerifyTarget(page);
				}

				OnPropertyChanging();
				_targets.Clear();
				_targets.AddRange(value);
				OnPropertyChanged();
			}
		}

		public void Add(Page target)
		{
			if (_targets.Contains(target))
				return;
			VerifyTarget(target);

			OnPropertyChanging("Targets");
			_targets.Add(target);
			OnPropertyChanged("Targets");
		}

		public void Remove(Page target)
		{
			if (_targets.Contains(target))
			{
				OnPropertyChanging("Targets");
				if (_targets.Remove(target))
					OnPropertyChanged("Targets");
			}
		}

		public IPlatformElementConfiguration<T, NavigationMenu> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendTargetSelected(Page target)
		{
			Navigation.PushAsync(target);
		}

		void VerifyTarget(Page target)
		{
			if (target.Icon == null || string.IsNullOrWhiteSpace(target.Icon.File))
				throw new Exception("Icon must be set for each page before adding them to a Navigation Menu");
		}
	}
}