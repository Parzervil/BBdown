# BBDown GUI 修订设计方案

## 1. 修订目标

本方案基于 BBDown 当前 CLI、Core 与现有 WPF GUI 代码进行修订。目标不是简单把命令行参数搬到界面上，而是把 BBDown 的核心能力组织成普通用户能顺着完成的下载工作流。

- **任务流优先**：围绕“输入链接 -> 解析 -> 选择内容 -> 下载 -> 查看结果”组织界面。
- **模块清晰**：让页面结构对应项目中的解析、下载、任务、历史、设置、登录和服务模块。
- **降低参数噪音**：常用选项放在下载页，高级参数收进设置页。
- **支持可视化选择**：用分 P 勾选、清晰度/编码/音轨选择替代命令行输入和 `--interactive`。
- **可扩展实现**：先补齐现有 WPF GUI，再逐步接入任务队列、历史记录、流选择和 API 服务控制。

---

## 2. 功能模块清单与 GUI 映射

| 项目功能模块 | 代码/CLI 来源 | GUI 页面归属 | 设计要点 |
| --- | --- | --- | --- |
| 内容解析 | `FetcherFactory`、`BBDownRunner.ParseAsync` | 下载页 | 支持普通视频、番剧、课程、空间、合集、收藏夹、系列列表 |
| API 源选择 | `--use-tv-api`、`--use-app-api`、`--use-intl-api` | 下载页快速设置 / 设置页 | WEB/TV/APP/国际版应以互斥或明确优先级呈现 |
| 分 P 选择 | `--select-page` | 下载页解析结果 | 表格勾选为主，文本范围输入作为高级入口 |
| 流选择 | `Parser.ExtractTracksAsync`、`--interactive` | 下载页流选择抽屉 | 展示视频流、音频流、多配音、杜比/Hi-Res 信息 |
| 下载执行 | `BBDownRunner.RunAsync`、`BBDownDownloadUtil` | 任务页 | 进度、速度、状态、取消、失败重试 |
| 内容类型 | `--video-only`、`--audio-only`、`--sub-only`、`--danmaku-only`、`--cover-only` | 下载页快速设置 | 做成单选下载模式，避免互相冲突 |
| 字幕/弹幕/封面 | `--download-danmaku`、`--skip-subtitle`、`--skip-cover`、`--skip-ai` | 下载页快速设置 / 设置页 | 常用开关保留在下载页，细节进入设置页 |
| 混流 | `--use-mp4box`、`--skip-mux`、`--simply-mux` | 设置页 | 与 ffmpeg/mp4box 路径配置放在一起 |
| 网络与下载器 | aria2c、UPOS、PCDN、ForceHttp | 设置页 | 作为高级下载设置，提供默认值说明和校验 |
| 鉴权 | `login`、`logintv`、Cookie、AccessToken | 登录页 | 二维码登录、登录状态、手动凭据管理 |
| 配置与输出 | `--work-dir`、文件名模板、`--config-file` | 设置页 | 可保存 GUI 配置，并支持模板变量提示 |
| 历史/跳过 | `--save-archives-to-file`、`BBDown.archives` | 历史页 / 设置页 | 历史记录与跳过已下载应联动 |
| API 服务 | `serve`、`BBDownApiServer` | 服务页 | 启停服务、监听地址、任务接口状态 |
| 日志 | `Logger.LogMessage` | 底部日志抽屉 | 默认折叠，错误时自动展开 |

---

## 3. 推荐信息架构

采用 **顶部主操作区 + 左侧导航 + 主工作区 + 可折叠日志区**。下载页仍是默认首页，不做营销式首页。

```
┌──────────────────────────────────────────────────────────────┐
│ BBDown   URL 输入框                         [解析] [加入下载] │
├──────────────┬───────────────────────────────────────────────┤
│ 下载         │ 解析结果 / 分P / 流选择 / 快速设置             │
│ 任务         │                                               │
│ 历史         │                                               │
│ 设置         │                                               │
│ 登录         │                                               │
│ 服务         │                                               │
├──────────────┴───────────────────────────────────────────────┤
│ 日志抽屉：默认收起，出错或调试模式展开                         │
└──────────────────────────────────────────────────────────────┘
```

