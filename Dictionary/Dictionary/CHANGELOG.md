# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## Unreleased
### Changed
- Value replacements in ObservableDictionaryBase subclasses no longer
  trigger a PropertyChangedEvent on Keys.
### Fixed
- Clearing ObservableDictionaryBase subclasses no longer throws an
  exception due to bad NotifyCollectionChangedEventArgs parameters.

## 0.2.0
### Added
- This change log
- ObservableDictionaryBase abstract class wrapping IDictionary and
  related interfaces in a manner supporting data binding.
- ObservableDictionary class implementing ObservableDictionaryBase for
  standard Dictionary object.