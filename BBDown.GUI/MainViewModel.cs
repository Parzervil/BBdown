using BBDown;
using BBDown.Core;
using BBDown.GUI.Commands;
using BBDown.GUI.Models;
using BBDown.GUI.Services;
using BBDown.GUI.ViewModels;
using BBDown.GUI.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BBDown.GUI
{
    public class MainViewModel : ViewModelBase
    {
        private readonly GuiSettingsService _settingsService = new();
        private readonly HistoryService _historyService = new();

        public MainViewModel()
        {
            Download = new DownloadViewModel();
            Tasks = new TaskQueueViewModel();
            History = new HistoryViewModel(LoadHistoryRecordForRetry);
            Settings = new SettingsViewModel();
            Login = new LoginViewModel();
            Service = new ServiceViewModel();
            Log = new LogViewModel();
            Stream = new StreamSelectionViewModel();

            ParseCommand = new RelayCommand(async _ => await ParseAsync(), _ => !IsBusy);
            DownloadCommand = new RelayCommand(async _ => await DownloadAsync(), _ => !IsBusy && !string.IsNullOrEmpty(Url));
            BrowseOutputCommand = new RelayCommand(_ => BrowseOutput(), _ => true);
            LoginWebCommand = new RelayCommand(async _ => await LoginWebAsync(), _ => !IsBusy);
            LoginTvCommand = new RelayCommand(async _ => await LoginTvAsync(), _ => !IsBusy);
            ClearLogCommand = new RelayCommand(_ => Logs.Clear(), _ => true);
            SelectPageCommand = new RelayCommand(ChangePage, _ => true);
            SelectAllPagesCommand = new RelayCommand(_ => SetAllPagesSelected(true), _ => Pages.Count > 0);
            InvertPageSelectionCommand = new RelayCommand(_ => InvertPageSelection(), _ => Pages.Count > 0);
            SelectLatestPageCommand = new RelayCommand(_ => SelectLatestPage(), _ => Pages.Count > 0);
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings(), _ => true);

            ApplySettings(_settingsService.Load());
            History.LoadFrom(_historyService.Load());
            Logger.LogMessage += OnLogMessage;
        }

        #region Properties

        private string _url = "";
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private bool _useTvApi;
        public bool UseTvApi
        {
            get => _useTvApi;
            set => SetProperty(ref _useTvApi, value);
        }

        private bool _useAppApi;
        public bool UseAppApi
        {
            get => _useAppApi;
            set => SetProperty(ref _useAppApi, value);
        }

        private bool _useIntlApi;
        public bool UseIntlApi
        {
            get => _useIntlApi;
            set => SetProperty(ref _useIntlApi, value);
        }

        private bool _useMp4box;
        public bool UseMp4box
        {
            get => _useMp4box;
            set => SetProperty(ref _useMp4box, value);
        }

        private string _encodingPriority = "hevc,av1,avc";
        public string EncodingPriority
        {
            get => _encodingPriority;
            set => SetProperty(ref _encodingPriority, value);
        }

        private string _dfnPriority = "8K 超高清,4K 超清,1080P 高码率,1080P 高清";
        public string DfnPriority
        {
            get => _dfnPriority;
            set => SetProperty(ref _dfnPriority, value);
        }

        private string _outputDir = "";
        public string OutputDir
        {
            get => _outputDir;
            set => SetProperty(ref _outputDir, value);
        }

        private string _filePattern = "<videoTitle>";
        public string FilePattern
        {
            get => _filePattern;
            set => SetProperty(ref _filePattern, value);
        }

        private string _multiFilePattern = "<videoTitle>/[P<pageNumberWithZero>]<pageTitle>";
        public string MultiFilePattern
        {
            get => _multiFilePattern;
            set => SetProperty(ref _multiFilePattern, value);
        }

        private string _selectPage = "";
        public string SelectPage
        {
            get => _selectPage;
            set => SetProperty(ref _selectPage, value);
        }

        private bool _downloadDanmaku;
        public bool DownloadDanmaku
        {
            get => _downloadDanmaku;
            set => SetProperty(ref _downloadDanmaku, value);
        }

        private bool _skipSubtitle;
        public bool SkipSubtitle
        {
            get => _skipSubtitle;
            set => SetProperty(ref _skipSubtitle, value);
        }

        private bool _skipCover;
        public bool SkipCover
        {
            get => _skipCover;
            set => SetProperty(ref _skipCover, value);
        }

        private bool _audioOnly;
        public bool AudioOnly
        {
            get => _audioOnly;
            set => SetProperty(ref _audioOnly, value);
        }

        private bool _videoOnly;
        public bool VideoOnly
        {
            get => _videoOnly;
            set => SetProperty(ref _videoOnly, value);
        }

        private bool _useAria2c;
        public bool UseAria2c
        {
            get => _useAria2c;
            set => SetProperty(ref _useAria2c, value);
        }

        private bool _multiThread = true;
        public bool MultiThread
        {
            get => _multiThread;
            set => SetProperty(ref _multiThread, value);
        }

        private bool _showAll;
        public bool ShowAll
        {
            get => _showAll;
            set => SetProperty(ref _showAll, value);
        }

        private bool _skipMux;
        public bool SkipMux
        {
            get => _skipMux;
            set => SetProperty(ref _skipMux, value);
        }

        private bool _debug;
        public bool Debug
        {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }

        private string _cookie = "";
        public string Cookie
        {
            get => _cookie;
            set => SetProperty(ref _cookie, value);
        }

        private string _accessToken = "";
        public string AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private string _progressText = "";
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        private string _videoTitle = "";
        public string VideoTitle
        {
            get => _videoTitle;
            set => SetProperty(ref _videoTitle, value);
        }

        private string _videoInfo = "";
        public string VideoInfo
        {
            get => _videoInfo;
            set => SetProperty(ref _videoInfo, value);
        }

        public ObservableCollection<LogEntry> Logs { get; } = new();
        public ObservableCollection<PageViewModel> Pages { get; } = new();

        public DownloadViewModel Download { get; }
        public TaskQueueViewModel Tasks { get; }
        public HistoryViewModel History { get; }
        public SettingsViewModel Settings { get; }
        public LoginViewModel Login { get; }
        public ServiceViewModel Service { get; }
        public LogViewModel Log { get; }
        public StreamSelectionViewModel Stream { get; }

        private AppPage _currentPage = AppPage.Download;
        public AppPage CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    OnPropertyChanged(nameof(CurrentPageTitle));
                    OnPropertyChanged(nameof(IsDownloadPage));
                    OnPropertyChanged(nameof(IsTasksPage));
                    OnPropertyChanged(nameof(IsHistoryPage));
                    OnPropertyChanged(nameof(IsSettingsPage));
                    OnPropertyChanged(nameof(IsLoginPage));
                    OnPropertyChanged(nameof(IsServicePage));
                }
            }
        }

        public string CurrentPageTitle => CurrentPage switch
        {
            AppPage.Download => "下载",
            AppPage.Tasks => "任务",
            AppPage.History => "历史",
            AppPage.Settings => "设置",
            AppPage.Login => "登录",
            AppPage.Service => "服务",
            _ => "下载"
        };

        public bool IsDownloadPage => CurrentPage == AppPage.Download;
        public bool IsTasksPage => CurrentPage == AppPage.Tasks;
        public bool IsHistoryPage => CurrentPage == AppPage.History;
        public bool IsSettingsPage => CurrentPage == AppPage.Settings;
        public bool IsLoginPage => CurrentPage == AppPage.Login;
        public bool IsServicePage => CurrentPage == AppPage.Service;

        #endregion

        #region Commands

        public ICommand ParseCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand LoginWebCommand { get; }
        public ICommand LoginTvCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand SelectPageCommand { get; }
        public ICommand SelectAllPagesCommand { get; }
        public ICommand InvertPageSelectionCommand { get; }
        public ICommand SelectLatestPageCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        #endregion

        private void ChangePage(object? parameter)
        {
            if (parameter is AppPage page)
            {
                CurrentPage = page;
                return;
            }

            if (parameter is string pageName && Enum.TryParse(pageName, ignoreCase: true, out AppPage parsedPage))
            {
                CurrentPage = parsedPage;
            }
        }

        private void OnLogMessage(object? sender, LogEventArgs e)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                Logs.Add(new LogEntry
                {
                    Time = e.Timestamp,
                    Level = e.Level,
                    Message = e.Message
                });
            });
        }

        public void SaveSettings()
        {
            _settingsService.Save(CreateSettingsSnapshot());
        }

        public void SaveHistory()
        {
            _historyService.Save(History.Records);
        }

        private void LoadHistoryRecordForRetry(HistoryRecord record)
        {
            Url = record.Url;
            if (!string.IsNullOrWhiteSpace(record.OutputPath))
            {
                OutputDir = record.OutputPath;
            }

            CurrentPage = AppPage.Download;
            ProgressText = "已从历史记录载入任务，可点击下载重新执行。";
        }

        private void ApplySettings(GuiSettings settings)
        {
            OutputDir = settings.OutputDir;
            EncodingPriority = string.IsNullOrWhiteSpace(settings.EncodingPriority) ? EncodingPriority : settings.EncodingPriority;
            DfnPriority = string.IsNullOrWhiteSpace(settings.DfnPriority) ? DfnPriority : settings.DfnPriority;
            FilePattern = string.IsNullOrWhiteSpace(settings.FilePattern) ? FilePattern : settings.FilePattern;
            MultiFilePattern = string.IsNullOrWhiteSpace(settings.MultiFilePattern) ? MultiFilePattern : settings.MultiFilePattern;
            DownloadDanmaku = settings.DownloadDanmaku;
            SkipSubtitle = settings.SkipSubtitle;
            SkipCover = settings.SkipCover;
            SkipMux = settings.SkipMux;
            UseAria2c = settings.UseAria2c;
            MultiThread = settings.MultiThread;
            UseMp4box = settings.UseMp4box;
            ShowAll = settings.ShowAll;
            Debug = settings.Debug;

            UseTvApi = settings.ApiMode.Equals("TV", StringComparison.OrdinalIgnoreCase);
            UseAppApi = settings.ApiMode.Equals("APP", StringComparison.OrdinalIgnoreCase);
            UseIntlApi = settings.ApiMode.Equals("INTL", StringComparison.OrdinalIgnoreCase);

            AudioOnly = settings.DownloadMode.Equals("AudioOnly", StringComparison.OrdinalIgnoreCase);
            VideoOnly = settings.DownloadMode.Equals("VideoOnly", StringComparison.OrdinalIgnoreCase);
        }

        private GuiSettings CreateSettingsSnapshot()
        {
            return new GuiSettings
            {
                OutputDir = OutputDir,
                ApiMode = GetApiMode(),
                DownloadMode = GetDownloadMode(),
                EncodingPriority = EncodingPriority,
                DfnPriority = DfnPriority,
                FilePattern = FilePattern,
                MultiFilePattern = MultiFilePattern,
                DownloadDanmaku = DownloadDanmaku,
                SkipSubtitle = SkipSubtitle,
                SkipCover = SkipCover,
                SkipMux = SkipMux,
                UseAria2c = UseAria2c,
                MultiThread = MultiThread,
                UseMp4box = UseMp4box,
                ShowAll = ShowAll,
                Debug = Debug
            };
        }

        private string GetApiMode()
        {
            if (UseTvApi) return "TV";
            if (UseAppApi) return "APP";
            if (UseIntlApi) return "INTL";
            return "WEB";
        }

        private string GetDownloadMode()
        {
            if (AudioOnly) return "AudioOnly";
            if (VideoOnly) return "VideoOnly";
            return "Full";
        }

        private MyOption BuildOption()
        {
            var selectedPageText = BuildSelectedPageText();
            return new MyOption
            {
                Url = Url,
                UseTvApi = UseTvApi,
                UseAppApi = UseAppApi,
                UseIntlApi = UseIntlApi,
                UseMP4box = UseMp4box,
                EncodingPriority = EncodingPriority,
                DfnPriority = DfnPriority,
                WorkDir = OutputDir,
                FilePattern = FilePattern,
                MultiFilePattern = MultiFilePattern,
                SelectPage = selectedPageText,
                DownloadDanmaku = DownloadDanmaku,
                SkipSubtitle = SkipSubtitle,
                SkipCover = SkipCover,
                AudioOnly = AudioOnly,
                VideoOnly = VideoOnly,
                UseAria2c = UseAria2c,
                MultiThread = MultiThread,
                ShowAll = ShowAll,
                SkipMux = SkipMux,
                Debug = Debug,
                Cookie = Cookie,
                AccessToken = AccessToken,
                ForceHttp = true
            };
        }

        private string BuildSelectedPageText()
        {
            if (Pages.Count == 0)
            {
                return SelectPage;
            }

            var selected = Pages.Where(p => p.IsSelected).Select(p => p.Index.ToString()).ToList();
            if (selected.Count == 0)
            {
                return SelectPage;
            }

            if (selected.Count == Pages.Count)
            {
                return "ALL";
            }

            return string.Join(",", selected);
        }

        private void SetAllPagesSelected(bool isSelected)
        {
            foreach (var page in Pages)
            {
                page.IsSelected = isSelected;
            }
        }

        private void InvertPageSelection()
        {
            foreach (var page in Pages)
            {
                page.IsSelected = !page.IsSelected;
            }
        }

        private void SelectLatestPage()
        {
            if (Pages.Count == 0) return;

            SetAllPagesSelected(false);
            Pages[^1].IsSelected = true;
        }

        private async Task ParseAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                System.Windows.MessageBox.Show("请输入视频地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsBusy = true;
            Logs.Clear();
            Pages.Clear();
            Stream.Clear("正在解析视频信息...");
            VideoTitle = "";
            VideoInfo = "";

            try
            {
                var option = BuildOption();
                var runner = new BBDownRunner();
                var vInfo = await runner.ParseAsync(option);

                VideoTitle = vInfo.Title;
                VideoInfo = $"共 {vInfo.PagesInfo.Count} 个分P";
                foreach (var page in vInfo.PagesInfo)
                {
                    Pages.Add(new PageViewModel
                    {
                        IsSelected = true,
                        Index = page.index,
                        Title = page.title,
                        Duration = BBDownUtil.FormatTime(page.dur),
                        Aid = page.aid,
                        Cid = page.cid
                    });
                }

                var fetchedAid = await BBDownUtil.GetAvIdAsync(Url);
                var firstEncoding = GetFirstEncoding(EncodingPriority);
                await LoadFirstPageStreamsAsync(option, fetchedAid, firstEncoding ?? "", vInfo.PagesInfo.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Stream.Clear("流信息未加载。");
                System.Windows.MessageBox.Show($"解析失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string GetFirstEncoding(string? encodingPriority)
        {
            if (string.IsNullOrWhiteSpace(encodingPriority))
            {
                return "";
            }

            return encodingPriority
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? "";
        }

        private async Task LoadFirstPageStreamsAsync(MyOption option, string fetchedAid, string firstEncoding, BBDown.Core.Entity.Entity.Page? page)
        {
            if (page == null)
            {
                Stream.Clear("没有可展示的分P。");
                return;
            }

            Stream.IsLoading = true;
            Stream.Clear($"正在加载 P{page.index} 的流信息...");
            try
            {
                var parsedResult = await Parser.ExtractTracksAsync(
                    fetchedAid,
                    page.aid,
                    page.cid,
                    page.epid,
                    option.UseTvApi,
                    option.UseIntlApi,
                    option.UseAppApi,
                    firstEncoding);

                foreach (var video in parsedResult.VideoTracks)
                {
                    Stream.VideoStreams.Add(new VideoStreamOption
                    {
                        Dfn = video.dfn,
                        Resolution = video.res ?? "",
                        Codecs = video.codecs,
                        Fps = video.fps ?? "",
                        Bandwidth = FormatBandwidth(video.bandwith),
                        Size = FormatStreamSize(video.size)
                    });
                }

                foreach (var audio in parsedResult.AudioTracks)
                {
                    Stream.AudioStreams.Add(new AudioStreamOption
                    {
                        Kind = "音频",
                        Codecs = audio.codecs,
                        Bandwidth = FormatBandwidth(audio.bandwith),
                        Size = EstimateAudioSize(audio.bandwith, audio.dur)
                    });
                }

                foreach (var audio in parsedResult.BackgroundAudioTracks)
                {
                    Stream.AudioStreams.Add(new AudioStreamOption
                    {
                        Kind = "背景音",
                        Codecs = audio.codecs,
                        Bandwidth = FormatBandwidth(audio.bandwith),
                        Size = EstimateAudioSize(audio.bandwith, audio.dur)
                    });
                }

                foreach (var role in parsedResult.RoleAudioList)
                {
                    foreach (var audio in role.audio)
                    {
                        Stream.AudioStreams.Add(new AudioStreamOption
                        {
                            Kind = string.IsNullOrWhiteSpace(role.title) ? "配音" : $"配音: {role.title}",
                            Codecs = audio.codecs,
                            Bandwidth = FormatBandwidth(audio.bandwith),
                            Size = EstimateAudioSize(audio.bandwith, audio.dur)
                        });
                    }
                }

                if (Stream.VideoStreams.Count == 0 && Stream.AudioStreams.Count == 0)
                {
                    Stream.StatusText = "未获取到可展示的流信息。";
                }
                else
                {
                    Stream.StatusText = $"已加载 P{page.index}：{Stream.VideoStreams.Count} 条视频流，{Stream.AudioStreams.Count} 条音频流。";
                }
            }
            catch (Exception ex)
            {
                Stream.Clear($"流信息加载失败：{ex.Message}");
            }
            finally
            {
                Stream.IsLoading = false;
            }
        }

        private static string FormatBandwidth(long bandwidth)
        {
            return bandwidth <= 0 ? "" : $"{bandwidth:N0} kbps";
        }

        private static string FormatStreamSize(double size)
        {
            return size <= 0 ? "" : BBDownUtil.FormatFileSize(size);
        }

        private static string EstimateAudioSize(long bandwidth, int duration)
        {
            if (bandwidth <= 0 || duration <= 0) return "";
            var bytes = bandwidth * 1000d / 8d * duration;
            return BBDownUtil.FormatFileSize(bytes);
        }

        private async Task DownloadAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                System.Windows.MessageBox.Show("请输入视频地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsBusy = true;
            Logs.Clear();
            ProgressValue = 0;
            ProgressText = "准备下载...";
            DownloadJob? currentJob = null;

            try
            {
                var option = BuildOption();
                currentJob = new DownloadJob
                {
                    Url = Url,
                    Title = string.IsNullOrWhiteSpace(VideoTitle) ? Url : VideoTitle,
                    Status = DownloadJobStatus.Downloading,
                    Progress = 0,
                    OutputPath = OutputDir,
                    Option = option
                };
                Tasks.Jobs.Add(currentJob);

                var runner = new BBDownRunner();
                await runner.RunAsync(option);
                ProgressValue = 100;
                ProgressText = "下载完成";
                currentJob.Status = DownloadJobStatus.Completed;
                currentJob.Progress = 100;
                History.Add(new HistoryRecord
                {
                    FinishedAt = DateTime.Now,
                    Title = currentJob.Title,
                    Url = currentJob.Url,
                    OutputPath = currentJob.OutputPath,
                    IsSuccessful = true
                });
                SaveHistory();
                System.Windows.MessageBox.Show("下载完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProgressText = "下载失败";
                if (currentJob != null)
                {
                    if (currentJob.Status == DownloadJobStatus.Downloading)
                    {
                        currentJob.Status = DownloadJobStatus.Failed;
                        currentJob.ErrorMessage = ex.Message;
                    }

                    History.Add(new HistoryRecord
                    {
                        FinishedAt = DateTime.Now,
                        Title = currentJob.Title,
                        Url = currentJob.Url,
                        OutputPath = currentJob.OutputPath,
                        IsSuccessful = false,
                        ErrorMessage = ex.Message
                    });
                    SaveHistory();
                }
                System.Windows.MessageBox.Show($"下载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void BrowseOutput()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择下载目录",
                SelectedPath = string.IsNullOrEmpty(OutputDir) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : OutputDir,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputDir = dialog.SelectedPath;
            }
        }

        private async Task LoginWebAsync()
        {
            IsBusy = true;
            var cts = new System.Threading.CancellationTokenSource();
            QrCodeWindow? qrWindow = null;
            Task? loginTask = null;

            try
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    qrWindow = new QrCodeWindow("WEB 登录", cts);
                    qrWindow.Owner = System.Windows.Application.Current.MainWindow;
                    qrWindow.Show();
                });

                var qrFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qrcode.png");
                var qrWatcher = Task.Run(async () =>
                {
                    for (int i = 0; i < 60; i++)
                    {
                        if (File.Exists(qrFile))
                        {
                            qrWindow?.SetQrCode(qrFile);
                            break;
                        }
                        await Task.Delay(500, cts.Token);
                    }
                }, cts.Token);

                loginTask = BBDownLoginUtil.LoginWEB();
                _ = Task.Run(async () =>
                {
                    while (!loginTask.IsCompleted)
                    {
                        if (cts.Token.IsCancellationRequested)
                            return;
                        await Task.Delay(500, cts.Token);
                    }
                }, cts.Token);

                await Task.WhenAny(loginTask, Task.Delay(Timeout.Infinite, cts.Token));
                qrWindow?.CloseSafely();

                if (loginTask.IsCompletedSuccessfully)
                {
                    System.Windows.MessageBox.Show("WEB登录完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (loginTask.IsFaulted)
                {
                    throw loginTask.Exception?.InnerException ?? loginTask.Exception!;
                }
            }
            catch (OperationCanceledException)
            {
                qrWindow?.CloseSafely();
            }
            catch (Exception ex)
            {
                qrWindow?.CloseSafely();
                System.Windows.MessageBox.Show($"登录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoginTvAsync()
        {
            IsBusy = true;
            var cts = new System.Threading.CancellationTokenSource();
            QrCodeWindow? qrWindow = null;
            Task? loginTask = null;

            try
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    qrWindow = new QrCodeWindow("TV 登录", cts);
                    qrWindow.Owner = System.Windows.Application.Current.MainWindow;
                    qrWindow.Show();
                });

                var qrFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qrcode.png");
                var qrWatcher = Task.Run(async () =>
                {
                    for (int i = 0; i < 60; i++)
                    {
                        if (File.Exists(qrFile))
                        {
                            qrWindow?.SetQrCode(qrFile);
                            break;
                        }
                        await Task.Delay(500, cts.Token);
                    }
                }, cts.Token);

                loginTask = BBDownLoginUtil.LoginTV();
                _ = Task.Run(async () =>
                {
                    while (!loginTask.IsCompleted)
                    {
                        if (cts.Token.IsCancellationRequested)
                            return;
                        await Task.Delay(500, cts.Token);
                    }
                }, cts.Token);

                await Task.WhenAny(loginTask, Task.Delay(Timeout.Infinite, cts.Token));
                qrWindow?.CloseSafely();

                if (loginTask.IsCompletedSuccessfully)
                {
                    System.Windows.MessageBox.Show("TV登录完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (loginTask.IsFaulted)
                {
                    throw loginTask.Exception?.InnerException ?? loginTask.Exception!;
                }
            }
            catch (OperationCanceledException)
            {
                qrWindow?.CloseSafely();
            }
            catch (Exception ex)
            {
                qrWindow?.CloseSafely();
                System.Windows.MessageBox.Show($"登录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
