# EVE-SDE-Database-Builder
EVE SDE Database Builder is a Windows app that lets anyone download the most current SDE and allows options for building 6 different Database types whenever they want and they can customize the import by language type and selecting the data they want or do not want from the SDE.

<p align="center">
<img src="https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Screenshot.png">
</p>

### System Requirements
* .net 4.8
* Windows 64-bit OS
* Appropriate server software/connections for SQL Server, PostgreSQL, and MySQL data versions.

## Key features:
* Imports all YAML files in about 30 minutes (the universe files take about 23 minutes alone).
* Settings to import tables with translated fields for English, French, German, Japanese, Chinese, Russian, and Korean. When a field is translatable, the program will import the selected language.
* Allows the selection of specific YAML files so users don’t have to import or build the entire database each time.
* Imports the SDE YAML files into Microsoft Access, SQLite, and CSV files with the option to save CSV to SSV – Semi-colon separated values with European decimal format (10.000,00) for use in Excel by non-US decimal format users.
* Imports the SDE YAML files into local servers for Microsoft SQL Server, PostgreSQL, and MySQL. To use these import types, you need to have a local server installed on your machine to connect to.
* Threaded processing allows for increasing import times. Users can select to use maximum threads (no limit) or a number of 1-24 threads depending on their system. Users can set the threads in the File menu.
* Contains an updater function to update the program when changes are uploaded to GitHub.

## Basic functions:
1. Select the SDE Download Folder where you want to save the SDE Download.
2. Select ‘Download’ to download the most current SDE. This will check the checksum file on the CCP server and if different than the one on your local machine, it will download the SDE from CCP. After downloading, the program will make a single folder named with the date of the extract, extract the SDE zip file to it, and set the SDE File Folder to that folder.
If needed: Select the SDE folder (the ‘sde’ folder part of whatever folder you extracted the data into).
3. If you plan to make an SQLite, Access, or CSV/SSV database, select the final folder where the database will be saved (CSV ‘database’ will be a new folder).
4. Enter the name of the database in the text box provided and enter other information to connect to your particular database type as necessary.
5. Select import language
6. Save Settings – you should see a list of yaml files in the file list. If not, you didn’t save the correct yaml SDE file folder with the base ‘sde’ folder as the root.
7. Select the files you want to import (save settings will save what you checked as well)
8. Press ‘Build Database’ to begin.

# Click here to download the [Binaries for installing the program](https://github.com/EVEIPH/EVE-SDE-Database-Builder/raw/master/Latest%20Files/EVE%20SDE%20Database%20Builder%20Install.zip)

## Known Issues:
* Korean translations are inconsistent.
* Spanish, Italian, and Korean languages are inconsistent throughout data with translations. When CCP updates this data, the program will be updated to allow for those language downloads.
* Users may get an "Local DB" error when updating related to translations but this is not a critical error and the program can continue but there may be a missing record in the final translations.
