@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
cd /d "%~dp0"

echo =====================================================================
echo    ONG VESTINDO SONHOS - PUXAR ATUALIZACOES DO GITHUB (1 CLIQUE)
echo =====================================================================
echo.
echo  Repositorio oficial:
echo  https://github.com/vinicius311006/projeto-pim-sistema-ong-main
echo.
echo =====================================================================
echo.

REM Verifica se o Git esta instalado na maquina
where git >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] O Git nao foi encontrado neste computador!
    echo Baixe e instale em: https://git-scm.com/download/win
    echo.
    pause
    exit /b 1
)

REM Caso o projeto tenha sido baixado via .ZIP (sem pasta .git), conecta automaticamente ao GitHub
if not exist ".git" (
    echo [1/3] Pasta baixada via ZIP detectada! Conectando ao repositorio GitHub...
    git init
    git remote add origin https://github.com/vinicius311006/projeto-pim-sistema-ong-main.git
) else (
    git remote set-url origin https://github.com/vinicius311006/projeto-pim-sistema-ong-main.git >nul 2>&1
    if !ERRORLEVEL! NEQ 0 (
        git remote add origin https://github.com/vinicius311006/projeto-pim-sistema-ong-main.git
    )
)

echo [2/3] Buscando as ultimas atualizacoes no GitHub (origin/main)...
git fetch origin main

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Nao foi possivel conectar ao GitHub. Verifique sua internet!
    pause
    exit /b 1
)

echo [3/3] Aplicando atualizacoes nos arquivos locais (Web, Mobile, Testes e ABNT)...
git checkout -B main origin/main --force
git reset --hard origin/main

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================================================
    echo  SUCESSO! Todos os arquivos foram atualizados com a versao mais
    echo  recente do GitHub!
    echo =====================================================================
    echo.
    echo Ultimo commit baixado:
    git log -1 --oneline
    echo.
) else (
    echo.
    echo =====================================================================
    echo  [AVISO] Ocorreu um problema ao atualizar os arquivos locais.
    echo =====================================================================
)

pause
