FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY /deploy /app
COPY /public /app/public
COPY /templates /app/templates
WORKDIR /app
EXPOSE 8080
ENTRYPOINT [ "dotnet", "PlayoffContentionWeb.dll" ]
