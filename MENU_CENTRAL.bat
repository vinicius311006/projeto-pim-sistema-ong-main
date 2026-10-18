@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul

:MENU
cls
echo =====================================================================
echo          ONG VESTINDO SONHOS - CENTRAL DE AUTOMAÇÕES (.BAT)
echo =====================================================================
echo.
echo  [1] Iniciar Sistema Completo (Servidor Web + APIs + Abrir Navegador)
echo  [2] Importar / Restaurar Banco de Dados no MySQL
echo  [3] Abrir Documentação ABNT do PIM IV (PDF Oficial)
echo  [4] Abrir Documentação ABNT do PIM IV (Word Editável .docx)
echo  [5] Gerar Executável Portátil (.exe para rodar em qualquer Windows)
echo  [6] Gerar Aplicativo Android (.apk para celular)
echo  [7] Salvar e Enviar alterações para o GitHub
echo  [8] Abrir pasta do sistema portátil (VestindoSonhos_Portavel)
echo.
echo  [0] Sair
echo =====================================================================
set /p OPCAO="Escolha uma opção [0-8]: "

if "%OPCAO%"=="1" goto OPCAO_1
if "%OPCAO%"=="2" goto OPCAO_2
if "%OPCAO%"=="3" goto OPCAO_3
if "%OPCAO%"=="4" goto OPCAO_4
if "%OPCAO%"=="5" goto OPCAO_5
if "%OPCAO%"=="6" goto OPCAO_6
if "%OPCAO%"=="7" goto OPCAO_7
if "%OPCAO%"=="8" goto OPCAO_8
if "%OPCAO%"=="0" exit /b 0

echo Opção inválida! Pressione qualquer tecla para tentar novamente...
pause > nul
goto MENU

:OPCAO_1
cls
call "%~dp01-INICIAR_SISTEMA.bat"
goto MENU

:OPCAO_2
cls
call "%~dp0importar-banco-local.bat"
goto MENU

:OPCAO_3
cls
echo Abrindo PDF da Documentação ABNT...
start "" "%~dp0Documentacao_PIM_IV_ABNT_VestindoSonhos.pdf"
timeout /t 2 /nobreak > nul
goto MENU

:OPCAO_4
cls
echo Abrindo Word (.docx) da Documentação ABNT...
start "" "%~dp0Documentacao_PIM_IV_ABNT_VestindoSonhos.docx"
timeout /t 2 /nobreak > nul
goto MENU

:OPCAO_5
cls
call "%~dp0gerar-executavel-portatil.bat"
goto MENU

:OPCAO_6
cls
call "%~dp0gerar-apk.bat"
goto MENU

:OPCAO_7
cls
call "%~dp0subir-para-github.bat"
goto MENU

:OPCAO_8
cls
explorer "%~dp0VestindoSonhos_Portavel"
goto MENU
