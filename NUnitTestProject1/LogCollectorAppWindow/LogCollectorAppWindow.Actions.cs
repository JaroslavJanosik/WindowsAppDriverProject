using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace EsetLogCollectorTestProject
{
    partial class LogCollectorAppWindow
    {
        private readonly WindowsDriver<WindowsElement> _driver;

        public LogCollectorAppWindow(WindowsDriver<WindowsElement> driver) => _driver = driver;

        public void ClickOnArtifactCheckbox(WindowsElement artifactCheckbox)
        {
            _driver.Mouse.MouseMove(artifactCheckbox.Coordinates, 10, 10);
            _driver.Mouse.Click(null);
        }

        public void ClickOnArtifactCheckbox(List<WindowsElement> artifactCheckboxes)
        {
            foreach (WindowsElement checkbox in artifactCheckboxes)
            {
                _driver.Mouse.MouseMove(checkbox.Coordinates, 10, 10);
                _driver.Mouse.Click(null);
            }
        }

        public WindowsElement GetElementByAutomationID(string automationId, int timeOut = 60)
        {
            WindowsElement element = null;

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(_driver)
            {
                Timeout = TimeSpan.FromSeconds(timeOut),
                Message = $"Element with automationId \"{automationId}\" not found."
            };

            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            try
            {
                wait.Until(Driver =>
                {
                    element = Driver.FindElementByAccessibilityId(automationId);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                Assert.Fail(ex.Message);
            }

            return element;
        }
    }
}

