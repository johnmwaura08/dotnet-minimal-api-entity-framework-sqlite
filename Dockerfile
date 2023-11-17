# Use the official .NET SDK image as the build image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the project files to the container
COPY . .

# Build the application
RUN dotnet publish "PizzaStore.csproj" -c Release -o out

# Use the official .NET runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published output from the build stage to the runtime stage
COPY --from=build /app/out .

# Expose the port that the application listens on
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "PizzaStore.dll"]
