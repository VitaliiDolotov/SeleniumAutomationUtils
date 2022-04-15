using OpenQA.Selenium;
using SeleniumAutomationUtils.Pages;
using WebDriverExtensions = SeleniumAutomationUtils.SeleniumExtensions.WebDriverExtensions;

namespace SeleniumAutomationUtils.Components
{
    public interface IWebComponent : IContextContainer
    {
        WebDriver Driver { get; set; }

        string Identifier { get; set; }

        By Frame { get; }

        WebDriverExtensions.WaitTime WaitTime { set; }
    }
}
