using OpenQA.Selenium;
using SeleniumAutomationUtils.Pages;
using WebDriverExtensions = SeleniumAutomationUtils.SeleniumExtensions.WebDriverExtensions;

namespace SeleniumAutomationUtils.Components
{
    public interface IWebComponent : IContextContainer
    {
        WebDriver Driver { get; set; }

        string Identifier { get; set; }

        // TODO Uncomment when logic will be implemented
        // By Frame { get; }

        WebDriverExtensions.WaitTime WaitTime { set; }
    }
}
