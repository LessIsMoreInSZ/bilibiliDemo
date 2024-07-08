@echo off

:: 关闭Windows时间服务
net stop w32time
:: 启动Windows时间服务
net start w32time

:: 设置NTP服务器地址
w32tm /config /manualpeerlist:"192.168.1.61" /syncfromflags:manual /reliable:YES /update

:: 设置自动同步间隔为15min
:: 注意：实际有效间隔可能受系统策略和Windows服务限制
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\W32Time\TimeProviders\NtpClient" /v SpecialPollInterval /t REG_DWORD /d 900 /f

:: 使更改生效
w32tm /config /update

:: 触发立即时间同步
w32tm /resync
pause