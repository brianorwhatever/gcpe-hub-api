FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

#TODO add tests

COPY */Gcpe.Hub.API.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/Gcpe.Hub.API/out/ .
ENTRYPOINT ["dotnet", "Gcpe.Hub.API.dll"]