说明：

- URL 输入和“解析/加入下载”是最高频动作，固定在顶部。
- 左侧导航承载模块切换，避免把所有参数平铺在首屏。
- 日志不占用主内容的一半空间，默认作为底部抽屉。
- 任务状态可以在导航或顶部显示小徽标，例如下载中数量、失败数量。

---

## 4. 页面设计

### 4.1 下载页

下载页承担主要工作流：输入链接、解析内容、选择分 P、选择流、设置常用下载选项。

#### URL 与主操作

- 输入框支持 BV、av、ep、ss、URL、合集、收藏夹、空间链接。
- 主按钮：
  - **解析**：只获取视频信息和分 P。
  - **加入下载**：使用当前选择创建任务。
  - **快速下载**：可选入口，按默认参数直接下载。
- 状态反馈：
  - 解析中显示加载状态。
  - 解析失败显示明确原因和重试按钮。
  - 登录受限时提示进入登录页。

#### 解析结果

- 展示封面、标题、UP 主、发布时间、总时长、分 P 数、内容类型。
- 分 P 使用 `DataGrid`：
  - 勾选框
  - P 序号
  - 标题
  - 时长
  - AID
  - CID
- 分 P 快捷操作：
  - 全选
  - 反选
  - 仅选当前/最新
  - 输入范围：如 `1,2,5-8,LAST,ALL`

#### 流选择

解析后提供“流选择”抽屉或 Tab。

- 视频流列表：清晰度、分辨率、编码、帧率、码率、估算大小。
- 音频流列表：格式、码率、语言、估算大小。
- 多配音/背景音频：单独分组展示。
- 默认选中由 `DfnPriority` 和 `EncodingPriority` 计算出的推荐流。
- 用户手动选择后，下载任务应保存选择结果，避免后续回到 CLI `Console.ReadLine()`。

#### 快速设置

只放高频、安全、用户能理解的选项：

- API 来源：WEB / TV / APP / 国际版。
- 输出目录。
- 下载模式：完整视频 / 仅视频 / 仅音频 / 仅字幕 / 仅弹幕 / 仅封面。
- 画质优先级预设。
- 编码优先级预设。
- 下载弹幕。
- 跳过字幕。
- 跳过 AI 字幕。
- 跳过封面。
- 跳过混流。

互斥项必须用单选、分段控件或下拉框，不应全部做成复选框。

---

### 4.2 任务页

任务页对应下载队列和运行状态。

#### 任务列表

每个任务行显示：

- 封面缩略图
- 标题与来源 URL
- 当前阶段：等待中 / 解析中 / 下载视频 / 下载音频 / 下载字幕 / 混流中 / 完成 / 失败 / 已取消
- 进度条和百分比
- 实时速度、已下载大小、估算剩余时间
- 输出目录
- 操作按钮：暂停/继续、取消、重试、打开目录

#### 全局控制

- 同时下载数量。
- 全部暂停。
- 全部取消。
- 清理已完成。
- 仅显示失败任务。

#### 实现注意

当前 `BBDownRunner.RunAsync` 只在入口检查 `CancellationToken`，下载链路内部还未贯穿取消逻辑。要实现取消/暂停，需要把 token 传入：

- `Program.DownloadPagesAsync`
- `DownloadPageAsync`
- `DownloadTrackAsync`
- `RangeDownloadToTmpAsync`
- aria2c 进程控制

暂停可先做成“取消后保留临时文件，重新开始时断点续传”的简化版本。

---

### 4.3 历史页

历史页承载“已完成”和“失败记录”，不和“下载中”混在一起。

列表字段：

- 完成时间
- 标题
- 文件路径
- API 类型
- 下载模式
- 是否成功
- 错误摘要

