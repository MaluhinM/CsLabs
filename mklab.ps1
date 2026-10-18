param(
    [Parameter(Mandatory)] [string]$LabName,
    [Parameter(Mandatory)] [string]$ProjName
)

$ErrorActionPreference = 'Stop'

# Обёртка, которая запускает dotnet и падает, если exit code != 0.
# Так мы эмулируем поведение "|| goto :error" из BAT.
# Работает одинаково в Windows PowerShell 5.1 и PowerShell 7.x.
function Invoke-Dotnet {
    param([Parameter(ValueFromRemainingArguments)] [string[]]$Args)
    & dotnet @Args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Args -join ' ') завершился с кодом $LASTEXITCODE"
    }
}

$TestProj = "${ProjName}_Tests"

Write-Host "Создаём лабораторную '$LabName' с проектом '$ProjName' через PowerShell..."

if (Test-Path $LabName) {
    Write-Host "Ошибка: каталог '$LabName' уже существует" -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Path $LabName | Out-Null
Push-Location $LabName

try {
    Write-Host "[1/6] Создаю файл решения..."
    Invoke-Dotnet new sln -n $ProjName

    Write-Host "[2/6] Создаю консольное приложение..."
    Invoke-Dotnet new console -n $ProjName

    Write-Host "[3/6] Добавляю проект в решение..."
    Invoke-Dotnet sln add "$ProjName/$ProjName.csproj"

    Write-Host "[4/6] Создаю тестовый проект NUnit..."
    Invoke-Dotnet new nunit -n $TestProj

    Write-Host "[5/6] Добавляю тестовый проект в решение..."
    Invoke-Dotnet sln add "$TestProj/$TestProj.csproj"

    Write-Host "[6/6] Добавляю ссылку на тестовый проект..."
    Invoke-Dotnet add "$TestProj/$TestProj.csproj" reference "$ProjName/$ProjName.csproj"

    Write-Host ""
    Write-Host "Лабораторная '$LabName' успешно создана." -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "Ошибка при создании лабораторной: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

exit 0