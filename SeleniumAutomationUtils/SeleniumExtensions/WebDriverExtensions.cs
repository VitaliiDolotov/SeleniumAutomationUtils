using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumAutomationUtils.Components;
using SeleniumAutomationUtils.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace SeleniumAutomationUtils.SeleniumExtensions
{
    public static class WebDriverExtensions
    {
        private static List<int> _refreshAttemptList = new List<int>();

        public enum WaitTime
        {
            [System.ComponentModel.Description("1")]
            Second,
            [System.ComponentModel.Description("6")]
            Short,
            [System.ComponentModel.Description("15")]
            Medium,
            [System.ComponentModel.Description("30")]
            Long,
            [System.ComponentModel.Description("55")]
            ExtraLong
        }

        #region NowAt

        public static T NowAt<T>(this WebDriver driver, bool pageIdentitySelectorsDisplayed = true) where T : SeleniumBasePage, new()
        {
            var page = new T { Driver = driver, Actions = new Actions(driver) };
            driver.WaitForLoadingElements(page, null, pageIdentitySelectorsDisplayed);
            page.InitElements();
            return page;
        }

        public static T NowAtWithContext<T>(this WebDriver driver, bool pageIdentitySelectorsDisplayed = true) where T : SeleniumBasePage, new()
        {
            var page = new T { Driver = driver, Actions = new Actions(driver) };
            driver.WaitForLoadingElements(page, null, pageIdentitySelectorsDisplayed);
            var contextPage = Activator.CreateInstance(typeof(T)) as IContextContainer;
            page.InitElements(contextPage.Context);
            return page;
        }

        public static T NowAtWithoutWait<T>(this WebDriver driver) where T : SeleniumBasePage, new()
        {
            var page = new T { Driver = driver, Actions = new Actions(driver) };
            page.InitElements();
            return page;
        }

        public static void WaitForLoadingElements(this WebDriver driver, SeleniumBasePage page, By bySelector, bool pageIdentitySelectorsDisplayed)
        {
            var bys = bySelector != null ? new List<By> { bySelector } : page.GetPageIdentitySelectors();

            foreach (var by in bys)
            {
                if (pageIdentitySelectorsDisplayed)
                {
                    driver.WaitForElementToBeDisplayed(by);
                }
                else
                {
                    driver.WaitForElementsToBeExists(by);
                }
            }
        }

        #endregion

        #region Availability of element

        public static bool IsElementDisplayed(this WebDriver driver, IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this WebDriver driver, IWebElement element, WaitTime waitTime)
        {
            try
            {
                driver.WaitForElementToBeDisplayed(element, waitTime);
                return element.Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this WebDriver driver, By selector)
        {
            try
            {
                return driver.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this WebDriver driver, By selector, WaitTime waitTime)
        {
            try
            {
                driver.WaitForElementToBeDisplayed(selector, waitTime);
                return driver.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementInElementDisplayed(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime)
        {
            try
            {
                driver.WaitForElementInElementToBeDisplayed(element, selector, waitTime);
                return element.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementExists(this IWebDriver driver, By @by)
        {
            try
            {
                driver.FindElement(@by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static bool IsElementExists(this IWebDriver driver, IWebElement element)
        {
            try
            {
                if (element == null)
                    return false;

                if (element.TagName.Contains("Exception"))
                    return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public static bool IsElementExists(this WebDriver driver, By @by, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                WaitForElementToBeInExistsCondition(driver, @by, true, time);
                return driver.IsElementExists(@by);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementInElementExists(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime)
        {
            try
            {
                driver.WaitForElementInElementToBeExists(element, selector, waitTime);
                return IsElementInElementExists(driver, element, selector);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementInElementExists(this WebDriver driver, IWebElement element, By selector)
        {
            try
            {
                var elementInElement = element.FindElement(selector);
                return IsElementExists(driver, elementInElement);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Actions

        public static void ClickByActions(this WebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.Click(element).Perform();
        }

        public static void ClickElementLeftCenter(this WebDriver driver, IWebElement element)
        {
            var width = element.Size.Width;
            var height = element.Size.Height;
            Actions action = new Actions(driver);
            action.MoveToElement(element, width / 4, height / 2).Click().Build().Perform();
        }

        public static void DoubleClick(this WebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.DoubleClick(element).Build().Perform();
        }

        public static void ContextClick(this WebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.ContextClick(element).Build().Perform();
        }

        public static void HoverAndClick(this WebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(element).Click(element).Perform();
        }

        public static void MoveToElement(this WebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(element).Perform();
        }

        public static void MoveToElement(this WebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            Actions action = new Actions(driver);
            action.MoveToElement(element).Perform();
        }

        public static void DragAndDrop(this WebDriver driver, IWebElement elementToBeMoved,
            IWebElement moveToElement)
        {
            Actions action = new Actions(driver);
            action.DragAndDrop(elementToBeMoved, moveToElement).Perform();
        }

        public static void InsertFromClipboard(this WebDriver driver, IWebElement textbox)
        {
            Actions action = new Actions(driver);
            //TODO: below code stopped work on Aug 13 2019; splitted into 2 rows
            //action.Click(textbox).SendKeys(Keys.Shift + Keys.Insert).Build()
            //.Perform();
            action.Click(textbox);
            textbox.SendKeys(Keys.Shift + Keys.Insert);

            action.KeyUp(Keys.Shift).Build().Perform();
        }

        public static void SearchOnPage(this WebDriver driver)
        {
            Actions action = new Actions(driver);
            action.KeyDown(Keys.Control).SendKeys("F").Build().Perform();
            action.KeyUp(Keys.Control).Build().Perform();
        }

        #endregion Actions

        #region Actions with Javascript

        public static void ClickByJavascript(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript("arguments[0].click();", element);
        }

        public static void ClearByJavascript(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript("arguments[0].value = '';", element);
        }

        public static void SendKeyByJavascript(this WebDriver driver, IWebElement element, string str)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript($"arguments[0].value = '{str}';", element);
        }

        public static void MouseHoverByJavascript(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript("arguments[0].scrollIntoView(true);", element);
        }

        public static void SetAttributeByJavascript(this WebDriver driver, IWebElement element, string attribute,
            string text)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript($"arguments[0].setAttribute('{attribute}', '{text}')", element);
        }

        public static String GetNetworkLogByJavascript(this WebDriver driver)
        {
            String scriptToExecute = "var performance = window.performance  || window.mozPerformance  || window.msPerformance  || window.webkitPerformance || {}; var network = performance.getEntries() || {}; return JSON.stringify(network);";
            IJavaScriptExecutor ex = driver;
            var netData = ex.ExecuteScript(scriptToExecute);
            return netData.ToString();
        }

        public static bool IsElementHaveVerticalScrollbar(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            bool result = (bool)ex.ExecuteScript("return arguments[0].scrollHeight > arguments[0].clientHeight", element);
            return result;
        }

        public static bool IsElementHaveHorizontalScrollbar(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            bool result = (bool)ex.ExecuteScript("return arguments[0].scrollHeight > arguments[0].clientHeight", element);
            return result;
        }

        public static void ScrollGridToTheTop(this WebDriver driver, IWebElement gridElement)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript($"arguments[0].scrollTop = 0;", gridElement);
        }

        public static void ScrollGridToTheLeft(this WebDriver driver, IWebElement gridElement)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript($"arguments[0].scrollLeft = 0;", gridElement);
        }

        public static void ScrollGridToTheEnd(this WebDriver driver, IWebElement gridElement)
        {
            IJavaScriptExecutor ex = driver;

            var clientHeight = int.Parse(ex.ExecuteScript("return arguments[0].clientHeight", gridElement).ToString());
            if (clientHeight <= 0)
                throw new Exception("Unable to get client Height");
            var scrollHeight = int.Parse(ex.ExecuteScript("return arguments[0].scrollHeight", gridElement).ToString());

            for (int i = 0; i < scrollHeight / clientHeight; i++)
            {
                ex.ExecuteScript($"arguments[0].scrollTo(0,{clientHeight * i});", gridElement);
            }
            //Final scroll to get to the grid bottom
            ex.ExecuteScript($"arguments[0].scrollTo(0,{scrollHeight});", gridElement);
        }

        public static Int64 HorizontalScrollPosition(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            var result = (Int64)ex.ExecuteScript("return arguments[0].scrollLeft", element);
            return result;
        }

        public enum Direction
        {
            Right,
            Left
        }

        public static void ScrollHorizontalyTo(this WebDriver driver, Direction direction, IWebElement element, int percentage = 100)
        {
            IJavaScriptExecutor ex = driver;
            var clientWidth = int.Parse(ex.ExecuteScript("return arguments[0].clientWidth", element).ToString());
            int percentageOfScroll = clientWidth * percentage / 100;
            if (direction == Direction.Right)
            {
                ex.ExecuteScript($"arguments[0].scrollBy({percentageOfScroll}, 0)", element);
            }
            else if (direction == Direction.Left)
            {
                ex.ExecuteScript($"arguments[0].scrollBy(-{percentageOfScroll}, 0)", element);
            }
            else
            {
                throw new Exception("There is no such 'Direction'. Use Direction.Right or Direction.Left");
            }
        }

        public static List<string> GetElementAttributes(this WebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            var attributesAndValues = (Dictionary<string, object>)ex.ExecuteScript("var items = { }; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", element);
            var attributes = attributesAndValues.Keys.ToList();
            return attributes;
        }

        public static string GetSelectedText(this WebDriver driver)
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript("return window.getSelection().toString()").ToString();
        }

        public static string GetPseudoElementValue(this WebDriver driver, IWebElement element, Pseudo pseudo, string value)
        {
            string script = $"return window.getComputedStyle(arguments[0], ':{pseudo.GetValue()}').getPropertyValue('{value}');";
            return driver.ExecuteScript(script, element).ToString().Trim('"');
        }

        public enum Pseudo
        {
            [Description("before")]
            Before,
            [Description("after")]
            After
        }

        #endregion Actions with Javascript

        #region JavaSctipt Alert

        public static void AcceptAlert(this WebDriver driver, WaitTime waitTime = WaitTime.Short)
        {
            WaitForAlert(driver, waitTime);
            driver.SwitchTo().Alert().Accept();
        }

        public static void DismissAlert(this WebDriver driver, WaitTime waitTime = WaitTime.Short)
        {
            WaitForAlert(driver, waitTime);
            driver.SwitchTo().Alert().Dismiss();
        }

        public static bool IsAlertPresent(this WebDriver driver, WaitTime waitTime = WaitTime.Short)
        {
            try
            {
                WaitForAlert(driver, waitTime);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WaitForAlert(this WebDriver driver, WaitTime waitTime = WaitTime.Short)
        {
            try
            {
                var waitSec = int.Parse(waitTime.GetValue());
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(AlertToBeExists());
            }
            catch (Exception)
            {
                throw new Exception($"Alert wat not appears in {waitTime.GetValue()} seconds");
            }
        }

        private static Func<IWebDriver, bool> AlertToBeExists()
        {
            return (driver) =>
            {
                try
                {
                    driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed

        public static void WaitForElementToBeNotDisplayed(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayCondition(driver, element, false, waitSec);
        }

        public static void WaitForElementToBeDisplayed(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayCondition(driver, element, true, waitSec);
        }

        public static void WaitForElementToBeDisplayed(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayCondition(driver, locator, true, waitSec);
        }

        public static void WaitForElementToBeNotDisplayed(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayCondition(driver, locator, false, waitSec);
        }

        internal static void WaitForElementDisplayCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedCondition(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayCondition(this WebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedCondition(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInDisplayedCondition(By locator, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    //If no elements found
                    if (!elements.Any())
                        return false.Equals(displayedCondition);
                    return elements.Any(x => x.Displayed().Equals(displayedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedCondition(IWebElement element, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Displayed().Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (NullReferenceException)
                {
                    // Return false as element not exists
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed in Element

        public static void WaitForElementInElementToBeNotDisplayed(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementDisplayCondition(driver, element, selector, false, waitSec);
        }

        public static void WaitForElementInElementToBeDisplayed(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementDisplayCondition(driver, element, selector, true, waitSec);
        }

        public static void WaitForElementInElementDisplayCondition(this WebDriver driver, IWebElement element, By selector, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementInElementIsInDisplayedCondition(element, selector, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element in element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementInElementIsInDisplayedCondition(IWebElement element, By selector, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.FindElement(selector).Displayed().Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Returns false because the element is not present in DOM
                    return false.Equals(displayedCondition);
                }
                catch (NullReferenceException)
                {
                    // Returns false because the element not exists
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed After Refresh

        public static void WaitForElementToBeNotDisplayedAfterRefresh(this WebDriver driver, IWebElement element, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayConditionAfterRefresh(driver, element, false, waitSec, waitForDataLoadingMethod);
        }

        //Only elements from PageObject are allowed!!!
        public static void WaitForElementToBeDisplayedAfterRefresh(this WebDriver driver, IWebElement element, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayConditionAfterRefresh(driver, element, true, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToBeDisplayedAfterRefresh(this WebDriver driver, IWebElement element, By by, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayConditionAfterRefresh(driver, element, by, true, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToBeDisplayedAfterRefresh(this WebDriver driver, By locator, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayConditionAfterRefresh(driver, locator, true, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToBeNotDisplayedAfterRefresh(this WebDriver driver, By locator, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementDisplayConditionAfterRefresh(driver, locator, false, waitSec, waitForDataLoadingMethod);
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this WebDriver driver, By by, bool condition, int waitSeconds, Action<WebDriver> waitForDataLoadingMethod)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(by, condition, waitForDataLoadingMethod, refreshAction => { }));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this WebDriver driver, IWebElement element, bool condition, int waitSeconds, Action<WebDriver> waitForDataLoadingMethod)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(element, condition, waitForDataLoadingMethod, refreshAction => { }));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this WebDriver driver, IWebElement element, By by, bool condition, int waitSeconds, Action<WebDriver> waitForDataLoadingMethod)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(element, by, condition, waitForDataLoadingMethod, refreshAction => { }));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(By locator, bool displayedCondition, Action<WebDriver> waitForDataLoadingMethod, Action<WebDriver> refresh)
        {
            return (driver) =>
            {
                try
                {
                    refresh((WebDriver)driver);
                    refresh = RefreshPage;

                    waitForDataLoadingMethod((WebDriver)driver);

                    return IsElementDisplayed((WebDriver)driver, locator, WebDriverExtensions.WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(IWebElement element, bool displayedCondition, Action<WebDriver> waitForDataLoadingMethod, Action<WebDriver> refresh)
        {
            return (driver) =>
            {
                try
                {
                    refresh((WebDriver)driver);
                    refresh = RefreshPage;

                    waitForDataLoadingMethod((WebDriver)driver);

                    return IsElementDisplayed((WebDriver)driver, element, WebDriverExtensions.WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(IWebElement element, By by, bool displayedCondition, Action<WebDriver> waitForDataLoadingMethod, Action<WebDriver> refresh)
        {
            return (driver) =>
            {
                try
                {
                    refresh((WebDriver)driver);
                    refresh = RefreshPage;

                    waitForDataLoadingMethod((WebDriver)driver);

                    return IsElementInElementDisplayed((WebDriver)driver, element, by,
                        WebDriverExtensions.WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for ElementS to be (not) Displayed

        public static void WaitForElementsToBeNotDisplayed(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium, bool allElements = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            if (allElements)
                WaitForElementsDisplayCondition(driver, by, false, waitSec);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, by, false, waitSec);
        }

        public static void WaitForElementsToBeNotDisplayed(this WebDriver driver, List<By> bys, WaitTime waitTime = WaitTime.Medium, bool allElements = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            if (allElements)
                WaitForElementsDisplayCondition(driver, bys, false, waitSec);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, bys, false, waitSec);
        }

        public static void WaitForElementsToBeNotDisplayed(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsDisplayCondition(driver, elements, false, waitSec);
        }

        public static void WaitForElementsToBeDisplayed(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium, bool allElements = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            if (allElements)
                WaitForElementsDisplayCondition(driver, by, true, waitSec);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, by, true, waitSec);
        }

        public static void WaitForElementsToBeDisplayed(this WebDriver driver, List<By> bys, WaitTime waitTime = WaitTime.Medium, bool allElements = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            if (allElements)
                WaitForElementsDisplayCondition(driver, bys, true, waitSec);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, bys, true, waitSec);
        }

        public static void WaitForElementsToBeDisplayed(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium, bool allElements = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            if (allElements)
                WaitForElementsDisplayCondition(driver, elements, true, waitSec);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, elements, true, waitSec);
        }

        private static void WaitForElementsDisplayCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAllElementsLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsDisplayCondition(this WebDriver driver, List<By> bys, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAllElementsLocatedBy(bys, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{bys}' selectors were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsDisplayCondition(this WebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAllElementsLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForAtLeastOneElementDisplayCondition(this WebDriver driver, List<By> bys, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAtLeastOneElementLocatedBy(bys, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{bys}' selectors were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForAtLeastOneElementDisplayCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAtLeastOneElementLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForAtLeastOneElementDisplayCondition(this WebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAtLeastOneElementLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAllElementsLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    //If we expect some elements to be displayed
                    //The some elements should be found
                    if (expectedCondition && elements.Count <= 0)
                    {
                        return false;
                    }
                    return elements.All(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAllElementsLocatedBy(List<By> locators, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    List<IWebElement> elements = new List<IWebElement>();
                    foreach (By locator in locators)
                    {
                        elements.AddRange(driver.FindElements(locator));
                    }

                    //If we expect some elements to be displayed
                    //The some elements should be found
                    if (expectedCondition && elements.Count <= 0)
                    {
                        return false;
                    }

                    return elements.All(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAllElementsLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    //If we expect some elements to be displayed
                    //The some elements should be found
                    if (expectedCondition && elements.Count <= 0)
                    {
                        return false;
                    }

                    return elements.All(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAtLeastOneElementLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    return elements.Any(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAtLeastOneElementLocatedBy(List<By> locators, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    List<IWebElement> elements = new List<IWebElement>();
                    foreach (By locator in locators)
                    {
                        elements.AddRange(driver.FindElements(locator));
                    }
                    return elements.Any(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAtLeastOneElementLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.Any(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Exists

        public static void WaitForElementToBeNotExists(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementToBeInExistsCondition(driver, by, false, waitSec);
        }

        public static void WaitForElementToBeNotExists(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementToBeInExistsCondition(driver, element, false, waitSec);
        }

        public static void WaitForElementToBeExists(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementToBeInExistsCondition(driver, by, true, waitSec);
        }

        public static void WaitForElementToBeExists(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementToBeInExistsCondition(driver, element, true, waitSec);
        }

        internal static void WaitForElementToBeInExistsCondition(this WebDriver driver, By by, bool expectedCondition, int waitTimeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTimeout));
                wait.Until(ElementExists(by, expectedCondition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception(
                    $"Element located by '{by}' selector was not in '{expectedCondition}' Exists condition after {waitTimeout} seconds", e);
            }
        }

        private static void WaitForElementToBeInExistsCondition(this WebDriver driver, IWebElement element, bool expectedCondition, int waitTimeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTimeout));
                wait.Until(ElementExists(element, expectedCondition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception(
                    $"Element was not in '{expectedCondition}' Exists condition after {waitTimeout} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementExists(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var existsState = IsElementExists(driver, element);
                    return existsState.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementExists(By selector, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var existsState = IsElementExists(driver, driver.FindElement(selector));
                    return existsState.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Exists in Element

        public static void WaitForElementInElementToBeNotExists(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementExistsCondition(driver, element, selector, false, waitSec);
        }

        public static void WaitForElementInElementToBeNotExists(this WebDriver driver, By parent, By child, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementExistsCondition(driver, parent, child, false, waitSec);
        }

        public static void WaitForElementInElementToBeExists(this WebDriver driver, IWebElement element, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementExistsCondition(driver, element, selector, true, waitSec);
        }

        public static void WaitForElementInElementToBeExists(this WebDriver driver, By parent, By child, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementInElementExistsCondition(driver, parent, child, true, waitSec);
        }

        internal static void WaitForElementInElementExistsCondition(this WebDriver driver, IWebElement element, By selector, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementInElementIsInExistsCondition(element, selector, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element in element was not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementInElementExistsCondition(this WebDriver driver, By parent, By child, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementInElementIsInExistsCondition(parent, child, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element in element was not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementInElementIsInExistsCondition(IWebElement element, By selector, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.IsElementExists(selector).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as element was staled
                    return false.Equals(displayedCondition);
                }
                catch (NullReferenceException)
                {
                    // Return false as element was not exists
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementInElementIsInExistsCondition(By parentElement, By childElement, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var parent = driver.FindElement(parentElement);
                    return parent.IsElementExists(childElement).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as element was staled
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for ElementS to be (not) Exists

        public static void WaitForElementsToBeExists(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsExistsCondition(driver, by, true, waitSec);
        }

        public static void WaitForElementsToBeExists(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsExistsCondition(driver, elements, true, waitSec);
        }

        public static void WaitForElementsToBeNotExists(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsExistsCondition(driver, by, false, waitSec);
        }

        public static void WaitForElementsToBeNotExists(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsExistsCondition(driver, elements, false, waitSec);
        }

        private static void WaitForElementsExistsCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ExistsConditionOfElementsLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsExistsCondition(this WebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ExistsConditionOfElementsLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ExistsConditionOfElementsLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    return elements.All(element => IsElementExists(driver, element).Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> ExistsConditionOfElementsLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.All(element => IsElementExists(driver, element).Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        #endregion

        #region Wait for text in Element after refresh

        public static void WaitForElementToNotContainsTextAfterRefresh(this WebDriver driver, IWebElement element, string expectedText, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextAfterRefresh(driver, element, expectedText, false, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToNotContainsTextAfterRefresh(this WebDriver driver, By selector, string expectedText, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextAfterRefresh(driver, selector, expectedText, false, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToContainsTextAfterRefresh(this WebDriver driver, IWebElement element, string expectedText, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextAfterRefresh(driver, element, expectedText, true, waitSec, waitForDataLoadingMethod);
        }

        public static void WaitForElementToContainsTextAfterRefresh(this WebDriver driver, By selector, string expectedText, Action<WebDriver> waitForDataLoadingMethod, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextAfterRefresh(driver, selector, expectedText, true, waitSec, waitForDataLoadingMethod);
        }

        private static void WaitElementContainsTextAfterRefresh(this WebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec, Action<WebDriver> waitForDataLoadingMethod)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAfterRefresh(element, expectedText, condition, waitForDataLoadingMethod));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsTextAfterRefresh(this WebDriver driver, By by, string expectedText, bool condition, int waitSec, Action<WebDriver> waitForDataLoadingMethod)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAfterRefresh(by, expectedText, condition, waitForDataLoadingMethod));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAfterRefresh(IWebElement element, string text, bool condition, Action<WebDriver> waitForDataLoadingMethod)
        {
            return (driver) =>
            {
                try
                {
                    WaitForElementToBeDisplayedAfterRefresh((WebDriver)driver, element, waitForDataLoadingMethod);

                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (TimeoutException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAfterRefresh(By by, string text, bool condition, Action<WebDriver> waitForDataLoadingMethod)
        {
            return (driver) =>
            {
                try
                {
                    WaitForElementToBeDisplayedAfterRefresh((WebDriver)driver, by, waitForDataLoadingMethod);

                    var element = driver.FindElement(by);
                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (TimeoutException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Element attribute

        public static void WaitForElementToNotContainsTextInAttribute(this WebDriver driver, IWebElement element, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInAttribute(driver, element, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToNotContainsTextInAttribute(this WebDriver driver, By selector, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInAttribute(driver, selector, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToContainsTextInAttribute(this WebDriver driver, IWebElement element, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInAttribute(driver, element, expectedText, attribute, true, waitSec);
        }

        public static void WaitForElementToContainsTextInAttribute(this WebDriver driver, By selector, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInAttribute(driver, selector, expectedText, attribute, true, waitSec);
        }

        public static void WaitForAnyElementToContainsTextInAttribute(this WebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInAttribute(driver, elements, expectedText, attribute, true, waitSec);
        }

        private static void WaitElementContainsTextInAttribute(this WebDriver driver, IWebElement element, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(element, expectedText, attribute, condition));
            }
            catch (Exception e)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute after {waitSec} seconds: {e}");
            }
        }

        private static void WaitElementContainsTextInAttribute(this WebDriver driver, By by, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(by, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsTextInAttribute(this WebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(elements, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' elements attribute after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(IWebElement element, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetAttribute(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(By by, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetAttribute(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(IEnumerable<IWebElement> elements, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.Any(x => x.GetAttribute(attribute).Contains(text).Equals(condition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Element cssValue

        public static void WaitForElementToNotContainsTextInCssValue(this WebDriver driver, IWebElement element, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInCssValue(driver, element, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToNotContainsTextInCssValue(this WebDriver driver, By selector, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInCssValue(driver, selector, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToContainsTextInCssValue(this WebDriver driver, IWebElement element, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInCssValue(driver, element, expectedText, attribute, true, waitSec);
        }

        public static void WaitForElementToContainsTextInCssValue(this WebDriver driver, By selector, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsTextInCssValue(driver, selector, expectedText, attribute, true, waitSec);
        }

        public static void WaitForAnyElementToContainsTextInCssValue(this WebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsTextInCssValue(driver, elements, expectedText, attribute, true, waitSec, false);
        }

        public static void WaitForAllElementsToContainsTextInCssValue(this WebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsTextInCssValue(driver, elements, expectedText, attribute, true, waitSec, true);
        }

        private static void WaitElementContainsTextInCssValue(this WebDriver driver, IWebElement element, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementCssValue(element, expectedText, attribute, condition));
            }
            catch (Exception e)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute after {waitSec} seconds: {e}");
            }
        }

        private static void WaitElementContainsTextInCssValue(this WebDriver driver, By by, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementCssValue(by, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementsContainsTextInCssValue(this WebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, bool condition, int waitSec, bool allElements)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementCssValue(elements, expectedText, attribute, condition, allElements));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' elements attribute after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementCssValue(IWebElement element, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetCssValue(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementCssValue(By by, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetCssValue(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementCssValue(IEnumerable<IWebElement> elements, string text, string attribute, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    return allElements ?
                        elements.All(x => x.GetCssValue(attribute).Contains(text).Equals(condition)) :
                        elements.Any(x => x.GetCssValue(attribute).Contains(text).Equals(condition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element(s) contains (not) text

        public static void WaitForElementToNotContainsText(this WebDriver driver, IWebElement element, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsText(driver, element, expectedText, false, waitSec);
        }

        public static void WaitForElementToNotContainsText(this WebDriver driver, By selector, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsText(driver, selector, expectedText, false, waitSec);
        }

        public static void WaitForElementToContainsText(this WebDriver driver, IWebElement element, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsText(driver, element, expectedText, true, waitSec);
        }

        public static void WaitForElementToContainsText(this WebDriver driver, By selector, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementContainsText(driver, selector, expectedText, true, waitSec);
        }

        public static void WaitForAllElementsToContainsText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsText(driver, elements, expectedText, true, true, waitSec);
        }

        public static void WaitForAllElementsToNotContainsText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsText(driver, elements, expectedText, false, true, waitSec);
        }

        public static void WaitForSomeElementsToContainsText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsText(driver, elements, expectedText, true, false, waitSec);
        }

        public static void WaitForSomeElementsToNotContainsText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsContainsText(driver, elements, expectedText, false, false, waitSec);
        }

        private static void WaitElementContainsText(this WebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(element, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception(
                    $"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsText(this WebDriver driver, By by, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(by, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception(
                    $"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementsContainsText(this WebDriver driver, IList<IWebElement> elements, string expectedText, bool condition, bool allElements, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(ElementsToContainsText(elements, expectedText, condition, allElements));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(IWebElement element, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Text.Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(By by, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.Text.Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementsToContainsText(IList<IWebElement> elements, string text, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    if (allElements)
                        return elements.All(x => x.GetText().Contains(text)).Equals(condition);
                    else
                        return elements.Any(x => x.GetText().Contains(text)).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for exact text in Element(s)

        public static void WaitForElementToNotHaveText(this WebDriver driver, IWebElement element, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, element, expectedText, false, waitSec);
        }

        public static void WaitForElementToNotHaveText(this WebDriver driver, By selector, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, selector, expectedText, false, waitSec);
        }

        public static void WaitForElementToHaveText(this WebDriver driver, IWebElement element, string expectedText, WaitTime waitTime = WaitTime.Medium, bool throwException = true)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            try
            {
                WaitElementHaveText(driver, element, expectedText, true, waitSec);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementToHaveText(this WebDriver driver, By selector, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, selector, expectedText, true, waitSec);
        }

        public static void WaitForAllElementsToHaveText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, expectedText, true, waitSec, true);
        }

        public static void WaitForAllElementsToNotHaveText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, expectedText, false, waitSec, true);
        }

        public static void WaitForSomeElementsToHaveText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, expectedText, true, waitSec, false);
        }

        public static void WaitForSomeElementsToNotHaveText(this WebDriver driver, IList<IWebElement> elements, string expectedText, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, expectedText, false, waitSec, false);
        }

        private static void WaitElementHaveText(this WebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElement(element, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementHaveText(this WebDriver driver, By by, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElement(by, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementsHaveText(this WebDriver driver, IList<IWebElement> elements, string expectedText, bool condition, int waitSec, bool allElements)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElements(elements, expectedText, condition, allElements));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the elements after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(IWebElement element, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetText().Equals(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(By by, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetText().Equals(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElements(IList<IWebElement> elements, string text, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    if (allElements)
                    {
                        return elements.All(x => x.GetText().Equals(text).Equals(condition));
                    }
                    else
                    {
                        return elements.Any(x => x.GetText().Equals(text).Equals(condition));
                    }
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element have (not have) any text

        public static void WaitForElementToHaveText(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, element, true, waitSec);
        }

        public static void WaitForElementToNotHaveText(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, element, false, waitSec);
        }

        public static void WaitForElementToHaveText(this WebDriver driver, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, selector, true, waitSec);
        }

        public static void WaitForElementToNotHaveText(this WebDriver driver, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementHaveText(driver, selector, false, waitSec);
        }

        public static void WaitForAllElementsToHaveText(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, true, true, waitSec);
        }

        public static void WaitForSomeElementsToHaveText(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, true, false, waitSec);
        }

        public static void WaitForAllElementsToNotHaveText(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, false, true, waitSec);
        }

        public static void WaitForSomeElementsToNotHaveText(this WebDriver driver, IList<IWebElement> elements, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitElementsHaveText(driver, elements, false, false, waitSec);
        }

        private static void WaitElementHaveText(this WebDriver driver, IWebElement element, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElement(element, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementHaveText(this WebDriver driver, By by, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElement(by, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementsHaveText(this WebDriver driver, IList<IWebElement> elements, bool condition, bool allElements, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElements(elements, condition, allElements));
            }
            catch (Exception e)
            {
                throw new Exception($"Text is not appears/disappears in the some/all element after {waitSec} seconds: {e}");
            }
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return !string.IsNullOrEmpty(element.GetText()).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(By by, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return !string.IsNullOrEmpty(element.GetText()).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElements(IList<IWebElement> elements, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    if (allElements)
                    {
                        return elements.All(x => !string.IsNullOrEmpty(x.GetText()).Equals(condition));
                    }
                    else
                    {
                        return elements.Any(x => !string.IsNullOrEmpty(x.GetText()).Equals(condition));
                    }
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Enabled

        public static void WaitForElementToBeNotEnabled(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementEnabledCondition(driver, element, false, waitSec);
        }

        public static void WaitForElementToBeEnabled(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementEnabledCondition(driver, element, true, waitSec);
        }

        public static void WaitForElementToBeEnabled(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementEnabledCondition(driver, locator, true, waitSec);
        }

        public static void WaitForElementToBeNotEnabled(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementEnabledCondition(driver, locator, false, waitSec);
        }

        private static void WaitForElementEnabledCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInEnabledCondition(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Enabled condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementEnabledCondition(this WebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInEnabledCondition(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Enabled condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInEnabledCondition(By locator, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    //If no elements found
                    if (!elements.Any())
                        return false.Equals(condition);
                    return elements.Any(x => x.Enabled.Equals(condition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInEnabledCondition(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Enabled.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Clickable

        public static void WaitForElementToBeNotClickable(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementClickableCondition(driver, element, false, waitSec);
        }

        public static void WaitForElementToBeClickable(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementClickableCondition(driver, element, true, waitSec);
        }

        public static void WaitForElementToBeClickable(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementClickableCondition(driver, locator, true, waitSec);
        }

        public static void WaitForElementToBeNotClickable(this WebDriver driver, By locator, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementClickableCondition(driver, locator, false, waitSec);
        }

        private static void WaitForElementClickableCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInClickableCondition(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Clickable condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementClickableCondition(this WebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInClickableCondition(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Clickable condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInClickableCondition(By locator, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    element.Click();
                    //If no elements found
                    return true.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (ElementClickInterceptedException)
                {
                    // Element not clickable
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInClickableCondition(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    element.Click();
                    return true.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false.Equals(condition);
                }
                catch (ElementClickInterceptedException)
                {
                    // Element not clickable
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Element has child

        /// <summary>
        /// Wait while element do not have specified number of child elements
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="childSelector"></param>
        /// <param name="expectedCount"></param>
        public static void WaitForElementChildElements(this WebDriver driver, IWebElement element,
            By childSelector, int expectedCount, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(ElementContainsChild(element, childSelector, expectedCount));
            }
            catch (Exception)
            {
                throw new Exception($"Required number of child elements are not appears in the element after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> ElementContainsChild(IWebElement element, By selector, int childCount)
        {
            return (driver) =>
            {
                try
                {
                    return element.FindElements(selector).Count >= childCount;
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false;
                }
                catch (NullReferenceException)
                {
                    //Element not exists
                    return false;
                }
            };
        }

        #endregion

        #region Element to be (not) Selected

        public static void WaitForElementToBeSelected(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsSelectedCondition(driver, by, true, waitSec);
        }

        public static void WaitForElementToBeSelected(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsSelectedCondition(driver, element, true, waitSec);
        }

        public static void WaitForElementToBeNotSelected(this WebDriver driver, By by, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsSelectedCondition(driver, by, false, waitSec);
        }

        public static void WaitForElementToBeNotSelected(this WebDriver driver, IWebElement element, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WaitForElementsSelectedCondition(driver, element, false, waitSec);
        }

        private static void WaitForElementsSelectedCondition(this WebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(SelectedConditionOfElementLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Selected condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsSelectedCondition(this WebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(SelectedConditionOfElement(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Selected condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> SelectedConditionOfElementLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    return element.Selected.Equals(expectedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> SelectedConditionOfElement(IWebElement element, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Selected.Equals(expectedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Element not exists
                    return false;
                }
            };
        }

        #endregion

        public static void WaitForNewTab(this WebDriver driver, int tabNum, int seconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            wait.Until(d => d.WindowHandles.Count > tabNum - 1);
        }

        public static void WaitFor(this WebDriver driver, Func<bool> flag, int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (flag())
                    break;

                Thread.Sleep(1000);
            }
        }

        public static void Wait(this IWebDriver driver, Func<bool> condition, string message, int seconds = 30, bool throwException = true)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException),
                    typeof(TargetInvocationException),
                    typeof(InvalidElementStateException),
                    typeof(NullReferenceException),
                    typeof(ElementNotInteractableException),
                    typeof(StaleElementReferenceException));
                wait.Message = message;
                wait.Until(d => condition.Invoke());
            }
            catch (Exception e)
            {
                if (throwException)
                    throw e;
            }
        }

        #region Frames

        public static void SwitchToFrame(this WebDriver driver, int frameNumber, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
            driver.SwitchTo().DefaultContent();
            wait.Until(x => x.FindElements(By.TagName("iframe")).Count > frameNumber);
            var frames = driver.FindElements(By.TagName("iframe"));
            driver.SwitchTo().Frame(frames[frameNumber]);
        }

        public static void SwitchToFrame(this WebDriver driver, string frameIdName, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
            driver.SwitchTo().DefaultContent();
            wait.Until(x => x.FindElements(By.Id(frameIdName)));
            driver.SwitchTo().Frame(frameIdName);
        }

        public static void SwitchToFrame(this WebDriver driver, By selector, WaitTime waitTime = WaitTime.Medium)
        {
            var waitSec = int.Parse(waitTime.GetValue());
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
            driver.SwitchTo().DefaultContent();
            wait.Until(x => x.FindElements(selector));
            driver.SwitchTo().Frame(driver.FindElement(selector));
        }

        #endregion

        #region Checkbox

        public static void SetCheckboxStateByAction(this WebDriver driver, IWebElement checkbox, bool desiredState)
        {
            if (!checkbox.Selected.Equals(desiredState))
            {
                driver.ClickByActions(checkbox);
            }
        }

        #endregion

        #region Download File

        public static string GetFileWithName(this WebDriver driver, string fileName, string hubUri)
        {
            var session = driver.SessionId;
            var client = new RestClient(hubUri.Replace("wd/hub", string.Empty));
            var request = new RestRequest($"/download/{session}/{fileName}");
            var response = client.Get(request);
            for (var i = 0; i < 15; i++)
            {
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    break;
                }

                Thread.Sleep(3000);

                response = client.Get(request);
            }

            response.Validate(HttpStatusCode.OK, $"'{fileName}' file was not downloaded");

            var download = client.DownloadData(request);

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var tempDir = Path.Combine(dir, $"TempDownloadDirectory_{Guid.NewGuid().ToString("N").ToUpper()}");
            // Create temp dir for downloaded file
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, fileName);
            File.WriteAllBytes(filePath, download.ToArray());
            return filePath;
        }

        public static string GetFileWithNamePart(this WebDriver driver, string fileNamePart, string hubUri, int waitSeconds = 15)
        {
            var fileName = driver
                .ExecuteFunc(() => driver.GetDownloadedFiles(hubUri).First(x => x.Contains(fileNamePart)), waitSeconds);
            var filePath = GetFileWithName(driver, fileName, hubUri);
            return filePath;
        }

        public static List<string> GetDownloadedFiles(this WebDriver driver, string hubUri)
        {
            var session = driver.SessionId;
            var client = new RestClient(hubUri.Replace("wd/hub", string.Empty));
            var request = new RestRequest($"/download/{session}/?json");
            var response = client.Get(request);
            var result = JsonConvert.DeserializeObject<List<string>>(response.Content);
            return result;
        }

        #endregion

        #region Component

        public static T Component<T>(this WebDriver driver) where T : BaseWebComponent, new()
        {
            var component = new T { Driver = driver, Identifier = string.Empty };
            component.Build();
            return component;
        }

        public static T Component<T>(this WebDriver driver, Properties props) where T : BaseWebComponent, new()
        {
            var component = new T { Driver = driver, Identifier = string.Empty, Props = props };
            component.Build();
            return component;
        }

        public static T Component<T>(this WebDriver driver, string identifier) where T : BaseWebComponent, new()
        {
            var component = new T { Driver = driver, Identifier = identifier };
            component.Build();
            return component;
        }

        public static T Component<T>(this WebDriver driver, string identifier, Properties props) where T : BaseWebComponent, new()
        {
            var component = new T { Driver = driver, Identifier = identifier, Props = props };
            component.Build();
            return component;
        }

        public static IWebElement GetComponent<T>(this WebDriver driver) where T : BaseWebComponent, new()
        {
            var component = driver.Component<T>();
            return component.Instance;
        }

        public static IWebElement GetComponent<T>(this WebDriver driver, Properties props) where T : BaseWebComponent, new()
        {
            var component = driver.Component<T>(props);
            return component.Instance;
        }

        public static IWebElement GetComponent<T>(this WebDriver driver, string identifier) where T : BaseWebComponent, new()
        {
            var component = driver.Component<T>(identifier);
            return component.Instance;
        }

        public static IWebElement GetComponent<T>(this WebDriver driver, string identifier, Properties props) where T : BaseWebComponent, new()
        {
            var component = driver.Component<T>(identifier, props);
            return component.Instance;
        }

        public static bool ComponentExistsState<T>(this WebDriver driver) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>();
                component.Build();
                return driver.IsElementExists(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentExistsState<T>(this WebDriver driver, Properties props) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(props);
                component.Build();
                return driver.IsElementExists(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentExistsState<T>(this WebDriver driver, string identifier) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(identifier);
                component.Build();
                return driver.IsElementExists(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentExistsState<T>(this WebDriver driver, string identifier, Properties props) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(identifier, props);
                component.Build();
                return driver.IsElementExists(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentDisplayedState<T>(this WebDriver driver) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>();
                component.Build();
                return driver.IsElementDisplayed(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentDisplayedState<T>(this WebDriver driver, Properties props) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(props);
                component.Build();
                return driver.IsElementDisplayed(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentDisplayedState<T>(this WebDriver driver, string identifier) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(identifier);
                component.Build();
                return driver.IsElementDisplayed(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        public static bool ComponentDisplayedState<T>(this WebDriver driver, string identifier, Properties props) where T : BaseWebComponent, new()
        {
            try
            {
                var component = driver.Component<T>(identifier, props);
                component.Build();
                return driver.IsElementDisplayed(component.Instance);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Clipboard

        public static string GetClipboard(this WebDriver driver, string hubUri)
        {
            SessionId sessionId = driver.SessionId;

            RestClient client = new RestClient(hubUri.Replace("wd/hub", string.Empty));
            RestRequest restRequest = new RestRequest($"/clipboard/{sessionId}");
            IRestResponse restResponse = client.Get(restRequest);

            if (!restResponse.StatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Unable to get clipboard");
            }

            var content = restResponse.Content;
            return content;
        }

        public static void SetClipboard(this WebDriver driver, string hubUri, string data)
        {
            SessionId sessionId = driver.SessionId;

            RestClient client = new RestClient(hubUri.Replace("wd/hub", string.Empty));

            RestRequest request = new RestRequest($"/clipboard/{sessionId}");
            request.AddParameter("data", data);
            var restResponse = client.Post(request);

            if (!restResponse.StatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Unable to set clipboard");
            }
        }

        #endregion

        #region Settings

        private static void AllowFileDetection(this RemoteWebDriver driver)
        {
            IAllowsFileDetection allowsDetection = driver;
            allowsDetection.FileDetector = new LocalFileDetector();
        }

        #endregion

        private static void RefreshPage(WebDriver driver)
        {
            driver.Navigate().Refresh();
        }

        public static void OpenInNewTab(this WebDriver driver, string url)
        {
            driver.ExecuteScript($"window.open('{url}','_blank');");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }

        public static void PingDriver(this WebDriver driver, int minutes)
        {
            for (int i = 0; i < minutes * 6; i++)
            {
                driver.FindElement(By.XPath(".//body"));
                Thread.Sleep(10000);
            }
        }

        public static void PingDriver(this WebDriver driver)
        {
            driver.FindElement(By.XPath(".//body"));
        }

        // For cases with _driver.FindBy
        public static void ExecuteAction(this WebDriver driver, Action actionToDo, int retryCount = 5)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    actionToDo.Invoke();
                    return;
                }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(1000);
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (NullReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (TargetInvocationException)
                {
                    Thread.Sleep(1000);
                }
                catch (ElementClickInterceptedException)
                {
                    Thread.Sleep(1000);
                }
                catch (InvalidElementStateException)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// For cases when need to check exists or displayed state
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="actionToDo">() =></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public static bool ExecuteFunc(this WebDriver driver, Func<bool> actionToDo, int retryCount = 2)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return actionToDo.Invoke();
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        /// <summary>
        /// For cases when need to return some Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="driver"></param>
        /// <param name="actionToDo">() =></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public static T ExecuteFunc<T>(this WebDriver driver, Func<T> actionToDo, int retryCount = 2)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return (T)actionToDo.Invoke();
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            throw new Exception("Unable to execute Function for WebDriver");
        }

        /// <summary>
        /// This method is used to check the REALLY (with naked eye) visibility of an element
        /// </summary>
        /// <returns></returns>
        public static bool Visible(this WebDriver driver, IWebElement element)
        {
            try
            {
                return (bool)driver.ExecuteScript(
                    "var elem = arguments[0],                 " +
                    "  box = elem.getBoundingClientRect(),    " +
                    "  cx = box.left + box.width / 2,         " +
                    "  cy = box.top + box.height / 2,         " +
                    "  e = document.elementFromPoint(cx, cy); " +
                    "for (; e; e = e.parentElement) {         " +
                    "  if (e === elem)                        " +
                    "    return true;                         " +
                    "}                                        " +
                    "return false;                            "
                    , element);
            }
            catch (NoSuchElementException)
            {
                // Returns false because the element is not present in DOM.
                return false;
            }
        }
    }
}
