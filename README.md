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
