# Changelog

## [0.1.3] - Unreleased

### Added
- Add support to identify target type from Ion annotation for deserialization (#61)
- Add tests and test objects for scenarios dealing with `IonAnnotateType` (#64)
- Add support for `IonAnnotateType` attribute to work on Property. (#65)

### Fixed
- The `Format` option will be valued by ObjectMapper now (#56)

### Changed
- Change the default serialization format to Ion binary (#58)

## [0.1.2] - 2021-10-29

### Fixed

- Fix error when serializing classes that implement interfaces (#51)

## [0.1.1] - 2021-09-07

### Changed

- Downgrade .NET version requirement from .NET 5 to .NET Core 3.1

## [0.1.0] - 2021-07-23

This is the first release of the `v0.1.0` Amazon IonObjectMapper.