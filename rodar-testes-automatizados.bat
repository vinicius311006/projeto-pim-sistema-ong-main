@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
echo =====================================================================
echo  ONG VESTINDO SONHOS - EXECUÇÃO DE TESTES AUTOMATIZADOS (.NET / xUnit)
echo =====================================================================

set "TEST_PROJ=%~dp0VestindoSonhos.Tests\VestindoSonhos.Tests.csproj"

if not exist "%TEST_PROJ%" (
    echo [ERRO] Nao foi possivel localizar o projeto de testes: %TEST_PROJ%
    pause
    exit /b 1
)

if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
    set "PATH=%USERPROFILE%\.dotnet;%PATH%"
)
if exist "C:\Program Files\dotnet\dotnet.exe" (
    set "PATH=C:\Program Files\dotnet;%PATH%"
)

where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [INFO] .NET 10 SDK nao encontrado. Baixando e instalando automaticamente...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile \"$env:TEMP\dotnet-install.ps1\"; & \"$env:TEMP\dotnet-install.ps1\" -Channel 10.0 -InstallDir \"$env:USERPROFILE\.dotnet\""
    set "PATH=%USERPROFILE%\.dotnet;%PATH%"
)

echo.
echo Executando suite de testes automatizados (Voluntarios, Beneficiarios, Financeiro, RBAC e Dashboard)...
echo.

dotnet test "%TEST_PROJ%" --logger "console;verbosity=normal"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================================================
    echo  [SUCESSO] Todos os testes automatizados passaram com sucesso!
    echo =====================================================================
) else (
    echo.
    echo =====================================================================
    echo  [FALHA] Alguns testes falharam. Verifique o log acima.
    echo =====================================================================
)
echo.
pause
