.PHONY: help deps build run
.DEFAULT_GOAL := help
SHELL := /bin/bash

export CERT_GEN=1

$(VERBOSE).SILENT:

ifeq ($(OS),Windows_NT)
	RELEASE_OS := win
    ifeq ($(PROCESSOR_ARCHITEW6432),AMD64)
		RELEASE_ARCH := x64
    else
        ifeq ($(PROCESSOR_ARCHITECTURE),AMD64)
			RELEASE_ARCH := x64
        endif
        ifeq ($(PROCESSOR_ARCHITECTURE),x86)
			RELEASE_ARCH := x86
        endif
    endif
else
    UNAME_S := $(shell uname -s)
    ifeq ($(UNAME_S),Linux)
		RELEASE_OS := linux
    endif
    ifeq ($(UNAME_S),Darwin)
		RELEASE_OS := osx
    endif

    UNAME_M := $(shell uname -m)
    ifeq ($(UNAME_M),x86_64)
		RELEASE_ARCH += x64
    endif
    ifneq ($(filter %86,$(UNAME_M)),)
		RELEASE_ARCH += x86
    endif
endif

RID := $(RELEASE_OS)-$(RELEASE_ARCH)

help:
	@grep -E '^[a-zA-Z0-9_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

require-%:
	if ! command -v ${*} 2>&1 >/dev/null; then \
		echo "! ${*} not installed"; \
		exit 1; \
	fi

deps: require-dotnet ## Restore dependencies
	dotnet restore

build: require-dotnet ## Build
	dotnet publish BeatTogether.MasterServer -c Release -p:PublishReadyToRun=true -r $(RID) -o out $(ARGS)

run: require-dotnet ## Run locally
	mkdir -p run
	cp BeatTogether.MasterServer/appsettings.json run/
	cp -n BeatTogether.MasterServer/cert.pem BeatTogether.MasterServer/key.pem run/ || true

	cd run && ../setup.sh
	cd run && dotnet run -p ../BeatTogether.MasterServer -c Debug $(ARGS)
