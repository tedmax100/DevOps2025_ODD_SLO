# Copyright The OpenTelemetry Authors
# SPDX-License-Identifier: Apache-2.0

FROM --platform=${BUILDPLATFORM} gradle:8-jdk17 AS builder

WORKDIR /usr/src/app/

COPY ./src/fraud-detection/ ./
COPY ./pb/ ./src/main/proto/

RUN gradle shadowJar

# 在 builder 階段下載 agent
RUN curl -L -o opentelemetry-javaagent.jar https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/download/v${OTEL_JAVA_AGENT_VERSION}/opentelemetry-javaagent.jar

# -----------------------------------------------------------------------------

FROM gcr.io/distroless/java17-debian12:nonroot

ENV JAVA_TOOL_OPTIONS=-javaagent:/app/opentelemetry-javaagent.jar

WORKDIR /app/

# 從 builder 階段複製文件
COPY --from=builder /usr/src/app/build/libs/fraud-detection-1.0-all.jar ./
COPY --from=builder /usr/src/app/opentelemetry-javaagent.jar ./

USER nonroot

ENTRYPOINT [ "java", "-jar", "fraud-detection-1.0-all.jar" ]