操作：

- 打开文件
- 打开目录
- 重新下载
- 复制来源 URL
- 删除记录
- 清空成功记录
- 清空失败记录

历史数据建议使用 JSON 起步，后续记录量变大再迁移 SQLite。

---

### 4.4 设置页

设置页按用户理解而不是 CLI 参数顺序分组。

#### 输出

- 工作目录：`--work-dir`
- 单 P 文件名模板：`--file-pattern`
- 多 P 文件名模板：`--multi-file-pattern`
- 音频语言：`--language`
- 模板变量帮助：展示 `<videoTitle>`、`<pageNumberWithZero>`、`<dfn>` 等变量。

#### 下载器

- 多线程下载：`--multi-thread`
- 使用 aria2c：`--use-aria2c`
- aria2c 参数：`--aria2c-args`
- ffmpeg 路径：`--ffmpeg-path`
- mp4box 路径：`--mp4box-path`
- aria2c 路径：`--aria2c-path`
- 分 P 间隔：`--delay-per-page`

#### 解析与内容

- 显示所有分 P：`--show-all`
- 隐藏可用流：`--hide-streams`
- 视频升序：`--video-ascending`
- 音频升序：`--audio-ascending`
- 保存已下载归档：`--save-archives-to-file`

#### 混流

- 使用 MP4Box：`--use-mp4box`
- 跳过混流：`--skip-mux`
- 简易混流：`--simply-mux`

#### 网络与鉴权

- User-Agent：`--user-agent`
- Cookie：`--cookie`
- AccessToken：`--access-token`
- UPOS Host：`--upos-host`
- 强制替换 Host：`--force-replace-host`
- 强制 HTTP：`--force-http`
- 允许 PCDN：`--allow-pcdn`
- BiliPlus Host：`--host`
- BiliPlus EP Host：`--ep-host`
- BiliPlus Area：`--area`

#### 高级

- 调试日志：`--debug`
- 配置文件路径：`--config-file`
- 启动时检查更新。
- 主题：跟随系统 / 浅色 / 深色。

---

### 4.5 登录页

登录页独立于设置页，避免 Cookie/Token 淹没在高级参数里。

功能：

- WEB 登录：显示二维码，成功后保存 `BBDown.data`。
- TV 登录：显示二维码，成功后保存 `BBDownTV.data`。
- APP Token 提示：说明可使用 TV token 或手动填写。
- 当前登录状态：
  - 未登录
  - 已登录
  - Cookie/Token 可能失效
- 手动凭据输入：
  - Cookie
  - AccessToken
- 操作：
  - 保存
  - 测试登录状态
  - 清除凭据

---

### 4.6 服务页

BBDown 当前支持 `serve` 模式，GUI 应提供独立服务页，而不是放进高级设置。

功能：

- 监听地址输入，例如 `http://127.0.0.1:23333`。
- 启动服务。
- 停止服务。
- 当前服务状态。
- API 地址展示：
  - `/get-tasks`
  - `/get-tasks/running`
  - `/get-tasks/finished`
  - `/add-task`
  - `/remove-finished`
- 服务任务列表可复用任务页组件。

---

## 5. 视觉与交互规范

- 主色可沿用 B 站蓝，但不要让所有强调元素都使用同一蓝色。
- 下载页主操作按钮最多两个主要按钮，避免“解析/下载/仅解析/快速下载”同级堆叠。
- 高级选项默认收起，避免首屏像参数表。
- 表格、任务行、设置表单的间距和标题层级保持一致。
- 错误信息应转成用户可理解的短句，同时保留“查看详细日志”入口。
- Cookie、AccessToken 等敏感字段默认隐藏，提供显示/复制/清除按钮。
- 长文本字段必须支持横向滚动或多行编辑，避免撑破布局。
- 所有后台更新 UI 的逻辑都必须通过 Dispatcher。

---

## 6. ViewModel 结构

建议从当前单个 `MainViewModel` 拆分：

