# Force Windows CMD as the shell
SHELL := cmd.exe
.SHELLFLAGS := /C

# Default engine name, can be overridden by passing EXE=...
EXE ?= Tortoise

ROOT_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
PUBLISH_DIR := $(ROOT_DIR)publish
EXECUTABLE := $(ROOT_DIR)$(EXE)

all: build

build:
	@echo Building...
	dotnet publish Tortoise.csproj -c Release -o $(PUBLISH_DIR) --self-contained false
	@echo Copying published files...
	@if exist "$(PUBLISH_DIR)\$(EXE).exe" ( \
		move /Y "$(PUBLISH_DIR)\$(EXE).exe" "$(EXECUTABLE).exe" \
	)
	@if exist "$(PUBLISH_DIR)\$(EXE)" ( \
		move /Y "$(PUBLISH_DIR)\$(EXE)" "$(EXECUTABLE)" \
	)
	@xcopy /E /Y /I "$(PUBLISH_DIR)\*" "$(ROOT_DIR)\"

run: build
	@echo Running...
	@if exist "$(EXECUTABLE).exe" ( \
		"$(EXECUTABLE).exe" \
	) else if exist "$(EXECUTABLE)" ( \
		"$(EXECUTABLE)" \
	)

clean:
	@echo Cleaning...
	dotnet clean
	@if exist "$(PUBLISH_DIR)" rd /s /q "$(PUBLISH_DIR)"
	@del /q "$(EXECUTABLE).exe" 2>nul
	@del /q "$(EXECUTABLE)" 2>nul

openbench: build
	@echo Writing OpenBench config...
	@echo {\"name\":\"$(EXE)\",\"command\":\"$(EXECUTABLE)\"} > engine.json

.PHONY: all build run clean openbench
