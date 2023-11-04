using OpenQA.Selenium.Appium.Windows;

namespace EsetLogCollectorTestProject
{
    partial class LogCollectorAppWindow
    {

        public WindowsElement MinimizeButton => _driver.FindElementByName("Minimalizovať");
        public WindowsElement MaximizeButton => _driver.FindElementByName("Maximalizovať");
        public WindowsElement CloseButton => _driver.FindElementByName("Zavrieť");
        public WindowsElement SupportedProductPanel => _driver.FindElementByAccessibilityId("1003");
        public WindowsElement CollectButton => _driver.FindElementByAccessibilityId("1002");
        public WindowsElement ArtifactsToCollectLabel => _driver.FindElementByAccessibilityId("1015");
        public WindowsElement WindowsProcessesLabel => _driver.FindElementByName("Procesy Windows");
        public WindowsElement RunningProcessesCheckbox => _driver.FindElementByName("Spuštěné procesy (otevřené popisovače a načtené DLL)");
        public WindowsElement WindowsLogsLabel => _driver.FindElementByName("Protokoly Windows");
        public WindowsElement AppEventLogCheckbox => _driver.FindElementByName("Události z protokolu aplikací");
        public WindowsElement SysEventLogCheckbox => _driver.FindElementByName("Události z protokolu systému");
        public WindowsElement SetupApiLogsCheckbox => _driver.FindElementByName("SetupAPI protokoly");
        public WindowsElement SysConfLabel => _driver.FindElementByName("Nastavení systému");
        public WindowsElement SysInspecLogCheckbox => _driver.FindElementByName("ESET SysInspector protokol");
        public WindowsElement NetConfCheckbox => _driver.FindElementByName("Nastavení sítě");
        public WindowsElement WfpFiltersCheckbox => _driver.FindElementByName("WFP filtry");
        public WindowsElement SelectAllArtifactsCheckbox => _driver.FindElementByAccessibilityId("1016");
        public WindowsElement LogsAgeLimitLabel => _driver.FindElementByAccessibilityId("1011");
        public WindowsElement LogsAgeLimitTextField => _driver.FindElementByXPath("//Edit[@Name='Stáří protokolů [dní]']");
        public WindowsElement LogsAgeLimitDropDown => _driver.FindElementByAccessibilityId("DropDown");
        public WindowsElement LogsCollectionModeLabel => _driver.FindElementByAccessibilityId("1014");
        public WindowsElement LogsCollectionModeDropDown => _driver.FindElementByAccessibilityId("1013");
        public WindowsElement SaveArchiveLabel => _driver.FindElementByAccessibilityId("1008");
        public WindowsElement SaveArchiveTextField => _driver.FindElementByAccessibilityId("1005");
        public WindowsElement SaveArchiveButton => _driver.FindElementByAccessibilityId("1007");
        public WindowsElement OperationLogLabel => _driver.FindElementByAccessibilityId("1009");
        public WindowsElement OperationLogTextArea => _driver.FindElementByAccessibilityId("1001");
        public WindowsElement ModalDialogMessage => GetElementByAutomationID("65535");
        public WindowsElement ModalDialogOkButton => _driver.FindElementByAccessibilityId("2");
        public WindowsElement ModalDialogYesButton => _driver.FindElementByAccessibilityId("6");
        public WindowsElement ModalDialogNoButton => _driver.FindElementByAccessibilityId("7");

    }
}