```
MainViewModel
├── ShellViewModel
├── DownloadViewModel
│   ├── ParseResultViewModel
│   ├── PageSelectionViewModel
│   └── StreamSelectionViewModel
├── TaskQueueViewModel
├── HistoryViewModel
├── SettingsViewModel
├── LoginViewModel
├── ServiceViewModel
└── LogViewModel
```

数据模型建议：

- `DownloadJob`：GUI 层任务对象，包含 URL、选项、选择的分 P、选择的流、状态、进度、错误。
- `GuiSettings`：GUI 持久化配置，不直接等同于 `MyOption`。
- `OptionProfile`：可选，保存常用下载参数预设。
- `HistoryRecord`：完成/失败记录。

`MyOption` 仍作为调用底层下载流程的参数对象，由 GUI 层在创建任务时转换生成。

---

## 7. 与现有代码集成的关键改造

### 7.1 必须改造

1. **取消令牌贯穿**
   - 现状：`BBDownRunner.RunAsync` 接收 token，但底层下载未真正使用。
   - 目标：下载、字幕、弹幕、封面、混流前后都能响应取消。

2. **任务进度上下文**
   - 现状：`ProgressBar` 更偏 CLI 输出，API server 通过 `DownloadTask` 记录部分进度。
   - 目标：抽象 `IDownloadProgressSink` 或类似上下文，让 CLI、GUI、API server 共享进度来源。

3. **GUI 流选择**
   - 现状：CLI 手动选择依赖 `Console.ReadLine()`。
   - 目标：解析流后把选择结果传入下载流程，避免 GUI 调用控制台交互。

4. **参数冲突处理前置**
   - 现状：部分冲突由底层处理。
   - 目标：GUI 在用户选择阶段就避免冲突，例如仅音频和仅视频不可同时启用。

5. **配置持久化**
   - 现状：GUI 没有完整配置保存。
   - 目标：保存 `GuiSettings`，启动时恢复常用设置、窗口尺寸、主题和默认目录。

### 7.2 可后置改造

1. API 服务页面。
2. 历史记录 SQLite 化。
3. 下载预设管理。
4. 托盘最小化。
5. 剪贴板监听。
6. 自动更新提示。

---

## 8. 实施优先级

### 第一阶段：整理现有界面

- 把单窗口平铺布局改成导航结构。
- 下载页只保留 URL、解析结果、分 P、快速设置、日志抽屉。
- 设置页承接高级参数。
- 登录按钮迁移到登录页。

### 第二阶段：任务队列

- 新增 `DownloadJob`。
- 支持添加多个任务。
- 支持任务状态、进度、速度、完成/失败。
- 先实现取消，暂停可后置。

### 第三阶段：分 P 与流选择

- 分 P 表格支持勾选。
- 解析后展示视频流和音频流。
- 手动选择结果传入下载流程。
- 替代 `--interactive` 的控制台输入。

### 第四阶段：持久化与历史

- 保存 GUI 设置。
- 保存下载历史。
- 支持重新下载、打开文件、清理历史。
- 支持 `--save-archives-to-file` 的可视化控制。

### 第五阶段：服务与高级体验

- 服务页接入 `BBDownApiServer`。
- 剪贴板监听。
- 主题切换。
- 托盘最小化。
- 下载预设。

---

## 9. 当前 GUI 实现的主要问题

当前 `MainWindow.xaml` 已经具备基本可用的解析、下载、登录、日志和部分参数输入，但与完整 GUI 目标相比存在以下问题：

- 没有真正的导航或 Tab，所有区域平铺在一个窗口中。
- 设置面板占据首屏过多，削弱下载主流程。
- 分 P 列表只能展示，不能勾选。
- 没有流选择界面，尚不能替代 `--interactive`。
- 没有任务队列、历史记录和失败重试。
- 进度只在开始和完成时粗略更新，不能反映每个任务的真实下载状态。
- 登录缺少二维码窗口和登录状态展示。
- API server 功能未被 GUI 覆盖。
- 多个互斥下载模式被做成复选框，容易产生冲突组合。
- 高级参数覆盖不完整，例如 `--simply-mux`、`--save-archives-to-file`、`--video-ascending`、`--audio-ascending`、`--force-replace-host` 等。

