FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app

# build the web application
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj", "Sigged.Repl.NetCore.Web/"]
RUN dotnet restore "Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj"
COPY . .
WORKDIR "/src/Sigged.Repl.NetCore.Web"
RUN dotnet build "Sigged.Repl.NetCore.Web.csproj" -c Release -o /app

# publish the web application
FROM build AS publish
RUN dotnet publish "Sigged.Repl.NetCore.Web.csproj" -c Release -o /app

# build the worker app
FROM microsoft/dotnet:2.2-sdk as workerbuild
WORKDIR /workersrc
COPY ["Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj", "Sigged.CodeHost.Worker/"]
RUN dotnet restore "Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj"
COPY . .
WORKDIR "/workersrc/Sigged.CodeHost.Worker"
RUN dotnet build "Sigged.CodeHost.Worker.csproj" -c Release -o /workerapp

# publish the worker app
FROM workerbuild AS workerpublish
RUN dotnet publish "Sigged.CodeHost.Worker.csproj" -c Release -o /workerapp

# copy publishes to final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app .								
COPY --from=workerpublish /workerapp ./_workerProcess/worker
#ENTRYPOINT ["dotnet", "Sigged.Repl.NetCore.Web.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Sigged.Repl.NetCore.Web.dll
