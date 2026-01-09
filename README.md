# Polaris Bulk Patron Update

Provided a CSV file of barcodes and fields this command line application will apply updates to patrons in Polaris via the Polaris API.

## Caution

**Before going any further, please understand this code was designed to perform bulk updates on 
multiple patron records with no undo ability. Please be certain about what you are doing before 
running a batch operation against hundreds or thousands of records and ensure you have a valid
and restorable backup before you run against production data. That means: back up your data,
restore your data into a test environment, and verify your backup is valid before proceeding.**

## Configuration

### appsettings.json

Configure the Polaris API settings here and provide a privileged user's authentication information.

```json
"PapiSettings": {
    "AccessID": "",
    "AccessKey": "",
    "Hostname": "",
    "PolarisOverrideAccount": {
        "Domain": "",
        "Password": "",
        "Username": ""
}
```

### Command line options

- `-c <file>` or `--csv <file>` - the CSV file to import data from
- `-g` or `--go` - confirmation to actually take the action and not just a dry run - there is no undo!
- `-d <#>` or `--delay <#>` - delay in milliseconds between Polaris updates so as to not overwhelm the server

Sample commands:

- `-c c:\test-run.csv` - process `c:\test-run.csv` but don't actually commit changes to Polaris
- `-g -d 1000 -c c:\batch.csv` - process `c:\batch.csv` with a 1 second delay between records and commit changes to Polaris

### CSV file format

Columns in the CSV file - barcode is required and at least one field to update:

- Barcode - required - patron barcode
- AddrCheckDate - optional - a new address check date
- AltEmailAddress - optional - patron's alternate email address
- EmailAddress - optional - patron's email address
- EnableSMS - optional - true to enable SMS, false to disable
- ExpirationDate - optional - a new patron expiration date
- User1 - optional - new text for the User 1 field
- User2 - optional - new text for the User 2 field
- User3 - optional - new text for the User 3 field
- User4 - optional - new text for the User 4 field
- User5 - optional - new text for the User 5 field

## Sample CSV file

Here's a sample CSV file:

```csv
Barcode,User5
1234567891234,I am not a number
1234567891235,I am a free man
```

## Logging

By default the code logs to the console, for more elaborate logging please see [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration).

When running from the console, customize the `Serilog` section in `appsettings.json`, for example:

```json
"Serilog": {
  "WriteTo": [
    { "Name": "File", "Args": { "path": "log.txt", "rollingInterval": "Day" } }
  ]  
}
```

When running from Docker, replace hierarchy with two underscores, for example:

```sh
Serilog__WriteTo__0__Name=Seq
Serilog__WriteTo__0__Args__serverUrl=http://seq:5341
```

## Credits

This application relies on the [Clc.Polaris.Api](https://www.nuget.org/packages/Clc.Polaris.Api/), created and maintained by the [Central Library Consortium](https://clcohio.org/).

## License

Polaris Bulk Patron Update source code is Copyright 2023 by the Maricopa County Library District and is distributed under [The MIT License](http://opensource.org/licenses/MIT).
