# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# WORKDIR /app
# EXPOSE 80

# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# RUN dotnet tool install --global dotnet-ef --version 7.0.0
# ENV PATH="$PATH:/root/.dotnet/tools"
# WORKDIR /src
# COPY ["PizzaStore.csproj", "."]
# RUN dotnet restore "./PizzaStore.csproj"
# COPY . .
# RUN dotnet ef database update
# RUN dotnet build "PizzaStore.csproj" -c Release -o /app/build


# FROM build AS publish
# RUN dotnet publish "PizzaStore.csproj" -c Release -o /app/publish /p:UseAppHost=false

# FROM base AS final

# WORKDIR /app
# COPY --from=build /src/PizzaStore/bin/Release/net7.0/publish .

# # COPY ["Todos.db", "Todos.db"]

# ENTRYPOINT ["dotnet", "PizzaStore.dll"]




FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN dotnet tool install --global dotnet-ef --version 7.0.10
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src
COPY ["PizzaStore.csproj", "."]
RUN dotnet restore "./PizzaStore.csproj"
COPY . .
RUN dotnet ef database update
WORKDIR "/src/."
RUN dotnet build "PizzaStore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PizzaStore.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# COPY ["Todos.db", "Todos.db"]

ENTRYPOINT ["dotnet", "PizzaStore.dll"]
