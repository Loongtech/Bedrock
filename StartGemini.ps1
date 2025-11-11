# 切换到工作目录
Set-Location -Path $PSScriptRoot

# 检测 gemini 命令是否存在
if (-not (Get-Command gemini -ErrorAction SilentlyContinue)) {
    Write-Host "找不到 gemini 命令，请检查 PATH"
    exit
}

# 检测 V2Ray 端口
$port = 10808
if (Test-NetConnection -ComputerName 127.0.0.1 -Port $port -InformationLevel Quiet) {
    $env:HTTP_PROXY="http://127.0.0.1:$port"
    $env:HTTPS_PROXY="http://127.0.0.1:$port"
    Write-Host "检测到 V2Ray 运行中，已启用代理"
} else {
    Remove-Item Env:HTTP_PROXY -ErrorAction SilentlyContinue
    Remove-Item Env:HTTPS_PROXY -ErrorAction SilentlyContinue
    Write-Host "未检测到 V2Ray，直连模式"
}

# 执行 gemini CLI
gemini
