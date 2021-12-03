#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Backend.csproj", "."]
RUN dotnet restore "./Backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY driveapi.p12 .

# Environment variables
ENV App__BackEnd=
ENV App__FrontEnd=
ENV Google__ServiceMail=
ENV Google__ApiKey=
ENV Discord__EndPoint=
ENV Discord__ScopeBot=
ENV Discord__IdBot=
ENV Discord__SecretBot=
ENV Discord__TokenBot=
ENV Discord__GuildId=
ENV Discord__RoleId=
ENV Discord__ChannelId=
ENV Databases__StandardString=
ENV Migration__MigrationString=
ENV Migration__ClipsPath=

ENTRYPOINT ["dotnet", "Backend.dll"]