@echo off
chcp 65001 > nul
echo =====================================================================
echo    INICIANDO ONG VESTINDO SONHOS
echo =====================================================================
echo - Nao necessita de VS Code aberto
echo - Nao necessita de .NET SDK
echo - Nao necessita de Android Studio
echo =====================================================================
echo.

set "EXE_PATH=%~dp0VestindoSonhos_Portavel\VestindoSonhos.exe"

if not exist "%EXE_PATH%" (
    echo [INFO] Executavel portatil nao encontrado. Gerando agora...
    call "%~dp0gerar-executavel-portatil.bat"
)

echo Fechando instancias antigas se houver...
taskkill /F /IM VestindoSonhos.exe >nul 2>&1
timeout /t 1 /nobreak >nul

echo Iniciando o servidor na porta 5000...
cd /d "%~dp0VestindoSonhos_Portavel"
if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
    start "" "%USERPROFILE%\.dotnet\dotnet.exe" "%~dp0VestindoSonhos_Portavel\VestindoSonhos.dll"
) else (
    start "" "%EXE_PATH%"
)

timeout /t 3 /nobreak > nul

echo Abrindo o navegador...
start http://localhost:5000/Login

echo.
echo =====================================================================
echo  SISTEMA PRONTO!
echo  - Plataforma Web: http://localhost:5000/Login
echo  - App Mobile no Navegador: http://localhost:5000/mobile/
echo  - Aplicativo Android para Celular: VestindoSonhos_Portavel\VestindoSonhos_Mobile.apk
echo =====================================================================
pause
