namespace Microsoft.Caboodle
{
    public static partial class Preferences
    {
        public static bool ContainsKey(string key) =>
            PlatformContainsKey(key, null);

        public static bool ContainsKey(string key, string sharedName) =>
            PlatformContainsKey(key, sharedName);

        public static void Remove(string key) =>
            PlatformRemove(key, null);

        public static void Remove(string key, string sharedName) =>
            PlatformRemove(key, sharedName);

        public static void Clear() =>
            PlatformClear(null);

        public static void Clear(string sharedName) =>
            PlatformClear(sharedName);

        public static string Get(string key, string defaultValue) =>
            PlatformGet<string>(key, defaultValue, null);

        public static bool Get(string key, bool defaultValue) =>
            PlatformGet<bool>(key, defaultValue, null);

        public static int Get(string key, int defaultValue) =>
            PlatformGet<int>(key, defaultValue, null);

        public static double Get(string key, double defaultValue) =>
            PlatformGet<double>(key, defaultValue, null);

        public static float Get(string key, float defaultValue) =>
            PlatformGet<float>(key, defaultValue, null);

        public static long Get(string key, long defaultValue) =>
            PlatformGet<long>(key, defaultValue, null);

        public static void Set(string key, string value) =>
            PlatformSet<string>(key, value, null);

        public static void Set(string key, bool value) =>
            PlatformSet<bool>(key, value, null);

        public static void Set(string key, int value) =>
            PlatformSet<int>(key, value, null);

        public static void Set(string key, double value) =>
            PlatformSet<double>(key, value, null);

        public static void Set(string key, float value) =>
            PlatformSet<float>(key, value, null);

        public static void Set(string key, long value) =>
            PlatformSet<long>(key, value, null);

        public static string Get(string key, string defaultValue, string sharedName) =>
            PlatformGet<string>(key, defaultValue, sharedName);

        public static bool Get(string key, bool defaultValue, string sharedName) =>
            PlatformGet<bool>(key, defaultValue, sharedName);

        public static int Get(string key, int defaultValue, string sharedName) =>
            PlatformGet<int>(key, defaultValue, sharedName);

        public static double Get(string key, double defaultValue, string sharedName) =>
            PlatformGet<double>(key, defaultValue, sharedName);

        public static float Get(string key, float defaultValue, string sharedName) =>
            PlatformGet<float>(key, defaultValue, sharedName);

        public static long Get(string key, long defaultValue, string sharedName) =>
            PlatformGet<long>(key, defaultValue, sharedName);

        public static void Set(string key, string value, string sharedName) =>
            PlatformSet<string>(key, value, sharedName);

        public static void Set(string key, bool value, string sharedName) =>
            PlatformSet<bool>(key, value, sharedName);

        public static void Set(string key, int value, string sharedName) =>
            PlatformSet<int>(key, value, sharedName);

        public static void Set(string key, double value, string sharedName) =>
            PlatformSet<double>(key, value, sharedName);

        public static void Set(string key, float value, string sharedName) =>
            PlatformSet<float>(key, value, sharedName);

        public static void Set(string key, long value, string sharedName) =>
            PlatformSet<long>(key, value, sharedName);
    }
}
