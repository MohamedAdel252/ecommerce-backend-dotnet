FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ECommerceAPI.csproj", "./"]
RUN dotnet restore "ECommerceAPI.csproj"

COPY . ./
RUN dotnet restore
RUN dotnet publish "ECommerceAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerceAPI.dll"]
