using BBDown.GUI;
using BBDown.GUI.Models;
using System.Reflection;
using System.Windows;

namespace BBDown.GUI.Tests
{
    public class MainViewModelTests
    {
        private static T InvokePrivateMethod<T>(object instance, string methodName, params object[] args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            var result = method.Invoke(instance, args);
            return (T)(result ?? default(T)!);
        }

        [Fact]
        public void BuildSelectedPageText_EmptyPages_ReturnsSelectPageText()
        {
            var vm = new MainViewModel();
            vm.SelectPage = "1,3,5";

            var result = InvokePrivateMethod<string>(vm, "BuildSelectedPageText");

            Assert.Equal("1,3,5", result);
        }

        [Fact]
        public void BuildSelectedPageText_AllSelected_ReturnsAll()
        {
            var vm = new MainViewModel();
            vm.Pages.Add(new PageViewModel { Index = 1, IsSelected = true });
            vm.Pages.Add(new PageViewModel { Index = 2, IsSelected = true });
            vm.Pages.Add(new PageViewModel { Index = 3, IsSelected = true });

            var result = InvokePrivateMethod<string>(vm, "BuildSelectedPageText");

            Assert.Equal("ALL", result);
        }

        [Fact]
        public void BuildSelectedPageText_PartialSelected_ReturnsCommaList()
        {
            var vm = new MainViewModel();
            vm.Pages.Add(new PageViewModel { Index = 1, IsSelected = true });
            vm.Pages.Add(new PageViewModel { Index = 2, IsSelected = false });
            vm.Pages.Add(new PageViewModel { Index = 3, IsSelected = true });

            var result = InvokePrivateMethod<string>(vm, "BuildSelectedPageText");

            Assert.Equal("1,3", result);
        }

        [Fact]
        public void GetApiMode_Default_ReturnsWeb()
        {
            var vm = new MainViewModel();
            var result = InvokePrivateMethod<string>(vm, "GetApiMode");
            Assert.Equal("WEB", result);
        }

        [Fact]
        public void GetApiMode_TvSelected_ReturnsTv()
        {
            var vm = new MainViewModel { UseTvApi = true };
            var result = InvokePrivateMethod<string>(vm, "GetApiMode");
            Assert.Equal("TV", result);
        }

        [Fact]
        public void GetDownloadMode_Default_ReturnsFull()
        {
            var vm = new MainViewModel();
            var result = InvokePrivateMethod<string>(vm, "GetDownloadMode");
            Assert.Equal("Full", result);
        }

        [Fact]
        public void GetDownloadMode_AudioOnly_ReturnsAudioOnly()
        {
            var vm = new MainViewModel { AudioOnly = true };
            var result = InvokePrivateMethod<string>(vm, "GetDownloadMode");
            Assert.Equal("AudioOnly", result);
        }

        [Fact]
        public void ChangePage_StringParameter_UpdatesCurrentPage()
        {
            var vm = new MainViewModel();
            Assert.Equal(AppPage.Download, vm.CurrentPage);

            var method = vm.GetType().GetMethod("ChangePage", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(vm, new object[] { "Settings" });

            Assert.Equal(AppPage.Settings, vm.CurrentPage);
            Assert.True(vm.IsSettingsPage);
        }

        [Fact]
        public void DownloadCommand_CanExecute_RequiresUrlAndNotBusy()
        {
            var vm = new MainViewModel();
            Assert.False(vm.DownloadCommand.CanExecute(null));

            vm.Url = "https://www.bilibili.com/video/BV1RZjd6ZE8K/";
            Assert.True(vm.DownloadCommand.CanExecute(null));

            vm.IsBusy = true;
            Assert.False(vm.DownloadCommand.CanExecute(null));
        }

        [Fact]
        public void ParseCommand_CanExecute_RequiresNotBusy()
        {
            var vm = new MainViewModel();
            Assert.True(vm.ParseCommand.CanExecute(null));

            vm.IsBusy = true;
            Assert.False(vm.ParseCommand.CanExecute(null));
        }
    }
}
