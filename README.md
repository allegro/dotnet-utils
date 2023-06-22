# Allegro.Extensions

This repo contains utility packages that can be shared between Allegro services, but are not required. Everyone can be an author of such package - the goal of this repository is to make it easier and quicker to share useful code snippets, helpers, extensions etc, with as little effort as possible.  

# Contribution

Every PR is welcomed. You can extend existing packages and add new ones.

Please refer to [Contributing guideline](CONTRIBUTING.md) for details. 

# Available packages

- [Allegro.Extensions.AspNetCore](src/Allegro.Extensions.AspNetCore)  
  This library contains useful extensions and utilities for ASP.NET Core based projects.
- [Allegro.Extensions.Cqrs](src/Allegro.Extensions.Cqrs)  
  Contains implementation of common tools to support Cqrs pattern based on Handlers approach.
- [Allegro.Extensions.Dapper](src/Allegro.Extensions.Dapper)  
  This library contains useful utilities for simpler usage of Dapper library.
- [Allegro.Extensions.Dapper.Postgres](src/Allegro.Extensions.Dapper)  
  This library contains useful utilities for simpler usage for Postgres database with Dapper library.
- [Allegro.Extensions.DependencyCall](src/Allegro.Extensions.DependencyCall)  
  The purpose of this package is to standardize a way of calling external dependencies and push developers to think about ways of dealing with some issues related to each external call.
- [Allegro.Extensions.Financials](src/Allegro.Extensions.Financials)  
  Contains money value object and currency.
- [Allegro.Extensions.Globalization](src/Allegro.Extensions.Globalization)  
  Contains classes that define culture-related information, including language, country/region, calendars in use, format patterns for dates, currency, and numbers etc.
- [Allegro.Extensions.Identifiers](src/Allegro.Extensions.Identifiers)  
  Contains markers and generators to support strongly typed identifiers (primitive obsession code smell fix)
- [Allegro.Extensions.NullableReferenceTypes](src/Allegro.Extensions.NullableReferenceTypes)  
  Contains useful classes and extensions for projects which use NRT.
- [Allegro.Extensions.RateLimiting](src/Allegro.Extensions.RateLimiting)  
  RateLimiter allows specifying precise limit of operations performed per configured interval (for example per second).
- [Allegro.Extensions.Serialization](src/Allegro.Extensions.Serialization)  
  Contains serialization extensions and enum member helpers.
- [Allegro.Extensions.Validators](src/Allegro.Extensions.Validators)  
  Contains useful classes and extensions validators.


## License

Copyright Allegro Group

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.