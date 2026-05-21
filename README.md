# ChatApp

基于 Avalonia UI 的跨平台聊天应用程序，支持自定义角色和 AI 集成。

## 功能特性

- 跨平台支持 (Windows, macOS, Linux)
- 自定义聊天角色管理
- AI 代理集成
- Markdown 消息渲染
- 工具调用支持 (计算器、时间、天气等)

## 构建要求

- .NET 10.0 SDK
- Windows 10+ / macOS 10.14+ / Linux

## 构建说明

```bash
dotnet restore
dotnet build
dotnet run
```

## 项目结构

```
ChatApp/
├── ViewModels/      # MVVM ViewModels
├── Views/           # Avalonia 视图
├── Services/        # 业务服务和 AI 代理
├── Converters/      # 数据转换器
└── Assets/          # 静态资源
```

## 许可证

MIT
