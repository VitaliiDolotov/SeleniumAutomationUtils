using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumAutomationUtils.Utils
{
    public class FluentWait : IWait<IWebDriver>
    {
        private readonly TimeSpan _deafultTimeout = TimeSpan.FromSeconds(5);
        private IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private FluentWait(IWebDriver webDriver)
        {
            _wait = new WebDriverWait(webDriver, _deafultTimeout);
            _driver = webDriver;
        }

        public void IgnoreExceptionTypes(params Type[] exceptionTypes)
        {
            _wait.IgnoreExceptionTypes(exceptionTypes);
        }

        public TResult Until<TResult>(Func<IWebDriver, TResult> condition)
        {
            return _wait.Until(condition);
        }

        public TimeSpan Timeout
        {
            get => _wait.Timeout;
            set => _wait.Timeout = value;
        }

        public TimeSpan PollingInterval
        {
            get => _wait.PollingInterval;
            set => _wait.PollingInterval = value;
        }

        public string Message
        {
            get => _wait.Message;
            set => _wait.Message = value;
        }

        public static FluentWait Create(IWebDriver webDriver)
        {
            return new FluentWait(webDriver);
        }

        public FluentWait WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public FluentWait WithPollingInterval(TimeSpan pollingInterval)
        {
            PollingInterval = pollingInterval;
            return this;
        }

        public FluentWait WithExceptionsToIgnore(params Type[] exceptionTypes)
        {
            IgnoreExceptionTypes(exceptionTypes);
            return this;
        }
    }
}
