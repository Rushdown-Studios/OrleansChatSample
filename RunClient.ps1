param (
    [int]$Num = 5 # Default to 5 instances, modify as needed
)

for ($i = 1; $i -le $Num; $i++) {
    Start-Process "./WebSocketClient/bin/Debug/net9.0/WebSocketClient.exe"
    Write-Output "Started instance $i"
}

Write-Output "All $Num instances started."