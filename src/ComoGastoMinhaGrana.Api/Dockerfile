FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/ComoGastoMinhaGrana.Api/ComoGastoMinhaGrana.Api.csproj", "src/ComoGastoMinhaGrana.Api/"]
COPY ["src/ComoGastoMinhaGrana.Infrastructure/ComoGastoMinhaGrana.Infrastructure.csproj", "src/ComoGastoMinhaGrana.Infrastructure/"]
COPY ["src/ComoGastoMinhaGrana.Application/ComoGastoMinhaGrana.Application.csproj", "src/ComoGastoMinhaGrana.Application/"]
COPY ["src/ComoGastoMinhaGrana.Domain/ComoGastoMinhaGrana.Domain.csproj", "src/ComoGastoMinhaGrana.Domain/"]

RUN dotnet restore "src/ComoGastoMinhaGrana.Api/ComoGastoMinhaGrana.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/ComoGastoMinhaGrana.Api"
RUN dotnet build "ComoGastoMinhaGrana.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComoGastoMinhaGrana.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComoGastoMinhaGrana.Api.dll"]
