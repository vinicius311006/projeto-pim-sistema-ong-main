@echo off
chcp 65001 > nul
echo Finalizando containers do sistema...
if exist "%~dp0projeto-pim-sistema-ong-main\docker-compose.yml" (
    cd /d "%~dp0projeto-pim-sistema-ong-main"
) else (
    cd /d "%~dp0"
)
docker compose down
echo Containers encerrados com sucesso!
pause
