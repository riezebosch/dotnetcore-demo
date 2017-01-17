FROM microsoft/dotnet:1.1.0-runtime
ENTRYPOINT ["dotnet", "magazijn-service.dll"]
ARG source=.
WORKDIR /app
COPY $source .
