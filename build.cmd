@echo off
cls

dotnet clean -c Release -o ".\deploy"
Remove-Item .\deploy -Recurse
.\.paket\paket restore
dotnet publish -c Release -o ".\deploy"

docker build -t evanturner/playoff-contention-test .
