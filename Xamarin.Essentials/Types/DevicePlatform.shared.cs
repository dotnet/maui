using System;

namespace Xamarin.Essentials
{
    public readonly struct DevicePlatform : IEquatable<DevicePlatform>
    {
        readonly string deviceIdiom;

        public static DevicePlatform Android { get; } = new DevicePlatform(nameof(Android));

        public static DevicePlatform iOS { get; } = new DevicePlatform(nameof(iOS));

        public static DevicePlatform UWP { get; } = new DevicePlatform(nameof(UWP));

        public static DevicePlatform Unknown { get; } = new DevicePlatform(nameof(Unknown));

        DevicePlatform(string deviceIdiom)
        {
            if (deviceIdiom == null)
                throw new ArgumentNullException(nameof(deviceIdiom));

            if (deviceIdiom.Length == 0)
                throw new ArgumentException(nameof(deviceIdiom));

            this.deviceIdiom = deviceIdiom;
        }

        public static DevicePlatform Create(string deviceIdiom) =>
            new DevicePlatform(deviceIdiom);

        public bool Equals(DevicePlatform other) =>
            Equals(other.deviceIdiom);

        internal bool Equals(string other) =>
            string.Equals(deviceIdiom, other, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is DevicePlatform && Equals((DevicePlatform)obj);

        public override int GetHashCode() =>
            deviceIdiom == null ? 0 : deviceIdiom.GetHashCode();

        public override string ToString() =>
            deviceIdiom ?? string.Empty;

        public static bool operator ==(DevicePlatform left, DevicePlatform right) =>
            left.Equals(right);

        public static bool operator !=(DevicePlatform left, DevicePlatform right) =>
            !(left == right);
    }
}
