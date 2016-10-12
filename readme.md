# .NET Core demo
**ASP.NET MVC Core** using  
**ASP.NET WebApi Core**  using  
**EntityFramework Core** using  
**MySql** 

all in **docker** containers

.\src\mvc-demo:  
`.\dockerTask.ps1 -Build -Environment Release; .\dockerTask.ps1 -Compose -Environment Release`

.\src\webapi-demo:  
`.\dockerTask.ps1 -Build -Environment Release; .\dockerTask.ps1 -Compose -Environment Release`

```
docker run --name efcoredemo-mysql -e 'MYSQL_ROOT_PASSWORD=Pa$$w0rd' -d mysql:latest
docker network connect dotnetcoredemo_default efcoredemo-msql
```



