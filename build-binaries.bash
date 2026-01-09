#!/usr/bin/env bash

set -Eeuo pipefail
trap cleanup SIGINT SIGTERM ERR EXIT

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" &>/dev/null && pwd -P)

usage() {
  cat <<EOF
Usage: $(basename "${BASH_SOURCE[0]}") [-h]

Script description here.

Available options:

-h, --help      Print this help and exit

EOF
    exit
}

cleanup() {
    trap - SIGINT SIGTERM ERR EXIT
}

setup_colors() {
    if [[ -t 2 ]] && [[ -z "${NO_COLOR-}" ]] && [[ "${TERM-}" != "dumb" ]]; then
        NOFORMAT='\033[0m' RED='\033[0;31m' GREEN='\033[0;32m' ORANGE='\033[0;33m' BLUE='\033[0;34m' PURPLE='\033[0;35m' CYAN='\033[0;36m' YELLOW='\033[1;33m'
    else
        NOFORMAT='' RED='' GREEN='' ORANGE='' BLUE='' PURPLE='' CYAN='' YELLOW=''
    fi
}

msg() {
    echo >&2 -e "${1-}"
}

die() {
    local msg=$1
    local code=${2-1} # default exit status 1
    msg "$msg"
    exit "$code"
}

parse_params() {
    while :; do
        case "${1-}" in
            -h | --help) usage ;;
            --no-color) NO_COLOR=1 ;;
            -?*) die "Unknown option: $1" ;;
            *) break ;;
        esac
        shift
    done
    
    args=("$@")
    
    return 0
}

parse_params "$@"
setup_colors

# script logic here

readonly BUILD_STARTAT=$SECONDS

msg "${BLUE}===${NOFORMAT} Building binaries..."

for RID in linux-arm linux-arm64 linux-x64 osx-arm64 osx-x64 win-arm64 win-x64 win-x86
do
    msg "${CYAN}===${NOFORMAT} Building RID: ${RID}"
    RID_STARTAT=$SECONDS
    dotnet publish \
        /property:WarningLevel=0 \
        -c Release \
        -p AssemblyName=PolarisBulkPatronUpdate-${RID} \
        -p:PublishSingleFile=true \
        --property:PublishDir="/app/publish" \
        -r ${RID} \
        --self-contained true \
        -v m \
    && msg "${GREEN}===${NOFORMAT} Build for ${RID} complete in $((SECONDS - RID_STARTAT)) seconds."
done

msg "${PURPLE}===${NOFORMAT} Binary builds complete in $((SECONDS - BUILD_STARTAT)) seconds."
