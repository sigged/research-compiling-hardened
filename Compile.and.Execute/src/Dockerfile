# DOCKERFILE for the front-end web app (Sigged.CsC.NetCore.Web)

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app

# ensure necessary groups exist
# ensure "cscwebuser" user for the web app, member of both cscwebuser and docker groups
RUN groupadd cscwebuser && \
    groupadd -f --gid 999 docker && \
    useradd --system --create-home --home-dir /home/cscwebuser --shell /bin/bash --gid cscwebuser --groups docker --uid 8001 cscwebuser

# build the web and worker apps
FROM microsoft/dotnet:2.2-sdk AS publish
WORKDIR /src
COPY ["Sigged.CsC.NetCore.Web/Sigged.CsC.NetCore.Web.csproj", "Sigged.CsC.NetCore.Web/"]
COPY . .
RUN dotnet publish "Sigged.CsC.NetCore.Web/Sigged.CsC.NetCore.Web.csproj" -c Release -o /app

# copy publishes to final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# enforce rights & limitations on user
RUN chown -R cscwebuser:cscwebuser /app
USER cscwebuser

# important for compatibility. CMD ensures env vars
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Sigged.CsC.NetCore.Web.dll
