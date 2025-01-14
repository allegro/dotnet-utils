# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.2.0] - 2025-01-14
### Allegro.Extensions.Cqrs
- Added publicOnly parameter to AddQueries and AddCommands methods
- Unified commands/queries registration behavior across different Scrutor versions
### Allegro.Extensions.Cqrs.FluentValidations
- Added publicOnly parameter to AddCqrsFluentValidations method
- Unified validators registration behavior across different Scrutor versions

## [2.1.0] - 2023-01-26

### Allegro.Extensions.Cqrs

Command and Query dispatchers will explicitly fail when multiple handler registrations are registered. 

## [2.0.0] - 2023-01-26

### Allegro.Extensions.Cqrs.Abstractions

Replace ICommand and IQuery with Command and Query (based on the approach
from [CQRS.Mediatr.Lite](https://github.com/microsoft/CQRS.Mediatr.Lite/blob/a6f63cf62a5e2b1b48a55b7917ba036c8ae6f3b9/src/sdk/Command/Command.cs))

## [1.1.0] - 2022-11-17

### Allegro.Extensions.Cqrs

* Adjust implementation to Allegro.Extensions.Cqrs.Abstractions 1.1.0
* Some code cleanup

### Allegro.Extensions.Cqrs.Abstractions

* Remove ICommandExecutionActions - as we already have possibility to decorate
* Add IQueryValidator
* Change query result not nullable

### Allegro.Extensions.Cqrs.FluentValidations

* Add `AddCqrsFluentValidations` extension to enable fluent validations

## [1.0.0] - 2022-11-09
### Allegro.Extensions.Cqrs

* Initiated Allegro.Extensions.Cqrs with default implementation of dispatchers and command actions support

### Allegro.Extensions.Cqrs.Abstractions

* Initiated Allegro.Extensions.Cqrs.Abstractions with basic abstraction for cqrs.
