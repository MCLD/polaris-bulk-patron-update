# Get build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy source
COPY . ./

# Run tests if they are present
RUN dotnet restore

# Publish
RUN ./build-binaries.bash

# Copy release-publish.bash script
RUN cp /app/release-publish.bash "/app/publish/"

# Get runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS publish

WORKDIR /app

# Bring in metadata via --build-arg
ARG BRANCH=unknown
ARG IMAGE_CREATED=unknown
ARG IMAGE_REVISION=unknown
ARG IMAGE_VERSION=unknown

# Configure image labels
LABEL branch=$BRANCH \
    maintainer="Maricopa County Library District developers <development@mcldaz.org>" \
    org.opencontainers.image.authors="Maricopa County Library District developers <development@mcldaz.org>" \
    org.opencontainers.image.created=$IMAGE_CREATED \
    org.opencontainers.image.description="Apply updates to patrons from a CSV file in Polaris via the Polaris API" \
    org.opencontainers.image.documentation="https://github.com/MCLD/polaris-bulk-patron-update" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.revision=$IMAGE_REVISION \
    org.opencontainers.image.source="https://github.com/MCLD/polaris-bulk-patron-update" \
    org.opencontainers.image.title="Polaris Bulk Patron Update" \
    org.opencontainers.image.url="https://github.com/MCLD/polaris-bulk-patron-update" \
    org.opencontainers.image.vendor="Maricopa County Library District" \
    org.opencontainers.image.version=$IMAGE_VERSION

# Default image environment variable settings
ENV org.opencontainers.image.created=$IMAGE_CREATED \
    org.opencontainers.image.revision=$IMAGE_REVISION \
    org.opencontainers.image.version=$IMAGE_VERSION

# Copy source
COPY --from=build "/app/publish/" .

# Set entrypoint
ENTRYPOINT ["PolarisBulkPatronUpdate.exe"]
