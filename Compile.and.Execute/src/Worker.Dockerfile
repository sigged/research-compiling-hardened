# DOCKERFILE for the worker app (Sigged.CodeHost.Worker)

FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /workerapp

# ensure necessary groups exist
# ensure "cscwebuser" user for the web app, member of both cscwebuser and docker groups
RUN groupadd -f --gid 7002 cscworkeruser && \
    useradd --system --shell /bin/dash --gid cscworkeruser --uid 8002 cscworkeruser

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj", "Sigged.CodeHost.Worker/"]
COPY . .

FROM build AS publish
RUN dotnet publish "Sigged.CodeHost.Worker/Sigged.CodeHost.Worker.csproj" -c Release -o /workerapp

FROM base AS final
WORKDIR /workerapp
COPY --from=publish /workerapp .

# enforce rights & limitations on user
RUN chown -R cscworkeruser:cscworkeruser /workerapp
USER cscworkeruser

ENTRYPOINT ["dotnet", "./Sigged.CodeHost.Worker.dll"]