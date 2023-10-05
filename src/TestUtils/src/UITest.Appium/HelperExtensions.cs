using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Appium;
using UITest.Core;
using OpenQA.Selenium;
using System.Xml.Linq;
using System.Collections.Immutable;

namespace UITest.Appium
{
    public static class HelperExtensions
    {
        static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        public static void Click(this IApp app, string element)
        {
            app.FindElement(element).Click();
        }

        public static string? GetText(this IUIElement element)
        {
            var response = element.Command.Execute("getText", new Dictionary<string, object>()
            {
                { "element", element },
            });
            return (string?)response.Value;
        }

        public static T? GetAttribute<T>(this IUIElement element, string attributeName)
        {
            var response = element.Command.Execute("getAttribute", new Dictionary<string, object>()
            {
                { "element", element },
                { "attributeName", attributeName },
            });
            return (T?)response.Value;
        }

        public static Rectangle GetRect(this IUIElement element)
        {
            var response = element.Command.Execute("getRect", new Dictionary<string, object>()
            {
                { "element", element },
            });

            if (response?.Value != null)
            {
                return (Rectangle)response.Value;
            }

            throw new InvalidOperationException($"Could not get Rect of element");
        }

        public static void EnterText(this IApp app, string element, string text)
        {
            var appElement = app.FindElement(element);
            appElement.SendKeys(text);
            app.DismissKeyboard();
        }

        public static void DismissKeyboard(this IApp app)
        {
            app.CommandExecutor.Execute("dismissKeyboard", ImmutableDictionary<string, object>.Empty);
        }

        public static bool IsKeyboardShown(this IApp app)
        {
            var response = app.CommandExecutor.Execute("isKeyboardShown", ImmutableDictionary<string,object>.Empty);
            var responseValue = response?.Value ?? false;
            return (bool)responseValue;
        }

        public static void ClearText(this IApp app, string element)
        {
            app.FindElement(element).Clear();
        }

        public static void Click(this IUIElement element)
        {
            element.Command.Execute("click", new Dictionary<string, object>()
            {
                { "element", element }
            });
        }

        public static void SendKeys(this IUIElement element, string text)
        {
            element.Command.Execute("sendKeys", new Dictionary<string, object>()
            {
                { "element", element },
                { "text", text }
            });
        }

        public static void Clear(this IUIElement element)
        {
            element.Command.Execute("clear", new Dictionary<string, object>()
            {
                { "element", element },
            });
        }

        public static void DoubleClick(this IApp app, string element)
        {
            var elementToClick = app.FindElement(element);
            app.CommandExecutor.Execute("doubleClick", new Dictionary<string, object>
            {
                { "element", elementToClick },
            });
        }

        public static void DragAndDrop(this IApp app, string dragSource, string dragTarget)
        {
            var dragSourceElement = app.FindElement(dragSource);
            var targetSourceElement = app.FindElement(dragTarget);

            app.CommandExecutor.Execute("dragAndDrop", new Dictionary<string, object>
            {
                { "sourceElement", dragSourceElement },
                { "destinationElement", targetSourceElement }
            });
        }

        public static void ScrollTo(this IApp app, string toElementId, bool down = true)
        {
            app.CommandExecutor.Execute("scrollTo", new Dictionary<string, object>
            {
                { "elementId", toElementId},
                { "down", down }
            });
        }

        public static IUIElement WaitForElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            IUIElement result() => app.FindElement(marked);
            var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

            return results;
        }

        public static void WaitForNoElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            IUIElement result() => app.FindElement(marked);
            WaitForNone(result, timeoutMessage, timeout, retryFrequency);
        }

        public static bool WaitForTextToBePresentInElement(this IApp app, string automationId, string text)
        {
            TimeSpan timeout = DefaultTimeout;
            TimeSpan retryFrequency = TimeSpan.FromMilliseconds(500);
            string timeoutMessage = $"Timed out on {nameof(WaitForTextToBePresentInElement)}.";

            DateTime start = DateTime.Now;

            while (true)
            {
                var element = app.FindElements(automationId).FirstOrDefault();
                if (element != null && (element.GetText()?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    return true;
                }

                long elapsed = DateTime.Now.Subtract(start).Ticks;
                if (elapsed >= timeout.Ticks)
                {
                    Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Ticks}");

                    return false;
                }

                Task.Delay(retryFrequency.Milliseconds).Wait();
            }
        }

        static IUIElement Wait(Func<IUIElement> query,
            Func<IUIElement, bool> satisfactory,
            string? timeoutMessage = null,
            TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
        {
            timeout ??= DefaultTimeout;
            retryFrequency ??= TimeSpan.FromMilliseconds(500);
            timeoutMessage ??= "Timed out on query.";

            DateTime start = DateTime.Now;

            IUIElement result = query();

            while (!satisfactory(result))
            {
                long elapsed = DateTime.Now.Subtract(start).Ticks;
                if (elapsed >= timeout.Value.Ticks)
                {
                    Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

                    throw new TimeoutException(timeoutMessage);
                }

                Task.Delay(retryFrequency.Value.Milliseconds).Wait();
                result = query();
            }

            return result;
        }

        static IUIElement WaitForAtLeastOne(Func<IUIElement> query,
            string? timeoutMessage = null,
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null)
        {
            var results = Wait(query, i => i != null, timeoutMessage, timeout, retryFrequency);

            return results;
        }

        static void WaitForNone(Func<IUIElement> query,
            string? timeoutMessage = null,
            TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
        {
            Wait(query, i => i == null, timeoutMessage, timeout, retryFrequency);
        }
    }
}
