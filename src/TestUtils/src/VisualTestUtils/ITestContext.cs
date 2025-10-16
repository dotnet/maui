namespace VisualTestUtils
{
    /// <summary>
    /// This interface can be implemented by the client and is used to associate
    /// extra information like file attachments with failed visual tests, if supported
    /// by the client's testing framework.
    ///
    /// A typical client implementation would do this, depending on client test framework:
    /// NUnit: Call NUnit TestContext.AddTestAttachment (https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html)
    /// XUnit: Not currently supported; see https://github.com/xunit/xunit/issues/2457
    /// MSTest: Call TestContext.AddResultFile (https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.testcontext.addresultfile?view=visualstudiosdk-2022)
    /// </summary>
    public interface ITestContext
    {
        /// <summary>
        /// Attach the specified file to the test result, with an optional file description.
        /// </summary>
        /// <param name="filePath">path to file to attach</param>
        /// <param name="description">file description</param>
        void AddTestAttachment(string filePath, string description = null);
    }
}
