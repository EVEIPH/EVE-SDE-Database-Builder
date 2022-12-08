# EVE-SDE-Database-Builder
EVE SDE Database Builder is a Windows app that lets anyone download the most current SDE and allows options for building 6 different Database types whenever they want and they can customize the import by language type and selecting the data they want or do not want from the SDE.

Main links for the application:

    Screenshot of the program 102
    Binaries for installing the program 52
    Github for the code 55
    Schema I’m using in SQLite 26 (other database types use similar types as much as possible and the table schema shouldn’t be any huge surprises)

Key features:

    Imports all YAML files in about 30 minutes (the universe files take about 23 minutes alone).
    Settings to import tables with translated fields for English, French, German, Japanese, Chinese, Russian, and Korean. When a field is translatable, the program will import the selected language.
    Allows the selection of specific YAML files so users don’t have to import or build the entire database each time.
    Imports the SDE YAML files into Microsoft Access, SQLite, and CSV files with the option to save CSV to SSV – Semi-colon separated values with European decimal format (10.000,00) for use in Excel by non-US decimal format users.
    Imports the SDE YAML files into local servers for Microsoft SQL Server, PostgreSQL, and MySQL. To use these import types, you need to have a local server installed on your machine to connect to.
    Threaded processing allows for increasing import times. Users can select to use maximum threads (no limit) or a number of 1-24 threads depending on their system. Users can set the threads in the File menu.
    Contains an updater function to update the program when changes are uploaded to GitHub.

Basic functions:

    Select the SDE Download Folder where you want to save the SDE Download.
    Select ‘Download’ to download the most current SDE. This will check the checksum file on the CCP server and if different than the one on your local machine, it will download the SDE from CCP. After downloading, the program will make a single folder with today’s date to extract the SDE zip file and set the SDE File Folder to that folder.
    If needed: Select the SDE folder (the ‘sde’ folder part of whatever folder you extracted the data into).
    If you plan to make an SQLite, Access, or CSV/SSV database, select the final folder where the database will be saved (CSV ‘database’ will be a new folder).
    Enter the name of the database in the text box provided and enter other information to connect to your particular database type as necessary.
    Select import language
    Save Settings – you should see a list of yaml files in the file list. If not, you didn’t save the correct yaml SDE file folder with the base ‘sde’ folder as the root.
    Select the files you want to import (save settings will save what you checked as well)
    Press ‘Build Database’ to begin.

