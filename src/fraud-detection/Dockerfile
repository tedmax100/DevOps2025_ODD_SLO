# Copyright The OpenTelemetry Authors
# SPDX-License-Identifier: Apache-2.0

FROM --platform=${BUILDPLATFORM} gradle:8-jdk17 AS builder

# 在 FROM 後面立即定義 ARG，這樣可以在這個構建階段使用
ARG OTEL_JAVA_AGENT_VERSION

WORKDIR /usr/src/app/

COPY ./src/fraud-detection/ ./
COPY ./pb/ ./src/main/proto/
COPY ./src/fraud-detection/build.gradle ./src/fraud-detection/settings.gradle ./

RUN gradle shadowJar --no-daemon

# 使用傳入的 OTEL_JAVA_AGENT_VERSION
RUN curl -L -o opentelemetry-javaagent.jar https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/download/v${OTEL_JAVA_AGENT_VERSION}/opentelemetry-javaagent.jar

FROM gcr.io/distroless/java17-debian12:nonroot

ENV JAVA_TOOL_OPTIONS=-javaagent:/app/opentelemetry-javaagent.jar

WORKDIR /app/

COPY --from=builder /usr/src/app/build/libs/fraud-detection-1.0-all.jar ./
COPY --from=builder /usr/src/app/opentelemetry-javaagent.jar ./

USER nonroot

ENTRYPOINT [ "java", "-jar", "fraud-detection-1.0-all.jar" ]
