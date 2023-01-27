using Microsoft.VisualBasic;
using OpenQA.Selenium;
using static SeleniumAutomationUtils.SeleniumExtensions.WebDriverExtensions;

namespace SeleniumAutomationUtils.Components
{
    public class Properties
    {
        public By ParentSelector = null;

        public IWebElement Parent = null;

        public TriState Displayed = TriState.True;

        public TriState Exist = TriState.UseDefault;

        public WaitTime WaitTime = WaitTime.Medium;

        // Page factory will use Driver as context for factory
        public bool InitWithoutContext = false;
    }
}
