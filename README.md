# gelf4net

## Overview

[GELF][1]

[1]: https://github.com/Graylog2/graylog2-docs/wiki/GELF

## Usage

**Properties**

- <strike>~~Dictionary<string, string> AdditionalFields~~</strike>
- string AdditionalFields //Key:Value CSV ex: app:MyApp,version:1.0
- string Facility
- string GrayLogServerHost
- string GrayLogServerHostIpAddress
- int GrayLogServerPort
- string Host
- bool IncludeLocationInformation
- bool SendAsGelfOrAmqp
- int MaxChunkSize

- int GrayLogServerAmqpPort
- string GrayLogServerAmqpUser
- string GrayLogServerAmqpPassword
- string GrayLogServerAmqpVirtualHost
- string GrayLogServerAmqpQueue

Accept loggingEvent.Properties, to send the variables to graylog2 as additional fields

**log4net Xml Configuration**
	<?xml version="1.0"?>
	<configuration>
		<configSections>
			<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net"/>
		</configSections>

		<log4net>
			<root>
				<level value="DEBUG"/>
					<appender-ref ref="GelfFileAppender"/>
			</root>

			<appender name="GelfFileAppender" type="Esilog.Gelf4net.Appender.Gelf4NetAppender, Esilog.Gelf4net">
				<param name="GrayLogServerHost" value="public-graylog2.taulia.com" />
				<param name="Facility" value="RandomPhrases" />
				<param name="AdditionalFields" value="app:RandomSentece,version:1.0" />

				<layout type="log4net.Layout.PatternLayout">
					<param name="ConversionPattern" value="%-5p%d{yyyy-MM-dd hh:mm:ss}%m%n"/>
				</layout>
			</appender>

		</log4net>

		<startup>
			<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
		</startup>
	</configuration>

## Copyright and License

gelf4net created by Juan J. Chiw - Copyright 2011

based on:
gelf4j created by Philip Stehlik - Copyright 2011

See LICENSE for license details