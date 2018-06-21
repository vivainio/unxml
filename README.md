# Unxml

Simplify and "flatten" XML files.

## Installation

```
$ dotnet tool install -g unxml
```

This command line application was developed for a need to compare XML files (e.g. database/application state dumps). It takes an XML file and "flattens" in to sorted records that are supposed to be easier to read and compare. E.g. a section of this project's unxml.fsproj XML file used to look like this:

```
PropertyGroup:
  - PropertyGroup:
     AssemblyName: unxml
     AutoGenerateBindingRedirects: true
     Configuration: Debug
     Name: unxml
     OutputType: Exe
     Platform: AnyCPU
     ProjectGuid: 6b2684f4-360e-4877-92c6-57c00911eaf9
     RootNamespace: unxml
     SchemaVersion: 2.0
     TargetFSharpCoreVersion: 4.4.0.0
     TargetFrameworkVersion: v4.5
  - PropertyGroup:
     [Condition]:  '$(Configuration)|$(Platform)' == 'Debug|AnyCPU'
     DebugSymbols: true
     DebugType: full
     DefineConstants: DEBUG;TRACE
     DocumentationFile: bin\Debug\unxml.XML
     Optimize: false
     OutputPath: bin\Debug\
     PlatformTarget: AnyCPU
     Prefer32Bit: true
     StartArguments: FSharp.Core.xml
     Tailcalls: false
     WarningLevel: 3
```

While a single PropertyGroup look like this in XML

```xml
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <Tailcalls>false</Tailcalls>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <WarningLevel>3</WarningLevel>
    <PlatformTarget>AnyCPU</PlatformTarget>
    <DocumentationFile>bin\Debug\unxml.XML</DocumentationFile>
    <Prefer32Bit>true</Prefer32Bit>
    <StartArguments>FSharp.Core.xml</StartArguments>
  </PropertyGroup>

```

Note how element attributes are presented by [square brackets] while tagged elements are represented without square brackets.

As you can see, the emitted yaml like format is easier on the eyes, easier to grep and result in a more deterministic alphabetic order (for easier comparison).
