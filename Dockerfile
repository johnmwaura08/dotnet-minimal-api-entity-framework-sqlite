

# Use the official .NET SDK image as the build image

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

ENV TZ=America/Chicago


# Set the working directory in the container
WORKDIR /app
EXPOSE 80

# Set the entry point for the container

ENTRYPOINT ["dotnet", "PizzaStore.dll"]
