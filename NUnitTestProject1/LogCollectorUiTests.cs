using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace EsetLogCollectorTestProject
{
    [TestFixture]
    public class LogCollectorUiTests
    {
        private const string ProjectDirectoryPath = @"C:\Automation\GitHub\ESET_QA_TASK\";
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string AppExecutionPath = ProjectDirectoryPath + @"ESETLogCollector.exe";
        private const string DefaultArchiveFileName = ProjectDirectoryPath + @"ELC_logs.zip";
        private const string DefaultDirectoryName = ProjectDirectoryPath + @"ELC_logs";
        private const string CustomDirectoryName = ProjectDirectoryPath + @"CustomDirectory";
        private const string CustomArchiveFileName = CustomDirectoryName + "\\" + @"Custom_ELC_logs.zip";
        private const string AllFilesCollectedMessage = "Všechny soubory byly úspěšně sesbírány a zabaleny do archivu.";
        List<string> ArchivesDirectories = new List<string>() {
                @"\metadata.txt",
                @"\collector_log.txt",
                @"\Windows\Processes.txt",
                @"\Windows\Logs\Application.xml",
                @"\Windows\Logs\System.xml",
                @"\Windows\Logs\SetupAPI\setupapi.dev.log",
                @"\Config\SysInspector.xml",
                @"\Config\network.txt",
                @"\Config\WFPFilters.xml"
            };

        private WindowsDriver<WindowsElement> _driver;
        private LogCollectorAppWindow _logCollectorAppWindow;

        [SetUp]
        public void TestInit()
        {
            // Deletes an old archive if already exists.
            DeleteArchive(DefaultArchiveFileName);
            // Deletes an old directory if already exists.
            DeleteDirectory(DefaultDirectoryName);
            DeleteDirectory(CustomDirectoryName);
            // Sets up a compatible version of the SysInspector application as default.
            SetUpSysInspector_DefaultSetting();

            // Launches Log Collector application if it is not yet launched.
            if (_driver == null)
            {
                // Create a new session to bring up an instance of the ESET Log Collector application.
                var appiumOptions = new AppiumOptions();
                appiumOptions.AddAdditionalCapability("app", AppExecutionPath);
                appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
                _driver = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appiumOptions);
                Assert.IsNotNull(_driver);
                _logCollectorAppWindow = new LogCollectorAppWindow(_driver);

                // Sets implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times.
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            }
        }

        [Test]
        public void DefaultAppSettingIsCorrect()
        {
            var expectedAgeLimitValue = "30";
            var expectedLogsCollModeValue = "Filtrovaná binární data";

            List<WindowsElement> artifactsCheckboxes = new List<WindowsElement>() {
                _logCollectorAppWindow.RunningProcessesCheckbox,
                _logCollectorAppWindow.AppEventLogCheckbox,
                _logCollectorAppWindow.SysEventLogCheckbox,
                _logCollectorAppWindow.SetupApiLogsCheckbox,
                _logCollectorAppWindow.SysInspecLogCheckbox,
                _logCollectorAppWindow.NetConfCheckbox,
                _logCollectorAppWindow.WfpFiltersCheckbox,
                _logCollectorAppWindow.SelectAllArtifactsCheckbox
            };

            foreach (WindowsElement checkbox in artifactsCheckboxes)
            {
                Assert.IsTrue(_logCollectorAppWindow.IsElementSelected(checkbox));
            }

            _logCollectorAppWindow.HasTextElementCorrectValue(_logCollectorAppWindow.LogsAgeLimitTextField, expectedAgeLimitValue);
            _logCollectorAppWindow.HasTextElementCorrectValue(_logCollectorAppWindow.LogsCollectionModeDropDown, expectedLogsCollModeValue);
            _logCollectorAppWindow.HasTextElementCorrectValue(_logCollectorAppWindow.SaveArchiveTextField, DefaultArchiveFileName);
            Assert.IsTrue(_logCollectorAppWindow.OperationLogTextArea.GetAttribute("Value.IsReadOnly") == "True");
            
            _logCollectorAppWindow.CloseButton.Click();             
        }

        [Test]
        public void CollectData_DefaultSetting_Positive()
        {
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(DefaultArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());

            ZipFile.ExtractToDirectory(DefaultArchiveFileName, DefaultDirectoryName);

            foreach (string directory in ArchivesDirectories)
            {
                Assert.IsTrue(File.Exists(DefaultDirectoryName + directory));
            }

            CheckXmlLogsAge(DefaultDirectoryName + @"\Windows\Logs\Application.xml", 30);
            CheckXmlLogsAge(DefaultDirectoryName + @"\Windows\Logs\System.xml", 30);
        }        

        [Test]
        public void CollectData_CustomDirectory_RewriteArchive_Positive()
        {
            CreateDirectory(CustomDirectoryName);            

            _logCollectorAppWindow.SaveArchiveTextField.Clear();
            _logCollectorAppWindow.SaveArchiveTextField.SendKeys(CustomArchiveFileName);
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(CustomArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());

            ZipFile.ExtractToDirectory(CustomArchiveFileName, CustomDirectoryName);

            foreach (string directory in ArchivesDirectories)
            {
                Assert.IsTrue(File.Exists(CustomDirectoryName + directory));
            }

            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, "Cílový soubor již existuje. Chcete jej přepsat?");
            _logCollectorAppWindow.ModalDialogYesButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(CustomArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());            
        }                     

        [Test]
        [TestCase("RunningProcesses", @"\Windows\Processes.txt")]
        [TestCase("AppEventLog", @"\Windows\Logs\Application.xml")]
        [TestCase("SysEventLog", @"\Windows\Logs\System.xml")]
        [TestCase("SetupApi", @"\Windows\Logs\SetupAPI\setupapi.dev.log")]
        [TestCase("SysInspecLog", @"\Config\SysInspector.xml")]
        [TestCase("NetConf", @"\Config\network.txt")]
        [TestCase("WfpFilters", @"\Config\WFPFilters.xml")]
        public void CollectData_CustomArtifactsSetting_Positive(string artifact, string archiveDirectory)
        {
            List<string> archivesDirectories = new List<string>() {
                @"\metadata.txt",
                @"\collector_log.txt",
                archiveDirectory
            };

            // Unselects all artifacts checkboxes.
            _logCollectorAppWindow.SelectAllArtifactsCheckbox.Click();

            switch (artifact)
            {
                case "RunningProcesses":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.RunningProcessesCheckbox);
                    break;
                case "AppEventLog":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.AppEventLogCheckbox);
                    break;
                case "SysEventLog":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.SysEventLogCheckbox);
                    break;
                case "SetupApi":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.SetupApiLogsCheckbox);
                    break;
                case "SysInspecLog":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.SysInspecLogCheckbox);
                    break;
                case "NetConf":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.NetConfCheckbox);
                    break;
                case "WfpFilters":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.WfpFiltersCheckbox);
                    break;
                default:
                    break;
            }

            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(DefaultArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());

            ZipFile.ExtractToDirectory(DefaultArchiveFileName, DefaultDirectoryName);

            foreach (string directory in archivesDirectories)
            {
                Assert.IsTrue(File.Exists(DefaultDirectoryName + directory));
            }
        }

        [Test]
        [TestCase("AppEventLog", @"\Windows\Logs\Application.xml", "1")]
        [TestCase("SysEventLog", @"\Windows\Logs\System.xml", "5")]
        public void CollectData_CustomLogsAgeLimit_Positive(string artifact, string archiveDirectory, string logsAgeLimit)
        {
            // Unselects all artifacts checkboxes.
            _logCollectorAppWindow.SelectAllArtifactsCheckbox.Click();

            switch (artifact)
            {
                case "AppEventLog":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.AppEventLogCheckbox);
                    break;
                case "SysEventLog":
                    _logCollectorAppWindow.ClickOnArtifactCheckbox(_logCollectorAppWindow.SysEventLogCheckbox);
                    break;
                default:
                    break;
            }

            _logCollectorAppWindow.LogsAgeLimitDropDown.Click();
            _driver.FindElementByName(logsAgeLimit).Click();
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(DefaultArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());

            ZipFile.ExtractToDirectory(DefaultArchiveFileName, DefaultDirectoryName);

            Assert.IsTrue(File.Exists(DefaultDirectoryName + archiveDirectory));

            CheckXmlLogsAge(DefaultDirectoryName + archiveDirectory, int.Parse(logsAgeLimit));
        }

        [Test]
        public void CollectData_ErrorDuringCollecting_Negative()
        {
            var expectedDialogMessage = "Během sesbírání souborů došlo k chybě. \r\n\r\nPro více informací se podívejte do protokolu.";

            SetUpSysInspector_ErrorSetting();

            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, expectedDialogMessage);
            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }

        [Test]
        public void CollectData_InterruptCollecting_Positive()
        {
            var interruptionStartDialogMessage = "Ukončit proces?";

            _logCollectorAppWindow.CollectButton.Click();
            // Interrupts collecting
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, interruptionStartDialogMessage);
            _logCollectorAppWindow.ModalDialogNoButton.Click();

            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, AllFilesCollectedMessage);

            Assert.IsTrue(File.Exists(DefaultArchiveFileName));

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());

            ZipFile.ExtractToDirectory(DefaultArchiveFileName, DefaultDirectoryName);

            foreach (string directory in ArchivesDirectories)
            {
                Assert.IsTrue(File.Exists(DefaultDirectoryName + directory));
            }
        }

        [Test]
        public void CollectData_InterruptCollecting_Negative()
        {
            var interruptionStartDialogMessage = "Ukončit proces?";
            var interruptionEndDialogMessage = "Operace byla zrušena.";

            _logCollectorAppWindow.CollectButton.Click();
            // Interrupts collecting
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, interruptionStartDialogMessage);
            _logCollectorAppWindow.ModalDialogYesButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, interruptionEndDialogMessage);
            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }

        [Test]
        public void CollectData_ElementsAreDisabledDuringCollecting()
        {
            List<WindowsElement> elements = new List<WindowsElement>() {
                _logCollectorAppWindow.RunningProcessesCheckbox,
                _logCollectorAppWindow.AppEventLogCheckbox,
                _logCollectorAppWindow.SysEventLogCheckbox,
                _logCollectorAppWindow.SetupApiLogsCheckbox,
                _logCollectorAppWindow.SysInspecLogCheckbox,
                _logCollectorAppWindow.NetConfCheckbox,
                _logCollectorAppWindow.WfpFiltersCheckbox,
                _logCollectorAppWindow.SelectAllArtifactsCheckbox,
                _logCollectorAppWindow.LogsAgeLimitTextField,
                _logCollectorAppWindow.LogsCollectionModeDropDown,
                _logCollectorAppWindow.SaveArchiveTextField,
                _logCollectorAppWindow.SaveArchiveButton
            };

            _logCollectorAppWindow.CollectButton.Click();

            foreach (WindowsElement element in elements)
            {
                Assert.IsFalse(_logCollectorAppWindow.IsElementEnabled(element), $"{ element.GetAttribute("IsEnabled")} {element.GetAttribute("Name")}");
                _logCollectorAppWindow.ClickOnArtifactCheckbox(element);
            }

            Assert.IsTrue(_logCollectorAppWindow.IsElementEnabled(_logCollectorAppWindow.OperationLogTextArea));
            Assert.IsTrue(_logCollectorAppWindow.IsElementEnabled(_logCollectorAppWindow.CollectButton));

            // Interrupts collecting
            _logCollectorAppWindow.CollectButton.Click();            
            _logCollectorAppWindow.ModalDialogYesButton.Click();           
            _logCollectorAppWindow.ModalDialogOkButton.Click();
        }        

        [Test]
        public void CollectData_NoArtifactsSelected_Negative()
        {
            var expectedDialogMessage = "Nevybrali jste žádnou akci.";
            List<WindowsElement> artifactsCheckboxes = new List<WindowsElement>() {
                _logCollectorAppWindow.RunningProcessesCheckbox,
                _logCollectorAppWindow.AppEventLogCheckbox,
                _logCollectorAppWindow.SysEventLogCheckbox,
                _logCollectorAppWindow.SetupApiLogsCheckbox,
                _logCollectorAppWindow.SysInspecLogCheckbox,
                _logCollectorAppWindow.NetConfCheckbox,
                _logCollectorAppWindow.WfpFiltersCheckbox,
                _logCollectorAppWindow.SelectAllArtifactsCheckbox
            };

            _logCollectorAppWindow.SelectAllArtifactsCheckbox.Click();

            foreach (WindowsElement checkbox in artifactsCheckboxes)
            {
                Assert.IsFalse(_logCollectorAppWindow.IsElementSelected(checkbox));
            }

            _logCollectorAppWindow.CollectButton.Click();

            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, expectedDialogMessage);

            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }

        [Test]
        [TestCase("")]
        [TestCase("*")]
        [TestCase("x")]
        [TestCase("-1")]
        [TestCase("1-")]
        [TestCase("1*1")]
        public void LogsAgeLimitTextFieldValidations(string testValue)
        {
            var expectedDialogMessage = "Neplatný limit stáří protokolů.";

            _logCollectorAppWindow.LogsAgeLimitTextField.Clear();
            _logCollectorAppWindow.LogsAgeLimitTextField.SendKeys(testValue);
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, expectedDialogMessage);
            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }

        [Test]
        [TestCase("0")]
        public void LogsAgeLimitTextFieldValidations_NoLimitation(string testValue)
        {
            var expectedDialogMessage = "Neomezili jste stáří protokolů.\r\nVýsledný soubor může být obrovský.\r\n\r\nChcete pokračovat?";

            _logCollectorAppWindow.LogsAgeLimitTextField.Clear();
            _logCollectorAppWindow.LogsAgeLimitTextField.SendKeys(testValue);
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, expectedDialogMessage);
            _logCollectorAppWindow.ModalDialogNoButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }

        [Test]
        [TestCase("", "Název cílového archivu nemůže být prázdný.")]
        [TestCase("*", "")]
        [TestCase(@"C:\ESETQA_TASK\", "")]
        public void SaveArchiveTextFieldValidations(string testValue, string cannotBeEmptyMessage)
        {
            var expectedDialogMessage = "Cílový archiv nelze otevřít pro zápis.\r\n\r\nUjistěte se, zda máte potřebná oprávnění pro přístup k tomuto souboru, zda složka existuje, případně vyberte jiný soubor.";

            _logCollectorAppWindow.SaveArchiveTextField.Clear();
            _logCollectorAppWindow.SaveArchiveTextField.SendKeys(testValue);
            _logCollectorAppWindow.CollectButton.Click();
            _logCollectorAppWindow.HasModalDialogCorrectMessage(_logCollectorAppWindow.ModalDialogMessage, testValue == "" ? cannotBeEmptyMessage : expectedDialogMessage);
            _logCollectorAppWindow.ModalDialogOkButton.Click();

            Assert.IsFalse(_logCollectorAppWindow.IsModalDialogOpen());
            Assert.IsFalse(File.Exists(DefaultArchiveFileName));
        }
        
        [TearDown]
        public void TestCleanUp()
        {
            // Closes the application and delete the session.
            if (_driver != null)
            {
                _driver.Quit();
                _driver = null;
            }
        }

        private void DeleteArchive(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                // Handles the case of the file already being opened by another process.
                try
                {
                    System.IO.File.Delete(fileName);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }        

        private void CreateDirectory(string directoryPath)
        {
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(directoryPath))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(directoryPath);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(directoryPath));

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        private void DeleteDirectory(string directoryName)
        {
            // Deletes a directory and all subdirectories with Directory.
            if (System.IO.Directory.Exists(directoryName))
            {
                try
                {
                    System.IO.Directory.Delete(directoryName, true);
                }

                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void RenameFile(string oldFileName, string newFileName)
        {
            if (System.IO.File.Exists(oldFileName))
            {
                // Handles the case of the file already being opened by another process.
                try
                {
                    System.IO.File.Move(oldFileName, newFileName);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        private void SetUpSysInspector_ErrorSetting()
        {
            var FileName = ProjectDirectoryPath + @"SysInspector.exe";
            var tempFileName = ProjectDirectoryPath + @"SysInspectorTemp.exe";
            var FileName2 = ProjectDirectoryPath + @"SysInspector2.exe";

            if (System.IO.File.Exists(FileName) & System.IO.File.Exists(FileName2))
            {
                {
                    if (FileVersionInfo.GetVersionInfo(FileName).FileVersion.Equals("1.2.012.0") & FileVersionInfo.GetVersionInfo(FileName2).FileVersion.Equals("10.16.5.0"))
                    {
                        RenameFile(FileName, tempFileName);
                        RenameFile(FileName2, FileName);
                    }
                }
            }
        }

        private void SetUpSysInspector_DefaultSetting()
        {
            var FileName = ProjectDirectoryPath + @"SysInspector.exe";
            var tempFileName = ProjectDirectoryPath + @"SysInspectorTemp.exe";
            var FileName2 = ProjectDirectoryPath + @"SysInspector2.exe";


            if (System.IO.File.Exists(tempFileName) & System.IO.File.Exists(FileName))
            {
                {
                    if (FileVersionInfo.GetVersionInfo(tempFileName).FileVersion.Equals("1.2.012.0") & FileVersionInfo.GetVersionInfo(FileName).FileVersion.Equals("10.16.5.0"))
                    {
                        RenameFile(FileName, FileName2);
                        RenameFile(tempFileName, FileName);
                    }
                }
            }
        }        

        // Checks that logs in a xml file are not older than the logs age limit
        private void CheckXmlLogsAge(string xmlFilePath, int logsAgeLimit)
        {
            XmlDocument doc = new XmlDocument();

            if (System.IO.File.Exists(xmlFilePath))
            {
                // Handles the case of the file already being opened by another process.
                try
                {
                    doc.Load(xmlFilePath);
                    var items = doc.GetElementsByTagName("Event");

                    var xmlTime = new string[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        var xmlAttributeCollection = items[i].Attributes;
                        if (xmlAttributeCollection != null)
                        {
                            var time = xmlAttributeCollection["Time"];
                            xmlTime[i] = time.Value;
                        }
                    }

                    foreach (string time in xmlTime)
                    {
                        Assert.IsTrue(DateTime.Parse(time) > (DateTime.Now.Subtract(TimeSpan.FromDays(logsAgeLimit))), $"Event log time: {DateTime.Parse(time)}\n Log age limit: {DateTime.Now.Subtract(TimeSpan.FromDays(logsAgeLimit))}");
                    }
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }        
    }
}