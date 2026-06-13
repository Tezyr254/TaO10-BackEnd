# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TaO10-BackEnd.csproj", "./"]
RUN dotnet restore "TaO10-BackEnd.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet publish "TaO10-BackEnd.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variable for ASP.NET Core port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaO10-BackEnd.dll"]
