Thank you for your interest in Gallery Server. Below are some notes to help you get the server up and running.

- Build
After downloading the source code or cloning the repository, run ClickToBuild.bat in the root directory to build and run all of the tests in both projects. This will also copy the necessary files from Plugins into the GalleryServer's bin.


- Web Server
The next step is to get the Gallery.Server project running in IIS. The easiest way to do this is to simply open the solution in Visual Studio, which will configure an IIS virtual directory at http://localhost/GalleryServer. (Note that you have to run Visual Studio as an administrator for it to be able to create the web site). Alternatively you can manually configure the web site/virtual directory in IIS. The Gallery.Server project cannot be run in the built-in Visual Studio development web server (Cassini) at this time due to a limitation with Cassini hosting OData services.


- Database
The Gallery Server will use SQL Server Compact 4 by default, and the database will be created automatically the first time the feed is accessed. You can change to using a SQL Server install (e.g. SQL Server Express) by changing configuration settings. Note that an empty SQL Server database needs to be created manually; it will not be created automatically.

In the AppSettings.config file in the web site folder, change the MigratorProvider value from SqlServerCe to SqlServer.
In the ConnectionStrings.config file in the web site folder, change the GalleryFeedEntities connection string value to be a valid SQL Server connection string and change the provider name to "System.Data.SqlClient".

- API
The contents of the gallery are exposed as an OData feed. In order to upload and update packages, a REST API is provided using the WCF 4 WebHttp Services. Most of the methods in the REST API take in a string "key" parameter as a means of authenticating that the caller has the rights to make the requested update. Each of the methods that take the key will first call back to an endpoint (configurable in the AppSettings.config file) to validate the call.

Note: You can disable the call back to the endpoint by setting "AuthenticatePackageRequests" to false in the configuration.
