using System;
using System.Collections.Generic;
using System.Linq;
using AutomationUtils.Extensions;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using SeleniumAutomationUtils.SeleniumExtensions;
using SeleniumExtras.PageObjects;

namespace SeleniumAutomationUtils.Components
{
    public abstract class BaseWebComponent : IWebComponent
    {
        public virtual void CheckAutomationClass() { }

        public Properties Props = new Properties();

        protected IList<IWebElement> Components;

        protected abstract By Construct();

        public void Build()
        {
            if (!(Props.ParentSelector is null) && !(Props.Parent is null))
            {
                throw new Exception("Not allowed to use Parent element selector and Parent element together");
            }

            ParentSelector = Props.ParentSelector;
            Parent = Props.Parent;

            WaitTime = Props.WaitTime;

            var waitSec = int.Parse(WaitTime.GetValue());

            #region Parent element

            if (!(Props.ParentSelector is null))
            {
                if (!Driver.IsElementExists(Props.ParentSelector, WaitTime))
                {
                    // Throw new Exception($"Unable to find Parent element with '{Props.ParentSelector}' selector");
                    return;
                }

                Parent = Driver.FindElement(ParentSelector);
            }

            #endregion

            var selector = Construct();

            // Displayed check
            if (!Props.Displayed.Equals(TriState.UseDefault))
            {
                var displayedCondition = Props.Displayed.Equals(TriState.True);
                if (Parent is null)
                {
                    Driver.WaitForElementDisplayCondition(selector, displayedCondition, waitSec);
                }
                else
                {
                    Driver.WaitForElementInElementDisplayCondition(Parent, selector, displayedCondition, waitSec);
                }
            }

            // Exist check
            if (!Props.Exist.Equals(TriState.UseDefault))
            {
                var existCondition = Props.Exist.Equals(TriState.True);
                if (Parent is null)
                {
                    Driver.WaitForElementToBeInExistsCondition(selector, existCondition, waitSec);
                }
                else
                {
                    Driver.WaitForElementInElementExistsCondition(Parent, selector, existCondition, waitSec);
                }
            }

            if (Props.Displayed.Equals(TriState.True) || Props.Exist.Equals(TriState.True))
            {
                Components = Parent is null ? Driver.FindElements(selector).ToList() : Parent.FindElements(selector).ToList();

                // Sometimes component lay outside of Parent and PageFactory should be called with Driver context
                if (Props.InitWithoutContext)
                {
                    PageFactory.InitElements(Driver, this);
                }
                else
                {
                    PageFactory.InitElements(Components.First(), this);
                }
            }

            try
            {
                CheckAutomationClass();
            }
            catch { }
        }

        public IWebElement Instance =>
            Props.Displayed.Equals(TriState.True) || Props.Exist.Equals(TriState.True) ? Components.First() : null;

        public IList<IWebElement> Instances =>
            Props.Displayed.Equals(TriState.True) || Props.Exist.Equals(TriState.True) ? Components : null;

        public WebDriver Driver { get; set; }

        public WebDriverExtensions.WaitTime WaitTime { get; set; }

        public string Identifier { get; set; }

        private By ParentSelector { get; set; }

        private IWebElement Parent { get; set; }

        public By Context { get; }

        public By Frame { get; }
    }

    public class Properties
    {
        public By ParentSelector = null;

        public IWebElement Parent = null;

        public TriState Displayed = TriState.True;

        public TriState Exist = TriState.UseDefault;

        public WebDriverExtensions.WaitTime WaitTime = WebDriverExtensions.WaitTime.Medium;

        // Page factory will use Driver as context for factory
        public bool InitWithoutContext = false;
    }
}
