using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumAutomationUtils.SeleniumExtensions;
using SeleniumAutomationUtils.Utils;
using SeleniumExtras.PageObjects;

namespace SeleniumAutomationUtils.Pages
{
    public abstract class SeleniumBasePage : IContextContainer
    {
        private const string BodySelector = ".//body";

        public By Context => By.XPath(BodySelector);

        [FindsBy(How = How.XPath, Using = BodySelector)]
        public IWebElement BodyContainer { get; set; }

        public WebDriver Driver { get; set; }

        public Actions Actions { get; set; }

        public void InitElements(By context = null)
        {
            if (context is null)
            {
                PageFactory.InitElements(Driver, this);
            }
            else
            {
                Driver.WaitForElementToBeExists(context);
                var contextElement = Driver.FindElement(context);
                PageFactory.InitElements(contextElement, this);
            }
        }

        public virtual List<By> GetPageIdentitySelectors()
        {
            return GetType()
                .GetProperties()
                .Select(p => p.GetFirstDecoration<FindsByAttribute>())
                .Where(a =>
                    (object)a != null
                    && a != null)
                .Select(ByFactory.From)
                .ToList();
        }

        public IWebElement ContextElement => Driver.FindElement(Context);

        public By SelectorFor<TPage, TProperty>(TPage page, Expression<Func<TPage, TProperty>> expression)
        {
            var attribute = ReflectionExtensions.ResolveMember(page, expression)
                .GetFirstDecoration<FindsByAttribute>();
            return ByFactory.From(attribute);
        }

        //Usage By selector = page.GetByFor(() => page.LoginButton);
        public By GetByFor<TProperty>(Expression<Func<TProperty>> expression)
        {
            var attribute = ReflectionExtensions.ResolveMember(expression).GetFirstDecoration<FindsByAttribute>();
            return ByFactory.From(attribute);
        }

        //Usage By selector = page.GetByFor(() => page.LoginButton);
        public string GetStringByFor<TProperty>(Expression<Func<TProperty>> expression)
        {
            var attribute = ReflectionExtensions.ResolveMember(expression).GetFirstDecoration<FindsByAttribute>();
            return ByFactory.From(attribute).ToString().Split(": ").Last();
        }

        //Usage By selector = page.Click(() => page.LoginButton);
        //For cases when element can be Staled
        public void Click<TProperty>(Expression<Func<TProperty>> expression)
        {
            var by = GetByFor(expression);
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Driver.FindElement(by).Click();
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
            }
        }
    }
}
