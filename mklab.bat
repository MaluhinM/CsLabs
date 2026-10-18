@echo off
chcp 65001 > nul

if "%1"=="" (
    echo Ошибка: Вы не передали первый параметр: имя_лабы!
    echo Использование: mklab.bat [ИМЯ_ЛАБЫ] имя_проекта
    exit /b 1
)

if "%2"=="" (
    echo Ошибка: Вы не передали второй параметр: имя_проекта!
    echo Использование: mklab.bat имя_лабы [ИМЯ_ПРОЕКТА]
    exit /b 1
)

set "LabName=%~1"
set "ProjName=%~2"
set "TestProj=%ProjName%_Tests"

mkdir %LabName% && cd %LabName%
dotnet new sln -n %ProjName%
dotnet new console -n %ProjName% && dotnet sln add %ProjName%
dotnet new nunit -n %TestProj% && dotnet sln add %TestProj%
cd %TestProj% && dotnet add reference ..\%ProjName%\%ProjName%.csproj