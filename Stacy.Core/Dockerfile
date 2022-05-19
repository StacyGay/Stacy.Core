#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Stacy.Core/Stacy.Core.csproj", "Stacy.Core/"]
RUN dotnet restore "Stacy.Core/Stacy.Core.csproj"
COPY . .
WORKDIR "/src/Stacy.Core"
RUN dotnet build "Stacy.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stacy.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stacy.Core.dll"]