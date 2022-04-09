using System;
using OpenQA.Selenium;
using RestSharp;

namespace SeleniumAutomationUtils.SeleniumExtensions
{
    public static class CookieExtensions
    {
        public static Cookie ToSeleniumCookies(this System.Net.Cookie cookie)
        {
            try
            {
                return new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expires);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to convert cookie with '{cookie.Name}' name: {e}");
            }
        }

        public static Cookie ToSeleniumCookies(this RestResponseCookie cookie)
        {
            try
            {
                return new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, new DateTime?(cookie.Expires));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to convert cookie with '{0}' name: {1}", (object)cookie.Name, (object)ex));
            }
        }
    }
}
