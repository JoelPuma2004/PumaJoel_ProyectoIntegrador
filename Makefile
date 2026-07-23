PORT ?= 5203
HOST ?= 0.0.0.0
PROJECT := FrontendAdministrativo/FrontendAdministrativo.csproj

.PHONY: restore build run

restore:
	dotnet restore $(PROJECT)

build:
	dotnet build $(PROJECT)

run:
	ASPNETCORE_URLS=http://$(HOST):$(PORT) dotnet run --no-launch-profile --project $(PROJECT)
