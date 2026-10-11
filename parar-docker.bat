@echo off
chcp 65001 > nul
echo Finalizando containers do sistema...
docker compose down
echo Containers encerrados com sucesso!
pause
