# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2023-01-25

### Allegro.Extensions.Cqrs.Abstractions

* Remove Id from ICommand - not used

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
