FROM mcr.microsoft.com/dotnet/core/sdk:2.2
COPY /deploy /app
COPY /public /app/public
COPY /templates /app/templates
WORKDIR /app
EXPOSE 8080
ENTRYPOINT [ "dotnet", "PlayoffContentionWeb.dll" ]