---

## 10. Codex 实施指引

本节用于后续让 Codex 按阶段推进 GUI 代码实现。每一阶段都应保持项目可编译，并避免无关重构。

### 10.1 当前基线

当前 GUI 已有能力：

- `BBDown.GUI/MainWindow.xaml` 是单窗口平铺布局，包含 URL 输入、部分设置、视频信息、日志和底部进度。
- `BBDown.GUI/MainViewModel.cs` 已有 `ParseCommand`、`DownloadCommand`、`LoginWebCommand`、`LoginTvCommand`、`ClearLogCommand`。
- `MainViewModel.BuildOption()` 已能把部分 GUI 字段转换为 `MyOption`。
- `BBDownRunner.ParseAsync()` 可复用解析入口。
- `BBDownRunner.RunAsync()` 可复用下载入口。
- `Logger.LogMessage` 已接入 GUI 日志列表，并通过 Dispatcher 更新 UI。

当前缺口：

- 没有 Shell 导航和独立页面。
- 没有任务队列、任务状态、历史记录和失败重试。
- 分 P 表格没有勾选能力。
- 没有流选择 UI。
- 没有 GUI 设置持久化。
- 没有二维码登录窗口和登录状态展示。
- 没有 API 服务页。
- `CancellationToken` 没有贯穿到底层下载过程。

### 10.2 推荐推进顺序

#### Phase 1：Shell 导航重构

- [ ] 新增页面枚举或导航状态，例如 `CurrentPage`。
- [ ] 将主窗口改为顶部 URL 操作区、左侧导航、主内容区、底部日志抽屉。
- [ ] 先用原生 WPF 控件，不引入新 UI 框架。
- [ ] 保留现有解析、下载、登录、日志逻辑，不改 Core。
- [ ] 新增占位页面：下载、任务、历史、设置、登录、服务。
- [ ] 把现有下载相关控件迁移到下载页。

完成定义：

- GUI 可启动。
- 左侧导航可切换页面。
- 解析按钮仍能调用原有 `ParseAsync()`。
- 下载按钮仍能调用原有 `DownloadAsync()`。
- `dotnet build BBDown.sln` 通过。

建议文件范围：

- `BBDown.GUI/MainWindow.xaml`
- `BBDown.GUI/MainViewModel.cs`
- `BBDown.GUI/Views/DownloadView.xaml`
- `BBDown.GUI/Views/TaskQueueView.xaml`
- `BBDown.GUI/Views/HistoryView.xaml`
- `BBDown.GUI/Views/SettingsView.xaml`
- `BBDown.GUI/Views/LoginView.xaml`
- `BBDown.GUI/Views/ServiceView.xaml`

#### Phase 2：ViewModel 拆分与基础模型

- [ ] 新增 `ViewModelBase`，承载 `INotifyPropertyChanged` 和 `SetProperty`。
- [ ] 新增 `DownloadViewModel`，迁移 URL、解析结果、快速设置和下载命令。
- [ ] 新增 `LogViewModel`，管理日志集合和清空命令。
- [ ] 新增 `SettingsViewModel`，承载高级参数。
- [ ] 新增 `DownloadJob` 和 `DownloadJobStatus`。
- [ ] `MainViewModel` 只保留导航、子 ViewModel 和 Shell 状态。

完成定义：

- 页面绑定不报错。
- 原有解析和下载能力不丢失。
- 不改变 CLI 项目行为。
- `dotnet build BBDown.sln` 通过。

建议文件范围：

