FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AGDPMS.Web/AGDPMS.Web.csproj AGDPMS.Web/
COPY AGDPMS.Shared/AGDPMS.Shared.csproj AGDPMS.Shared/
RUN dotnet restore AGDPMS.Web/AGDPMS.Web.csproj
COPY . .
RUN dotnet publish AGDPMS.Web/AGDPMS.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 7034
ENTRYPOINT ["dotnet", "AGDPMS.Web.dll"]
