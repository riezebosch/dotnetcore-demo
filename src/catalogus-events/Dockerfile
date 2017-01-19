FROM microsoft/dotnet:1.1.0-runtime
ENTRYPOINT ["dotnet", "catalogus-events.dll"]
ARG source=.
WORKDIR /app
COPY $source .
