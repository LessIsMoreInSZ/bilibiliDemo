# BindingListThreadPitfallDemo

这个 WinForms 示例用于向初学者演示 `BindingList<T>` 数据绑定的一个常见坑：

`BindingList<T>` 只负责发出 `ListChanged` 通知，它不会自动把通知切回 UI 线程。如果后台线程直接修改已经绑定到 `DataGridView` 的 `BindingList<T>`，WinForms 控件可能会在非 UI 线程上被刷新，从而抛出跨线程异常。

常见异常类似：

```text
System.InvalidOperationException:
线程间操作无效: 从不是创建控件 "..." 的线程访问它。
```

## 运行

```powershell
dotnet run --project .\BindingListThreadPitfallDemo.csproj
```

## 演示步骤

1. 启动程序。
2. 点击 `错误示例：后台线程直接 Add`。
3. 观察异常弹窗和下方日志。错误点是后台线程直接执行 `_rows.Add(row)`。
4. 点击 `正确示例：Invoke 后再 Add`。
5. 观察数据正常添加到 `DataGridView`。

## 关键代码

错误写法：

```csharp
await Task.Run(() =>
{
    _rows.Add(row);
});
```

正确写法：

```csharp
if (InvokeRequired)
{
    BeginInvoke(() => _rows.Add(row));
    return;
}

_rows.Add(row);
```

## 教学重点

- `BindingList<T>` 不是线程安全集合。
- `BindingList<T>` 的 `ListChanged` 事件在哪个线程触发，取决于你在哪个线程修改它。
- WinForms 控件只能在创建它的 UI 线程访问。
- 后台线程可以负责拉取、计算、准备数据，但更新已绑定集合时要回到 UI 线程。
