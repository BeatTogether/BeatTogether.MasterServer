#!/bin/bash

: "${HOST_NAME:=127.0.0.1}"
: "${PORT:=2328}"
: "${REDIS_ADDR:=127.0.0.1:6379}"

# Replace default listening address
sed -i "s/127.0.0.1:2328/${HOST_NAME}:${PORT}/" appsettings.json
# Replace default redis address
sed -i "s/127.0.0.1:6379/${REDIS_ADDR}/" appsettings.json

# Regenerate cert if needed
if ! openssl x509 -noout -subject -in cert.pem | grep -q "[= ]${HOST_NAME//./\\.}$"; then
    echo "! Mismatching CN in cert" >&2 && exit 1
fi
