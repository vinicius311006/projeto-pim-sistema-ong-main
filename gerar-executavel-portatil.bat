@echo off
chcp 65001 > nul
echo =====================================================================
echo  GERANDO EXECUTAVEL PORTATIL DA ONG VESTINDO SONHOS
echo  (Nao necessita de .NET SDK instalado em outros PCs)
echo =====================================================================
echo Compilando em modo Release (win-x64 Self-Contained)...

dotnet publish VestindoSonhos\VestindoSonhos.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "VestindoSonhos_Portavel"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao compilar o executavel portatil!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =====================================================================
echo  SUCESSO! Pasta 'VestindoSonhos_Portavel' criada.
echo =====================================================================
echo Voce pode zipar ou copiar a pasta 'VestindoSonhos_Portavel' para
echo qualquer computador Windows. No outro PC, basta executar:
echo   VestindoSonhos.exe
echo (Nenhum .NET SDK precisa ser baixado ou instalado no outro PC!)
echo =====================================================================
pause
