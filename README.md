# Dotnet Actuator middleware

A small library that provides some useful endpoints similar to the endpoints provided by the Spring Boot Actuator package
for Java applications.

The library currently provides the following endpoints:

| Endpoint  | Description                                                                  |
|-----------|------------------------------------------------------------------------------|
| `/health` | Returns a JSON response containing results from user-defined health checks.  |
| `/info`   | Returns a JSON response containing the running application name and version. |
| `/env`    | Returns detailed information about the application's running environment     |

Further details on each endpoint can be found in the wiki.

## Installation

The package is available in the Nuget gallery and can be installed with:

```shell
dotnet add package DotnetActuatorMiddleware
```

or through your favourite IDE.

## Working with this repository

If you wish to contribute to this package then you will need the following to work with this repository:

- .NET SDK (Currently built against .NET 6.0)
- Node.JS 16.x or later

As this repository uses semantic-release for generating release artifacts the package.json contains 
a prepare hook to install Husky for commit linting.