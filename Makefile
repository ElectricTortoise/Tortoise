# Source: https://github.com/lynx-chess/Lynx/blob/main/Makefile

.DEFAULT_GOAL := publish

EXE=

RUNTIME=
OUTPUT_DIR=artifacts/Tortoise/

ifeq ($(OS),Windows_NT)
	ifeq ($(PROCESSOR_ARCHITEW6432),AMD64)
		RUNTIME=win-x64
	else
		RUNTIME=win-x86
	endif
else
	UNAME_S := $(shell uname -s)
	UNAME_P := $(shell uname -p)
	ifeq ($(UNAME_S),Linux)
		RUNTIME=linux-x64
		ifneq ($(filter aarch64%,$(UNAME_P)),)
			RUNTIME=linux-arm64
		else ifneq ($(filter armv8%,$(UNAME_P)),)
			RUNTIME=linux-arm64
		else ifneq ($(filter arm%,$(UNAME_P)),)
			RUNTIME=linux-arm
		endif
	else ifneq ($(filter arm%,$(UNAME_P)),)
		RUNTIME=osx-arm64
	else
		RUNTIME=osx-x64
	endif
endif

ifndef RUNTIME
	$(error RUNTIME is not set for $(OS) $(UNAME_S) $(UNAME_P))
endif

ifdef EXE
	OUTPUT_DIR=./
endif

publish:
	dotnet publish Tortoise.csproj --self-contained --runtime ${RUNTIME} /p:PublishSingleFile=true /p:ExecutableName=$(EXE) -o ${OUTPUT_DIR}
