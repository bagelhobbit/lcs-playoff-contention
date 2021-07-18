Param(
    [Switch] $IsBeta
)

Clear-Host

dotnet clean -c Release -o ".\deploy"
Remove-Item .\deploy\ -Recurse
dotnet paket restore
dotnet publish -c Release -o ".\deploy"

if ($IsBeta) {
    docker build -t emmakturner/playoff-contention-beta .
} else {
    docker build -t emmakturner/playoff-contention .
}
