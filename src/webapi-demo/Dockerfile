FROM microsoft/dotnet:1.0.0-core
WORKDIR /app
ENV ASPNETCORE_URLS http://*:1889
EXPOSE 1889
ENTRYPOINT ["dotnet", "webapi-demo.dll"]
COPY . /app
