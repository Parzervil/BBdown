using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;
using System.IO;

namespace BBDown.GUI.Tests
{
    public class MainWindowUiTests : IDisposable
    {
        private readonly Application _app;
        private readonly UIA3Automation _automation;
        private readonly Window _mainWindow;

        public MainWindowUiTests()
        {
            var exePath = Path.GetFullPath(@"..\..\..\..\BBDown.GUI\bin\Debug\net8.0-windows\BBDown.GUI.exe");
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException("GUI executable not found", exePath);
            }

            _automation = new UIA3Automation();
            _app = Application.Launch(exePath);
            _mainWindow = RetryFindMainWindow(TimeSpan.FromSeconds(15));
        }

        public void Dispose()
        {
            try
            {
                _app?.Close();
                _automation?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        [Fact]
        public void MainWindow_IsVisible_WithCorrectTitle()
        {
            Assert.NotNull(_mainWindow);
            Assert.True(_mainWindow.IsEnabled);
            Assert.Equal("BBDown GUI", _mainWindow.Title);
        }

        [Fact]
        public void ParseButton_Click_ParsesTargetUrl()
        {
            var urlBox = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlTextBox"))?.AsTextBox();
            Assert.NotNull(urlBox);

            urlBox.Enter("https://www.bilibili.com/video/BV1RZjd6ZE8K/");

            var parseButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ParseButton"))?.AsButton();
            Assert.NotNull(parseButton);
            Assert.True(parseButton.IsEnabled);

            parseButton.Click();

            // Wait for async parse and UI update
            Thread.Sleep(TimeSpan.FromSeconds(10));

            // After parse, a MessageBox may appear if failed; check for title/info content
            var messageBox = _automation.GetDesktop().FindFirstDescendant(cf => cf.ByClassName("#32770"));
            if (messageBox != null)
            {
                var msgText = messageBox.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text))?.AsLabel();
                Assert.Fail($"Unexpected dialog: {msgText?.Text ?? "unknown"}");
            }

            var resultTitle = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("VideoTitleTextBlock"))?.AsLabel();
            Assert.NotNull(resultTitle);
            Assert.False(string.IsNullOrWhiteSpace(resultTitle.Text), "Video title should be populated after parse");
        }

        private Window RetryFindMainWindow(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                try
                {
                    var window = _app.GetMainWindow(_automation);
                    if (window != null && window.IsAvailable && window.Title == "BBDown GUI")
                    {
                        return window;
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(200);
            }
            throw new TimeoutException("Could not find main window");
        }
    }
}
