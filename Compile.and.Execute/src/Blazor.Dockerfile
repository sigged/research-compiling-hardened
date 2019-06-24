# DOCKERFILE for the worker app (Sigged.CodeHost.Worker)

FROM microsoft/dotnet:3.0-runtime AS base
WORKDIR /blazorapp

# ensure necessary groups exist
# ensure "cscblazoruser" user for the web app, member of cscblazoruser group
RUN groupadd -f --gid 7003 cscblazoruser && \
    useradd --system --shell /bin/dash --gid cscblazoruser --uid 8003 cscblazoruser

FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /src
COPY ["Sigged.CsC.Mono.Blazor/Sigged.CsC.Mono.Blazor.csproj", "Sigged.CsC.Mono.Blazor/"]
COPY . .

FROM build AS publish
RUN dotnet publish "Sigged.CsC.Mono.Blazor/Sigged.CsC.Mono.Blazor.csproj" -c Release -o /blazorapp

FROM base AS final
WORKDIR /blazorapp
COPY --from=publish /blazorapp .

# enforce rights & limitations on user
RUN chown -R cscblazoruser:cscblazoruser /blazorapp
USER cscblazoruser

# important for compatibility. CMD ensures env vars
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Sigged.CsC.Mono.Blazor.dll