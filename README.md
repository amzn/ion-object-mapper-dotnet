## Ion Object Mapper for .NET

This is the Ion Object Mapper for .NET, which is a convenience library built to assist
developers when dealing with data that's in the format of [Amazon Ion](https://amzn.github.io/ion-docs/).
It is used to easily transform C# classes and objects into Ion format and vice-versa.

[![codecov](https://codecov.io/gh/amzn/ion-object-mapper-dotnet/branch/main/graph/badge.svg?token=w6PsKN4xZ3)](https://codecov.io/gh/amzn/ion-object-mapper-dotnet)

## Requirements

### .NET

The Ion Object Mapper targets .NET Core 3.1. Please see the link below for more information on compatibility:

- [.NET Core](https://dotnet.microsoft.com/download/dotnet-core)

## Getting Started

See the [Cookbook](COOKBOOK.md) for a simple usage guide.

## Spec

See the [Spec](SPEC.md) for a detailed specification of the Ion Object Mapper.

## Contributing

If you are interested in contributing to the Ion Object Mapper, please take a look at [CONTRIBUTING](CONTRIBUTING.md).

## Development

### Setup

Assuming that you have .NET Core SDK version 3.1 or later installed from the
[Microsoft .NET downloads](https://dotnet.microsoft.com/download) site,
use the below command to clone the repository.

```
$ git clone https://github.com/amzn/ion-object-mapper-dotnet.git
$ cd ion-object-mapper-dotnet
```

Changes can now be made in the repository.

The project currently uses default dotnet [CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x) tools,
you can build the project simply by:

```
$ dotnet build
```

### Running Tests

You can run the unit tests by:

```
$ dotnet test Amazon.IonObjectMapper.Test
```

You can also run the performance tests by:

```
$ dotnet test Amazon.IonObjectMapper.PerformanceTest
```

### Documentation 

DocFX is used for documentation. Please see the link below for more detail to install DocFX
* [DocFX installation](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool)

You can generate the docstring HTML locally by running the following in the root directory of this repository:

```
$ docfx docs/docfx.json --serve
```

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This project is licensed under the Apache-2.0 License.
