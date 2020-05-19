// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// A static class for retail validated assertions.
    /// Instead of breaking into the debugger an exception is thrown.
    /// </summary>
    internal static class Verify
    {
        /// <summary>
        /// Ensure that the current thread's apartment state is what's expected.
        /// </summary>
        /// <param name="requiredState">
        /// The required apartment state for the current thread.
        /// </param>
        /// <param name="message">
        /// The message string for the exception to be thrown if the state is invalid.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the calling thread's apartment state is not the same as the requiredState.
        /// </exception>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsApartmentState(ApartmentState requiredState, string message)
        {
            if (Thread.CurrentThread.GetApartmentState() != requiredState)
            {
                Assert.Fail();
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure that an argument is neither null nor empty.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        [DebuggerStepThrough]
        public static void IsNeitherNullNorEmpty(string value, string name)
        {
            // catch caller errors, mixing up the parameters.  Name should never be empty.
            Assert.IsNeitherNullNorEmpty(name);

            // Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
            const string errorMessage = "The parameter can not be either null or empty.";
            if (null == value)
            {
                Assert.Fail();
                throw new ArgumentNullException(name, errorMessage);
            }
            if ("" == value)
            {
                Assert.Fail();
                throw new ArgumentException(errorMessage, name);
            }
        }

        /// <summary>
        /// Ensure that an argument is neither null nor does it consist only of whitespace.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        [DebuggerStepThrough]
        public static void IsNeitherNullNorWhitespace(string value, string name)
        {
            // catch caller errors, mixing up the parameters.  Name should never be empty.
            Assert.IsNeitherNullNorEmpty(name);

            // Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
            const string errorMessage = "The parameter can not be either null or empty or consist only of white space characters.";
            if (null == value)
            {
                Assert.Fail();
                throw new ArgumentNullException(name, errorMessage);
            }
            if ("" == value.Trim())
            {
                Assert.Fail();
                throw new ArgumentException(errorMessage, name);
            }
        }

        /// <summary>Verifies that an argument is not null.</summary>
        /// <typeparam name="T">Type of the object to validate.  Must be a class.</typeparam>
        /// <param name="obj">The object to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNotDefault<T>(T obj, string name) where T : struct
        {
            if (default(T).Equals(obj))
            {
                Assert.Fail();
                throw new ArgumentException("The parameter must not be the default value.", name);
            }
        }

        /// <summary>Verifies that an argument is not null.</summary>
        /// <typeparam name="T">Type of the object to validate.  Must be a class.</typeparam>
        /// <param name="obj">The object to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNotNull<T>(T obj, string name) where T : class
        {
            if (null == obj)
            {
                Assert.Fail();
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>Verifies that an argument is null.</summary>
        /// <typeparam name="T">Type of the object to validate.  Must be a class.</typeparam>
        /// <param name="obj">The object to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsNull<T>(T obj, string name) where T : class
        {
            if (null != obj)
            {
                Assert.Fail();
                throw new ArgumentException("The parameter must be null.", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void PropertyIsNotNull<T>(T obj, string name) where T : class
        {
            if (null == obj)
            {
                Assert.Fail();
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} cannot be null at this time.", name));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void PropertyIsNull<T>(T obj, string name) where T : class
        {
            if (null != obj)
            {
                Assert.Fail();
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} must be null at this time.", name));
            }
        }

        /// <summary>
        /// Verifies the specified statement is true.  Throws an ArgumentException if it's not.
        /// </summary>
        /// <param name="statement">The statement to be verified as true.</param>
        /// <param name="name">Name of the parameter to include in the ArgumentException.</param>
        /// <param name="message">The message to include in the ArgumentException.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsTrue(bool statement, string name, string message = null)
        {
            if (!statement)
            {
                Assert.Fail();
                throw new ArgumentException(message ?? "", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void IsFalse(bool statement, string name, string message = null)
        {
            if (statement)
            {
                Assert.Fail();
                throw new ArgumentException(message ?? "", name);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void AreEqual<T>(T expected, T actual, string parameterName, string message)
        {
            if (null == expected)
            {
                // Two nulls are considered equal, regardless of type semantics.
                if (null != actual && !actual.Equals(expected))
                {
                    Assert.Fail();
                    throw new ArgumentException(message, parameterName);
                }
            }
            else if (!expected.Equals(actual))
            {
                Assert.Fail();
                throw new ArgumentException(message, parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void AreNotEqual<T>(T notExpected, T actual, string parameterName, string message)
        {
            if (null == notExpected)
            {
                // Two nulls are considered equal, regardless of type semantics.
                if (null == actual || actual.Equals(notExpected))
                {
                    Assert.Fail();
                    throw new ArgumentException(message, parameterName);
                }
            }
            else if (notExpected.Equals(actual))
            {
                Assert.Fail();
                throw new ArgumentException(message, parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void UriIsAbsolute(Uri uri, string parameterName)
        {
            Verify.IsNotNull(uri, parameterName);
            if (!uri.IsAbsoluteUri)
            {
                Assert.Fail();
                throw new ArgumentException("The URI must be absolute.", parameterName);
            }
        }

        /// <summary>
        /// Verifies that the specified value is within the expected range.  The assertion fails if it isn't.
        /// </summary>
        /// <param name="lowerBoundInclusive">The lower bound inclusive value.</param>
        /// <param name="value">The value to verify.</param>
        /// <param name="upperBoundExclusive">The upper bound exclusive value.</param>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void BoundedInteger(int lowerBoundInclusive, int value, int upperBoundExclusive, string parameterName)
        {
            if (value < lowerBoundInclusive || value >= upperBoundExclusive)
            {
                Assert.Fail();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The integer value must be bounded with [{0}, {1})", lowerBoundInclusive, upperBoundExclusive), parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void BoundedDoubleInc(double lowerBoundInclusive, double value, double upperBoundInclusive, string message, string parameter)
        {
            if (value < lowerBoundInclusive || value > upperBoundInclusive)
            {
                Assert.Fail();
                throw new ArgumentException(message, parameter);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void TypeSupportsInterface(Type type, Type interfaceType, string parameterName)
        {
            Assert.IsNeitherNullNorEmpty(parameterName);
            Verify.IsNotNull(type, "type");
            Verify.IsNotNull(interfaceType, "interfaceType");

            if (type.GetInterface(interfaceType.Name) == null)
            {
                Assert.Fail();
                throw new ArgumentException("The type of this parameter does not support a required interface", parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        public static void FileExists(string filePath, string parameterName)
        {
            Verify.IsNeitherNullNorEmpty(filePath, parameterName);
            if (!File.Exists(filePath))
            {
                Assert.Fail();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No file exists at \"{0}\"", filePath), parameterName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DebuggerStepThrough]
        internal static void ImplementsInterface(object parameter, Type interfaceType, string parameterName)
        {
            Assert.IsNotNull(parameter);
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);

            bool isImplemented = false;
            foreach (var ifaceType in parameter.GetType().GetInterfaces())
            {
                if (ifaceType == interfaceType)
                {
                    isImplemented = true;
                    break;
                }
            }

            if (!isImplemented)
            {
                Assert.Fail();
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter must implement interface {0}.", interfaceType.ToString()), parameterName);
            }
        }
    }
}
