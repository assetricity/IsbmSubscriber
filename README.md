# ISBM Subscriber

A .NET application that provides a GUI to subscribe to an OpenO&M ISBM-compliant server. The application will periodically poll the ISBM for published messages. Received messages are written to an XML file in the same directory as the application.

ISBM server endpoints and default configuration options are set in the `app.config` / `IsbmSubscriber.exe.config` files.

## License

Copyright 2013 [Assetricity, LLC](http://assetricity.com)

ISBM Subscriber is released under the MIT License. See [LICENSE](https://github.com/assetricity/IsbmSubscriber/blob/master/LICENSE) for details.
