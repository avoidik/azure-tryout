#!/bin/bash

# List the blobs in an Azure storage container.
#
# usage: azure_curl.sh [blob-name]
#

storage_account="myedustorage"
container_name="images"
access_key="xlWnfjfLjwQklTzzKqK/iZ9Ibdn1xVnZCPlMci+wWE05eGEVXXlXQwqHt/eZKY3hVcDudjSQjj07OjNMo3hW5A=="

blob_store_url="blob.core.windows.net"
authorization="SharedKey"

request_method="GET"
request_date=$(TZ=GMT date "+%a, %d %h %Y %H:%M:%S %Z")
storage_service_version="2011-08-18"

# HTTP Request headers
x_ms_date_h="x-ms-date:$request_date"
x_ms_version_h="x-ms-version:$storage_service_version"

# Build the signature string
canonicalized_headers="${x_ms_date_h}\n${x_ms_version_h}"

if [[ "$1" != "" ]]; then
  canonicalized_resource="/${storage_account}/${container_name}/$1"
else
  canonicalized_resource="/${storage_account}/${container_name}\ncomp:list\nrestype:container"
fi

string_to_sign="${request_method}\n\n\n\n\n\n\n\n\n\n\n\n${canonicalized_headers}\n${canonicalized_resource}"

# Decode the Base64 encoded access key, convert to Hex.
decoded_hex_key="$(echo -n $access_key | base64 -d -w0 | xxd -p -c256)"

# Create the HMAC signature for the Authorization header
signature=$(printf "$string_to_sign" | openssl dgst -sha256 -mac HMAC -macopt "hexkey:$decoded_hex_key" -binary | base64 -w0)

authorization_header="Authorization: $authorization $storage_account:$signature"

if [[ "$1" != "" ]]; then
  URL="https://${storage_account}.${blob_store_url}/${container_name}/$1"
else
  URL="https://${storage_account}.${blob_store_url}/${container_name}?restype=container&comp=list"
fi

curl \
  -H "$x_ms_date_h" \
  -H "$x_ms_version_h" \
  -H "$authorization_header" \
  "$URL"
