using System.Collections.Immutable;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
    public class AppiumQuery : IQuery
    {
        const string ClassToken = "class";
        const string IdToken = "id";
        const string NameToken = "name";
        const string AccessibilityToken = "accessibilityid";
        const string QuerySeparatorToken = "&";
        const string IdQuery = IdToken + "={0}";
        const string NameQuery = NameToken + "={0}";
        const string AccessibilityQuery = AccessibilityToken + "={0}";
        const string ClassQuery = ClassToken + "={0}";
        readonly string _queryStr;

        public AppiumQuery(string queryStr)
        {
            _queryStr = queryStr;
        }

        public AppiumQuery(AppiumQuery query, string queryStr)
        {
            _queryStr = string.Join(query._queryStr, QuerySeparatorToken, queryStr);
        }

        IQuery IQuery.ByClass(string classQuery)
        {
            return new AppiumQuery(this, string.Format(ClassQuery, classQuery));
        }

        IQuery IQuery.ById(string id)
        {
            return new AppiumQuery(this, string.Format(IdQuery, id));
        }

        IQuery IQuery.ByAccessibilityId(string id)
        {
            return new AppiumQuery(this, string.Format(AccessibilityQuery, id));
        }

        IQuery IQuery.ByName(string nameQuery)
        {
            return new AppiumQuery(this, string.Format(NameQuery, nameQuery));
        }

        public static AppiumQuery ById(string id)
        {
            return new AppiumQuery(string.Format(IdQuery, id));
        }

        public static AppiumQuery ByName(string nameQuery)
        {
            return new AppiumQuery(string.Format(NameQuery, nameQuery));
        }

        public static AppiumQuery ByAccessibilityId(string id)
        {
            return new AppiumQuery(string.Format(AccessibilityQuery, id));
        }

        public static AppiumQuery ByClass(string classQuery)
        {
            return new AppiumQuery(string.Format(ClassQuery, classQuery));
        }

#nullable disable
        public IUIElement FindElement(AppiumApp appiumApp)
        {
            // e.g. class=button&id=MyButton
            string[] querySplit = _queryStr.Split(QuerySeparatorToken);
            string queryStr = querySplit[0];
            string[] argSplit = queryStr.Split('=');

            if (argSplit.Length != 2)
            {
                throw new ArgumentException("Invalid Query");
            }

            var queryBy = GetQueryBy(argSplit[0], argSplit[1]);
            var foundElement = appiumApp.Driver.FindElements(queryBy).FirstOrDefault();

            if (foundElement == null)
            {
                return null;
            }

            for (int i = 1; i < querySplit.Length; i++)
            {
                foundElement = FindElement(foundElement, querySplit[i]);
            }

            return new AppiumDriverElement(foundElement, appiumApp);
        }

        public IReadOnlyCollection<IUIElement> FindElements(AppiumApp appiumApp)
        {
            // e.g. class=button&id=MyButton
            string[] querySplit = _queryStr.Split(QuerySeparatorToken);
            string queryStr = querySplit[0];
            string[] argSplit = queryStr.Split('=');

            if (argSplit.Length != 2)
            {
                throw new ArgumentException("Invalid Query");
            }

            var queryBy = GetQueryBy(argSplit[0], argSplit[1]);
            var foundElements = appiumApp.Driver.FindElements(queryBy);

            // TODO: What is the expected way to handle multiple queries when multiple elements are returned?
            //for(int i = 1; i < querySplit.Length; i++)
            //{
            //    foundElement = FindElement(foundElement, querySplit[i]);
            //}

            return foundElements.Select(e => new AppiumDriverElement(e, appiumApp)).ToList();
        }
#nullable enable

        public IReadOnlyCollection<IUIElement> FindElements(AppiumElement element, AppiumApp appiumApp)
        {
            string[] querySplit = _queryStr.Split(QuerySeparatorToken);

            AppiumElement appiumElement = element;
            string queryStr = querySplit[0];
            string[] argSplit = queryStr.Split('=');
            var queryBy = GetQueryBy(argSplit[0], argSplit[1]);
            var foundElements = element.FindElements(queryBy);
            //for (int i = 0; i < querySplit.Length; i++)
            //{
            //    appiumElement = FindElement(appiumElement, querySplit[i]);
            //}

            return foundElements.Select(e => new AppiumDriverElement((AppiumElement)e, appiumApp)).ToList();
        }

        public IUIElement FindElement(AppiumElement element, AppiumApp appiumApp)
        {
            string[] querySplit = _queryStr.Split(QuerySeparatorToken);

            AppiumElement appiumElement = element;
            for (int i = 0; i < querySplit.Length; i++)
            {
                appiumElement = FindElement(appiumElement, querySplit[i]);
            }

            return new AppiumDriverElement(appiumElement, appiumApp);
        }

        private static IReadOnlyCollection<AppiumElement> FindElements(AppiumElement element, string query)
        {
            var argSplit = query.Split('=');
            if (argSplit.Length != 2)
            {
                throw new ArgumentException("Invalid Query");
            }

            var queryBy = GetQueryBy(argSplit[0], argSplit[1]);
            return element.FindElements(queryBy).Select(e => (AppiumElement)e).ToList();
        }

        private static AppiumElement FindElement(AppiumElement element, string query)
        {
            var argSplit = query.Split('=');
            if (argSplit.Length != 2)
            {
                throw new ArgumentException("Invalid Query");
            }

            var queryBy = GetQueryBy(argSplit[0], argSplit[1]);
            return (AppiumElement)element.FindElement(queryBy);
        }

        private static By GetQueryBy(string token, string value)
        {
           return token.ToLowerInvariant() switch
            {
                ClassToken => MobileBy.ClassName(value),
                NameToken => MobileBy.Name(value),
                AccessibilityToken => MobileBy.AccessibilityId(value),
                IdToken => MobileBy.Id(value),
                _ => throw new ArgumentException("Unknown query type"),
            };
        }
    }
}
