az acr login --name acrh7y345we4dao4
docker build -t acrh7y345we4dao4.azurecr.io/otel-collector:latest .
docker push acrh7y345we4dao4.azurecr.io/otel-collector:latest
