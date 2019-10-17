FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS builder
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -r linux-musl-x64 -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine
WORKDIR /app
COPY --from=builder /app .
COPY --from=node /app/build ./wwwroot
ENTRYPOINT ["./Barforce_Backend"]