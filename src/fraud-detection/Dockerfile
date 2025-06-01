# Copyright The OpenTelemetry Authors
# SPDX-License-Identifier: Apache-2.0

FROM --platform=${BUILDPLATFORM} gradle:8-jdk17 AS builder

ARG OTEL_JAVA_AGENT_VERSION

WORKDIR /usr/src/app/

# 修改 COPY 指令，分開複製 build.gradle 和其他文件
COPY ./src/fraud-detection/ ./
COPY ./pb/ ./src/main/proto/
COPY ./src/fraud-detection/build.gradle ./

# 如果 settings.gradle 不存在，我們可以創建一個基本的
RUN echo "rootProject.name = 'fraud-detection'" > settings.gradle

RUN gradle shadowJar --no-daemon

RUN curl -L -o opentelemetry-javaagent.jar https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/download/v${OTEL_JAVA_AGENT_VERSION}/opentelemetry-javaagent.jar

FROM gcr.io/distroless/java17-debian12:nonroot

ENV JAVA_TOOL_OPTIONS=-javaagent:/app/opentelemetry-javaagent.jar

WORKDIR /app/

COPY --from=builder /usr/src/app/build/libs/fraud-detection-1.0-all.jar ./
COPY --from=builder /usr/src/app/opentelemetry-javaagent.jar ./

USER nonroot

ENTRYPOINT [ "java", "-jar", "fraud-detection-1.0-all.jar" ]
