# gelf4net

## Overview

[GELF][1]

[1]: https://github.com/Graylog2/graylog2-docs/wiki/GELF

## Usage

**Properties**

- <strike>~~Dictionary<string, string> AdditionalFields~~</strike>
- string AdditionalFields // Key:Value CSV ex: app:MyApp,version:1.0
- string Facility
- string GrayLogServerHost
- string GrayLogServerHostIpAddress
- int GrayLogServerPort
- string Host
- bool IncludeLocationInformation
- bool SendAsGelfOrAmqp
- int MaxChunkSize
// The amount of bytes a message chunk can contain.
Default: 1024

- bool LogStackTraceFromMessage

Defines if a logged Throwable should be logged with the complete stack trace or just with the info returned by the toString() implementation on the Throwable (which is usually calling getMessage()).
This applies to messages logged by doing things like "log.error(myException)" message logged via "log.error("Oh... something bad happened", myException)" have the stack trace logged and you can't change that.
Default: true

- int GrayLogServerAmqpPort
- string GrayLogServerAmqpUser
- string GrayLogServerAmqpPassword
- string GrayLogServerAmqpVirtualHost
- string GrayLogServerAmqpQueue

Accept LoggingEvent.Properties, to send the variables to graylog2 as additional fields

**log4net Xml Configuration**

	<?xml version="1.0"?>
	<configuration>
		<configSections>
			<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net"/>
		</configSections>

		<log4net>
			<root>
			  <level value="DEBUG"/>
			  <appender-ref ref="GelfUdpAppender"/>
			  <appender-ref ref="GelfAmqpAppender"/>
			</root>

			<appender name="GelfUdpAppender" type="Esilog.Gelf4net.Appender.GelfUdpAppender, Esilog.Gelf4net">
			  <remoteAddress value="127.0.0.1" />
			  <remotePort value="12201" />
			  <layout type="Esilog.Gelf4net.Layout.GelfLayout, Esilog.Gelf4net">
				<param name="AdditionalFields" value="app:RandomSentence,version:1.0" />
				<param name="Facility" value="RandomPhrases" />
				<param name="IncludeLocationInformation" value="true"/>
			  </layout>
			</appender>

			<appender name="GelfAmqpAppender" type="Esilog.Gelf4net.Appender.GelfAmqpAppender, Esilog.Gelf4net">
			  <remoteAddress value="127.0.0.1" />
			  <remotePort value="5672" />
			  <username value="guest" />
			  <password value="guest" />
			  <virtualHost value="/" />
			  <remoteQueue value="queue1" />
			  <layout type="Esilog.Gelf4net.Layout.GelfLayout, Esilog.Gelf4net">
				<param name="AdditionalFields" value="app:RandomSentence,version:1.0" />
				<param name="Facility" value="RandomPhrases" />
				<param name="IncludeLocationInformation" value="true"/>
			  </layout>
			</appender>
		</log4net>

		<startup>
			<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
		</startup>
	</configuration>

## Copyright and License

gelf4net created by Juan J. Chiw

based on:
gelf4j created by Philip Stehlik - Copyright 2011

See LICENSE for license details