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

echo.
echo [1/3] Compilando executavel em modo Release (win-x64 Self-Contained)...
set "OUTDIR=%~dp0VestindoSonhos_Portavel"

dotnet publish "%CSPROJ%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%OUTDIR%"

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
