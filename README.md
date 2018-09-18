# gelf4net
gelf4net is a log4net adapter that formats logs to the [GELF][1] specification and makes it easy to send them over Udp, Amqp or http.

## Installation

You can install the latest stable release using on of the the nugets package:

All the libraries suppots `net45` and `netstandard1.5`

* [`Gelf4Net`](https://www.nuget.org/packages/Gelf4Net/). Contains all the Appenders
* [`Gelf4Net.UdpAppender`](https://www.nuget.org/packages/Gelf4Net.UdpAppender/). Only UdpAppenders
* [`Gelf4Net.AmqpAppender`](https://www.nuget.org/packages/Gelf4Net.AmqpAppender/). Only AmqpAppenders
* [`Gelf4Net.HttpAppender`](https://www.nuget.org/packages/Gelf4Net.HttpAppender/). Only HttpAppenders

## Configuration

Gelf4Net gives you the ability to log messages either through Udp, Amqp or Http.

## Gelf4Net

*net45 & dotnetstandard1.5*

```
<?xml version="1.0"?>
<configuration>
    <configSections>
        <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net"/>
    </configSections>

    <log4net>
        <appender name="GelfUdpAppenderCC" type="Gelf4Net.Appender.GelfUdpAppender, Gelf4Net">
            <remoteAddress value="192.168.44.10" />
            <remotePort value="12201" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:UdpAppenderCC,version:1.0,Environment:Dev,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <param name="SendTimeStampAsString" value="true"/>
                <!-- Sets the full_message and short_message to the specified pattern-->
                <param name="ConversionPattern" value="[%t] %c{1} - %m" />
            </layout>
        </appender>

        <appender name="GelfUdpAppender" type="Gelf4Net.Appender.GelfUdpAppender, Gelf4Net">
            <remoteAddress value="192.168.44.10" />
            <remotePort value="12201" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:UdpAppender,version:1.0,Environment:Dev,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <param name="SendTimeStampAsString" value="false"/>
            </layout>
        </appender>

        <appender name="AsyncGelfUdpAppender" type="Gelf4Net.Appender.AsyncGelfUdpAppender, Gelf4Net">
            <remoteAddress value="192.168.44.10" />
            <remotePort value="12201" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:AsyncUdpAppender,version:1.0,Environment:Dev,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <param name="SendTimeStampAsString" value="true"/>
            </layout>
        </appender>

        <appender name="GelfHttpAppender" type="Gelf4Net.Appender.GelfHttpAppender, Gelf4Net">
            <url value="http://192.168.44.10:12201/gelf" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:HttpAppender,version:1.0,Environment:Dev,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <param name="SendTimeStampAsString" value="false"/>
                <!--Sets the full_message and short_message to the specified pattern-->
                <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
            </layout>
        </appender>

        <appender name="GelfAmqpAppender" type="Gelf4Net.Appender.GelfAmqpAppender, Gelf4Net">
            <remoteAddress value="192.168.44.10" />
            <remotePort value="5672" />
            <username value="guest" />
            <password value="guest" />
            <virtualHost value="/" />
            <exchange value="log-messages" />
            <key value="#" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:GelfAmqpAppender,version:1.0,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <param name="SendTimeStampAsString" value="false"/>
                <!-- Sets the full_message and short_message to the specified pattern-->
                <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
            </layout>
        </appender>

        <appender name="AsyncGelfAmqpAppender" type="Gelf4Net.Appender.AsyncGelfAmqpAppender, Gelf4Net">
            <remoteAddress value="192.168.44.10" />
            <remotePort value="5672" />
            <username value="guest" />
            <password value="guest" />
            <virtualHost value="/" />
            <exchange value="log-messages" />
            <key value="#" />
            <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net">
                <param name="AdditionalFields" value="app:AsyncGelfAmqpAppender,version:1.0,Level:%level" />
                <param name="Facility" value="RandomPhrases" />
                <param name="IncludeLocationInformation" value="true" />
                <!-- Sets the full_message and short_message to the specified pattern-->
                <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
            </layout>
        </appender>

        <root>
            <level value="ALL" />
            <appender-ref ref="GelfUdpAppender" />
            <appender-ref ref="GelfUdpAppenderCC" />
            <appender-ref ref="AsyncGelfUdpAppender" />
            <appender-ref ref="GelfHttpAppender" />
            <appender-ref ref="GelfAmqpAppender" />
            <appender-ref ref="AsyncGelfAmqpAppender" />
        </root>
    </log4net>
</configuration>
```

## Gelf4Net.AmqpAppender

*net45*

```
<log4net>
    <appender name="GelfAmqpAppender" type="Gelf4Net.Appender.GelfAmqpAppender, Gelf4Net.AmqpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="5672" />
        <username value="guest" />
        <password value="guest" />
        <virtualHost value="/" />
        <exchange value="log-messages" />
        <key value="#" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.AmqpAppender">
            <param name="AdditionalFields" value="app:GelfAmqpAppender,version:1.0,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
            <!-- Sets the full_message and short_message to the specified pattern-->
            <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
        </layout>
    </appender>

    <appender name="AsyncGelfAmqpAppender" type="Gelf4Net.Appender.AsyncGelfAmqpAppender, Gelf4Net.AmqpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="5672" />
        <username value="guest" />
        <password value="guest" />
        <virtualHost value="/" />
        <exchange value="log-messages" />
        <key value="#" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.AmqpAppender">
            <param name="AdditionalFields" value="app:AsyncGelfAmqpAppender,version:1.0,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
            <!-- Sets the full_message and short_message to the specified pattern-->
            <!--<param name="ConversionPattern" value="[%t] %c{1} - %m" />-->
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfAmqpAppender" />
        <appender-ref ref="AsyncGelfAmqpAppender" />
    </root>
</log4net>
```

*netstandard1.5*

```
<log4net>
    <appender name="GelfAmqpAppender" type="Gelf4Net.Appender.GelfAmqpAppender, Gelf4Net.AmqpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="5672" />
        <username value="guest" />
        <password value="guest" />
        <virtualHost value="/" />
        <exchange value="log-messages" />
        <key value="#" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.Core">
            <param name="AdditionalFields" value="app:DotnetcoreGelfAmqpAppender,version:1.0,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
        </layout>
    </appender>

    <appender name="AsyncGelfAmqpAppender" type="Gelf4Net.Appender.AsyncGelfAmqpAppender, Gelf4Net.AmqpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="5672" />
        <username value="guest" />
        <password value="guest" />
        <virtualHost value="/" />
        <exchange value="log-messages" />
        <key value="#" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.Core">
            <param name="AdditionalFields" value="app:DotnetcoreAsyncGelfAmqpAppender,version:1.0,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="ConversionPattern" value="[%t] %c{1} - %m" />
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfAmqpAppender" />
        <appender-ref ref="AsyncGelfAmqpAppender" />
    </root>
</log4net>
```

## Gelf4Net.HttpAppender

*net45*

```
<log4net>
    <appender name="GelfHttpAppender" type="Gelf4net.Appender.GelfHttpAppender, Gelf4Net.HttpAppender">
        <url value="http://192.168.44.10:12201/gelf" />
        <layout type="Gelf4net.Layout.GelfLayout, Gelf4Net.HttpAppender">
            <param name="AdditionalFields" value="app:GelfHttpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
            <!-- Sets the full_message and short_message to the specified pattern-->
            <param name="ConversionPattern" value="[%t] %c{1} - %m" />
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfHttpAppender" />
    </root>
</log4net>
```

*dotnetstandard1.5*

```
<log4net>

    <appender name="GelfHttpAppender" type="Gelf4Net.Appender.GelfHttpAppender, Gelf4Net.HttpAppender">
        <url value="http://192.168.44.10:12201/gelf" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.Core">
            <param name="AdditionalFields" value="app:DotnetcoreHttpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfHttpAppender" />
    </root>
</log4net>
```

## Gelf4Net.UdpAppender

*net45*

```
<log4net>
    <appender name="AsyncGelfUdpAppender" type="Gelf4Net.Appender.AsyncGelfUdpAppender, Gelf4Net.UdpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="12201" />
        <maxChunkSize value="3" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.UdpAppender">
            <param name="AdditionalFields" value="app:AsyncGelfUdpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
        </layout>
    </appender>

    <appender name="GelfUdpAppender" type="Gelf4Net.Appender.GelfUdpAppender, Gelf4Net.UdpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="12201" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.UdpAppender">
            <param name="AdditionalFields" value="app:GelfUdpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfUdpAppender" />
        <appender-ref ref="AsyncGelfUdpAppender" />
    </root>
</log4net>
```

*dotnetstandard1.5*

```
<log4net>
    <appender name="GelfUdpAppenderCC" type="Gelf4Net.Appender.GelfUdpAppender, Gelf4Net.UdpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="12201" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.Core">
            <param name="AdditionalFields" value="app:DotnetcoreUdpAppenderCC,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="ConversionPattern" value="[%t] %c{1} - %m" />
        </layout>
    </appender>

    <appender name="GelfUdpAppender" type="Gelf4Net.Appender.GelfUdpAppender, Gelf4Net.UdpAppender">
        <remoteAddress value="192.168.44.10" />
        <remotePort value="12201" />
        <layout type="Gelf4Net.Layout.GelfLayout, Gelf4Net.Core">
            <param name="AdditionalFields" value="app:DotnetcoreUdpAppender,version:1.0,Environment:Dev,Level:%level" />
            <param name="Facility" value="RandomPhrases" />
            <param name="IncludeLocationInformation" value="true" />
            <param name="SendTimeStampAsString" value="false"/>
        </layout>
    </appender>

    <root>
        <level value="ALL" />
        <appender-ref ref="GelfUdpAppender" />
        <appender-ref ref="GelfUdpAppenderCC" />
    </root>
</log4net>
```

## Additional Properties

There are several ways that additional properties can be added to a log.

**Configuration**
Any static information can be set through configuration by adding a comma separated list of key:value pairs.
You can also use conversion patterns like you would if you were using the [PatternLayout class][3]:

```
<layout type="Gelf4net.Layout.GelfLayout, Gelf4net">
    <param name="AdditionalFields" value="app:RandomSentence,version:1.0,Level:%level" />
</layout>
```

This will add the following fields to your GELF log:

```
{
    ...
    "_app":"RandomSentence",
    "_version":"1.0",
    "_Level":"DEBUG",
    ...
}
```

You can also use your own custom field and key/value separators to deal with the case when the additional fields contain commas or colons
```
<layout type="Gelf4net.Layout.GelfLayout, Gelf4net">
    <param name="AdditionalFields" value="app¬:¬RandomSentence¬|¬version:1.0¬|¬Level¬:¬%level" />
    <param name="FieldSeparator" value="¬|¬" />
    <param name="KeyValueSeparator" value="¬:¬" />
</layout>
```


**Custom Properties**
Any properties you add to the `log4net.ThreadContext.Properties` object
will automatically be added to the message as additional fields

```
log4net.ThreadContext.Properties["TraceID"] = Guid.NewGuid();
```

This will be added to the log:

```
{
    ...
    "_TraceID":"3449DDF8-C3B4-46DD-8B83-0BDF1ABC92E2",
    ...
}
```

**Custom Objects**
You can use custom objects to log additional fields to the output. Here is an example:

```
_logger.Debug(new {
    Type = "Request",
    Method = request.Method,
    Url = request.Url
});

```

This will add the following additional fields to the output:

```
{
    ...
    "_Type":"Request",
    "_Method":"GET",
    "_Url":"http://whatever.com/gelf",
    ...
}
```

Under the hood the `GelfLayout` class takes any object that is not a string and adds it's public
properties to a dictionary. All non-numeric values are then converted in to strings. You can also pass in a dictionary
directly and get the same output. When passing in a dictionary rather than using the public properties
of the object it uses the Key/Value pairs stored internally.

```
_logger.Debug(new Dictionary<string,string>{
    { "Type", "Request" },
    { "Method", request.Method },
    { "Url", request.Url.ToString() }
});

```

## Formatting Messages
If you are just logging a simple string then that message will show up in the `full_message` and
be truncated to 250 characters in the `short_message` field.

```
_logger.Debug("This is a ridiculously short message but pretending it's longer than 250 characters");

```

Pretending the previous message is longer than 250 characters, this will be your output:

```
{
    ...
    "full_message":"This is a ridiculously short message but pretending it's longer than 250 characters",
    "short_message":"This is a ridiculously short message but pretending",
    ...
}
```

If you want to format your message using a conversion pattern you can do so by specifying the `ConversionPattern` parameter.
Again you can specify all the same parameters that you would if you were using the [PatternLayout][3].

```
          <layout type="Gelf4net.Layout.GelfLayout, Gelf4net">
            <param name="ConversionPattern" value="[%t] %c{1} - %m" />
          </layout>
```


You can also specify the message when logging custom objects:

```
_logger.Debug(new Dictionary<string,string>{
    { "Type", "Request" },
    { "Method", request.Method },
    { "Message", request.RawUrl }
    { "ShortMessage", request.Url.ToString() }
});

```

If the custom object does not have a `Message` or `ShortMessage` field than the message will be the
output of the `ToString()` of that object.

## dotnetstandard1.5 Appenders

dotnetstandard1.5~Appenders doesn't log the file an line...

## Custom LoggingEventData

Using `LoggingEventData` with all the goodies of gelf4net.

Remember if you're not setting the `LocationInfo` in `LoggingEventData` you should set it to false
in the `appender` configuration `<param name="IncludeLocationInformation" value="false"/>`

```
var loggingEventData = new LoggingEventData
{
    Message = "This is a message",
    LoggerName = "Test.Logger.Class",
    Level = Level.Debug,
    TimeStamp = DateTime.Now
};
var loggingEvent = new LoggingEvent(loggingEventData);
```

If you want to send a custom properties, serialize the object.

```
var customObject = new {
    Type = "Request",
    Method = request.Method,
    Url = request.Url
}

var loggingEventData = new LoggingEventData
{
    Message = JsonConvert.SerializeObject(customObject),
    LoggerName = "Test.Logger.Class",
    Level = Level.Debug,
    TimeStamp = DateTime.Now
};
var loggingEvent = new LoggingEvent(loggingEventData);
```

## Thanks

The dotnetcore web project sample was based in http://dotnetliberty.com/index.php/2015/11/09/asp-net-5-logging-with-log4net/

## License
This project is licensed under the [MIT](2) license


[1]: https://github.com/Graylog2/graylog2-docs/wiki/GELF
[2]: https://opensource.org/licenses/MIT
[3]: https://logging.apache.org/log4net/log4net-2.0.8/release/sdk/html/T_log4net_Layout_PatternLayout.htm
