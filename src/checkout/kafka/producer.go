// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0
package kafka

import (
	"context"
	"log/slog"

	"github.com/IBM/sarama"
	//"github.com/sirupsen/logrus"
)

var (
	Topic           = "orders"
	ProtocolVersion = sarama.V3_0_0_0
)

// func CreateKafkaProducer(brokers []string, log *logrus.Logger) (sarama.AsyncProducer, error) {
func CreateKafkaProducer(brokers []string, logger *slog.Logger) (sarama.AsyncProducer, error) {
	//sarama.Logger = log
	sarama.Logger = saramaLogAdapter{logger: logger}
	saramaConfig := sarama.NewConfig()
	saramaConfig.Producer.Return.Successes = true
	saramaConfig.Producer.Return.Errors = true

	// Sarama has an issue in a single broker kafka if the kafka broker is restarted.
	// This setting is to prevent that issue from manifesting itself, but may swallow failed messages.
	saramaConfig.Producer.RequiredAcks = sarama.NoResponse

	saramaConfig.Version = ProtocolVersion

	// So we can know the partition and offset of messages.
	saramaConfig.Producer.Return.Successes = true

	producer, err := sarama.NewAsyncProducer(brokers, saramaConfig)
	if err != nil {
		return nil, err
	}

	// We will log to STDOUT if we're not able to produce messages.
	go func() {
		for err := range producer.Errors() {
			slog.ErrorContext(context.Background(), "Failed to write message: %+v", err)
			//log.Errorf("Failed to write message: %+v", err)
		}
	}()
	return producer, nil
}

// 自定義 sarama 日誌適配器
type saramaLogAdapter struct {
	logger *slog.Logger
}

func (a saramaLogAdapter) Print(v ...interface{}) {
	a.logger.Info("sarama", "msg", v)
}

func (a saramaLogAdapter) Printf(format string, v ...interface{}) {
	a.logger.Info("sarama", "msg", format, "args", v)
}

func (a saramaLogAdapter) Println(v ...interface{}) {
	a.logger.Info("sarama", "msg", v)
}
