# WPF ComboBox 联动示例

当前目录包含两个 `.NET 6` WPF 示例项目：

- `WpfComboCascade.CodeBehind`
- `WpfComboCascade.Prism`

联动规则：

- 第 1 行选 `1`，第 2 到第 6 行自动显示 `2 3 4 5 6`
- 第 1 行选 `2`，第 2 到第 6 行自动显示 `3 4 5 6 空`
- 第 1 行选 `3`，第 2 到第 6 行自动显示 `4 5 6 空 空`

运行命令：

```powershell
dotnet build
dotnet run --project .\WpfComboCascade.CodeBehind\WpfComboCascade.CodeBehind.csproj
dotnet run --project .\WpfComboCascade.Prism\WpfComboCascade.Prism.csproj
```
