using System.Linq;
using OpenQA.Selenium;

namespace SeleniumAutomationUtils.SeleniumExtensions
{
    public static class ByExtensions
    {
        public static string Selector(this By @by)
        {
            var selector = @by.ToString().Split(": ").Last();
            return selector;
        }
    }
}
