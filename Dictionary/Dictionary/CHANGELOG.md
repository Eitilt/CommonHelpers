# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## 0.2.2
### Added
- `IObservableReadOnlyDictionary` interface combining observable
  interfaces with `IReadOnlyDictionary`
- `INotifyPropertyChanging` (as opposed to `...Changed`) for frameworks
  that support it

## 0.2.1
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