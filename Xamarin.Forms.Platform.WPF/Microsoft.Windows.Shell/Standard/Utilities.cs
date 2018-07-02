// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    internal enum SafeCopyFileOptions
    {
        PreserveOriginal,
        Overwrite,
        FindBetterName,
    }

    internal static partial class Utility
    {
        private static readonly Random _randomNumberGenerator = new Random();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _MemCmp(IntPtr left, IntPtr right, long cb)
        {
            int offset = 0;

            for (; offset < (cb - sizeof(Int64)); offset += sizeof(Int64))
            {
                Int64 left64 = Marshal.ReadInt64(left, offset);
                Int64 right64 = Marshal.ReadInt64(right, offset);

                if (left64 != right64)
                {
                    return false;
                }
            }

            for (; offset < cb; offset += sizeof(byte))
            {
                byte left8 = Marshal.ReadByte(left, offset);
                byte right8 = Marshal.ReadByte(right, offset);

                if (left8 != right8)
                {
                    return false;
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Exception FailableFunction<T>(Func<T> function, out T result)
        {
            return FailableFunction(5, function, out result);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FailableFunction<T>(Func<T> function)
        {
            T result;
            Exception e = FailableFunction(function, out result);
            if (e != null)
            {
                throw e;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FailableFunction<T>(int maxRetries, Func<T> function)
        {
            T result;
            Exception e = FailableFunction(maxRetries, function, out result);
            if (e != null)
            {
                throw e;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Exception FailableFunction<T>(int maxRetries, Func<T> function, out T result)
        {
            Assert.IsNotNull(function);
            Assert.BoundedInteger(1, maxRetries, 100);
            int i = 0;
            while (true)
            {
                try
                {
                    result = function();
                    return null;
                }
                catch (Exception e)
                {
                    if (i == maxRetries)
                    {
                        result = default(T);
                        return e;
                    }
                }
                ++i;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetHashString(string value)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] signatureHash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                string signature = signatureHash.Aggregate(
                    new StringBuilder(),
                    (sb, b) => sb.Append(b.ToString("x2", CultureInfo.InvariantCulture))).ToString();
                return signature;
            }
        }

        // See: http://stackoverflow.com/questions/7913325/win-api-in-c-get-hi-and-low-word-from-intptr/7913393#7913393
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static System.Windows.Point GetPoint(IntPtr ptr)
        {
            var xy = unchecked(Environment.Is64BitProcess ? (uint)ptr.ToInt64() : (uint)ptr.ToInt32());
            var x = unchecked((short)xy);
            var y = unchecked((short)(xy >> 16));
            return new System.Windows.Point(x, y);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int HIWORD(int i)
        {
            return (short)(i >> 16);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int LOWORD(int i)
        {
            return (short)(i & 0xFFFF);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static bool AreStreamsEqual(Stream left, Stream right)
        {
            if (null == left)
            {
                return right == null;
            }
            if (null == right)
            {
                return false;
            }

            if (!left.CanRead || !right.CanRead)
            {
                throw new NotSupportedException("The streams can't be read for comparison");
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            var length = (int)left.Length;

            // seek to beginning
            left.Position = 0;
            right.Position = 0;

            // total bytes read
            int totalReadLeft = 0;
            int totalReadRight = 0;

            // bytes read on this iteration
            int cbReadLeft = 0;
            int cbReadRight = 0;

            // where to store the read data
            var leftBuffer = new byte[512];
            var rightBuffer = new byte[512];

            // pin the left buffer
            GCHandle handleLeft = GCHandle.Alloc(leftBuffer, GCHandleType.Pinned);
            IntPtr ptrLeft = handleLeft.AddrOfPinnedObject();

            // pin the right buffer
            GCHandle handleRight = GCHandle.Alloc(rightBuffer, GCHandleType.Pinned);
            IntPtr ptrRight = handleRight.AddrOfPinnedObject();

            try
            {
                while (totalReadLeft < length)
                {
                    Assert.AreEqual(totalReadLeft, totalReadRight);

                    cbReadLeft = left.Read(leftBuffer, 0, leftBuffer.Length);
                    cbReadRight = right.Read(rightBuffer, 0, rightBuffer.Length);

                    // verify the contents are an exact match
                    if (cbReadLeft != cbReadRight)
                    {
                        return false;
                    }

                    if (!_MemCmp(ptrLeft, ptrRight, cbReadLeft))
                    {
                        return false;
                    }

                    totalReadLeft += cbReadLeft;
                    totalReadRight += cbReadRight;
                }

                Assert.AreEqual(cbReadLeft, cbReadRight);
                Assert.AreEqual(totalReadLeft, totalReadRight);
                Assert.AreEqual(length, totalReadLeft);

                return true;
            }
            finally
            {
                handleLeft.Free();
                handleRight.Free();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool GuidTryParse(string guidString, out Guid guid)
        {
            Verify.IsNeitherNullNorEmpty(guidString, "guidString");

            try
            {
                guid = new Guid(guidString);
                return true;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            // Doesn't seem to be a valid guid.
            guid = default(Guid);
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(int value, int mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(uint value, uint mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(long value, long mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsFlagSet(ulong value, ulong mask)
        {
            return 0 != (value & mask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsInterfaceImplemented(Type objectType, Type interfaceType)
        {
            Assert.IsNotNull(objectType);
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);

            return objectType.GetInterfaces().Any(type => type == interfaceType);
        }

        /// <summary>
        /// Wrapper around File.Copy to provide feedback as to whether the file wasn't copied because it didn't exist.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string SafeCopyFile(string sourceFileName, string destFileName, SafeCopyFileOptions options)
        {
            switch (options)
            {
                case SafeCopyFileOptions.PreserveOriginal:
                    if (!File.Exists(destFileName))
                    {
                        File.Copy(sourceFileName, destFileName);
                        return destFileName;
                    }
                    return null;
                case SafeCopyFileOptions.Overwrite:
                    File.Copy(sourceFileName, destFileName, true);
                    return destFileName;
                case SafeCopyFileOptions.FindBetterName:
                    string directoryPart = Path.GetDirectoryName(destFileName);
                    string fileNamePart = Path.GetFileNameWithoutExtension(destFileName);
                    string extensionPart = Path.GetExtension(destFileName);
                    foreach (string path in GenerateFileNames(directoryPart, fileNamePart, extensionPart))
                    {
                        if (!File.Exists(path))
                        {
                            File.Copy(sourceFileName, path);
                            return path;
                        }
                    }
                    return null;
            }
            throw new ArgumentException("Invalid enumeration value", "options");
        }

        /// <summary>
        /// Simple guard against the exceptions that File.Delete throws on null and empty strings.
        /// </summary>
        /// <param name="path">The path to delete.  Unlike File.Delete, this can be null or empty.</param>
        /// <remarks>
        /// Note that File.Delete, and by extension SafeDeleteFile, does not throw an exception
        /// if the file does not exist.
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDeleteFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }


        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SafeDispose<T>(ref T disposable) where T : IDisposable
        {
            // Dispose can safely be called on an object multiple times.
            IDisposable t = disposable;
            disposable = default(T);
            if (null != t)
            {
                t.Dispose();
            }
        }

        /// <summary>
        /// Utility to help classes catenate their properties for implementing ToString().
        /// </summary>
        /// <param name="source">The StringBuilder to catenate the results into.</param>
        /// <param name="propertyName">The name of the property to be catenated.</param>
        /// <param name="value">The value of the property to be catenated.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void GeneratePropertyString(StringBuilder source, string propertyName, string value)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            if (0 != source.Length)
            {
                source.Append(' ');
            }

            source.Append(propertyName);
            source.Append(": ");
            if (string.IsNullOrEmpty(value))
            {
                source.Append("<null>");
            }
            else
            {
                source.Append('\"');
                source.Append(value);
                source.Append('\"');
            }
        }

        /// <summary>
        /// Generates ToString functionality for a struct.  This is an expensive way to do it,
        /// it exists for the sake of debugging while classes are in flux.
        /// Eventually this should just be removed and the classes should
        /// do this without reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Obsolete]
        public static string GenerateToString<T>(T @object) where T : struct
        {
            var sbRet = new StringBuilder();
            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (0 != sbRet.Length)
                {
                    sbRet.Append(", ");
                }
                Assert.AreEqual(0, property.GetIndexParameters().Length);
                object value = property.GetValue(@object, null);
                string format = null == value ? "{0}: <null>" : "{0}: \"{1}\"";
                sbRet.AppendFormat(format, property.Name, value);
            }
            return sbRet.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void CopyStream(Stream destination, Stream source)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(destination);

            destination.Position = 0;

            // If we're copying from, say, a web stream, don't fail because of this.
            if (source.CanSeek)
            {
                source.Position = 0;

                // Consider that this could throw because 
                // the source stream doesn't know it's size...
                destination.SetLength(source.Length);
            }

            var buffer = new byte[4096];
            int cbRead;

            do
            {
                cbRead = source.Read(buffer, 0, buffer.Length);
                if (0 != cbRead)
                {
                    destination.Write(buffer, 0, cbRead);
                }
            }
            while (buffer.Length == cbRead);

            // Reset the Seek pointer before returning.
            destination.Position = 0;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string HashStreamMD5(Stream stm)
        {
            stm.Position = 0;
            var hashBuilder = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                foreach (byte b in md5.ComputeHash(stm))
                {
                    hashBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return hashBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void EnsureDirectory(string path)
        {
            if (!path.EndsWith(@"\", StringComparison.Ordinal))
            {
                path += @"\";
            }

            path = Path.GetDirectoryName(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool MemCmp(byte[] left, byte[] right, int cb)
        {
            Assert.IsNotNull(left);
            Assert.IsNotNull(right);

            Assert.IsTrue(cb <= Math.Min(left.Length, right.Length));

            // pin this buffer
            GCHandle handleLeft = GCHandle.Alloc(left, GCHandleType.Pinned);
            IntPtr ptrLeft = handleLeft.AddrOfPinnedObject();

            // pin the other buffer
            GCHandle handleRight = GCHandle.Alloc(right, GCHandleType.Pinned);
            IntPtr ptrRight = handleRight.AddrOfPinnedObject();

            bool fRet = _MemCmp(ptrLeft, ptrRight, cb);

            handleLeft.Free();
            handleRight.Free();

            return fRet;
        }

        private class _UrlDecoder
        {
            private readonly Encoding _encoding;
            private readonly char[] _charBuffer;
            private readonly byte[] _byteBuffer;
            private int _byteCount;
            private int _charCount;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public _UrlDecoder(int size, Encoding encoding)
            {
                _encoding = encoding;
                _charBuffer = new char[size];
                _byteBuffer = new byte[size];
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddByte(byte b)
            {
                _byteBuffer[_byteCount++] = b;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddChar(char ch)
            {
                _FlushBytes();
                _charBuffer[_charCount++] = ch;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private void _FlushBytes()
            {
                if (_byteCount > 0)
                {
                    _charCount += _encoding.GetChars(_byteBuffer, 0, _byteCount, _charBuffer, _charCount);
                    _byteCount = 0;
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string GetString()
            {
                _FlushBytes();
                if (_charCount > 0)
                {
                    return new string(_charBuffer, 0, _charCount);
                }
                return "";
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string UrlDecode(string url)
        {
            if (url == null)
            {
                return null;
            }

            var decoder = new _UrlDecoder(url.Length, Encoding.UTF8);
            int length = url.Length;
            for (int i = 0; i < length; ++i)
            {
                char ch = url[i];

                if (ch == '+')
                {
                    decoder.AddByte((byte)' ');
                    continue;
                }

                if (ch == '%' && i < length - 2)
                {
                    // decode %uXXXX into a Unicode character.
                    if (url[i + 1] == 'u' && i < length - 5)
                    {
                        int a = _HexToInt(url[i + 2]);
                        int b = _HexToInt(url[i + 3]);
                        int c = _HexToInt(url[i + 4]);
                        int d = _HexToInt(url[i + 5]);
                        if (a >= 0 && b >= 0 && c >= 0 && d >= 0)
                        {
                            decoder.AddChar((char)((a << 12) | (b << 8) | (c << 4) | d));
                            i += 5;

                            continue;
                        }
                    }
                    else
                    {
                        // decode %XX into a Unicode character.
                        int a = _HexToInt(url[i + 1]);
                        int b = _HexToInt(url[i + 2]);

                        if (a >= 0 && b >= 0)
                        {
                            decoder.AddByte((byte)((a << 4) | b));
                            i += 2;

                            continue;
                        }
                    }
                }

                // Add any 7bit character as a byte.
                if ((ch & 0xFF80) == 0)
                {
                    decoder.AddByte((byte)ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }

            return decoder.GetString();
        }

        /// <summary>
        /// Encodes a URL string.  Duplicated functionality from System.Web.HttpUtility.UrlEncode.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>
        /// Duplicated from System.Web.HttpUtility because System.Web isn't part of the client profile.
        /// URL Encoding replaces ' ' with '+' and unsafe ASCII characters with '%XX'.
        /// Safe characters are defined in RFC2396 (http://www.ietf.org/rfc/rfc2396.txt).
        /// They are the 7-bit ASCII alphanumerics and the mark characters "-_.!~*'()".
        /// This implementation does not treat '~' as a safe character to be consistent with the System.Web version.
        /// </remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string UrlEncode(string url)
        {
            if (url == null)
            {
                return null;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(url);

            bool needsEncoding = false;
            int unsafeCharCount = 0;
            foreach (byte b in bytes)
            {
                if (b == ' ')
                {
                    needsEncoding = true;
                }
                else if (!_UrlEncodeIsSafe(b))
                {
                    ++unsafeCharCount;
                    needsEncoding = true;
                }
            }

            if (needsEncoding)
            {
                var buffer = new byte[bytes.Length + (unsafeCharCount * 2)];
                int writeIndex = 0;
                foreach (byte b in bytes)
                {
                    if (_UrlEncodeIsSafe(b))
                    {
                        buffer[writeIndex++] = b;
                    }
                    else if (b == ' ')
                    {
                        buffer[writeIndex++] = (byte)'+';
                    }
                    else
                    {
                        buffer[writeIndex++] = (byte)'%';
                        buffer[writeIndex++] = _IntToHex((b >> 4) & 0xF);
                        buffer[writeIndex++] = _IntToHex(b & 0xF);
                    }
                }
                bytes = buffer;
                Assert.AreEqual(buffer.Length, writeIndex);
            }

            return Encoding.ASCII.GetString(bytes);
        }

        // HttpUtility's UrlEncode is slightly different from the RFC.
        // RFC2396 describes unreserved characters as alphanumeric or
        // the list "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
        // The System.Web version unnecessarily escapes '~', which should be okay...
        // Keeping that same pattern here just to be consistent.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _UrlEncodeIsSafe(byte b)
        {
            if (_IsAsciiAlphaNumeric(b))
            {
                return true;
            }

            switch ((char)b)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                //case '~':
                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool _IsAsciiAlphaNumeric(byte b)
        {
            return (b >= 'a' && b <= 'z')
                || (b >= 'A' && b <= 'Z')
                || (b >= '0' && b <= '9');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static byte _IntToHex(int n)
        {
            Assert.BoundedInteger(0, n, 16);
            if (n <= 9)
            {
                return (byte)(n + '0');
            }
            return (byte)(n - 10 + 'A');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _HexToInt(char h)
        {
            if (h >= '0' && h <= '9')
            {
                return h - '0';
            }

            if (h >= 'a' && h <= 'f')
            {
                return h - 'a' + 10;
            }

            if (h >= 'A' && h <= 'F')
            {
                return h - 'A' + 10;
            }

            Assert.Fail("Invalid hex character " + h);
            return -1;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string MakeValidFileName(string invalidPath)
        {
            return invalidPath
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace(':', '_')
                .Replace('*', '_')
                .Replace('?', '_')
                .Replace('\"', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('|', '_');
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<string> GenerateFileNames(string directory, string primaryFileName, string extension)
        {
            Verify.IsNeitherNullNorEmpty(directory, "directory");
            Verify.IsNeitherNullNorEmpty(primaryFileName, "primaryFileName");

            primaryFileName = MakeValidFileName(primaryFileName);

            for (int i = 0; i <= 50; ++i)
            {
                if (0 == i)
                {
                    yield return Path.Combine(directory, primaryFileName) + extension;
                }
                else if (40 >= i)
                {
                    yield return Path.Combine(directory, primaryFileName) + " (" + i.ToString((IFormatProvider)null) + ")" + extension;
                }
                else
                {
                    // At this point we're hitting pathological cases.  This should stir things up enough that it works.
                    // If this fails because of naming conflicts after an extra 10 tries, then I don't care.
                    yield return Path.Combine(directory, primaryFileName) + " (" + _randomNumberGenerator.Next(41, 9999) + ")" + extension;
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool TryFileMove(string sourceFileName, string destFileName)
        {
            if (!File.Exists(destFileName))
            {
                try
                {
                    File.Move(sourceFileName, destFileName);
                }
                catch (IOException)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
