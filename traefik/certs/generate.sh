#!/bin/sh
set -e

CERT_DIR="/etc/traefik/certs"
CERT_FILE="$CERT_DIR/local.pem"
KEY_FILE="$CERT_DIR/local-key.pem"

if [ -f "$CERT_FILE" ] && [ -f "$KEY_FILE" ]; then
    echo "Certificates already exist, skipping generation."
    exit 0
fi

echo "Downloading mkcert..."
wget -q -O /usr/local/bin/mkcert \
    "https://github.com/FiloSottile/mkcert/releases/download/v1.4.4/mkcert-v1.4.4-linux-amd64"
chmod +x /usr/local/bin/mkcert

echo "Installing local CA..."
mkcert -install

echo "Generating certificates..."
mkdir -p "$CERT_DIR"
mkcert -cert-file "$CERT_FILE" -key-file "$KEY_FILE" \
    trampline.localhost api.trampline.localhost traefik.trampline.localhost

echo "Certificates generated successfully."
