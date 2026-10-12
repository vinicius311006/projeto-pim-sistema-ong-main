@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul

echo =====================================================================
echo       GERADOR DO APLICATIVO ANDROID (APK) - VESTINDO SONHOS
echo =====================================================================
echo.
echo Verificando instalacao do Flutter...

where flutter >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] O Flutter nao foi encontrado no PATH do Windows!
    echo Certifique-se de ter o Flutter SDK instalado nesta maquina para gerar o APK.
    echo.
    pause
    exit /b 1
)

set "FLUTTER_DIR=%~dp0vestindo_sonhos_mobile"
if not exist "%FLUTTER_DIR%\pubspec.yaml" (
    echo [ERRO] Pasta do projeto mobile nao encontrada em:
    echo %FLUTTER_DIR%
    pause
    exit /b 1
)

echo.
echo [1/3] Compilando APK Android (Modo Debug/Fast)...
cd /d "%FLUTTER_DIR%"
call flutter build apk --debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao compilar o APK! Verifique os erros acima.
    pause
    exit /b %ERRORLEVEL%
)

set "APK_ORIGEM=%FLUTTER_DIR%\build\app\outputs\flutter-apk\app-debug.apk"
set "APK_DESTINO=%~dp0VestindoSonhos_Portavel\VestindoSonhos_Mobile.apk"

if not exist "%APK_ORIGEM%" (
    echo [ERRO] O arquivo APK gerado nao foi localizado!
    pause
    exit /b 1
)

echo.
echo [2/3] Copiando APK para a pasta VestindoSonhos_Portavel...
if not exist "%~dp0VestindoSonhos_Portavel" mkdir "%~dp0VestindoSonhos_Portavel"
copy /y "%APK_ORIGEM%" "%APK_DESTINO%" > nul

echo.
echo [3/3] Atualizando versao Web do Flutter (para iPhone / Navegador)...
call flutter build web --base-href /mobile/
if %ERRORLEVEL% EQU 0 (
    if not exist "%~dp0VestindoSonhos\wwwroot\mobile" mkdir "%~dp0VestindoSonhos\wwwroot\mobile"
    xcopy /e /y /q "%FLUTTER_DIR%\build\web\*" "%~dp0VestindoSonhos\wwwroot\mobile\" > nul
    if exist "%~dp0VestindoSonhos_Portavel\wwwroot\mobile" (
        xcopy /e /y /q "%FLUTTER_DIR%\build\web\*" "%~dp0VestindoSonhos_Portavel\wwwroot\mobile\" > nul
    )
)

echo.
echo =====================================================================
echo  SUCESSO! APK ATUALIZADO COM SUCESSO!
echo  Arquivo pronto em:
echo  %APK_DESTINO%
echo =====================================================================
echo Voce ja pode enviar esse arquivo para o celular Android ou testar!
echo =====================================================================
pause
