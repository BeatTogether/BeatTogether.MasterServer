#!/bin/bash

: "${CERT_GEN:=0}"
: "${HOST_NAME:=127.0.0.1}"
: "${PORT:=2328}"
: "${REDIS_ADDR:=127.0.0.1:6379}"

# Replace default listening address
sed -i "s/127.0.0.1:2328/${HOST_NAME}:${PORT}/" appsettings.json
# Replace default redis address
sed -i "s/127.0.0.1:6379/${REDIS_ADDR}/" appsettings.json

# Regenerate cert if needed
if [ "$CERT_GEN" == "1" ] && ! openssl x509 -noout -subject -in cert.pem | grep -q "[= ]${HOST_NAME//./\\.}$"; then
    echo "Generating certificate for ${HOST_NAME}"
    openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 3650 -nodes -subj "/CN=${HOST_NAME}"
fi
