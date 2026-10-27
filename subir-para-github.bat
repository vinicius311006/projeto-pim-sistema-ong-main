@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
cd /d "%~dp0"

echo =====================================================================
echo    ONG VESTINDO SONHOS - ATUALIZAR REPOSITORIO GITHUB COM 1 CLIQUE
echo =====================================================================
echo.
echo Este comando envia TODO o projeto de uma vez so:
echo   [+] Sistema Web ASP.NET Core (Dashboard Chart.js, Dark Mode, SaaS)
echo   [+] Aplicativo Mobile Flutter (vestindo_sonhos_mobile atualizado)
echo   [+] Banco de Dados MySQL (ScriptVestindoSonhos_MySQL.sql)
echo   [+] Scripts de automacao e CI/CD (.github/workflows)
echo.
echo =====================================================================
echo.

REM Garante que o repositorio Git esteja inicializado (caso tenha sido baixado via .zip)
if not exist ".git" (
    echo [INFO] Inicializando repositorio Git local e conectando ao GitHub...
    git init
    git remote add origin https://github.com/vinicius311006/projeto-pim-sistema-ong-main.git
    git fetch origin main
    git reset --mixed origin/main
    git branch -M main
)

REM Garante autor configurado caso o PC tenha sido formatado recentemente
git config user.name >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    git config user.name "vinicius"
    git config user.email "vinicius311006@gmail.com"
)

echo Arquivos alterados / novos prontos para envio:
echo ---------------------------------------------------------------------
git status -s
echo ---------------------------------------------------------------------

echo.
set /p MSG="Digite a descricao da atualizacao (ou aperte Enter para padrao): "
if "%MSG%"=="" (
    set MSG=feat: dashboard interativo principal, dark mode global, painel SaaS admin e atualizacao mobile
)

echo.
echo [1/3] Adicionando todos os arquivos do projeto...
git add -A

echo [2/3] Criando o commit ("%MSG%")...
git commit -m "%MSG%"

echo [3/3] Enviando para o GitHub (origin main)...
git push -u origin main

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================================================
    echo  SUCESSO TOTAL! O GitHub foi atualizado com todas as mudancas!
    echo  Repositorio: https://github.com/vinicius311006/projeto-pim-sistema-ong-main
    echo =====================================================================
) else (
    echo.
    echo =====================================================================
    echo  [AVISO] Verifique sua autenticacao do GitHub ou conexao com a internet.
    echo =====================================================================
)

pause
