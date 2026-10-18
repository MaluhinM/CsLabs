@echo off
setlocal
chcp 65001 > nul

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage

set "LabName=%~1"
set "ProjName=%~2"
set "TestProj=%ProjName%_Tests"

echo Создаём лабораторную '%LabName%' с проектом '%ProjName%' через BAT...

if exist "%LabName%" (
    echo Ошибка: каталог "%LabName%" уже существует
    echo Удалите его или выберите другое имя.
    exit /b 1
)

mkdir "%LabName%" 2>nul
if not exist "%LabName%" (
    echo Ошибка: не удалось создать каталог "%LabName%"
    exit /b 1
)

pushd "%LabName%" || exit /b 1

echo [1/6] Создаю Файл решения...
dotnet new sln -n "%ProjName%" || goto :error
echo [2/6] Создаю Консольное приложение...
dotnet new console -n "%ProjName%" || goto :error
echo [3/6] Добавляю проект в решение...
dotnet sln add "%ProjName%\%ProjName%.csproj" || goto :error

echo [4/6] Создаю Тестовый проект NUnit...
dotnet new nunit -n "%TestProj%" || goto :error
echo [5/6] Добавляю тестовый проект в решение...
dotnet sln add "%TestProj%\%TestProj%.csproj" || goto :error

echo [6/6] Добавляю ссылку на тестовый проект...
dotnet add "%TestProj%\%TestProj%.csproj" reference "%ProjName%\%ProjName%.csproj" || goto :error

popd
echo.
echo Лабораторная "%LabName%" успешно создана.
exit /b 0

:error
echo.
echo Ошибка при создании лабораторной. Проверьте вывод выше.
popd
exit /b 1

:usage
echo Использование: %~nx0 имя_лабы имя_проекта
echo Пример:        %~nx0 sem-1-lab-06 path-finder
exit /b 1