@echo off
chcp 65001 > nul
echo =====================================================================
echo  INICIANDO SISTEMA VIA DOCKER (BANCO + WEB + PHPMYADMIN)
echo =====================================================================
docker compose up -d --build
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Certifique-se de que o Docker Desktop esta instalado e aberto!
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo =====================================================================
echo  CONTAINERS INICIADOS COM SUCESSO!
echo =====================================================================
echo - Sistema Web:      http://localhost:5000/Login
echo - Banco phpMyAdmin: http://localhost:8080 (Login: root / root)
echo =====================================================================
pause
