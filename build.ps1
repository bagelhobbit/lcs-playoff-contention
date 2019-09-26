Param(
    [Switch] $IsDev
)

Clear-Host

dotnet clean -c Release -o ".\deploy"
Remove-Item .\deploy\ -Recurse
.\.paket\paket restore
dotnet publish -c Release -o ".\deploy"

if ($IsDev) {
    docker build -t evanturner/playoff-contention-dev .
} else {
    docker build -t evanturner/playoff-contention .
}
