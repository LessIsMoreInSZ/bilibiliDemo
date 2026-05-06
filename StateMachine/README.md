# C# 状态机 Demo

这个目录包含两个 `.NET Framework 4.8` 控制台示例，业务场景保持一致，都是一个简单的订单流转状态机：

- `CustomStateMachineDemo`
  - 纯手写状态机实现
  - 展示状态、触发器、守卫条件、进入动作
- `StatelessStateMachineDemo`
  - 使用成熟三方库 `Stateless`
  - 展示声明式配置、守卫条件、状态迁移回调

## 业务状态

- `Draft`
- `Submitted`
- `Approved`
- `Rejected`
- `Paid`
- `Shipped`
- `Cancelled`

## 运行方式

```powershell
dotnet run --project .\CustomStateMachineDemo\CustomStateMachineDemo.csproj
dotnet run --project .\StatelessStateMachineDemo\StatelessStateMachineDemo.csproj
```

## 对比点

- 手写版适合学习状态机原理，代码完全可控。
- `Stateless` 版配置更紧凑，扩展层级状态、导出图、外部持久化状态会更方便。
