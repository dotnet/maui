using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreMotion;
using EventKit;
using Foundation;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		internal static class EventPermission
		{
			internal static PermissionStatus CheckPermissionStatus(EKEntityType entityType)
			{
				var status = EKEventStore.GetAuthorizationStatus(entityType);
				return status switch
				{
					EKAuthorizationStatus.Authorized => PermissionStatus.Granted,
					EKAuthorizationStatus.Denied => PermissionStatus.Denied,
					EKAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			internal static async Task<PermissionStatus> RequestPermissionAsync(EKEntityType entityType)
			{
				var eventStore = new EKEventStore();

				Tuple<bool, NSError> results = null;
#if NET8_0_OR_GREATER
				if (OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17))
				{
					if (entityType == EKEntityType.Reminder)
						results = await eventStore.RequestFullAccessToRemindersAsync();
					if (entityType == EKEntityType.Event)
						results = await eventStore.RequestFullAccessToEventsAsync();
				}
				else
#endif
				{
					results = await eventStore.RequestAccessAsync(entityType);
				}
				return results.Item1 ? PermissionStatus.Granted : PermissionStatus.Denied;
			}
		}

		public partial class CalendarRead : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys
			{
				get
				{
					if (OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17))
					{
						return () => new string[] { "NSCalendarsFullAccessUsageDescription" };
					}
					else
					{
						return () => new string[] { "NSCalendarsUsageDescription" };
					}
				}
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(EventPermission.CheckPermissionStatus(EKEntityType.Event));
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = EventPermission.CheckPermissionStatus(EKEntityType.Event);
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				return EventPermission.RequestPermissionAsync(EKEntityType.Event);
			}
		}

		public partial class CalendarWrite : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys
			{
				get
				{
					if (OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17))
					{
						return () => new string[] { "NSCalendarsWriteOnlyAccessUsageDescription" };
					}
					else
					{
						return () => new string[] { "NSCalendarsUsageDescription" };
					}
				}
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(EventPermission.CheckPermissionStatus(EKEntityType.Event));
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = EventPermission.CheckPermissionStatus(EKEntityType.Event);
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				return EventPermission.RequestPermissionAsync(EKEntityType.Event);
			}
		}

		public partial class Reminders : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys
			{
				get
				{
					if (OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17))
					{
						return () => new string[] { "NSRemindersFullAccessUsageDescription" };
					}
					else
					{
						return () => new string[] { "NSRemindersUsageDescription" };
					}
				}
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(EventPermission.CheckPermissionStatus(EKEntityType.Reminder));
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = EventPermission.CheckPermissionStatus(EKEntityType.Reminder);
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				return EventPermission.RequestPermissionAsync(EKEntityType.Reminder);
			}
		}

		public partial class Sensors : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSMotionUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetSensorPermissionStatus());
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetSensorPermissionStatus();
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return RequestSensorPermission();
			}

			internal static PermissionStatus GetSensorPermissionStatus()
			{
				// Check if it's available
				if (!CMMotionActivityManager.IsActivityAvailable)
					return PermissionStatus.Disabled;

				if (OperatingSystem.IsIOSVersionAtLeast(11, 0) || OperatingSystem.IsWatchOSVersionAtLeast(4, 0))
				{
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
					switch (CMMotionActivityManager.AuthorizationStatus)
					{
						case CMAuthorizationStatus.Authorized:
							return PermissionStatus.Granted;
						case CMAuthorizationStatus.Denied:
							return PermissionStatus.Denied;
						case CMAuthorizationStatus.NotDetermined:
							return PermissionStatus.Unknown;
						case CMAuthorizationStatus.Restricted:
							return PermissionStatus.Restricted;
					}
#pragma warning restore CA1416
				}

				return PermissionStatus.Unknown;
			}

			internal static async Task<PermissionStatus> RequestSensorPermission()
			{
				var activityManager = new CMMotionActivityManager();

				try
				{
					var results = await activityManager.QueryActivityAsync(NSDate.DistantPast, NSDate.DistantFuture, NSOperationQueue.MainQueue);
					if (results != null)
						return PermissionStatus.Granted;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to query activity manager: " + ex.Message);
					return PermissionStatus.Denied;
				}

				return PermissionStatus.Unknown;
			}
		}

		public partial class LocationAlways : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[]
				{
					"NSLocationAlwaysAndWhenInUseUsageDescription",
					"NSLocationAlwaysUsageDescription"
				};

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(LocationWhenInUse.GetLocationStatus(false));
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = LocationWhenInUse.GetLocationStatus(false);
				if (status == PermissionStatus.Granted)
					return status;

				EnsureMainThread();

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
				return await LocationWhenInUse.RequestLocationAsync(false, lm => lm.RequestAlwaysAuthorization());
#pragma warning restore CA1416
			}
		}
	}
}
