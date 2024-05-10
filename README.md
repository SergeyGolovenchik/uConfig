# uConfig

[![Release Notes](https://img.shields.io/badge/Version-1.0.0-red)](https://github.com/SergeyGolovenchik/uConfig/blob/main/RELEASE-NOTES.md) ![Umbraco](https://img.shields.io/badge/Umbraco-v13+-3544B1) [![License](https://img.shields.io/badge/License-MIT-darkgreen.svg)](https://github.com/SergeyGolovenchik/uConfig/blob/main/LICENSE)

## Overview
uConfig is a plugin for Umbraco CMS that simplifies process of viewing and managing server's configuration via special backoffice dashboard. It introduces SQL configuration source for Umbraco database seamlessly intregrated into ASP.NET core architecture by implementing IConfiguration interface and providing configuration editing in runtime. uConfig may be considered as a solution when: 

- Storing sensitive configuration (such as passwords) in repository considered as security fail.
- Usage of other providers (such as Azure KeyVault) are not applicable.
- There are no direct access to filesystem or environment variables.
- Configuration changes required in runtime.

## Features

- **SQL-Based Configuration Provider:** Store and manage configuration values directly in the Umbraco database.
- **Easy Management:** A user-friendly interface to view and update server configuration values without the need for direct database or file access.

## Security

- uConfig uses Umbraco database for storing configuration values.
- Dashboard and API protected with Auth filters and available for users of **Admin** group only.
- Some values may be additionally protected with value-masking (see Configuration section).

## Getting Started
uConfig is available via Nuget, Umbraco Marketplace or GitHub Packages.

### Usage:
To enable plugin all you need is to call `.AddUConfig()` extention method for your UmbracoBuilder from `uConfig` namespace: 

```csharp
using uConfig;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
	.AddBackOffice()
	.AddWebsite()
	.AddDeliveryApi()
	.AddComposers()
	.Build();

builder.AddUConfig();

WebApplication app = builder.Build();
```

**Note**: uConfig utilized default .NET Core configuration, so sources order matters: it's able to override all configurations that was added before it and has no power over configurations added after (Keys will be automatically marked as readonly without ability to modify it). 

### Configuration:
uConfig ships with it's own configuration file `uConfigSettings.json` which should appear at your project's root after package installation. This file introduces `Umbraco:uConfig` configuration section with 4 sub-keys with default configuration that should cover most of scenarios:

- **`Selected`** *[String Array of RegEx patterns]*: Configuration key will be selected for displaying if it matches any of them. All keys will be selected if empty. Default value: `[ "^ConnectionStrings", "^Umbraco" ]` - everything that starts with Umbraco or ConnectionStrings.
- **`Excluded`** *[String Array of RegEx patterns]*: Configuration key will be excluded from selection if it matches any of them. None of keys will be filtered if empty. Default value: `[ "^Umbraco:uConfig" ]` - uConfig's configuration.
- **`Readonly`** *[String Array of RegEx patterns]*: Configuration key will be marked as "Readonly" (no ability to change thru uConfig's dashboard) if it matches any of them. Default value: `[ "^ConnectionStrings" ]` - everything that starts with ConnectionStrings.
- **`Protected`** *[String Array of RegEx patterns]*: Configuration key will be marked as "Protected" (value will be masked with '•' during viewing or editing) if it matches any of them. Default value: `[ "^ConnectionStrings", "Password$" ]` - all keys that starts with ConnetionStrings or ends with Password.

#### Custom Configuration
If your configuration includes additional nodes and you want to show/edit them thru the dashboard you may edit this configuration file. Due to nuget specific of package delivering `uConfigSettings.json` may be readonly in your project after installation because it was added from nuget cache folder. It's recommended to replace it with local copy with same name and desired content in that case. 

## Contributing
We welcome contributions from the community. If you'd like to contribute, please fork the repository and submit a pull request.

## License
uConfig is released under the MIT License. See the LICENSE file for more details.
