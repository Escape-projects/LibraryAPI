# ============================================
# STAGE 1: BUILD — собираем проект
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Копируем .csproj-файлы (только их — чтобы закэшировать restore)
COPY ["LibraryAPI/LibraryAPI.csproj", "LibraryAPI/"]
COPY ["LibraryAPI.Contracts/LibraryAPI.Contracts.csproj", "LibraryAPI.Contracts/"]

# 2. Восстанавливаем NuGet-пакеты
RUN dotnet restore "LibraryAPI/LibraryAPI.csproj"

# 3. Копируем весь остальной код
COPY . .

# 4. Собираем проект в Release
RUN dotnet publish "LibraryAPI/LibraryAPI.csproj" -c Release -o /app/publish

# ============================================
# STAGE 2: RUNTIME — запускаем готовый .dll
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 5. Копируем только результат сборки из Stage 1
COPY --from=build /app/publish .

# 6. Сообщаем Docker, что API слушает порт 8080
EXPOSE 8080

# 7. Команда запуска
ENTRYPOINT ["dotnet", "LibraryAPI.dll"]