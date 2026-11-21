# Learn Docker – Full DevOps Course for Deploying Containerized Apps https://www.youtube.com/watch?v=rjjES5IsPdg
# -- Stage 1: Build ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY ["ST10439147_PRO7311_POE/ST10439147_PRO7311_POE.csproj", "ST10439147_PRO7311_POE/"]

RUN dotnet restore "ST10439147_PRO7311_POE/ST10439147_PRO7311_POE.csproj"

COPY . .
WORKDIR "/src/ST10439147_PRO7311_POE"
RUN dotnet publish "ST10439147_PRO7311_POE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -- Stage 2: Runtime ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ST10439147_PRO7311_POE.dll"]
