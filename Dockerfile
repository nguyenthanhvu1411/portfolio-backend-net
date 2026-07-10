# =========================
# Giai đoạn build project
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Sao chép file project trước để restore package
COPY ["Portfolio/Portfolio.csproj", "Portfolio/"]

RUN dotnet restore "Portfolio/Portfolio.csproj"

# Sao chép toàn bộ source code
COPY . .

WORKDIR "/src/Portfolio"

# Publish ứng dụng
RUN dotnet publish "Portfolio.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Giai đoạn chạy ứng dụng
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Portfolio.dll"]