- `BBDown.GUI/MainViewModel.cs`
- `BBDown.GUI/ViewModels/ViewModelBase.cs`
- `BBDown.GUI/ViewModels/DownloadViewModel.cs`
- `BBDown.GUI/ViewModels/LogViewModel.cs`
- `BBDown.GUI/ViewModels/SettingsViewModel.cs`
- `BBDown.GUI/Models/DownloadJob.cs`
- `BBDown.GUI/Models/DownloadJobStatus.cs`

#### Phase 3：任务队列

- [ ] 新增 `TaskQueueViewModel`。
- [ ] 下载按钮改为创建 `DownloadJob` 并加入队列。
- [ ] 每个任务显示标题、URL、状态、进度、错误信息。
- [ ] 起步阶段允许串行执行，后续再加并发数控制。
- [ ] 支持取消排队中任务。
- [ ] 运行中取消可以先标记为暂不支持，等底层 token 改造后再接入。

完成定义：

- 可以添加多个任务。
- 任务状态能从排队变为运行、完成或失败。
- 单任务下载仍使用现有 `BBDownRunner.RunAsync()`。
- 失败时保留错误消息，不吞异常。

建议文件范围：

- `BBDown.GUI/ViewModels/TaskQueueViewModel.cs`
- `BBDown.GUI/Models/DownloadJob.cs`
- `BBDown.GUI/Views/TaskQueueView.xaml`
- `BBDown.GUI/ViewModels/DownloadViewModel.cs`

#### Phase 4：分 P 勾选

- [ ] 扩展 `PageViewModel`，增加 `IsSelected`。
- [ ] 分 P 表格增加勾选列。
- [ ] 解析完成后默认全选或按 `SelectPage` 规则选中。
- [ ] 加入下载时把勾选结果转换为 `MyOption.SelectPage`。
- [ ] 保留手动范围输入作为高级入口。

完成定义：

- 用户可以通过表格勾选分 P。
- 多选结果能正确生成 `1,2,5-8` 或简单逗号列表。
- 未解析时仍允许直接使用手动 `SelectPage`。

建议文件范围：

- `BBDown.GUI/Models/PageViewModel.cs`
- `BBDown.GUI/ViewModels/DownloadViewModel.cs`
- `BBDown.GUI/Views/DownloadView.xaml`

#### Phase 5：设置持久化

- [ ] 新增 `GuiSettings`。
- [ ] 新增 `GuiSettingsService`，读写 `BBDown.GUI.config.json`。
- [ ] 启动时加载设置。
- [ ] 关闭或点击保存时写入设置。
- [ ] 不直接序列化 `MyOption`，避免 GUI 状态和 CLI 参数耦合过深。

完成定义：

- 输出目录、API 选择、画质/编码预设、常用开关可持久化。
- 配置文件损坏时能回退默认值并提示。
- 不影响 CLI 的 `BBDown.config` 读取逻辑。

建议文件范围：

- `BBDown.GUI/Models/GuiSettings.cs`
- `BBDown.GUI/Services/GuiSettingsService.cs`
- `BBDown.GUI/ViewModels/SettingsViewModel.cs`
- `BBDown.GUI/App.xaml.cs`

#### Phase 6：流选择展示

- [ ] 解析视频信息后，为选中的分 P 调用 `Parser.ExtractTracksAsync()`。
- [ ] 新增 `StreamSelectionViewModel`。
- [ ] 展示视频流、音频流、背景音轨和角色配音。
- [ ] 起步阶段只做展示和选择状态。
- [ ] 真正传入底层下载流程需要后续 Core 改造，不在本阶段强行完成。

完成定义：

- 用户能看到可用清晰度、编码、分辨率、码率和音频格式。
- 默认选中项与当前优先级规则一致或接近。
- 不触发 `Console.ReadLine()`。

建议文件范围：

- `BBDown.GUI/ViewModels/StreamSelectionViewModel.cs`
- `BBDown.GUI/Models/VideoStreamOption.cs`
- `BBDown.GUI/Models/AudioStreamOption.cs`
- `BBDown.GUI/Views/DownloadView.xaml`
- `BBDown.GUI/ViewModels/DownloadViewModel.cs`

