FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/ComoGastoMinhaGrana.Worker/ComoGastoMinhaGrana.Worker.csproj", "src/ComoGastoMinhaGrana.Worker/"]
COPY ["src/ComoGastoMinhaGrana.Infrastructure/ComoGastoMinhaGrana.Infrastructure.csproj", "src/ComoGastoMinhaGrana.Infrastructure/"]
COPY ["src/ComoGastoMinhaGrana.Application/ComoGastoMinhaGrana.Application.csproj", "src/ComoGastoMinhaGrana.Application/"]
COPY ["src/ComoGastoMinhaGrana.Domain/ComoGastoMinhaGrana.Domain.csproj", "src/ComoGastoMinhaGrana.Domain/"]

RUN dotnet restore "src/ComoGastoMinhaGrana.Worker/ComoGastoMinhaGrana.Worker.csproj"

COPY . .
WORKDIR "/src/src/ComoGastoMinhaGrana.Worker"
RUN dotnet build "ComoGastoMinhaGrana.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComoGastoMinhaGrana.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComoGastoMinhaGrana.Worker.dll"]
