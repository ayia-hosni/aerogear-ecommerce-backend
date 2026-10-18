FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SportEcommerce.sln ./
COPY src/SportEcommerce.Domain/*.csproj src/SportEcommerce.Domain/
COPY src/SportEcommerce.Application/*.csproj src/SportEcommerce.Application/
COPY src/SportEcommerce.Infrastructure/*.csproj src/SportEcommerce.Infrastructure/
COPY src/SportEcommerce.Api/*.csproj src/SportEcommerce.Api/
COPY tests/SportEcommerce.Tests/*.csproj tests/SportEcommerce.Tests/
RUN dotnet restore
COPY . .
RUN dotnet publish src/SportEcommerce.Api -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
RUN mkdir /data && chown $APP_UID /data
USER $APP_UID
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SportEcommerce.Api.dll"]
