@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
echo =====================================================================
echo  GERANDO EXECUTAVEL PORTATIL DA ONG VESTINDO SONHOS
echo  (Nao necessita de .NET SDK instalado em outros PCs)
echo =====================================================================

set "CSPROJ=%~dp0VestindoSonhos\VestindoSonhos.csproj"

if not exist "%CSPROJ%" (
    for /r "%~dp0" %%f in (VestindoSonhos.csproj) do (
        if exist "%%f" set "CSPROJ=%%f"
    )
)

if "%CSPROJ%"=="" (
    echo [ERRO] Nao foi possivel localizar o arquivo VestindoSonhos.csproj!
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
echo [1/3] Compilando executavel em modo Release (win-x64 Self-Contained)...
set "OUTDIR=%~dp0VestindoSonhos_Portavel"

taskkill /F /IM VestindoSonhos.exe >nul 2>&1
timeout /t 1 /nobreak >nul

dotnet publish "%CSPROJ%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o "%OUTDIR%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao compilar o executavel portatil!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [2/3] Sincronizando scripts e banco de dados na pasta portatil...
if exist "%~dp0ScriptVestindoSonhos_MySQL.sql" (
    copy /y "%~dp0ScriptVestindoSonhos_MySQL.sql" "%OUTDIR%\" > nul
)
if exist "%~dp0importar-banco-local.bat" (
    copy /y "%~dp0importar-banco-local.bat" "%OUTDIR%\" > nul
)
if exist "%~dp0VestindoSonhos\wwwroot" (
    if not exist "%OUTDIR%\wwwroot" mkdir "%OUTDIR%\wwwroot"
    xcopy /e /y /q "%~dp0VestindoSonhos\wwwroot\*" "%OUTDIR%\wwwroot\" > nul
)

(
echo @echo off
echo chcp 65001 ^> nul
echo echo =====================================================================
echo echo    INICIANDO ONG VESTINDO SONHOS ^(MODO PORTATIL^)
echo echo =====================================================================
echo taskkill /F /IM VestindoSonhos.exe ^>nul 2^>^&1
echo timeout /t 1 /nobreak ^>nul
echo cd /d "%%~dp0"
echo start "" "%%~dp0VestindoSonhos.exe"
echo timeout /t 3 /nobreak ^> nul
echo start http://localhost:5000/Login
echo echo.
echo echo Sistema iniciado em: http://localhost:5000/Login
echo pause
) > "%OUTDIR%\1-CLIQUE_INICIAR_SISTEMA.bat"

echo.
echo [3/3] Verificando se o Flutter esta disponivel para atualizar o APK...
where flutter >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Deseja atualizar o arquivo APK para o celular tambem? (S/N)
    set /p RESP="Opcao [S/N] (padrao S): "
    if /i "!RESP!" NEQ "N" (
        echo Compilando APK Android...
        call "%~dp0gerar-apk.bat"
    )
) else (
    echo [INFO] Flutter nao instalado nesta maquina. O APK existente foi mantido na pasta.
)

echo.
echo =====================================================================
echo  SUCESSO! Pasta 'VestindoSonhos_Portavel' atualizada com sucesso:
echo  %OUTDIR%
echo =====================================================================
echo Voce pode copiar a pasta 'VestindoSonhos_Portavel' para um pendrive
echo ou outro computador Windows. Para rodar, basta executar:
echo   1-CLIQUE_INICIAR_SISTEMA.bat   (ou VestindoSonhos.exe)
echo =====================================================================
pause
