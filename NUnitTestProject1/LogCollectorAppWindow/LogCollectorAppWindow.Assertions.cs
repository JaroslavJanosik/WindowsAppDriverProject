using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;

namespace EsetLogCollectorTestProject
{
    partial class LogCollectorAppWindow
    {
        public bool? IsElementSelected(WindowsElement windowsElement)
        {
            if (windowsElement.GetAttribute("Toggle.ToggleState") == "1")
            {
                return true;
            }
            else if (windowsElement.GetAttribute("Toggle.ToggleState") == "0")
            {
                return false;
            }
            return null;
        }

        public bool? IsElementEnabled(WindowsElement windowsElement)
        {
            if (windowsElement.GetAttribute("IsEnabled") == "True")
            {
                return true;
            }
            else if (windowsElement.GetAttribute("IsEnabled") == "False")
            {
                return false;
            }
            return null;
        }

        public bool IsModalDialogOpen()
        {
            var modalDialog = _driver.FindElements(By.XPath("//*[@AutomationId='65535']"));

            if (modalDialog.Count > 0)
            {
                return true;
            }

            return false;
        }

        public void HasTextElementCorrectValue(WindowsElement windowsElement, string expectedValue)
        {
            Assert.AreEqual(expectedValue, windowsElement.GetAttribute("Value.Value"));
        }

        public void HasModalDialogCorrectMessage(WindowsElement windowsElement, string expectedValue)
        {
            Assert.AreEqual(expectedValue, windowsElement.GetAttribute("Name"));
        }
    }
}
