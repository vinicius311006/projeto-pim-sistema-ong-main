@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul

echo =====================================================================
echo    ONG VESTINDO SONHOS - ATUALIZAR REPOSITÓRIO GITHUB COM 1 CLIQUE
echo =====================================================================
echo.
echo Este comando envia TODO o projeto de uma vez só:
echo   [+] Sistema Web ASP.NET Core (VestindoSonhos)
echo   [+] Aplicativo Mobile Flutter (vestindo_sonhos_mobile)
echo   [+] Banco de Dados MySQL (ScriptVestindoSonhos_MySQL.sql)
echo   [+] Scripts e configurações
echo.
echo =====================================================================
echo.

git status -s

echo.
set /p MSG="Digite a descricao da atualizacao (ou aperte Enter): "
if "%MSG%"=="" (
    set MSG=Atualizacao do sistema e aplicativo mobile
)

echo.
echo [1/3] Adicionando arquivos modificados...
git add .

echo [2/3] Criando o commit ("%MSG%")...
git commit -m "%MSG%"

echo [3/3] Enviando para o GitHub (origin main)...
git push origin main

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================================================
    echo  SUCESSO TOTAL! O GitHub foi atualizado com todas as mudancas!
    echo  Repositorio: https://github.com/vinicius311006/projeto-pim-sistema-ong-main
    echo =====================================================================
) else (
    echo.
    echo =====================================================================
    echo  [AVISO] Verifique se sua conexao com a internet esta ativa
    echo  ou se existem conflitos pendentes.
    echo =====================================================================
)

pause
