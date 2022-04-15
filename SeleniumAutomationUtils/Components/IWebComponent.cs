using OpenQA.Selenium;
using SeleniumAutomationUtils.Pages;
using WebDriverExtensions = SeleniumAutomationUtils.SeleniumExtensions.WebDriverExtensions;

namespace SeleniumAutomationUtils.Components
{
    public interface IWebComponent : IContextContainer, IFrameContainer
    {
        WebDriver Driver { get; set; }

        string Identifier { get; set; }

        WebDriverExtensions.WaitTime WaitTime { set; }
    }
}
