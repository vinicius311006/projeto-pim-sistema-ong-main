@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
echo =====================================================================
echo  GERANDO EXECUTAVEL PORTATIL DA ONG VESTINDO SONHOS
echo  (Nao necessita de .NET SDK instalado em outros PCs)
echo =====================================================================

set "CSPROJ="
if exist "%~dp0VestindoSonhos\VestindoSonhos.csproj" (
    set "CSPROJ=%~dp0VestindoSonhos\VestindoSonhos.csproj"
) else if exist "%~dp0projeto-pim-sistema-ong-main\VestindoSonhos\VestindoSonhos.csproj" (
    set "CSPROJ=%~dp0projeto-pim-sistema-ong-main\VestindoSonhos\VestindoSonhos.csproj"
) else if exist "%~dp0VestindoSonhos.csproj" (
    set "CSPROJ=%~dp0VestindoSonhos.csproj"
) else (
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
echo [OK] Projeto localizado:
echo %CSPROJ%
echo.
echo Compilando em modo Release (win-x64 Self-Contained)...

set "OUTDIR=%~dp0VestindoSonhos_Portavel"

dotnet publish "%CSPROJ%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%OUTDIR%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao compilar o executavel portatil!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =====================================================================
echo  SUCESSO! Pasta 'VestindoSonhos_Portavel' criada com sucesso:
echo  %OUTDIR%
echo =====================================================================
echo Voce pode zipar ou copiar a pasta 'VestindoSonhos_Portavel' para
echo qualquer computador Windows. No outro PC, basta abrir a pasta e executar:
echo   VestindoSonhos.exe
echo (Nenhum .NET SDK precisa ser baixado ou instalado no outro PC!)
echo =====================================================================
pause
