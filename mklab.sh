#!/usr/bin/env bash
set -euo pipefail

# Цветной вывод — только если stdout это терминал (иначе логи будут в кракозябрах)
if [ -t 1 ]; then
    RED=$'\033[31m'
    GREEN=$'\033[32m'
    RESET=$'\033[0m'
else
    RED=""
    GREEN=""
    RESET=""
fi

usage() {
    echo "Использование: $(basename "$0") имя_лабы имя_проекта"
    echo "Пример:        $(basename "$0") sem-1-lab-06 path-finder"
    exit 1
}

# ─── Проверка аргументов ───
[ $# -ge 2 ] || usage

LAB_NAME="$1"
PROJ_NAME="$2"
TEST_PROJ="${PROJ_NAME}_Tests"

# ─── Проверка dotnet ───
if ! command -v dotnet >/dev/null 2>&1; then
    echo "${RED}Ошибка: команда 'dotnet' не найдена в PATH${RESET}" >&2
    echo "Установите .NET SDK: https://dotnet.microsoft.com/download" >&2
    exit 1
fi

echo "Создаём лабораторную '$LAB_NAME' с проектом '$PROJ_NAME' через Bash..."

# ─── Проверка существования каталога ───
if [ -e "$LAB_NAME" ]; then
    echo "${RED}Ошибка: каталог '$LAB_NAME' уже существует${RESET}" >&2
    echo "Удалите его или выберите другое имя." >&2
    exit 1
fi

mkdir -p "$LAB_NAME"
cd "$LAB_NAME"

# ─── Ловушка на любую ошибку: покажем красное сообщение ───
trap 'echo; echo "${RED}Ошибка при создании лабораторной.${RESET}" >&2' ERR

echo "[1/6] Создаю файл решения..."
dotnet new sln -n "$PROJ_NAME"

echo "[2/6] Создаю консольное приложение..."
dotnet new console -n "$PROJ_NAME"

echo "[3/6] Добавляю проект в решение..."
dotnet sln add "$PROJ_NAME/$PROJ_NAME.csproj"

echo "[4/6] Создаю тестовый проект NUnit..."
dotnet new nunit -n "$TEST_PROJ"

echo "[5/6] Добавляю тестовый проект в решение..."
dotnet sln add "$TEST_PROJ/$TEST_PROJ.csproj"

echo "[6/6] Добавляю ссылку на тестовый проект..."
dotnet add "$TEST_PROJ/$TEST_PROJ.csproj" reference "$PROJ_NAME/$PROJ_NAME.csproj"

echo
echo "${GREEN}Лабораторная '$LAB_NAME' успешно создана.${RESET}"