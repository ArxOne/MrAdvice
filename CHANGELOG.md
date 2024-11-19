## 2.17

- Improvement: support for dotnet9

## 2.16

- Fix: multiple load of same assembly was leading to type resolution failure

## 2.15

- New: Upgraded dnlib dependency, now supports all `PDB` formats (thaks to @rgroenewoudt)

## 2.14

Sorry for missing changelog in previous releases 😥
- Fix: `NullReferenceException` in `TypeDefinitionExtensions.GetAllInterfacesRaw`

## 2.9.5

- New: support for .NET6

## 2.9.3 (2021-11-27)

- Fix: MrAdvice assembly file version was stuck to 2.0

## 2.9.2 (2021-09-17)

- Fix: broken build when too many local copy references ([issue #172](https://github.com/ArxOne/MrAdvice/issues/172))
- New: a CHANGELOG file (yay!)
