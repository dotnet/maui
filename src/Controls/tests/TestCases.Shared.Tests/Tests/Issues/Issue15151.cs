using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue15151 : _IssuesUITest
    {
        public override string Issue => "15151";

        public Issue15151(TestDevice device) : base(device)
        {
        }

        [Test]
        [Category(UITestCategories.Graphics)]
        public void PathFBoundsShouldReturnAccurateBounds()
        {
            // Wait for the test page to load and elements to be displayed
            var rectangleTightBounds = App.WaitForElement("RectangleTightBounds");
            var rectangleFlattenedBounds = App.WaitForElement("RectangleFlattenedBounds");
            var bezierTightBounds = App.WaitForElement("BezierTightBounds");
            var bezierFlattenedBounds = App.WaitForElement("BezierFlattenedBounds");
            var ovalTightBounds = App.WaitForElement("OvalTightBounds");
            var ovalFlattenedBounds = App.WaitForElement("OvalFlattenedBounds");

            // Verify the bounds elements exist and have text 
            Assert.IsNotEmpty(rectangleTightBounds);
            Assert.IsNotEmpty(rectangleFlattenedBounds);
            Assert.IsNotEmpty(bezierTightBounds);
            Assert.IsNotEmpty(bezierFlattenedBounds);
            Assert.IsNotEmpty(ovalTightBounds);
            Assert.IsNotEmpty(ovalFlattenedBounds);

            // Get the text from the bounds labels
            var rectTightText = App.GetText("RectangleTightBounds");
            var rectFlatText = App.GetText("RectangleFlattenedBounds");
            var bezierTightText = App.GetText("BezierTightBounds");
            var bezierFlatText = App.GetText("BezierFlattenedBounds");
            var ovalTightText = App.GetText("OvalTightBounds");
            var ovalFlatText = App.GetText("OvalFlattenedBounds");
            
            // Save a screenshot for verification
            VerifyScreenshot("PathFBoundsTightVsFlattened");

            // Basic value verifications
            // 1. For rectangle, tight and flattened bounds should be the same or very close
            Assert.AreEqual(rectTightText, rectFlatText);
            
            // 2. For bezier curve, tight bounds should be smaller than flattened bounds
            // Extract the height values for comparison
            var bezierTightHeight = ExtractHeightFromBounds(bezierTightText);
            var bezierFlatHeight = ExtractHeightFromBounds(bezierFlatText);
            Assert.Less(bezierTightHeight, bezierFlatHeight, "Bezier tight bounds height should be less than flattened bounds height");
            
            // 3. For oval, tight bounds should also be smaller than flattened bounds
            var ovalTightHeight = ExtractHeightFromBounds(ovalTightText);
            var ovalFlatHeight = ExtractHeightFromBounds(ovalFlatText);
            Assert.Less(ovalTightHeight, ovalFlatHeight, "Oval tight bounds height should be less than flattened bounds height");
        }
        
        // Helper method to extract the height value from a bounds text string
        private float ExtractHeightFromBounds(string boundsText)
        {
            // Example input: "Tight bounds: {X=50 Y=50 Width=150 Height=100}"
            // Extract the value after "Height=" and before "}"
            var heightStart = boundsText.IndexOf("Height=") + 7;
            var heightEnd = boundsText.IndexOf("}", heightStart);
            var heightString = boundsText.Substring(heightStart, heightEnd - heightStart);
            
            // Handle potential spaces or commas
            heightString = heightString.Trim();
            if (heightString.Contains(" "))
            {
                heightString = heightString.Substring(0, heightString.IndexOf(" "));
            }
            
            return float.Parse(heightString);
        }
    }
}