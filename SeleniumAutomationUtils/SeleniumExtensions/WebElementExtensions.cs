using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace SeleniumAutomationUtils.SeleniumExtensions
{
    public static class WebElementExtensions
    {
        public static bool Displayed(this IWebElement element)
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

        public static bool IsElementExists(this IWebElement element, By by)
        {
            try
            {
                element.FindElement(by);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Textbox 

        public static string GetText(this IWebElement element)
        {
            var elementText = !string.IsNullOrEmpty(element.Text) ? element.Text :
                element.IsAttributePresent("value") ? element.GetAttribute("value") : string.Empty;

            return elementText;
        }

        public static void ClearWithBackspaces(this IWebElement textbox, int charectersCount = 50)
        {
            for (int i = 0; i < charectersCount; i++)
            {
                textbox.SendKeys(Keys.Backspace);
                Thread.Sleep(40);
            }
        }

        public static void ClearWithHomeButton(this IWebElement textbox, WebDriver driver)
        {
            Actions action = new Actions(driver);
            action.Click(textbox).SendKeys(Keys.End).KeyDown(Keys.Shift).SendKeys(Keys.Home).KeyUp(Keys.Shift)
                .SendKeys(Keys.Backspace).Perform();
        }

        public static void SendkeysWithDelay(this IWebElement textbox, string input, int sleepMs = 100)
        {
            foreach (char letter in input)
            {
                textbox.SendKeys(letter.ToString());
                Thread.Sleep(sleepMs);
            }
        }

        #endregion

        #region Sendkeys

        public static void SendString(this IWebElement textbox, string text)
        {
            if (!string.IsNullOrEmpty(text))
                textbox.SendKeys(text);
        }

        public static void ClearSendKeys(this IWebElement textbox, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                textbox.Clear();
                textbox.SendKeys(text);
            }
        }

        #endregion

        #region Selectbox

        public static void SelectboxSelect(this IWebElement selectbox, string option, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                var selectElement = new SelectElement(selectbox);
                IList<IWebElement> options = selectElement.Options;
                for (int i = 0; i < options.Count; i++)
                    if (string.Equals(options[i].Text, option, StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectElement.SelectByIndex(i);
                        break;
                    }
            }
            else
            {
                var selectElement = new SelectElement(selectbox);
                selectElement.SelectByText(option);
            }
        }

        public static void SelectboxSelectByIndex(this IWebElement selectbox, int index)
        {
            var selectElement = new SelectElement(selectbox);
            IList<IWebElement> options = selectElement.Options;

            if (index > options.Count)
                throw new Exception("Index is greater than options count");
            else
            {
                selectElement.SelectByIndex(index);
            }
        }

        #endregion

        public static bool IsAttributePresent(this IWebElement element, string attribute)
        {
            try
            {
                var value = element.GetAttribute(attribute);
                return value != null;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IWebElement UntilElementHasChilds(this IWebElement element, WebDriver driver, By locator,
            TimeSpan timeOut, int childsCount = 4)
        {
            new WebDriverWait(driver, timeOut).Until(d => element.FindElements(locator).Count >= childsCount);

            return element;
        }
    }
}
