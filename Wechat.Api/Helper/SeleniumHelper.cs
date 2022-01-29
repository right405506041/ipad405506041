using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Wechat.Api.Helper
{
    public enum Browsers
    {
        IE,
        Firefox,
        Chrome,
        Safari
    }
    public class SeleniumHelper
    {
        private IWebDriver wd = null;
        private Browsers browser = Browsers.IE;
        public SeleniumHelper(Browsers theBrowser)
        {
            this.browser = theBrowser;
            this.wd = this.InitWebDriver();
        }
        private IWebDriver InitWebDriver()
        {
            IWebDriver webDriver;
            switch (this.browser)
            {
                case Browsers.IE:
                    webDriver = new InternetExplorerDriver(new InternetExplorerOptions
                    {
                        IntroduceInstabilityByIgnoringProtectedModeSettings = true
                    });
                    break;
                case Browsers.Firefox:
                    webDriver = new FirefoxDriver();
                    break;
                case Browsers.Chrome:
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.AddArgument("headless");
                        webDriver = new ChromeDriver("C:\\", chromeOptions);
                        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10.0);
                        break;
                    }
                case Browsers.Safari:
                    webDriver = new SafariDriver();
                    break;
                default:
                    webDriver = new InternetExplorerDriver(new InternetExplorerOptions
                    {
                        IntroduceInstabilityByIgnoringProtectedModeSettings = true
                    });
                    break;
            }
            return webDriver;
        }
        /// <summary>
        /// Wait for the expected condition is satisfied, return immediately
        /// </summary>
        /// <param name="expectedCondition"></param>
        public void WaitForPage(string title)
        {
            WebDriverWait webDriverWait = new WebDriverWait(this.wd, TimeSpan.FromSeconds(10.0));
            webDriverWait.Until<bool>((IWebDriver d) => d.Title.ToLower().StartsWith(title.ToLower()));
        }
        /// <summary>
        /// Load a new web page in current browser
        /// </summary>
        /// <param name="url"></param>
        public void GoToUrl(string url)
        {
            this.wd.Navigate().GoToUrl(url);
        }
        public void Refresh()
        {
            this.wd.Navigate().Refresh();
        }
        public void Back()
        {
            this.wd.Navigate().Back();
        }
        public void Forward()
        {
            this.wd.Navigate().Forward();
        }
        /// <summary>
        /// Get the url of current browser window
        /// </summary>
        /// <returns></returns>
        public string GetUrl()
        {
            return this.wd.Url;
        }
        /// <summary>
        /// Get page title of current browser window
        /// </summary>
        /// <returns></returns>
        public string GetPageTitle()
        {
            return this.wd.Title;
        }
        /// <summary>
        /// Set focus to a browser window with a specified title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="exactMatch"></param>
        public void GoToWindow(string title, bool exactMatch)
        {
            string currentWindowHandle = this.wd.CurrentWindowHandle;
            IList<string> windowHandles = this.wd.WindowHandles;
            if (exactMatch)
            {
                foreach (string current in windowHandles)
                {
                    this.wd.SwitchTo().Window(current);
                    bool flag = this.wd.Title.ToLower() == title.ToLower();
                    if (flag)
                    {
                        return;
                    }
                }
            }
            else
            {
                foreach (string current2 in windowHandles)
                {
                    this.wd.SwitchTo().Window(current2);
                    bool flag2 = this.wd.Title.ToLower().Contains(title.ToLower());
                    if (flag2)
                    {
                        return;
                    }
                }
            }
            this.wd.SwitchTo().Window(currentWindowHandle);
        }
        /// <summary>
        /// Set focus to a frame with a specified name
        /// </summary>
        /// <param name="name"></param>
        public void GoToFrame(string name)
        {
            IWebElement webElement = null;
            ReadOnlyCollection<IWebElement> readOnlyCollection = this.wd.FindElements(By.TagName("iframe"));
            foreach (IWebElement current in readOnlyCollection)
            {
                bool flag = current.GetAttribute("name").ToLower() == name.ToLower();
                if (flag)
                {
                    webElement = current;
                    break;
                }
            }
            bool flag2 = webElement != null;
            if (flag2)
            {
                this.wd.SwitchTo().Frame(webElement);
            }
        }
        public void GoToFrame(IWebElement frame)
        {
            this.wd.SwitchTo().Frame(frame);
        }
        /// <summary>
        /// Switch to default after going to a frame
        /// </summary>
        public void GoToDefault()
        {
            this.wd.SwitchTo().DefaultContent();
        }
        /// <summary>
        /// Get the alert text
        /// </summary>
        /// <returns></returns>
        public string GetAlertString()
        {
            string result = string.Empty;
            IAlert alert = this.wd.SwitchTo().Alert();
            bool flag = alert != null;
            if (flag)
            {
                result = alert.Text;
            }
            return result;
        }
        /// <summary>
        /// Accepts the alert
        /// </summary>
        public void AlertAccept()
        {
            IAlert alert = this.wd.SwitchTo().Alert();
            bool flag = alert != null;
            if (flag)
            {
                alert.Accept();
            }
        }
        /// <summary>
        /// Dismisses the alert
        /// </summary>
        public void AlertDismiss()
        {
            IAlert alert = this.wd.SwitchTo().Alert();
            bool flag = alert != null;
            if (flag)
            {
                alert.Dismiss();
            }
        }
        /// <summary>
        /// Find the element of a specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IWebElement FindElementById(string id)
        {
            return this.wd.FindElement(By.Id(id));
        }
        /// <summary>
        /// Find the element of a specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWebElement FindElementByName(string name)
        {
            return this.wd.FindElement(By.Name(name));
        }
        /// <summary>
        /// Find the element by xpath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public IWebElement FindElementByXPath(string xpath)
        {
            return this.wd.FindElement(By.XPath(xpath));
        }
        public IWebElement FindElementByLinkText(string text)
        {
            IWebElement result = null;
            try
            {
                result = this.wd.FindElement(By.LinkText(text));
            }
            catch
            {
            }
            return result;
        }
        public IList<IWebElement> FindElementsByLinkText(string text)
        {
            return this.wd.FindElements(By.LinkText(text));
        }
        public IList<IWebElement> FindElementsByPartialLinkText(string text)
        {
            return this.wd.FindElements(By.PartialLinkText(text));
        }
        public IList<IWebElement> FindElementsByClassName(string clsName)
        {
            return this.wd.FindElements(By.ClassName(clsName));
        }
        public IList<IWebElement> FindElementsByTagName(string tagName)
        {
            return this.wd.FindElements(By.TagName(tagName));
        }
        public IList<IWebElement> FindElementsByCssSelector(string css)
        {
            return this.wd.FindElements(By.CssSelector(css));
        }
        public IList<IWebElement> FindElementsByXPathName(string xpath)
        {
            return this.wd.FindElements(By.XPath(xpath));
        }
        public void ClickElement(IWebElement element)
        {
            new Actions(this.wd).Click(element).Perform();
        }
        public void DoubleClickElement(IWebElement element)
        {
            new Actions(this.wd).DoubleClick(element).Perform();
        }
        public void ClickAndHoldOnElement(IWebElement element)
        {
            new Actions(this.wd).ClickAndHold(element).Perform();
        }
        public void ContextClickOnElement(IWebElement element)
        {
            new Actions(this.wd).ContextClick(element).Perform();
        }
        public void DragAndDropElement(IWebElement source, IWebElement target)
        {
            new Actions(this.wd).DragAndDrop(source, target).Perform();
        }
        public void SendKeysToElement(IWebElement element, string text)
        {
            new Actions(this.wd).SendKeys(element, text).Perform();
        }
        /// <summary>
        /// Quit this server, close all windows associated to it
        /// </summary>
        public void Cleanup()
        {
            this.wd.Quit();
        }
    }
}