#### Phase 7：登录与历史

- [ ] 登录页展示 WEB/TV 登录入口。
- [ ] 增加 Cookie 和 AccessToken 的隐藏输入框。
- [ ] 增加保存、清除、测试状态按钮。
- [ ] 新增 `HistoryRecord`。
- [ ] 任务完成或失败后写入历史。
- [ ] 历史页支持打开目录、重新下载、删除记录。

完成定义：

- 凭据输入不再混在下载页。
- 历史记录可保存和重新加载。
- 失败任务能在历史页看到错误摘要。

#### Phase 8：API 服务页

- [ ] 增加服务页监听地址输入。
- [ ] 显示服务状态。
- [ ] 提供启动、停止按钮。
- [ ] 展示常用 API 路径。
- [ ] 可后置服务进程生命周期管理，先做状态和说明也可接受。

完成定义：

- 用户能理解 `serve` 功能和当前监听地址。
- 不影响 CLI `serve` 命令。

### 10.3 最小数据模型草案

```csharp
public enum AppPage
{
    Download,
    Tasks,
    History,
    Settings,
    Login,
    Service
}

public enum DownloadJobStatus
{
    Queued,
    Parsing,
    Downloading,
    Muxing,
    Completed,
    Failed,
    Canceled
}

public sealed class DownloadJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public DownloadJobStatus Status { get; set; }
    public double Progress { get; set; }
    public string SpeedText { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public MyOption Option { get; set; } = new();
}

public sealed class GuiSettings
{
    public string OutputDir { get; set; } = "";
    public string ApiMode { get; set; } = "WEB";
    public string DownloadMode { get; set; } = "Full";
    public string EncodingPriority { get; set; } = "hevc,av1,avc";
    public string DfnPriority { get; set; } = "8K 超高清,4K 超清,1080P 高码率,1080P 高清";
    public bool DownloadDanmaku { get; set; }
    public bool SkipSubtitle { get; set; }
    public bool SkipAi { get; set; } = true;
    public bool SkipCover { get; set; }
    public bool SkipMux { get; set; }
}
```

### 10.4 GUI 到 `MyOption` 的映射

| GUI 字段 | `MyOption` 字段 | 规则 |
| --- | --- | --- |
| URL | `Url` | 必填，解析和下载共用 |
| API = WEB | `UseTvApi/UseAppApi/UseIntlApi` | 三者均为 `false` |
| API = TV | `UseTvApi` | `true`，APP/INTL 为 `false` |
| API = APP | `UseAppApi` | `true`，TV/INTL 为 `false` |
| API = 国际版 | `UseIntlApi` | `true`，TV/APP 为 `false` |
| 输出目录 | `WorkDir` | 空值表示使用当前工作目录 |
| 单 P 模板 | `FilePattern` | 空值使用 Core 默认 |
| 多 P 模板 | `MultiFilePattern` | 空值使用 Core 默认 |
| 分 P 勾选 | `SelectPage` | 生成 `1,2,3`、`ALL` 或空值 |
| 画质优先级 | `DfnPriority` | 直接传入逗号分隔字符串 |
| 编码优先级 | `EncodingPriority` | 直接传入逗号分隔字符串 |
| 下载模式 = 完整 | `VideoOnly/AudioOnly/SubOnly/DanmakuOnly/CoverOnly` | 全部为 `false` |
| 下载模式 = 仅视频 | `VideoOnly` | `true`，其他 only 为 `false` |
| 下载模式 = 仅音频 | `AudioOnly` | `true`，其他 only 为 `false` |
| 下载模式 = 仅字幕 | `SubOnly` | `true`，其他 only 为 `false` |
| 下载模式 = 仅弹幕 | `DanmakuOnly` | `true`，其他 only 为 `false` |
| 下载模式 = 仅封面 | `CoverOnly` | `true`，其他 only 为 `false` |
| 下载弹幕 | `DownloadDanmaku` | 完整模式下可用 |
| 跳过字幕 | `SkipSubtitle` | 与仅字幕模式互斥 |
| 跳过 AI 字幕 | `SkipAi` | 默认 `true` |
| 跳过封面 | `SkipCover` | 与仅封面模式互斥 |
| 跳过混流 | `SkipMux` | 完整/仅视频/仅音频场景可用 |
| 使用 MP4Box | `UseMP4box` | 高级混流设置 |
| 简易混流 | `SimplyMux` | 高级混流设置 |
| 多线程 | `MultiThread` | 默认 `true` |
| 使用 aria2c | `UseAria2c` | 需要校验路径或 PATH |
| aria2c 参数 | `Aria2cArgs` | 仅 `UseAria2c=true` 时启用 |
| ffmpeg 路径 | `FFmpegPath` | 设置页 |
| mp4box 路径 | `Mp4boxPath` | 设置页 |
| aria2c 路径 | `Aria2cPath` | 设置页 |
| Cookie | `Cookie` | 登录页或设置页高级 |
| AccessToken | `AccessToken` | 登录页或设置页高级 |
| User-Agent | `UserAgent` | 网络高级设置 |
| UPOS Host | `UposHost` | 网络高级设置 |
| 强制替换 Host | `ForceReplaceHost` | 默认 `true` |
| 强制 HTTP | `ForceHttp` | 默认 `true` |
| 允许 PCDN | `AllowPcdn` | 默认 `false` |
| 保存归档 | `SaveArchivesToFile` | 历史/跳过设置 |
| 调试日志 | `Debug` | 开启后展开日志抽屉 |

