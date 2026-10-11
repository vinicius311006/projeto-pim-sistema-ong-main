@echo off
chcp 65001 > nul
echo =====================================================================
echo  IMPORTADOR AUTOMATICO DO BANCO DE DADOS (MySQL)
echo =====================================================================
echo Este script importa as tabelas, views e dados para o MySQL local sem
echo precisar abrir o MySQL Workbench!
echo.
set /p MYSQL_USER=Digite o usuario do MySQL (pressione Enter para 'root'): 
if "%MYSQL_USER%"=="" set MYSQL_USER=root
set /p MYSQL_PASS=Digite a senha do MySQL (pressione Enter para 'root'): 
if "%MYSQL_PASS%"=="" set MYSQL_PASS=root

mysql -u %MYSQL_USER% -p%MYSQL_PASS% < ScriptVestindoSonhos_MySQL.sql
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
