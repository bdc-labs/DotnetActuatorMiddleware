# [3.0.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v2.1.0...v3.0.0) (2025-08-26)


### Features

* Convert to system.text.json ([bf17580](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/bf175808389d11819e678ee597ec9c46f97ca6d7))
* Massive clean up ([1b14f84](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/1b14f84088dd3d68976a3625c339de3ada2e60f0))
* Update to .NET 9 ([399d8d8](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/399d8d8b327438f0b8a5d9e4c3f932510a03ce09))


### Performance Improvements

* Improve performance of ConfigurationRegistry ([d2bfcbf](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/d2bfcbfb0a9e4e9bdb31dc6bca748164ea38ad14))


### BREAKING CHANGES

* Converted to system.text.json and made quite a lot of code changes and while behaviour appears to be the same according to the test suite I will publish this as a major version bump to due the wide scope of changes.

# [2.1.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v2.0.0...v2.1.0) (2024-03-03)


### Features

* Bump semantic-release deps ([dad05e7](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/dad05e7b3278d6ae532e523241458e4cd295cc9f))

# [2.0.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.4.2...v2.0.0) (2024-03-03)


### Bug Fixes

* Trigger release ([95387dc](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/95387dc7bc23c0bd9cbd8901f3a9afa1acc51654))


### Features

* Update to .NET 8 and bump deps ([e3544c0](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/e3544c091286c8cb4ba85eaffb1f58f25ac375c9))


### BREAKING CHANGES

* This is a major version update of .NET from 6 to 8 and also switches the Elastic health check over to the new
Elastic.Clients.Elasticsearch package.

# [2.0.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.4.2...v2.0.0) (2024-03-03)


### Features

* Update to .NET 8 and bump deps ([e3544c0](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/e3544c091286c8cb4ba85eaffb1f58f25ac375c9))


### BREAKING CHANGES

* This is a major version update of .NET from 6 to 8 and also switches the Elastic health check over to the new
Elastic.Clients.Elasticsearch package.

## [1.4.2](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.4.1...v1.4.2) (2022-07-22)


### Bug Fixes

* Switch to StaticConnectionPool ([fd44cb5](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/fd44cb5e90683ff74598c91d865cbf642f17a82c))

## [1.4.1](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.4.0...v1.4.1) (2022-07-22)


### Bug Fixes

* Console spam from MySQL health check ([9610220](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/9610220883387554b84a15b99a82479a0cac3898))

# [1.4.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.3.0...v1.4.0) (2022-05-04)


### Features

* Add GetConfigurationSection helper method ([c80794a](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/c80794a4cc8185a134f2d35c896e18aa47914fd8))

# [1.3.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.2.0...v1.3.0) (2022-04-30)


### Bug Fixes

* Resolve compilation errors on latest C# version ([43bab02](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/43bab02ea19112d2ddce2d2d0e7e785d74edc611))


### Features

* Allow SSL validation to be disabled in Elasticsearch health check ([e42f255](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/e42f255ea50b4582496a90023d6828ebfbe386cd))

# [1.2.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.1.0...v1.2.0) (2022-02-04)


### Features

* Added job status helpers ([7a56874](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/7a568743c29c5d3ddb3de0f2fb6dbe14d9afe4e3))

# [1.1.0](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.0.1...v1.1.0) (2022-02-03)


### Bug Fixes

* Fixup compiler null warnings ([96bbadc](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/96bbadc6a717afcf6f821d7824eb19f143cc2a60))


### Features

* Add Quartz status endpoint ([86c28d5](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/86c28d5449e74f8d0a532845f5177a2961699c52))

## [1.0.1](https://github.com/bdc-labs/DotnetActuatorMiddleware/compare/v1.0.0...v1.0.1) (2022-02-02)


### Bug Fixes

* Trigger a release ([8c277a9](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/8c277a9dccddf77d8aa50d5c20f4dd8fb70e7120))

# 1.0.0 (2022-02-02)


### Features

* First release ([1a7c38f](https://github.com/bdc-labs/DotnetActuatorMiddleware/commit/1a7c38f0fbf61f6e74cccaceca8beaa928b04c0d))