### 10.5 实现约束

- 每个阶段必须保持可编译。
- 不改变 `BBDown` CLI 的命令、参数和默认行为。
- 不把大段 Core 下载逻辑复制到 GUI。
- 不在 Phase 1 引入 HandyControl、WPF UI 或 MaterialDesign 等新依赖。
- 不把所有逻辑继续堆进 `MainViewModel`。
- 不在没有底层支持前承诺真正暂停。
- 不在 GUI 中调用任何需要 `Console.ReadLine()` 的流程。
- Cookie、AccessToken 默认隐藏，不在日志中输出。
- 后台线程更新 UI 必须通过 Dispatcher。

### 10.6 验收命令与检查项

优先使用：

```powershell
dotnet build BBDown.sln
```

每阶段通用检查：

- 构建通过。
- GUI 项目没有 XAML 编译错误。
- 主窗口能启动。
- 页面切换不会抛绑定异常。
- 解析按钮仍能进入 `BBDownRunner.ParseAsync()`。
- 下载按钮仍能进入 `BBDownRunner.RunAsync()` 或任务队列包装后的调用。
- 日志仍能接收 `Logger.LogMessage`。

涉及设置持久化时额外检查：

- 删除配置文件后能用默认值启动。
- 配置文件损坏时不崩溃。
- 保存后重启可恢复关键设置。

涉及任务队列时额外检查：

- 成功任务进入完成状态。
- 失败任务保留错误摘要。
- 多个任务不会互相覆盖 UI 状态。

### 10.7 暂缓事项

以下内容可以在 GUI 主流程稳定后再做：

- 真正暂停/继续。
- 完整取消令牌贯穿 Core。
- 流选择结果直接注入下载核心。
- SQLite 历史库。
- 托盘最小化。
- 剪贴板监听。
- 第三方 UI 组件库。
- API server 的完整进程生命周期管理。

---

## 11. 推荐结论

BBDown GUI 的方向应从“命令行参数可视化”调整为“下载任务工作台”。首屏服务于下载，设置页服务于参数，任务页服务于进度，历史页服务于结果，登录页服务于鉴权，服务页服务于 API 模式。

短期最值得做的是：**导航拆分 + 分 P 勾选 + 任务队列 + 设置持久化**。这四项完成后，GUI 才会从“能调用 BBDown 的窗口”变成“真正适合普通用户使用的桌面下载器”。
