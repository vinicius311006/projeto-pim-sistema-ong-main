@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul
echo =====================================================================
echo  IMPORTADOR AUTOMATICO DO BANCO DE DADOS (MySQL)
echo =====================================================================

set "SQLFILE="
if exist "%~dp0ScriptVestindoSonhos_MySQL.sql" (
    set "SQLFILE=%~dp0ScriptVestindoSonhos_MySQL.sql"
) else (
    for /r "%~dp0" %%f in (ScriptVestindoSonhos_MySQL.sql) do (
        if exist "%%f" set "SQLFILE=%%f"
    )
)

if "%SQLFILE%"=="" (
    echo [ERRO] Nao foi possivel localizar o arquivo ScriptVestindoSonhos_MySQL.sql!
    pause
    exit /b 1
)

echo.
echo [OK] Script localizado:
echo %SQLFILE%
echo.
set /p MYSQL_USER=Digite o usuario do MySQL (pressione Enter para 'root'): 
if "%MYSQL_USER%"=="" set MYSQL_USER=root
set /p MYSQL_PASS=Digite a senha do MySQL (pressione Enter para 'root'): 
if "%MYSQL_PASS%"=="" set MYSQL_PASS=root

mysql -u %MYSQL_USER% -p%MYSQL_PASS% < "%SQLFILE%"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [AVISO] Se o comando 'mysql' nao foi encontrado no PATH do Windows,
    echo abra o MySQL Workbench e execute o arquivo 'ScriptVestindoSonhos_MySQL.sql'.
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo =====================================================================
echo  BANCO DE DADOS, VIEWS E USUARIO ADMIN IMPORTADOS COM SUCESSO!
echo =====================================================================
pause
