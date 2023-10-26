# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `ADMIN_ENVIRONMENT` configuration instead of `TEST_ENVIRONMENT`. 
- Add the `USER_ID` request (only for `ADMIN_ENVIRONMENT`).

### Changed

- Replace `YDB API` with `Document API` in `DOWNLOAD` and `LOAD_IMAGE` requests to reduce the cost of requests.
- Copyright in LICENSE.

## [1.0.0] - 2023-09-14

Initial release.