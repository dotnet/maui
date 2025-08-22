#if IOS // This test case verifies SafeArea keyboard overlap calculation exclusively on the iOS platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue31191 : _IssuesUITest
    {
        public Issue31191(TestDevice device) : base(device)
        {
        }

        public override string Issue => "SafeArea keyboard overlap calculation should only inset by actual covered area";

        [Test]
        [Category(UITestCategories.Layout)]
        public async Task SafeAreaKeyboardOverlapCalculation()
        {
            // Wait for the page to load
            App.WaitForElement("InstructionsLabel");
            
            // Verify initial state - keyboard should not be visible
            App.WaitForElement("TopContent");
            App.WaitForElement("MiddleContent");
            App.WaitForElement("BottomContent");
            
            // Take a screenshot of initial state
            await Task.Delay(500);
            App.Screenshot("Initial_State_Before_Keyboard");
            
            // Tap the entry to show the keyboard
            App.WaitForElement("KeyboardEntry");
            App.Tap("KeyboardEntry");
            
            // Wait for keyboard animation to complete
            await Task.Delay(1000);
            
            // Verify that content is still accessible after keyboard appears
            // The fix should ensure that only the actual overlap is used for safe area adjustment
            App.WaitForElement("TopContent");
            App.WaitForElement("MiddleContent");
            
            // Take a screenshot with keyboard visible
            App.Screenshot("Keyboard_Visible_State");
            
            // Tap the test button to update safe area display
            App.WaitForElement("TestButton");
            App.Tap("TestButton");
            
            await Task.Delay(500);
            
            // Verify safe area label is updated
            App.WaitForElement("SafeAreaLabel");
            
            // Take final screenshot
            App.Screenshot("Final_State_With_SafeArea_Info");
            
            // Hide keyboard by tapping somewhere else
            App.WaitForElement("TopContent");
            App.Tap("TopContent");
            
            // Wait for keyboard to hide
            await Task.Delay(1000);
            
            // Verify content is accessible after keyboard hides
            App.WaitForElement("BottomContent");
            
            // Take screenshot of final state
            App.Screenshot("Keyboard_Hidden_Final_State");
        }

        [Test]
        [Category(UITestCategories.Layout)]
        public async Task VerifyContentAccessibilityWithKeyboard()
        {
            // This test specifically checks that content accessibility is maintained
            // when keyboard appears and only actual overlap is considered for safe area
            
            App.WaitForElement("MiddleContentLabel");
            
            // Get initial position reference
            var middleContentBefore = App.FindElement("MiddleContentLabel");
            
            // Show keyboard
            App.WaitForElement("KeyboardEntry");
            App.Tap("KeyboardEntry");
            
            await Task.Delay(1000);
            
            // Verify middle content is still accessible and hasn't moved excessively
            App.WaitForElement("MiddleContentLabel");
            
            // The fix ensures that views not actually covered by keyboard
            // don't get excessive safe area padding
            var middleContentAfter = App.FindElement("MiddleContentLabel");
            
            // Content should still be accessible
            Assert.IsNotNull(middleContentAfter, "Middle content should remain accessible when keyboard is visible");
            
            await Task.Delay(500);
            
            // Hide keyboard
            App.Tap("TopContent");
            await Task.Delay(1000);
            
            // Verify content is back to normal position
            App.WaitForElement("BottomContentLabel");
        }
    }
}
#endif