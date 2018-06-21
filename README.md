# Unxml

Simplify and "flatten" XML files.

## Installation

```
$ dotnet tool install -g unxml
```

## Usage

```
$ unxml <xml file name>
```


## Introduction

This command line application was developed for a need to compare XML files (e.g. database/application state dumps). It takes an XML file and "flattens" in to sorted records that are easier to read and compare. E.g. this project's unxml.fsproj (the "SDK style" project file) looks like this when run through Unxml:


```yaml
Project
  [Sdk]: Microsoft.NET.Sdk
  PropertyGroup
    OutputType = Exe
    TargetFramework = netcoreapp2.1
    PackAsTool = true
    Description = Unxml 'pretty-prints' xml files in light, yamly, readable format
    PackageVersion = 1.0.0
    Authors = vivainio
    Title = unxml
    Copyright = 2018 Ville M. Vainio
    PackageLicenseUrl = https://raw.githubusercontent.com/vivainio/unxml/master/LICENSE
    PackageProjectUrl = https://raw.githubusercontent.com/vivainio/unxml/master/README.md
  ItemGroup
    Compile [Include]: FileSystemHelper.fs
    Compile [Include]: MutableCol.fs
    Compile [Include]: Program.fs
```

While it looks like this in original XML:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
    <PackAsTool>true</PackAsTool>
    <Description>Unxml 'pretty-prints' xml files in light, yamly, readable format</Description>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>vivainio</Authors>
    <Title>unxml</Title>
    <Copyright>2018 Ville M. Vainio</Copyright>
    <PackageLicenseUrl>https://raw.githubusercontent.com/vivainio/unxml/master/LICENSE</PackageLicenseUrl>
    <PackageProjectUrl>https://raw.githubusercontent.com/vivainio/unxml/master/README.md</PackageProjectUrl>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="FileSystemHelper.fs"/>
    <Compile Include="MutableCol.fs"/>
    <Compile Include="Program.fs" />
  </ItemGroup>
</Project>
```

Note how element attributes are presented by [square brackets] while tagged elements are represented without square brackets.

As you can see, the emitted yaml like format is easier on the eyes, easier to grep and result in a more deterministic alphabetic order (for easier comparison).
