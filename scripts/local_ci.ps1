$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock] $Command
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Push-Location (Join-Path $PSScriptRoot "..")
try {
    Invoke-Checked { dotnet restore CivicFlow.sln }
    Invoke-Checked { dotnet build CivicFlow.sln --configuration Release --no-restore }
    Invoke-Checked { dotnet test CivicFlow.sln --configuration Release --no-build }
    Invoke-Checked { python scripts\verify_static.py }
}
finally {
    Pop-Location
}
