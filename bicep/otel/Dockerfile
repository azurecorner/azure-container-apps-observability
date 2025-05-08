FROM otel/opentelemetry-collector-contrib:latest

# Copy your custom configuration into the image
COPY otel-config.yaml /etc/otel/otel-config.yaml

# Set the environment variable so the Collector uses your configuration file.
ENV OTEL_CONFIG_FILE=/etc/otel/otel-config.yaml
