# LCS Playoff Contention

Displays and calculates the standings and playoff positions for the LCS regular season.

To start simply run:

```bash
dotnet run
```

or with a docker container

```bash
docker-compose up -d
```

## Build the application

To build run the following commands:

```bash
.\.paket\paket restore
dotnet build
```

### Docker

Building the docker image:

```bash
dotnet publish -c Release
docker build -t <image-tag> .
```

or

```bash
.\build.cmd
```

Pushing the docker image:

```bash
docker login docker.io --username <DockerUser> --password <DockerPassword>
docker push <DockerUser>/<DockerImageName>
```

or

```bash
.\release.cmd
```
