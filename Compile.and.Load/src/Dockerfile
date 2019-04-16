FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app

# build the web and worker apps
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj", "Sigged.Repl.NetCore.Web/"]
COPY ["Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj", "Sigged.CodeHost.Worker/"]
RUN dotnet restore "Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj"
RUN dotnet restore "Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj"
COPY . .
RUN dotnet build "Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj" -c Release -o /app
RUN dotnet build "Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj" -c Release -o /workerapp

# publish the web and worker apps
FROM build AS publish
RUN dotnet publish "Sigged.Repl.NetCore.Web/Sigged.Repl.NetCore.Web.csproj" -c Release -o /app
RUN dotnet publish "Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj" -c Release -o /workerapp

# copy publishes to final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app .								
COPY --from=publish /workerapp ./_workerProcess/worker

# important for compatibility. CMD ensures env vars
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Sigged.Repl.NetCore.Web.dll
