# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Api/ECommerce/ ./Api/ECommerce/

RUN dotnet restore ./Api/ECommerce/ECommerce.Api.Admin/ECommerce.Api.Admin.csproj
RUN dotnet publish ./Api/ECommerce/ECommerce.Api.Admin/ECommerce.Api.Admin.csproj \
    -c Release -o /app/publish --no-restore

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    host=http://0.0.0.0:6001 \
    Logs__Action_Log=/app/Logs \
    Logs__Error_Log=/app/Logs/Error \
    Uploads__Path=/app/uploads

EXPOSE 6001

ENTRYPOINT ["dotnet", "ECommerce.Api.Admin.dll"]
