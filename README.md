## Dependency

- .NET 5 Runtime

## Usage

```yaml
version: "2.4"
services:
  web:
    image: ghcr.io/exit-path/server
    restart: always
    environment:
      AllowedOrigins__0: https://exit-path.github.io
      Multiplayer__Auth__TokenSecret: SECRET # replace this
```
OR edit appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Multiplayer": {
    "Auth": {
      "Authority": "exit-path",
      "TokenSecret": "Secret Token of 32 characters in length"
    }
  },
  "AllowedOrigins": {
    "0": "front end address eg: http://localhost:8080"
  }
}
```

