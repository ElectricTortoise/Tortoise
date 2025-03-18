EXE = Tortoise
ifeq ($(OS),Windows_NT)
	SRC := Tortoise.exe
	DEST := $(EXE).exe
else
	SRC := Tortoise
	DEST := $(EXE)
endif

all:
	dotnet publish -c Release Tortoise/ --output Tortoise/bin/OpenbenchBin
	mv Tortoise/bin/OpenbenchBin/$(SRC) ./$(DEST)