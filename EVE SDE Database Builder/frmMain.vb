
Imports System.IO
Imports System.Threading
Imports System.Globalization ' For culture info
Imports System.Xml
Imports System.IO.Compression

Public Class frmMain
    Private FirstLoad As Boolean
    Public Const BSDPath As String = "\bsd\"
    Private Const FSDPath As String = "\fsd\"
    Private Const EVEUniversePath As String = "\universe\"

    Private Const GridSettingsFileName As String = "GridSettings.txt"
    Public Const ThreadsFileName As String = "NumberofThreads.txt"

    ' For deploying the files to XML for updates
    Private ReadOnly LatestFilesFolder As String
    Private Const MainEXEFile As String = "EVE SDE Database Builder.exe"
    Private Const UpdaterEXEFile As String = "ESDEDB Updater.exe"
    Private Const MainEXEConfig As String = "EVE SDE Database Builder.exe.config"
    Private Const UpdaterEXEConfig As String = "ESDEDB Updater.exe.config"

    Private Const SQLiteInteropDLL86 As String = "x86\SQLite.Interop.dll"
    Private Const SQLiteInteropDLL64 As String = "x64\SQLite.Interop.dll"

    Private Const LatestVersionXML As String = "LatestVersionESDEDB.xml"

    ' File URLs
    Private Const MainEXEFileURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/EVE%20SDE%20Database%20Builder.exe"
    Private Const UpdaterEXEFileURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/ESDEDB%20Updater.exe"
    Private Const MainEXEConfigURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/EVE%20SDE%20Database%20Builder.exe.config"
    Private Const UpdaterEXEConfigURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/ESDEDB%20Updater.exe.config"
    Private Const SQLiteInteropURL86 As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/x86/SQLite.Interop.dll"
    Private Const SQLiteInteropURL64 As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/x64/SQLite.Interop.dll"

    ' For setting the number of threads to use
    Public SelectedThreads As Integer

    Private CheckedFilesList As List(Of String)

    Private ReadOnly LocalCulture As New CultureInfo("en-US")

    Private CSVImport As Boolean ' If we are using CSV to bulk import the data
    Private rs As New Resizer

    ' For use with updating the grid with files
    Public Structure FileListItem
        Dim FileName As String
        Dim RowLocation As Integer
    End Structure

    ' For use in filling the grid with checks
    Public Structure GridFileItem
        Dim FileName As String
        Dim Checked As Integer
    End Structure

    ' For threading
    Public Structure ThreadList
        Dim T As Thread
        Dim Params As YAMLFilesBase.ImportParameters
    End Structure

#Region "Settings"

    ''' <summary>
    ''' Saves all settings, including the files checked
    ''' </summary>
    Private Sub SaveSettings(SupressMessage As Boolean)

        If Not ConductErrorChecks(False) Then
            Exit Sub
        End If

        With UserApplicationSettings
            .DatabaseName = txtDBName.Text
            .SDEDirectory = lblSDEPath.Text
            .FinalDBPath = lblFinalDBPath.Text
            .DownloadFolderPath = lblDownloadFolderPath.Text
            .UseLargerVersion = UseLargerVersionToolStripMenuItem.Checked

            ' Get the specific settings for each option
            If rbtnAccess.Checked Then
                .SelectedDB = rbtnAccess.Text
                .AccessPassword = txtPassword.Text
            ElseIf rbtnSQLServer.Checked Then
                .SelectedDB = rbtnSQLServer.Text
                .SQLConnectionString = txtServerName.Text
                .SQLPassword = txtPassword.Text
                .SQLUserName = txtUserName.Text
            ElseIf rbtnMySQL.Checked Then
                .SelectedDB = rbtnMySQL.Text
                .MySQLPassword = txtPassword.Text
                .MySQLConnectionString = txtServerName.Text
                .MySQLUserName = txtUserName.Text
                .mySQLPort = txtPort.Text
            ElseIf rbtnPostgreSQL.Checked Then
                .SelectedDB = rbtnPostgreSQL.Text
                .PostgreSQLPassword = txtPassword.Text
                .PostgreSQLConnectionString = txtServerName.Text
                .PostgreSQLUserName = txtUserName.Text
                .PostgreSQLPort = txtPort.Text
            ElseIf rbtnSQLiteDB.Checked Then
                .SelectedDB = rbtnSQLiteDB.Text
            ElseIf rbtnCSV.Checked Then
                .SelectedDB = rbtnCSV.Text
                .CSVEUCheck = chkEUFormat.Checked
            End If

            ' Language
            If rbtnEnglish.Checked Then
                .SelectedLanguage = rbtnEnglish.Text
            ElseIf rbtnGerman.Checked Then
                .SelectedLanguage = rbtnGerman.Text
            ElseIf rbtnFrench.Checked Then
                .SelectedLanguage = rbtnFrench.Text
            ElseIf rbtnJapanese.Checked Then
                .SelectedLanguage = rbtnJapanese.Text
            ElseIf rbtnRussian.Checked Then
                .SelectedLanguage = rbtnRussian.Text
            ElseIf rbtnChinese.Checked Then
                .SelectedLanguage = rbtnChinese.Text
            ElseIf rbtnKorean.Checked Then
                .SelectedLanguage = rbtnKorean.Text
            End If
        End With

        ' Save the settings
        Call AllSettings.SaveApplicationSettings(UserApplicationSettings)

        ' Save the grid checks now as a stream
        Dim MyStream As StreamWriter
        MyStream = File.CreateText(GridSettingsFileName)

        ' Loop through the grid and save what is checked - nothing fancy just a list of the file names in a text file
        For i = 0 To dgMain.RowCount - 1
            If dgMain.Rows(i).Cells(0).Value <> 0 Then
                MyStream.Write(dgMain.Rows(i).Cells(1).Value & Environment.NewLine)
            End If
        Next

        MyStream.Flush()
        MyStream.Close()

        If Not SupressMessage Then
            MsgBox("Settings Saved", vbInformation, Application.ProductName)
        End If

    End Sub

    ''' <summary>
    ''' Gets the data for file paths and other settings from a simple text file saved in local directory
    ''' </summary>
    Private Sub GetSettings()
        ' Read the settings file and lines
        Dim BPStream As StreamReader = Nothing
        Dim FieldType As String = ""
        Dim Language As String = ""
        Dim TempLanguage As String = ""

        UserApplicationSettings = AllSettings.LoadApplicationSettings

        With UserApplicationSettings
            txtDBName.Text = .DatabaseName
            lblFinalDBPath.Text = .FinalDBPath
            lblSDEPath.Text = .SDEDirectory
            lblDownloadFolderPath.Text = .DownloadFolderPath
            UseLargerVersionToolStripMenuItem.Checked = .UseLargerVersion

            ' Set the option
            Select Case .SelectedDB
                Case rbtnAccess.Text
                    rbtnAccess.Checked = True
                Case rbtnCSV.Text
                    rbtnCSV.Checked = True
                Case rbtnSQLiteDB.Text
                    rbtnSQLiteDB.Checked = True
                Case rbtnSQLServer.Text
                    rbtnSQLServer.Checked = True
                Case rbtnMySQL.Text
                    rbtnMySQL.Checked = True
                Case rbtnPostgreSQL.Text
                    rbtnPostgreSQL.Checked = True
            End Select

            Select Case .SelectedLanguage
                Case rbtnEnglish.Text
                    rbtnEnglish.Checked = True
                Case rbtnFrench.Text
                    rbtnFrench.Checked = True
                Case rbtnGerman.Text
                    rbtnGerman.Checked = True
                Case rbtnJapanese.Text
                    rbtnJapanese.Checked = True
                Case rbtnRussian.Text
                    rbtnRussian.Checked = True
                Case rbtnChinese.Text
                    rbtnChinese.Checked = True
                Case rbtnKorean.Text
                    rbtnKorean.Checked = True
                Case Else
                    rbtnEnglish.Checked = True
            End Select
        End With

        ' Now load all the settings based on that option
        Call LoadFormSettings()

    End Sub

    ''' <summary>
    ''' Gets the boxes checked for loading into grid from file
    ''' </summary>
    Private Sub GetGridSettings()
        ' Read the settings file and save all the files checked
        Dim BPStream As StreamReader
        CheckedFilesList = New List(Of String)

        Dim Line As String

        If File.Exists(GridSettingsFileName) Then
            BPStream = New StreamReader(GridSettingsFileName)

            Do
                Line = BPStream.ReadLine()
                If Not IsNothing(Line) Then
                    CheckedFilesList.Add(Line)
                End If
            Loop Until Line Is Nothing

            BPStream.Close()
        End If
    End Sub

    ''' <summary>
    ''' Looks up the settings file for threads and returns the number found
    ''' </summary>
    ''' <returns>Number of threads, if not found then returns -1</returns>
    Private Function GetThreadSetting() As Integer
        ' Read the settings file and save all the files checked
        Dim BPStream As StreamReader
        CheckedFilesList = New List(Of String)

        Dim Line As String

        If File.Exists(ThreadsFileName) Then
            BPStream = New StreamReader(ThreadsFileName)
            Line = BPStream.ReadLine()
            BPStream.Close()
            If Trim(Line) = "" Then
                Return -1
            Else
                Return CInt(Line)
            End If

        Else
            Return -1
        End If

    End Function

#End Region

#Region "Resizing"

    Public Const MaxSizeModifier As Double = 1.36363636363636
    Public Const MinFontSize As Double = 8.25
    Public Const MaxFontSize As Double = 11.25

    ''' <summary>
    ''' Resizes the form by 36.3636% and uses font size 8.25 to 11.25. 
    ''' </summary>
    Private Sub SetFormSize()

        If UseLargerVersionToolStripMenuItem.Checked Then
            Me.Size = Me.MaximumSize
            'Dim w As Integer = Math.Ceiling(TableLayoutPanelMain.Size.Width * MaxSizeModifier)
            'Dim h As Integer = Math.Ceiling(TableLayoutPanelMain.Size.Height * MaxSizeModifier)

            'TableLayoutPanelMain.Size = New Size(w, h)
            'TableLayoutPanelMain.Font = New Font(TableLayoutPanelMain.Font.Name, MaxFontSize)
            'TableLayoutPanelMain.Left = TableLayoutPanelMain.Size.Width / 2

            For Each Cntl In Me.Controls
                Call SetMaximumSize(Cntl)
            Next
        Else
            ' Use the minimums
            Me.Size = Me.MinimumSize
            For Each Cntl In Me.Controls
                Call SetMinimumSize(Cntl)
            Next
        End If

        Me.Refresh()

    End Sub

    Private Sub SetMaximumSize(ByVal ctl As Control)
        Dim w As Integer = Math.Ceiling(ctl.Size.Width * MaxSizeModifier)
        Dim h As Integer = Math.Ceiling(ctl.Size.Height * MaxSizeModifier)

        ctl.Size = New Size(w, h)
        ctl.Font = New Font(ctl.Font.Name, MaxFontSize)

        If ctl.HasChildren Then
            For Each c As Control In ctl.Controls
                SetMaximumSize(c)
            Next
        End If
    End Sub

    Private Sub SetMinimumSize(ByVal ctl As Control)
        ctl.Size = ctl.MinimumSize
        If ctl.Name <> "MenuStrip1" Then
            ctl.Font = New Font(ctl.Font.Name, MinFontSize)
        Else
            ctl.Font = New Font(ctl.Font.Name, 9)
        End If
        If ctl.HasChildren Then
            For Each c As Control In ctl.Controls
                SetMinimumSize(c)
            Next
        End If
    End Sub

    Private Sub SaveMinimumSize(ByVal ctl As Control)
        ctl.MinimumSize = ctl.Size
        If ctl.HasChildren Then
            For Each c As Control In ctl.Controls
                SaveMinimumSize(c)
            Next
        End If
    End Sub

#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Update files first unless dev 
        If Not File.Exists("Developer.txt") Then
            Call CheckForUpdates(False)
        End If

        FirstLoad = True

        ' Remove the 'Developer' menu if no developer file
        If Not File.Exists("Developer.txt") Then
            MenuStrip1.Items.Remove(DeveloperToolStripMenuItem)
        End If

        ' Get number of threads to use
        SelectedThreads = GetThreadSetting()

        ' Set the latest files folder path, which is one folder up from the root directory
        LatestFilesFolder = IO.Path.GetDirectoryName(Application.StartupPath) & "\Latest Files\"

        ' Add any initialization after the InitializeComponent() call.
        Call GetSettings()
        Call GetGridSettings()

        ' set a tool tip for the EU check box
        ToolTip1.SetToolTip(chkEUFormat, "Replaces commas with semicolons and all decimals with commas in a CSV file")

        ' Sets the CurrentCulture 
        Thread.CurrentThread.CurrentCulture = LocalCulture

        TestForSDEChanges = False

        ' Save the control minimums first so we can go back to the original size
        For Each Cntrl In Me.Controls
            Call SaveMinimumSize(Cntrl)
        Next

        ' Now set the form size based on the option selected
        Call SetFormSize()

        FirstLoad = False

    End Sub

    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' Load the file names in the grid
        Call LoadFileListtoGrid()
        FirstLoad = False
    End Sub

    Private Sub btnBuildDatabase_Click(sender As Object, e As EventArgs) Handles btnBuildDatabase.Click
        Dim FullDBPathName As String = UserApplicationSettings.FinalDBPath & "\" & UserApplicationSettings.DatabaseName
        Dim WasSuccessful As Boolean = False

        If Not ConductErrorChecks(True) Then
            Exit Sub
        End If

        ' Prep form
        CancelImport = False
        btnBuildDatabase.Enabled = False
        btnSaveSettings.Enabled = False
        gbSelectDBType.Enabled = False
        gbFilePathSelect.Enabled = False
        MenuStrip1.Enabled = False
        dgMain.ReadOnly = True
        btnClose.Enabled = False
        btnCancel.Enabled = True
        btnCancel.Focus()

        Dim TimeCheck As Date = Now

        With UserApplicationSettings
            ' Build the db based on selections
            If rbtnSQLiteDB.Checked Then ' SQLite

                Dim NewSQLiteDB As New SQLiteDB(FullDBPathName & ".sqlite", UserApplicationSettings.FinalDBPath, WasSuccessful)

                If WasSuccessful Then
                    Call NewSQLiteDB.BeginSQLiteTransaction()
                    Call BuildEVEDatabase(NewSQLiteDB, DatabaseType.SQLite)
                    Call NewSQLiteDB.CommitSQLiteTransaction()

                    ' Run a vacuum on the new DB to optimize and save space
                    Call NewSQLiteDB.ExecuteNonQuerySQL("VACUUM")
                    Call NewSQLiteDB.ExecuteNonQuerySQL("PRAGMA integrity_check")
                    Call NewSQLiteDB.CloseDB()
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnSQLServer.Checked Then ' Microsoft SQL Server

                Dim NewSQLServerDB As New msSQLDB(.DatabaseName, .SQLConnectionString, .SQLUserName, .SQLPassword, WasSuccessful)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewSQLServerDB, DatabaseType.SQLServer)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnAccess.Checked Then ' Microsoft Access

                Dim NewAccessDB As New msAccessDB(FullDBPathName & ".accdb", .AccessPassword, WasSuccessful)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewAccessDB, DatabaseType.MSAccess)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnCSV.Checked Then ' CSV

                Dim NewCSVDB As New CSVDB(FullDBPathName & "_CSV", WasSuccessful, False, False, chkEUFormat.Checked)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewCSVDB, DatabaseType.CSV)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnMySQL.Checked Then ' MySQL

                Dim NewMySQLDB As New MySQLDB(.DatabaseName, .MySQLConnectionString, .MySQLUserName, .MySQLPassword, Trim(txtPort.Text), WasSuccessful)

                If WasSuccessful Then
                    Call BuildEVEDatabase(NewMySQLDB, DatabaseType.MySQL)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnPostgreSQL.Checked Then ' postgreSQL

                Dim NewPostgreSQLDB As New postgreSQLDB(.DatabaseName, .PostgreSQLConnectionString, .PostgreSQLUserName, .PostgreSQLPassword, .PostgreSQLPort, WasSuccessful)

                If WasSuccessful Then
                    Call BuildEVEDatabase(NewPostgreSQLDB, DatabaseType.PostgreSQL)
                Else
                    GoTo ExitProc
                End If

            End If
        End With

        If CancelImport Then
            CancelImport = False
            Call ResetProgressColumn()
            Call MsgBox("Import Canceled", vbInformation, Application.ProductName)
        Else
            Dim Seconds As Integer = CInt(DateDiff(DateInterval.Second, TimeCheck, Now))
            Call MsgBox("Files Imported in: " & CInt(Seconds \ 60) & " min " & CInt(Seconds Mod 60) & " sec", vbInformation, Application.ProductName)
        End If

ExitProc:
        btnBuildDatabase.Enabled = True
        btnSaveSettings.Enabled = True
        gbSelectDBType.Enabled = True
        gbFilePathSelect.Enabled = True
        MenuStrip1.Enabled = True
        btnClose.Enabled = True
        btnCancel.Enabled = False
        dgMain.ReadOnly = False
        Call ClearMainProgressBar()
        btnBuildDatabase.Focus()

    End Sub

    ''' <summary>
    ''' Builds the EVE Database for the database type sent.
    ''' </summary>
    ''' <param name="UpdateDatabase">Database class to use for building database and import data into.</param>
    ''' <param name="DatabaseType">Type of Database class</param>
    Private Sub BuildEVEDatabase(UpdateDatabase As Object, DatabaseType As DatabaseType)
        ' Keeps an array of threads if we need to abort update
        Dim ThreadsArray As New List(Of ThreadList)
        Dim ImportFileList As New List(Of FileListItem)
        Dim Parameters As YAMLFilesBase.ImportParameters
        Dim WorkingDirectory As String = ""
        Dim IgnoreTables As New List(Of String)
        Dim ImportTranslationData As Boolean = False
        Dim Translator As YAMLTranslations = Nothing
        Dim TempThreadList As ThreadList = Nothing
        Dim AddThread As Boolean
        Dim CheckedTranslationTables As New List(Of String)

        ' For later update
        Dim UF As YAMLUniverse = Nothing

        ' Set up the importfile list
        ImportFileList = GetImportFileList(ImportTranslationData)

        ' Reset the third column so it updates properly
        Call ResetProgressColumn()

        lblStatus.Text = "Preparing files..."
        Application.DoEvents()

        ' Depending on the database, we may need to change the CSV directory to process later - also set if we import records in insert statements or bulk here
        If DatabaseType = DatabaseType.MSAccess Or DatabaseType = DatabaseType.PostgreSQL Then
            WorkingDirectory = UserApplicationSettings.SDEDirectory & "\" & "CSVtemp"
            UpdateDatabase.SetCSVDirectory(WorkingDirectory)
            Parameters.InsertRecords = False
        ElseIf DatabaseType = DatabaseType.MySQL Then
            WorkingDirectory = UpdateDatabase.GetCSVDirectory
            Parameters.InsertRecords = False
        Else
            WorkingDirectory = UserApplicationSettings.SDEDirectory & "\" & "CSVtemp"
            ' Create the working directory
            Call Directory.CreateDirectory(WorkingDirectory)
            Parameters.InsertRecords = True
        End If

        ' Set the language for all imports
        Parameters.ImportLanguageCode = GetImportLanguage()
        Parameters.ReturnList = False

        ' Only set up the translator if we are importing records
        If Parameters.InsertRecords Then

            ' Run translations before anything else if they selected files that require them for lookups or saving
            Translator = New YAMLTranslations(UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, UserApplicationSettings.SDEDirectory)

            ' If we are adding translation files, then import them first so the tables that have data in them already can be queried
            If ImportTranslationData Then

                If CheckedFilesList.Contains(YAMLTranslations.translationLanguagesFile) Then
                    Parameters.RowLocation = GetRowLocation(YAMLTranslations.translationLanguagesFile)
                    Call Translator.ImportTranslationLanguages(Parameters)
                Else
                    ' Don't update in the grid
                    Call Translator.ImportTranslationLanguages(Parameters, False)
                End If

            End If
        End If

        If CancelImport Then
            GoTo CancelImportProcessing
        End If

        lblStatus.Text = "Importing files..."
        Application.DoEvents()

        ' Now open threads for each of the checked files and import them
        For Each YAMLFile In ImportFileList
            With YAMLFile
                ' Set the row location
                Parameters.RowLocation = .RowLocation
                AddThread = True

                Select Case .FileName
                    Case YAMLagents.agentsFile
                        Dim Agents As New YAMLagents(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf Agents.ImportFile)
                    Case YAMLagtAgentTypes.agtAgentTypesFile
                        Dim AgentTypes As New YAMLagtAgentTypes(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf AgentTypes.ImportFile)
                    Case YAMLresearchAgents.researchAgentsFile
                        Dim ResearchAgents As New YAMLresearchAgents(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf ResearchAgents.ImportFile)
                    Case YAMLancestries.ancestriesFile
                        Dim CharAncestry As New YAMLancestries(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CharAncestry.ImportFile)
                    Case YAMLcharacterAttributes.charactersAttributesFile
                        Dim CharAttributes As New YAMLcharacterAttributes(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CharAttributes.ImportFile)
                    Case YAMLbloodLines.chrBloodlinesFile
                        Dim CharBloodlines As New YAMLbloodLines(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CharBloodlines.ImportFile)
                    Case YAMLfactions.factionsFile
                        Dim CharFactions As New YAMLfactions(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CharFactions.ImportFile)
                    Case YAMLraces.racesFile
                        Dim CharRaces As New YAMLraces(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CharRaces.ImportFile)
                    Case YAMLcorporationActivities.corporationActivitiesFile
                        Dim CorpActivites As New YAMLcorporationActivities(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CorpActivites.ImportFile)
                    Case YAMLnpcCorporationDivisions.npcCorporationDivisionsFile
                        Dim CorpCorporationDivisions As New YAMLnpcCorporationDivisions(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CorpCorporationDivisions.ImportFile)
                    Case YAMLnpcCorporations.npcCorporationsFile
                        Dim CorpCorporations As New YAMLnpcCorporations(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf CorpCorporations.ImportFile)
                    Case YAMLinvContrabandTypes.invContrabandTypesFile
                        Dim INVContrabandTypes As New YAMLinvContrabandTypes(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVContrabandTypes.ImportFile)
                    Case YAMLinvFlags.invFlagsFile
                        Dim INVFlags As New YAMLinvFlags(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVFlags.ImportFile)
                    Case YAMLinvItems.invItemsFile
                        Dim INVItems As New YAMLinvItems(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVItems.ImportFile)
                    Case YAMLinvNames.invNamesFile
                        Dim INVNames As New YAMLinvNames(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVNames.ImportFile)
                    Case YAMLinvPositions.invPositionsFile
                        Dim INVPositions As New YAMLinvPositions(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVPositions.ImportFile)
                    Case YAMLinvTypeReactions.invTypeReactionsFile
                        Dim INVTypeReactions As New YAMLinvTypeReactions(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVTypeReactions.ImportFile)
                    Case YAMLinvUniqueNames.invUniqueNamesFile
                        Dim INVUniqueNames As New YAMLinvUniqueNames(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVUniqueNames.ImportFile)
                    Case YAMLmapUniverse.mapUniverseFile
                        Dim MapUniverse As New YAMLmapUniverse(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf MapUniverse.ImportFile)
                    Case YAMLplanetSchematics.planetSchematicsFile
                        Dim PlanetSchematics As New YAMLplanetSchematics(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf PlanetSchematics.ImportFile)
                    Case YAMLplanetResources.planetResourcesFile
                        Dim PlanetResources As New YAMLplanetResources(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf PlanetResources.ImportFile)
                    Case YAMLsovereigntyUpgrades.sovereigntyUpgradesFile
                        Dim SovereigntyUpgrades As New YAMLsovereigntyUpgrades(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf SovereigntyUpgrades.ImportFile)
                    Case YAMLramActivities.ramActivitiesFile
                        Dim RAMActivities As New YAMLramActivities(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMActivities.ImportFile)
                    Case YAMLramAssemblyLineStations.ramAssemblyLineStationsFile
                        Dim RAMAssemblyStations As New YAMLramAssemblyLineStations(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMAssemblyStations.ImportFile)
                    Case YAMLramAssemblyLineTypeDetailPerCategory.ramAssemblyLineTypeDetailPerCategoryFile
                        Dim RAMassemblyLineCategories As New YAMLramAssemblyLineTypeDetailPerCategory(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMassemblyLineCategories.ImportFile)
                    Case YAMLramAssemblyLineTypeDetailPerGroup.ramAssemblyLineTypeDetailPerGroupFile
                        Dim RAMassemblyLineGroups As New YAMLramAssemblyLineTypeDetailPerGroup(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMassemblyLineGroups.ImportFile)
                    Case YAMLramAssemblyLineTypes.ramAssemblyLineTypesFile
                        Dim RAMAssemblyLineTypes As New YAMLramAssemblyLineTypes(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMAssemblyLineTypes.ImportFile)
                    Case YAMLramInstallationTypeContents.ramInstallationTypeContentsFile
                        Dim RAMInstallationType As New YAMLramInstallationTypeContents(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf RAMInstallationType.ImportFile)
                    Case YAMLstationOperations.stationOperationsFile
                        Dim StaOperations As New YAMLstationOperations(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf StaOperations.ImportFile)
                    Case YAMLstaOperationServices.staOperationServicesFile
                        Dim StaOperationServices As New YAMLstaOperationServices(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf StaOperationServices.ImportFile)
                    Case YAMLstationServices.stationServicesFile
                        Dim StaServies As New YAMLstationServices(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf StaServies.ImportFile)
                    Case YAMLstaStations.staStationsFile
                        Dim StaStations As New YAMLstaStations(.FileName, UserApplicationSettings.SDEDirectory & BSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf StaStations.ImportFile)
                    Case YAMLagentsinSpace.agentsinSpaceFile
                        Dim AgentsInSpace As New YAMLagentsinSpace(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf AgentsInSpace.ImportFile)
                    Case YAMLblueprints.blueprintsFile
                        Dim BPs As New YAMLblueprints(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf BPs.ImportFile)
                    Case YAMLcategories.categoryiesFile
                        Dim Categories As New YAMLcategories(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf Categories.ImportFile)
                    Case YAMLcertificates.certificatesFile
                        Dim Certificates As New YAMLcertificates(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf Certificates.ImportFile)
                    Case YAMLcontrabandTypes.contrabandTypesFile
                        Dim ContrabandTypes As New YAMLcontrabandTypes(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf ContrabandTypes.ImportFile)
                    Case YAMLcontrolTowerResources.controlTowerResourcesFile
                        Dim INVControlTowerResourcePurposes As New YAMLcontrolTowerResources(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf INVControlTowerResourcePurposes.ImportFile)
                    Case YAMLdogmaAttributeTypes.dogmaAttributeTypesFile
                        Dim DGMAttributeTypes As New YAMLdogmaAttributeTypes(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf DGMAttributeTypes.ImportFile)
                    Case YAMLdogmaEffects.dogmaEffectsFile
                        Dim DGMEffects As New YAMLdogmaEffects(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf DGMEffects.ImportFile)
                    Case YAMLdogmaAttributeCategories.dogmaAttributeCategoriesFile
                        Dim DGMAttributeCategories As New YAMLdogmaAttributeCategories(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf DGMAttributeCategories.ImportFile)
                    Case YAMLeveGrpahics.eveGraphicsFile
                        Dim EVEGraphics As New YAMLeveGrpahics(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf EVEGraphics.ImportFile)
                    Case YAMLeveIcons.eveIconsFile
                        Dim EVEIcons As New YAMLeveIcons(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf EVEIcons.ImportFile)
                    Case YAMLskinLicenses.skinLicensesFile
                        Dim SkinLiscences As New YAMLskinLicenses(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf SkinLiscences.ImportFile)
                    Case YAMLskinMaterials.skinMaterialsFile
                        Dim SkinMats As New YAMLskinMaterials(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf SkinMats.ImportFile)
                    Case YAMLskins.skinsFile
                        Dim Skins As New YAMLskins(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf Skins.ImportFile)
                    Case YAMLtournamentRuleSets.tournamentRuleSetsFile
                        Dim TRS As New YAMLtournamentRuleSets(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf TRS.ImportFile)
                    Case YAMLtypeDogma.typeDogmaFile
                        Dim DGMTypeAttributes As New YAMLtypeDogma(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf DGMTypeAttributes.ImportFile)
                    Case YAMLtypes.typesFile
                        Dim TIDs As New YAMLtypes(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf TIDs.ImportFile)
                    Case YAMLgroups.groupsFile
                        Dim Groups As New YAMLgroups(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf Groups.ImportFile)
                    Case YAMLmarketGroups.marketGroupsFile
                        Dim MaG As New YAMLmarketGroups(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf MaG.ImportFile)
                    Case YAMLmetaGroups.metaGroupsFile
                        Dim MeG As New YAMLmetaGroups(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf MeG.ImportFile)
                    Case YAMLtypeMaterials.typeMaterialsFile
                        Dim typeMaterials As New YAMLtypeMaterials(.FileName, UserApplicationSettings.SDEDirectory & FSDPath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf typeMaterials.ImportFile)
                    Case YAMLUniverse.UniverseFiles
                        UF = New YAMLUniverse("", UserApplicationSettings.SDEDirectory & EVEUniversePath, UpdateDatabase, Translator)
                        TempThreadList.T = New Thread(AddressOf UF.ImportFile)
                    Case YAMLTranslations.translationLanguagesFile
                        ' They checked this so, copy for later import
                        Call CheckedTranslationTables.Add(YAMLTranslations.trnTranslationLanguagesTable)
                        If Not ImportTranslationData Then
                            TempThreadList.T = New Thread(AddressOf Translator.ImportTranslationLanguages)
                        Else
                            AddThread = False
                        End If
                End Select

                If AddThread Then
                    TempThreadList.T.Name = .FileName
                    TempThreadList.Params = Parameters
                    Call ThreadsArray.Add(TempThreadList)
                End If

            End With
        Next

        ' Run the threads based on the number of threads the user wants
        If SelectedThreads = -1 Then
            ' Max threads, just run them all now
            For i = 0 To ThreadsArray.Count - 1
                Call ImportFile(ThreadsArray(i).T, ThreadsArray(i).Params)
            Next
        Else
            Dim ThreadStarted As Boolean = False
            Dim ActiveThreads As Integer = 0
            ' Run only as many threads as they have chosen until done
            For i = 0 To ThreadsArray.Count - 1
                Do ' keep running this loop until the thread starts
                    ' See how many threads are active
                    ActiveThreads = 0
                    For Each Th In ThreadsArray
                        If Th.T.IsAlive Then
                            ActiveThreads += 1
                        End If
                    Next

                    ' Only run if we haven't gone over the max threads they wanted to run yet
                    If ActiveThreads <= SelectedThreads Then
                        Call ImportFile(ThreadsArray(i).T, ThreadsArray(i).Params)
                        ThreadStarted = True
                    Else
                        ThreadStarted = False ' Wait till a thread opens up
                    End If
                    Application.DoEvents()

                    If CancelImport Then
                        GoTo CancelImportProcessing
                    End If

                Loop Until ThreadStarted
            Next

        End If

        ' Now wait until all threads finish
        Do While Not ThreadsComplete(ThreadsArray)
            If CancelImport Then
                GoTo CancelImportProcessing
            End If
            Application.DoEvents()
        Loop

        ' Kill any remaining threads
        Call KillThreads(ThreadsArray)
        CSVImport = False

        Select Case DatabaseType
            Case DatabaseType.MSAccess, DatabaseType.PostgreSQL
                Dim NewCSVDB As CSVDB

                If DatabaseType = DatabaseType.MSAccess Then
                    NewCSVDB = New CSVDB(WorkingDirectory, Nothing, True)
                Else
                    NewCSVDB = New CSVDB(WorkingDirectory, Nothing, False)
                End If

                Call BuildEVEDatabase(NewCSVDB, DatabaseType.CSV)

                ' We are importing the data for these databases from CSV files, so clean up after we are complete
                CSVImport = True

            Case DatabaseType.MySQL
                Dim NewCSVDB As New CSVDB(WorkingDirectory, Nothing, True)
                Call BuildEVEDatabase(NewCSVDB, DatabaseType.CSV)
                ' MySQL has a fixed Upload directory, so don't delete it
                CSVImport = False
        End Select

        lblStatus.Text = "Finalizing import..."
        Me.Cursor = Cursors.WaitCursor
        Application.DoEvents()

        ' Finalize
        If Not CancelImport Then
            ' Finalize
            UpdateDatabase.FinalizeDataImport(Translator, CheckedTranslationTables)
            ' Close translator
            If Not IsNothing(Translator) Then
                Call Translator.Close()
            End If

            ' Clean up temp directory if we used CSV imports
            If Directory.Exists(WorkingDirectory) And CSVImport Then
                Directory.Delete(WorkingDirectory, True)
            End If

        End If

CancelImportProcessing:

        On Error Resume Next
        Call KillThreads(ThreadsArray)
        Call ResetProgressColumn()
        Call Translator.Close()
        Call UF.Close()
        lblStatus.Text = ""
        Me.Cursor = Cursors.Default
        Application.DoEvents()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        On Error GoTo 0

    End Sub

    ''' <summary>
    ''' Returns the import language we are using based on the radio buttons selected
    ''' </summary>
    ''' <returns>The LanguageCode of the radio button selected</returns>
    Private Function GetImportLanguage() As LanguageCode
        If rbtnEnglish.Checked Then
            Return LanguageCode.English
        ElseIf rbtnGerman.Checked Then
            Return LanguageCode.German
        ElseIf rbtnFrench.Checked Then
            Return LanguageCode.French
        ElseIf rbtnJapanese.Checked Then
            Return LanguageCode.Japanese
        ElseIf rbtnRussian.Checked Then
            Return LanguageCode.Russian
        ElseIf rbtnChinese.Checked Then
            Return LanguageCode.Chinese
        ElseIf rbtnKorean.Checked Then
            Return LanguageCode.Korean
        Else
            Return LanguageCode.English
        End If
    End Function

    ''' <summary>
    ''' A list of hard coded table names that require translation table input
    ''' </summary>
    ''' <returns>List of table names requiring translation table input</returns>
    Private Function GetRequiredTablesForTranslations() As List(Of String)
        ' TempList.Add("eveUnits.yaml")
        Dim TempList As New List(Of String) From {
            "ancestries.yaml",
            "bloodlines.yaml",
            "categories.yaml",
            "characterAttributes.yaml",
            "corporationActivities.yaml",
            "dogmaAttributes.yaml",
            "dogmaEffects.yaml",
            "factions.yaml",
            "groups.yaml",
            "invCategories.yaml",
            "landmarks.yaml",
            "marketGroups.yaml",
            "metaGroups.yaml",
            "npcCorporationDivisions.yaml",
            "npcCorporations.yaml",
            "planetSchematics.yaml",
            "races.yaml",
            "ramActivities.yaml",
            "stationOperations.yaml",
            "stationServices.yaml",
            "types.yaml"
        }

        Return TempList
    End Function

    ''' <summary>
    ''' Conducts error checks and returns boolean to determine if they pass
    ''' </summary>
    ''' <param name="CheckFileSelection">Boolean to check selection of files in the table.</param>
    ''' <returns>Boolean on whether the checks passed</returns>
    Private Function ConductErrorChecks(CheckFileSelection As Boolean) As Boolean

        ' Error / Data checks
        If CheckedFilesList.Count = 0 And CheckFileSelection Then
            Call MsgBox("No files selected for import", vbInformation, Application.ProductName)
            Return False
        End If

        If Trim(txtDBName.Text) = "" Then
            Call MsgBox("You must select a databasename", vbInformation, Application.ProductName)
            txtDBName.Focus()
            Return False
        End If

        If Trim(lblSDEPath.Text) = "" Then
            Call MsgBox("You must select a path for the SDE YAML files.", vbInformation, Application.ProductName)
            btnSelectSDEPath.Focus()
            Return False
        End If

        ' Do error checks based on the selections
        If rbtnAccess.Checked Or rbtnSQLiteDB.Checked Or rbtnCSV.Checked Then
            If Trim(lblFinalDBPath.Text) = "" Then
                Call MsgBox("You must select a final database path.", vbInformation, Application.ProductName)
                Return False
            End If
        End If

        If rbtnSQLServer.Checked Or rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then
            ' Check server name
            If Trim(txtServerName.Text) = "" Then
                Call MsgBox("You must select a server name", vbInformation, Application.ProductName)
                txtServerName.Focus()
                Return False
            End If
        End If

        If rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then
            ' Check user name
            If Trim(txtUserName.Text) = "" Then
                Call MsgBox("You must select a user name", vbInformation, Application.ProductName)
                txtUserName.Focus()
                Return False
            End If
        End If

        If rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then ' Access and sqlserver password can be blank
            ' Check password
            If Trim(txtPassword.Text) = "" Then
                Call MsgBox("You must select a password", vbInformation, Application.ProductName)
                txtPassword.Focus()
                Return False
            End If
        End If

        If rbtnPostgreSQL.Checked Then
            ' Check port - MySQL port can use 3306 as default if not set
            If Trim(txtPort.Text) = "" Then
                Call MsgBox("You must select a port number", vbInformation, Application.ProductName)
                txtPort.Focus()
                Return False
            End If
        End If

        Return True

    End Function

#Region "UpdaterFunctions"

    ''' <summary>
    ''' Checks for program file updates and prompts user to continue
    ''' </summary>
    Public Sub CheckForUpdates(ByVal ShowUpdateMessage As Boolean)
        Dim Response As DialogResult
        ' Program Updater
        Dim Updater As New ProgramUpdater
        Dim UpdateCode As UpdateCheckResult

        ' 1 = Update Available, 0 No Update Available, -1 an error occured and msg box already shown
        UpdateCode = Updater.IsProgramUpdatable

        Select Case UpdateCode
            Case UpdateCheckResult.UpdateAvailable

                Response = MsgBox("Update Available - Do you want to update now?", MessageBoxButtons.YesNo, Application.ProductName)

                If Response = DialogResult.Yes Then
                    ' Run the updater
                    Application.UseWaitCursor = True
                    Call Updater.RunUpdate()
                    Application.UseWaitCursor = False
                End If
            Case UpdateCheckResult.UpToDate
                If ShowUpdateMessage Then
                    MsgBox("No updates available.", vbInformation, Application.ProductName)
                End If
            Case UpdateCheckResult.UpdateError
                MsgBox("Unable to run update at this time. Please try again later.", vbInformation, Application.ProductName)
        End Select

        ' Clean up files used to check
        Call Updater.CleanUpFiles()

    End Sub

    Private Sub PrepareFilesForUpdateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PrepareFilesForUpdateToolStripMenuItem.Click
        Call CopyFilesBuildXML()
    End Sub

    Private Sub BuildBinaryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuildBinaryToolStripMenuItem.Click
        Call BuildBinaryFile()
    End Sub

    ''' <summary>
    ''' Copies all the files from directories and then builds the xml file and saves it here for upload to github
    ''' </summary>
    Private Sub CopyFilesBuildXML()
        Dim NewFilesAdded As Boolean = False
        Dim Updater As New ProgramUpdater

        On Error Resume Next
        Me.Cursor = Cursors.WaitCursor
        Application.DoEvents()

        If Updater.MD5CalcFile(MainEXEFile) <> Updater.MD5CalcFile(LatestFilesFolder & MainEXEFile) Then
            File.Copy(MainEXEFile, LatestFilesFolder & MainEXEFile, True)
            NewFilesAdded = True
        End If

        If Updater.MD5CalcFile(UpdaterEXEFile) <> Updater.MD5CalcFile(LatestFilesFolder & UpdaterEXEFile) Then
            File.Copy(UpdaterEXEFile, LatestFilesFolder & UpdaterEXEFile, True)
            NewFilesAdded = True
        End If

        If Updater.MD5CalcFile(MainEXEConfig) <> Updater.MD5CalcFile(LatestFilesFolder & MainEXEConfig) Then
            File.Copy(MainEXEConfig, LatestFilesFolder & MainEXEConfig, True)
            NewFilesAdded = True
        End If

        If Updater.MD5CalcFile(UpdaterEXEConfig) <> Updater.MD5CalcFile(LatestFilesFolder & UpdaterEXEConfig) Then
            File.Copy(UpdaterEXEConfig, LatestFilesFolder & UpdaterEXEConfig, True)
            NewFilesAdded = True
        End If

        If Updater.MD5CalcFile(SQLiteInteropDLL86) <> Updater.MD5CalcFile(LatestFilesFolder & SQLiteInteropDLL86) Then
            If Not Directory.Exists(LatestFilesFolder & "\x86") Then
                Directory.CreateDirectory(LatestFilesFolder & "\x86")
            End If
            File.Copy(SQLiteInteropDLL86, LatestFilesFolder & SQLiteInteropDLL86, True)
            NewFilesAdded = True
        End If

        If Updater.MD5CalcFile(SQLiteInteropDLL64) <> Updater.MD5CalcFile(LatestFilesFolder & SQLiteInteropDLL64) Then
            If Not Directory.Exists(LatestFilesFolder & "\x64") Then
                Directory.CreateDirectory(LatestFilesFolder & "\x64")
            End If
            File.Copy(SQLiteInteropDLL64, LatestFilesFolder & SQLiteInteropDLL64, True)
            NewFilesAdded = True
        End If

        'Copy all the remaining DLLs in the folder if they are updated
        Dim DLLFiles As String() = Directory.GetFiles(Environment.CurrentDirectory, "*.dll")
        Dim FileName As String

        For Each DLL In DLLFiles
            FileName = System.IO.Path.GetFileName(DLL)
            If Updater.MD5CalcFile(FileName) <> Updater.MD5CalcFile(LatestFilesFolder & FileName) Then
                File.Copy(FileName, LatestFilesFolder & FileName, True)
                NewFilesAdded = True
            End If
        Next

        On Error GoTo 0

        ' Output the Latest XML File if we have updates
        If NewFilesAdded Then
            Call WriteLatestXMLFile()
        End If

        Me.Cursor = Cursors.Default
        Application.DoEvents()

        MsgBox("Files Deployed, upload to Github for user download.", vbInformation, "Complete")

    End Sub

    ''' <summary>
    ''' Writes the sent settings to the final update file name
    ''' </summary>
    Private Sub WriteLatestXMLFile()
        Dim VersionNumber As String = String.Format("Version {0}", My.Application.Info.Version.ToString)
        Dim Updater As New ProgramUpdater

        ' Create XmlWriterSettings.
        Dim XMLSettings As New XmlWriterSettings With {
            .Indent = True
        }

        ' Delete the current latestversion file to rebuild
        File.Delete(LatestVersionXML)

        ' Loop through the settings sent and output each name and value
        Using writer As XmlWriter = XmlWriter.Create(LatestVersionXML, XMLSettings)
            writer.WriteStartDocument()
            writer.WriteStartElement("EVESDEDB") ' Root.
            writer.WriteAttributeString("Version", VersionNumber)
            writer.WriteStartElement("LastUpdated")
            writer.WriteString(CStr(Now))
            writer.WriteEndElement()

            writer.WriteStartElement("result")
            writer.WriteStartElement("rowset")
            writer.WriteAttributeString("name", "filelist")
            writer.WriteAttributeString("key", "version")
            writer.WriteAttributeString("columns", "Name,Version,MD5,URL")

            ' Main EXE program
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", MainEXEFile)
            writer.WriteAttributeString("Version", VersionNumber)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & MainEXEFile))
            writer.WriteAttributeString("URL", MainEXEFileURL)
            writer.WriteEndElement()

            ' Updater EXE
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", UpdaterEXEFile)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(UpdaterEXEFile).FileVersion)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & UpdaterEXEFile))
            writer.WriteAttributeString("URL", UpdaterEXEFileURL)
            writer.WriteEndElement()

            ' Main EXE program config file
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", MainEXEConfig)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(UpdaterEXEFile).FileVersion)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & MainEXEConfig))
            writer.WriteAttributeString("URL", MainEXEConfigURL)
            writer.WriteEndElement()

            ' Updater EXE config file
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", UpdaterEXEConfig)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(UpdaterEXEConfig).FileVersion)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & UpdaterEXEConfig))
            writer.WriteAttributeString("URL", UpdaterEXEConfigURL)
            writer.WriteEndElement()

            ' Add all the dlls 
            Dim DLLFiles As String() = Directory.GetFiles(LatestFilesFolder, "*.dll")
            Dim FileName As String
            For Each DLL In DLLFiles
                FileName = System.IO.Path.GetFileName(DLL)
                writer.WriteStartElement("row")
                writer.WriteAttributeString("Name", FileName)
                writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(FileName).FileVersion)
                writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & FileName))
                writer.WriteAttributeString("URL", "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/" & FileName)
                writer.WriteEndElement()
            Next

            ' Add the 32 and 64 bit SQLite DLLs
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", SQLiteInteropDLL86)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(SQLiteInteropDLL86).FileVersion)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & SQLiteInteropDLL86))
            writer.WriteAttributeString("URL", SQLiteInteropURL86)
            writer.WriteEndElement()

            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", SQLiteInteropDLL64)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(SQLiteInteropDLL64).FileVersion)
            writer.WriteAttributeString("MD5", Updater.MD5CalcFile(LatestFilesFolder & SQLiteInteropDLL64))
            writer.WriteAttributeString("URL", SQLiteInteropURL64)
            writer.WriteEndElement()

            ' End document.
            writer.WriteEndDocument()
        End Using

        ' Finally, replace all the update file's crlf with lf so that when it's uploaded to git, it works properly on download
        Dim FileText As String = File.ReadAllText(LatestVersionXML)
        FileText = FileText.Replace(vbCrLf, Chr(10))

        ' Write the file back out with new formatting
        File.WriteAllText(LatestVersionXML, FileText)
        File.WriteAllText(LatestFilesFolder & LatestVersionXML, FileText)

    End Sub

    ''' <summary>
    ''' Builds the binary file for downloading and installing the program to run
    ''' </summary>
    Private Sub BuildBinaryFile()
        ' Build this in the working directory
        Dim FinalBinaryFolderPath As String = LatestFilesFolder & "Temp\"
        Dim FinalBinaryZipPath As String = LatestFilesFolder
        Dim FinalBinaryZip As String = "EVE SDE Database Builder Install.zip"

        Application.UseWaitCursor = True
        Application.DoEvents()

        ' Make folder to put files in and zip
        If Directory.Exists(FinalBinaryFolderPath) Then
            Directory.Delete(FinalBinaryFolderPath, True)
        End If

        Directory.CreateDirectory(FinalBinaryFolderPath)

        ' Copy all these files from the media file directory (should be most up to date) to the working directory to make the zip
        File.Copy(LatestFilesFolder & LatestVersionXML, FinalBinaryFolderPath & LatestVersionXML)
        File.Copy(LatestFilesFolder & MainEXEFile, FinalBinaryFolderPath & MainEXEFile)
        File.Copy(LatestFilesFolder & UpdaterEXEFile, FinalBinaryFolderPath & UpdaterEXEFile)
        File.Copy(LatestFilesFolder & MainEXEConfig, FinalBinaryFolderPath & MainEXEConfig)
        File.Copy(LatestFilesFolder & UpdaterEXEConfig, FinalBinaryFolderPath & UpdaterEXEConfig)

        Dim DLLFiles As String() = Directory.GetFiles(LatestFilesFolder, "*.dll")
        Dim FileName As String
        For Each DLL In DLLFiles
            FileName = System.IO.Path.GetFileName(DLL)
            File.Copy(LatestFilesFolder & FileName, FinalBinaryFolderPath & FileName)
        Next

        If Not Directory.Exists(FinalBinaryFolderPath & "\x86") Then
            Directory.CreateDirectory(FinalBinaryFolderPath & "\x86")
        End If
        File.Copy(LatestFilesFolder & SQLiteInteropDLL86, FinalBinaryFolderPath & SQLiteInteropDLL86)

        If Not Directory.Exists(FinalBinaryFolderPath & "\x64") Then
            Directory.CreateDirectory(FinalBinaryFolderPath & "\x64")
        End If
        File.Copy(LatestFilesFolder & SQLiteInteropDLL64, FinalBinaryFolderPath & SQLiteInteropDLL64)

        ' Delete the file if it already exists
        File.Delete(FinalBinaryZipPath & FinalBinaryZip)
        ' Compress the whole file for download
        Call ZipFile.CreateFromDirectory(FinalBinaryFolderPath, FinalBinaryZipPath & FinalBinaryZip, CompressionLevel.Optimal, False)

        Application.UseWaitCursor = False
        Application.DoEvents()

        ' Clean up working folder
        If Directory.Exists(FinalBinaryFolderPath) Then
            Directory.Delete(FinalBinaryFolderPath, True)
        End If

        Application.DoEvents()

        MsgBox("Binary Built", vbInformation, "Complete")

    End Sub

#End Region

#Region "Grid and file list functions"

    ''' <summary>
    ''' Imports the file names for processing from the grid 
    ''' </summary>
    ''' <returns>List of files to process</returns>
    Private Function GetImportFileList(ByRef ImportTranslationData As Boolean) As List(Of FileListItem)
        Dim TempFileListItem As FileListItem
        Dim TempFileList As New List(Of FileListItem)
        Dim AddUniverseFiles As Boolean = False

        ' Build the list of tables that will require Translation Table data
        Dim FileListRequiringTranslationTables As List(Of String) = GetRequiredTablesForTranslations()

        If UserApplicationSettings.SDEDirectory <> "" Then
            ' First load all that are checked and sort by size decending - universe is always biggest at top (add at end)
            Dim DI As New DirectoryInfo(UserApplicationSettings.SDEDirectory & BSDPath)
            ' Get a reference to each file in that directory.
            Dim FilesList As FileInfo() = DI.GetFiles()

            For i = 0 To FilesList.Count - 1
                ' If it's a checked file, add it to the list
                If CheckedFilesList.Contains(FilesList(i).Name) Then
                    TempFileListItem.FileName = FilesList(i).Name
                    TempFileListItem.RowLocation = GetRowLocation(FilesList(i).Name)
                    ' Set the translation data flag here as we go through the bsd files
                    If FileListRequiringTranslationTables.Contains(FilesList(i).Name) Then
                        ImportTranslationData = True
                    End If
                    Call TempFileList.Add(TempFileListItem)
                End If
            Next

            ' Now FSD files
            DI = New DirectoryInfo(UserApplicationSettings.SDEDirectory & FSDPath)
            ' Get a reference to each file in that directory.
            FilesList = DI.GetFiles()

            For i = 0 To FilesList.Count - 1
                ' If it's a checked file, add it to the list
                If CheckedFilesList.Contains(FilesList(i).Name) Then
                    TempFileListItem.FileName = FilesList(i).Name
                    TempFileListItem.RowLocation = GetRowLocation(FilesList(i).Name)
                    ' Set the translation data flag here as we go through the fsd files
                    If FileListRequiringTranslationTables.Contains(FilesList(i).Name) Then
                        ImportTranslationData = True
                    End If
                    Call TempFileList.Add(TempFileListItem)
                End If
            Next

            '' Landmarks
            'DI = New DirectoryInfo(UserApplicationSettings.SDEDirectory & FSDLandMarksPath)
            '' Get a reference to each file in that directory.
            'FilesList = DI.GetFiles()

            'For i = 0 To FilesList.Count - 1
            '    ' If it's a checked file, add it to the list
            '    If CheckedFilesList.Contains(FilesList(i).Name) Then
            '        TempFileListItem.FileName = FilesList(i).Name
            '        TempFileListItem.RowLocation = GetRowLocation(FilesList(i).Name)
            '        ' Set the translation data flag here as we go through the landmark files
            '        If FileListRequiringTranslationTables.Contains(FilesList(i).Name) Then
            '            ImportTranslationData = True
            '        End If
            '        Call TempFileList.Add(TempFileListItem)
            '    End If
            'Next
        End If

        ' If selected, add the universe files to the top, which should be the largest to process
        If AddUniverseFiles Or CheckedFilesList.Contains(YAMLUniverse.UniverseFiles) Then
            TempFileListItem.FileName = YAMLUniverse.UniverseFiles
            TempFileListItem.RowLocation = GetRowLocation(YAMLUniverse.UniverseFiles)
            TempFileList.Insert(0, TempFileListItem)
            ImportTranslationData = True
        End If

        Return TempFileList

    End Function

    ''' <summary>
    ''' Loads the file names into the list from the SDE Directory
    ''' </summary>
    ''' <returns>Returns boolean if was able to load the list grid or not.</returns>
    Private Function LoadFileListtoGrid() As Boolean
        Dim Counter As Long = 1 ' Start at 1 since we are adding universe files manually
        Dim TotalFileList As New List(Of GridFileItem)
        Dim TempFile As New GridFileItem

        dgMain.Rows.Clear()

        If UserApplicationSettings.SDEDirectory <> "" Then
            Try
                Dim BSD_DI As New DirectoryInfo(UserApplicationSettings.SDEDirectory & BSDPath)
                Dim BSD_FilesList As FileInfo() = BSD_DI.GetFiles()

                For Each YAMLBSDFile In BSD_FilesList
                    If YAMLBSDFile.Name.Substring(0, 3) <> "dgm" Then ' Ignore the old dogma files
                        TempFile.FileName = YAMLBSDFile.Name
                        TempFile.Checked = GetGridCheckValue(YAMLBSDFile.Name)
                        TotalFileList.Add(TempFile)
                    End If
                Next

            Catch ex As Exception
                Call ShowErrorMessage(ex)
            End Try

            Try
                Dim FSD_DI As New DirectoryInfo(UserApplicationSettings.SDEDirectory & FSDPath)
                Dim FSD_FilesList As FileInfo() = FSD_DI.GetFiles()

                For Each YAMLFSDFile In FSD_FilesList
                    TempFile.FileName = YAMLFSDFile.Name
                    TempFile.Checked = GetGridCheckValue(YAMLFSDFile.Name)
                    TotalFileList.Add(TempFile)
                Next
            Catch ex As Exception
                Call ShowErrorMessage(ex)
            End Try

            'Try
            '    Dim LM_FSD_DI As New DirectoryInfo(UserApplicationSettings.SDEDirectory & FSDLandMarksPath)
            '    Dim LM_FSD_FilesList As FileInfo() = LM_FSD_DI.GetFiles()

            '    For Each YAMLLMFSDFile In LM_FSD_FilesList
            '        TempFile.FileName = YAMLLMFSDFile.Name
            '        TempFile.Checked = GetGridCheckValue(YAMLLMFSDFile.Name)
            '        TotalFileList.Add(TempFile)
            '    Next
            'Catch ex As Exception
            '    Call ShowErrorMessage(ex)
            'End Try

        End If

        If TotalFileList.Count > 0 Then
            ' Sort the file list by name
            TotalFileList.Sort(New GridFileItemComparer)

            ' Set the rows in the grid
            dgMain.RowCount = TotalFileList.Count + 1 ' Add 1 for Universe Data

            ' Add universe data manually
            dgMain.Rows(0).Cells(0).Value = GetGridCheckValue(YAMLUniverse.UniverseFiles)
            dgMain.Rows(0).Cells(1).Value = YAMLUniverse.UniverseFiles

            For Each YAMLFile In TotalFileList
                ' Add the name and a blank cell to the grid - check each one
                dgMain.Rows(Counter).Cells(0).Value = GetGridCheckValue(YAMLFile.FileName)
                dgMain.Rows(Counter).Cells(1).Value = YAMLFile.FileName
                Counter += 1
            Next
        End If
        Application.DoEvents()

        Return True

    End Function

    ''' <summary>
    ''' Returns an integer value to determine if the row is checked in the grid with the file name given
    ''' </summary>
    ''' <param name="FileName">Filename to search the grid for a check</param>
    ''' <returns></returns>
    Private Function GetGridCheckValue(FileName As String) As Integer
        If CheckedFilesList.Contains(FileName) Then
            Return 1
        Else
            Return 0
        End If
    End Function

#End Region

#Region "Thread Functions"

    ''' <summary>
    ''' Function to start a thread passed and return a ref to the thread
    ''' </summary>
    ''' <param name="T">Thread variable</param>
    ''' <param name="Params">Import Paramaters</param>
    Private Sub ImportFile(T As Thread, Params As YAMLFilesBase.ImportParameters)

        T.Start(Params)

    End Sub

    ''' <summary>
    ''' Checks to see if any threads are still open
    ''' </summary>
    ''' <param name="Threads"></param>
    ''' <returns>Returns true if threads are still open, false otherwise</returns>
    Private Function ThreadsComplete(Threads As List(Of ThreadList)) As Boolean
        Dim AllComplete As Boolean = True

        For Each Th In Threads
            If Th.T.IsAlive Then
                AllComplete = False
                Return AllComplete
            Else
                Call Th.T.Abort()
                Th.T = Nothing
            End If
        Next

        Return AllComplete
    End Function

    ''' <summary>
    ''' Aborts all threads in the Thread Array (public varaible)
    ''' </summary>
    Private Sub KillThreads(ByRef ListofThreads As List(Of ThreadList))
        ' Kill all the threads
        On Error Resume Next
        For i = 0 To ListofThreads.Count - 1
            If ListofThreads(i).T.IsAlive Then
                ListofThreads(i).T.Abort()
            End If
            Application.DoEvents()
        Next
        ListofThreads = Nothing
        On Error GoTo 0
    End Sub

#End Region

#Region "Row progress update functions"

    ''' <summary>
    ''' Resets the 3rd column (index 2) in the grid for showing progress bars
    ''' </summary>
    Private Sub ResetProgressColumn()
        ' Reset the grid progress
        ' Add the progress column
        Dim PColumn As New ProgressColumn With {
            .Name = "Progress"
        }

        If dgMain.Columns.Count = 3 Then
            dgMain.Columns.Remove("Progress")
        End If

        dgMain.Columns.Add(PColumn)
        dgMain.Columns(2).Width = 255

    End Sub

    ''' <summary>
    ''' Looks up the row that has the file name and returns the row number
    ''' </summary>
    ''' <param name="FileName">Filename to search in the grid</param>
    ''' <returns>Row number</returns>
    Private Function GetRowLocation(FileName As String) As Integer
        For i = 0 To dgMain.RowCount - 1
            If dgMain.Rows(i).Cells(1).Value = FileName Then
                Return i
            End If
        Next

        Return 0
    End Function

    ''' <summary>
    ''' Initializes the grid row sent
    ''' </summary>
    ''' <param name="Postion">Grid row</param>
    Public Sub InitGridRow(ByVal Postion As Integer)
        dgMain.Rows(Postion).Cells(2).Value = 0
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Updates the grid row as a percentage for the progress bar
    ''' </summary>
    ''' <param name="Postion">Row number to update</param>
    ''' <param name="Count">Current record count</param>
    ''' <param name="TotalRecords">Total records to process</param>
    Public Sub UpdateGridRowProgress(ByVal Postion As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)
        dgMain.Rows(Postion).Cells(2).Value = CInt(Math.Floor(Count / TotalRecords * 100))
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Finalizes the grid row by setting it to 100
    ''' </summary>
    ''' <param name="Postion">Row number</param>
    Public Sub FinalizeGridRow(ByVal Postion As Integer)
        dgMain.Rows(Postion).Cells(2).Value = 100
        Application.DoEvents()
    End Sub

#End Region

#Region "Update Progress Bar on main form"

    ''' <summary>
    ''' Initializes the progress bar on the main form
    ''' </summary>
    ''' <param name="PGMaxCount">Maximum progress bar count</param>
    ''' <param name="UpdateText">Text to display in status label</param>
    Public Sub InitalizeProgress(ByVal PGMaxCount As Long, ByVal UpdateText As String)
        lblStatus.Text = UpdateText

        pgMain.Value = 0
        pgMain.Maximum = PGMaxCount
        If PGMaxCount <> 0 Then
            pgMain.Visible = True
        End If

        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Resets the progressbar and status label on main form
    ''' </summary>
    Public Sub ClearProgress()
        pgMain.Visible = False
        lblStatus.Text = ""
        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Increments the progressbar
    ''' </summary>
    ''' <param name="Count">Current count to update on progress bar.</param>
    ''' <param name="UpdateText">Text to display in the status label</param>
    Public Sub UpdateProgress(ByVal Count As Long, ByVal UpdateText As String)
        Count += 1
        If Count < pgMain.Maximum - 1 And Count <> 0 Then
            pgMain.Value = Count
            pgMain.Value = pgMain.Value - 1
            pgMain.Value = Count
        Else
            pgMain.Value = Count
        End If

        lblStatus.Text = UpdateText
        Application.DoEvents()

    End Sub

#End Region

#Region "Option Checks processing"

    ''' <summary>
    ''' Processes when a check is clicked on or off in the grid
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub dgMain_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgMain.CellContentClick
        If Not FirstLoad Then
            If e.ColumnIndex = 0 Then
                dgMain.EndEdit() ' make sure it sets the check value correctly
                If Convert.ToBoolean(dgMain.CurrentCell.Value) = True Then
                    ' Checked it - add to the list
                    CheckedFilesList.Add(dgMain.Rows(e.RowIndex).Cells(1).Value)
                Else
                    ' Unchecked it - remove from the list
                    CheckedFilesList.Remove(dgMain.Rows(e.RowIndex).Cells(1).Value)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Enables or disables check boxes, text boxes, and labels on the main form depending on options sent.
    ''' </summary>
    ''' <param name="Server">Boolean for enabling/disabling the Server Label and Textbox</param>
    ''' <param name="UserName">Boolean for enabling/disabling the User Name Label and Textbox</param>
    ''' <param name="Password">Boolean for enabling/disabling the Password Label and Textbox</param>
    ''' <param name="EUFormatCheck">Boolean for enabling/disabling the EU Format Checkbox</param>
    ''' <param name="Port">Boolean for enabling/disabling the Port Label and Textbox</param>
    ''' <param name="FinalDBFolder">Boolean for enabling/disabling the Final DB Path Label, Button, and Textbox</param>
    Private Sub SetFormObjects(Server As Boolean, UserName As Boolean, Password As Boolean, EUFormatCheck As Boolean, Port As Boolean, FinalDBFolder As Boolean)
        lblServerName.Enabled = Server
        txtServerName.Enabled = Server
        lblUserName.Enabled = UserName
        txtUserName.Enabled = UserName
        lblPassword.Enabled = Password
        txtPassword.Enabled = Password
        chkEUFormat.Visible = EUFormatCheck
        lblPort.Enabled = Port
        txtPort.Enabled = Port

        lblFinalDBPath.Enabled = FinalDBFolder
        lblFinalDBFolder.Enabled = FinalDBFolder
        btnSelectFinalDBPath.Enabled = FinalDBFolder

        If chkEUFormat.Enabled = True And rbtnCSV.Checked Then
            lblDBName.Text = "Folder Name:"
        Else
            lblDBName.Text = "Database Name:"
        End If

        If rbtnSQLServer.Checked Then
            lblServerName.Text = "Instance Name:"
        Else
            lblServerName.Text = "Server Name:"
        End If

        If Not FirstLoad Then
            Call LoadFormSettings()
        End If

    End Sub

    ''' <summary>
    ''' Loads all the text boxes with settings on the main form depending on what radio button is selected
    ''' </summary>
    Private Sub LoadFormSettings()
        With UserApplicationSettings
            ' Set the variables
            If rbtnAccess.Checked Then
                txtServerName.Text = ""
                txtPassword.Text = .AccessPassword
                txtUserName.Text = ""
                txtPort.Text = ""
            ElseIf rbtnSQLiteDB.Checked Then
                txtServerName.Text = ""
                txtPassword.Text = ""
                txtUserName.Text = ""
                txtPort.Text = ""
            ElseIf rbtnSQLServer.Checked Then
                txtServerName.Text = .SQLConnectionString
                txtPassword.Text = .SQLPassword
                txtUserName.Text = .SQLUserName
                txtPort.Text = ""
            ElseIf rbtnCSV.Checked Then
                chkEUFormat.Checked = .CSVEUCheck
                txtServerName.Text = ""
                txtPassword.Text = ""
                txtUserName.Text = ""
                txtPort.Text = ""
            ElseIf rbtnMySQL.Checked Then
                txtServerName.Text = .MySQLConnectionString
                txtPassword.Text = .MySQLPassword
                txtUserName.Text = .MySQLUserName
                txtPort.Text = .MySQLPort
            ElseIf rbtnPostgreSQL.Checked Then
                txtServerName.Text = .PostgreSQLConnectionString
                txtPassword.Text = .PostgreSQLPassword
                txtUserName.Text = .PostgreSQLUserName
                txtPort.Text = .PostgreSQLPort
            End If
        End With
    End Sub

    Private Sub rbtnCSV_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnCSV.CheckedChanged
        If rbtnCSV.Checked Then
            Call SetFormObjects(False, False, False, True, False, True)
        End If
    End Sub

    Private Sub rbtnMySQL_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMySQL.CheckedChanged
        If rbtnMySQL.Checked Then
            Call SetFormObjects(True, True, True, False, True, False)
        End If
    End Sub

    Private Sub rbtnAccess_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnAccess.CheckedChanged
        If rbtnAccess.Checked Then
            Call SetFormObjects(False, False, True, False, False, True)
        End If
    End Sub

    Private Sub rbtnSQLiteDB_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnSQLiteDB.CheckedChanged
        If rbtnSQLiteDB.Checked Then
            Call SetFormObjects(False, False, False, False, False, True)
        End If
    End Sub

    Private Sub rbtnSQLServer_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnSQLServer.CheckedChanged
        If rbtnSQLServer.Checked Then
            Call SetFormObjects(True, True, True, False, False, False)
        End If
    End Sub

    Private Sub rbtnPostgreSQL_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnPostgreSQL.CheckedChanged
        If rbtnPostgreSQL.Checked Then
            Call SetFormObjects(True, True, True, False, True, False)
        End If
    End Sub

#End Region

#Region "Click event handlers"

    Private Sub SetThreadsUsedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetThreadsUsedToolStripMenuItem.Click
        Dim f1 As New frmThreadSelect With {
            .Threads = SelectedThreads
        }
        f1.ShowDialog()
    End Sub

    Private Sub btnSelectSDEPath_Click(sender As Object, e As EventArgs) Handles btnSelectSDEPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.SDEDirectory) Then
            FBDialog.SelectedPath = UserApplicationSettings.SDEDirectory
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblSDEPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.SDEDirectory = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If

        ' Load the file list since they just selected the folder
        Call LoadFileListtoGrid()

    End Sub

    Private Sub btnSelectFinalDBPath_Click(sender As Object, e As EventArgs) Handles btnSelectFinalDBPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.FinalDBPath) Then
            FBDialog.SelectedPath = UserApplicationSettings.FinalDBPath
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblFinalDBPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.FinalDBPath = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If
    End Sub

    Private Sub btnSaveFilePath_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        If LoadFileListtoGrid() Then
            Call SaveSettings(False)
        End If
    End Sub

    Private Sub btnCheckNoGridItems_Click(sender As Object, e As EventArgs) Handles btnCheckNoGridItems.Click
        For i = 0 To dgMain.RowCount - 1
            dgMain.Rows(i).Cells(0).Value = 0
        Next

        ' Reset all checked files
        CheckedFilesList = New List(Of String)
    End Sub

    Private Sub btnCheckAllGridItems_Click(sender As Object, e As EventArgs) Handles btnCheckAllGridItems.Click
        For i = 0 To dgMain.RowCount - 1
            dgMain.Rows(i).Cells(0).Value = 1
            ' Add all rows
            CheckedFilesList.Add(dgMain.Rows(i).Cells(1).Value)
        Next
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        CancelImport = True
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        End
    End Sub

    Private Sub txtDBName_TextChanged(sender As Object, e As EventArgs) Handles txtDBName.TextChanged
        UserApplicationSettings.DatabaseName = txtDBName.Text
    End Sub

    Private Sub txtDBName_GotFocus(sender As Object, e As EventArgs) Handles txtDBName.GotFocus
        txtDBName.SelectAll()
    End Sub

    Private Sub txtServerName_GotFocus(sender As Object, e As EventArgs) Handles txtServerName.GotFocus
        txtServerName.SelectAll()
    End Sub

    Private Sub txtUserName_GotFocus(sender As Object, e As EventArgs) Handles txtUserName.GotFocus
        txtUserName.SelectAll()
    End Sub

    Private Sub txtPassword_GotFocus(sender As Object, e As EventArgs) Handles txtPassword.GotFocus
        txtPassword.SelectAll()
    End Sub

    Private Sub txtPort_GotFocus(sender As Object, e As EventArgs) Handles txtPort.GotFocus
        txtPort.SelectAll()
    End Sub

    Private Sub AboutToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem1.Click
        Dim f1 = New frmAbout
        f1.ShowDialog()
    End Sub

    Private Sub CheckForUpdatesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckForUpdatesToolStripMenuItem.Click
        Call CheckForUpdates(True)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        End
    End Sub

#End Region

    ' Predicate for sorting a list of grid file items
    Public Class GridFileItemComparer

        Implements IComparer(Of GridFileItem)

        Public Function Compare(ByVal F1 As GridFileItem, ByVal F2 As GridFileItem) As Integer Implements IComparer(Of GridFileItem).Compare
            ' ascending sort
            Return F1.FileName.CompareTo(F2.FileName)
        End Function

    End Class

    Private Sub txtPassword_TextChanged(sender As Object, e As EventArgs) Handles txtPassword.TextChanged
        If rbtnAccess.Checked Then
            UserApplicationSettings.AccessPassword = txtPassword.Text
        ElseIf rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLPassword = txtPassword.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLPassword = txtPassword.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLPassword = txtPassword.Text
        End If
    End Sub

    Private Sub txtUserName_TextChanged(sender As Object, e As EventArgs) Handles txtUserName.TextChanged
        If rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLUserName = txtUserName.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLUserName = txtUserName.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLUserName = txtUserName.Text
        End If
    End Sub

    Private Sub txtServerName_TextChanged(sender As Object, e As EventArgs) Handles txtServerName.TextChanged
        If rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLConnectionString = txtServerName.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLConnectionString = txtServerName.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLConnectionString = txtServerName.Text
        End If
    End Sub

    Private Sub txtPort_TextChanged(sender As Object, e As EventArgs) Handles txtPort.TextChanged
        If rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLPort = txtPort.Text
        End If
    End Sub

    Private Sub btnSelectDownloadPath_Click(sender As Object, e As EventArgs) Handles btnSelectDownloadPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.SDEDirectory) Then
            FBDialog.SelectedPath = UserApplicationSettings.SDEDirectory
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblDownloadFolderPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.DownloadFolderPath = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
                Exit Sub
            End Try
        End If

    End Sub

    Private Sub btnDownloadSDE_Click(sender As Object, e As EventArgs) Handles btnDownloadSDE.Click
        Dim ChecksumFileName As String = UserApplicationSettings.DownloadFolderPath & "\" & "checksum"
        Dim OldChecksumFileName As String = UserApplicationSettings.DownloadFolderPath & "\" & "checksum-old"
        Dim NewDownloadDirectory As String ' Folder I'll download into and work with
        Dim NewChecksumValue As String
        Dim OldChecksumValue As String = ""
        Dim Splitter(1) As String
        Dim FileDate As Date ' to save the date of the download

        CancelDownload = False

        If Trim(lblDownloadFolderPath.Text) = "" Then
            Call MsgBox("You must select a SDE download folder path.", vbInformation, Application.ProductName)
            btnSelectDownloadPath.Focus()
            Exit Sub
        End If

        ' Now that we have a good directory, download the check sum to make sure we need an update
        If File.Exists(ChecksumFileName) Then
            ' rename this checksum before downloading the new one
            If File.Exists(OldChecksumFileName) Then
                File.Delete(OldChecksumFileName)
            End If
            File.Copy(ChecksumFileName, OldChecksumFileName)
            File.Delete(ChecksumFileName)
            OldChecksumValue = GetSDEChecksumValue(OldChecksumFileName)
            Splitter = OldChecksumValue.Split("  ")
            OldChecksumValue = Splitter(0)
        End If

        ' Download the new checksum
        Call DownloadFileFromServer("https://eve-static-data-export.s3-eu-west-1.amazonaws.com/tranquility/checksum", ChecksumFileName, FileDate)

        ' Compare the checksums and check if they are different
        NewChecksumValue = GetSDEChecksumValue(ChecksumFileName)

        If IsNothing(NewChecksumValue) Then
            MsgBox("Failed to download checksum. Try again.", vbExclamation, "SDE Database Builder")
            Exit Sub
        Else
            Splitter = NewChecksumValue.Split("  ")
            NewChecksumValue = Splitter(0)
        End If

        If NewChecksumValue <> OldChecksumValue Then
            Me.UseWaitCursor = True
            ' Need to download the new SDE
            btnDownloadSDE.Enabled = False
            btnSelectDownloadPath.Enabled = False
            gbSelectDBType.Enabled = False
            btnCancel.Enabled = False
            btnSelectSDEPath.Enabled = False
            btnSelectFinalDBPath.Enabled = False
            btnCheckAllGridItems.Enabled = False
            btnCheckNoGridItems.Enabled = False
            btnBuildDatabase.Enabled = False
            btnSaveSettings.Enabled = False
            btnClose.Enabled = False
            dgMain.Enabled = False
            btnCancelDownload.Enabled = True

            lblStatus.Text = "Preparing files..."
            Application.DoEvents()
            ' Create a folder for today's date and download the SDE into that folder - will overwrite anything there
            NewDownloadDirectory = UserApplicationSettings.DownloadFolderPath & "\" & MonthName(FileDate.Month) & "_" & CStr(FileDate.Day) & "_" & Year(FileDate)
            If Directory.Exists(NewDownloadDirectory) Then
                Call Directory.Delete(NewDownloadDirectory, True)
            End If
            Call Directory.CreateDirectory(NewDownloadDirectory)

            ' Now download into that folder
            lblStatus.Text = "Downloading SDE..."
            Call DownloadFileFromServer("https://eve-static-data-export.s3-eu-west-1.amazonaws.com/tranquility/sde.zip", NewDownloadDirectory & "\SDE.zip", Nothing, pgBar)

            If CancelDownload Then
                ' Delete new checksum and restore old one
                File.Delete(ChecksumFileName)
                If File.Exists(OldChecksumFileName) Then
                    ' Rename
                    File.Copy(OldChecksumFileName, ChecksumFileName)
                    File.Delete(OldChecksumFileName)
                End If
                MsgBox("Cancelled download", vbInformation, Application.ProductName)
                GoTo CancelDownload
            End If

            ' Unzip files and set the new download folder 
            lblStatus.Text = "Extracting files..."
            btnCancelDownload.Enabled = False ' Can't cancel anymore
            Application.DoEvents()
            Call ZipFile.ExtractToDirectory(NewDownloadDirectory & "\SDE.zip", NewDownloadDirectory)

            ' Finally delete old download zip file after extracted to save space
            lblStatus.Text = "Cleaning up files..."
            Application.DoEvents()
            File.Delete(NewDownloadDirectory & "\SDE.zip")

            lblSDEPath.Text = NewDownloadDirectory
            UserApplicationSettings.SDEDirectory = NewDownloadDirectory
            ' Save the settings as well
            Call SaveSettings(True)
            lblStatus.Text = ""
            ' Refersh the yaml file paths
            ' Load the file list since they just selected the folder
            Call LoadFileListtoGrid()

            Call MsgBox("SDE Downloaded and saved in SDE File Folder", vbInformation, Application.ProductName)
        Else
            Call MsgBox("You have the latest SDE Version downloaded", vbInformation, Application.ProductName)
        End If

        ' Delete the old check sum file in both cases
        File.Delete(OldChecksumFileName)

CancelDownload:
        Me.UseWaitCursor = False

        btnDownloadSDE.Enabled = True
        btnSelectDownloadPath.Enabled = True
        gbSelectDBType.Enabled = True
        btnCancel.Enabled = True
        btnSelectSDEPath.Enabled = True
        btnSelectFinalDBPath.Enabled = True
        btnCheckAllGridItems.Enabled = True
        btnCheckNoGridItems.Enabled = True
        btnBuildDatabase.Enabled = True
        btnSaveSettings.Enabled = True
        btnClose.Enabled = True
        dgMain.Enabled = True

    End Sub

    Private Function GetSDEChecksumValue(FileName As String) As String
        Dim CheckSumReader As StreamReader
        Dim ChecksumValue As String = ""
        ' Read each line in the file until it finds sde.zip

        CheckSumReader = New StreamReader(FileName)
        While Not CheckSumReader.EndOfStream
            ChecksumValue = CheckSumReader.ReadLine
            If ChecksumValue.Contains("sde.zip") Then
                Exit While
            End If
        End While

        Call CheckSumReader.Dispose() ' Release

        Return ChecksumValue

    End Function

    Private Sub TestForSDEChangesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TestForSDEChangesToolStripMenuItem.Click
        If TestForSDEChangesToolStripMenuItem.Checked Then
            TestForSDEChanges = True
        Else
            TestForSDEChanges = False
        End If
    End Sub

    Private Sub btnCancelDownload_Click(sender As Object, e As EventArgs) Handles btnCancelDownload.Click
        CancelDownload = True
    End Sub

    Private Sub UseLargerVersionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UseLargerVersionToolStripMenuItem.CheckedChanged
        If Not FirstLoad Then
            ' Set the setting flag for using the larger version and reload the form 
            UserApplicationSettings.UseLargerVersion = UseLargerVersionToolStripMenuItem.Checked

            ' Save the settings
            Call AllSettings.SaveApplicationSettings(UserApplicationSettings)

            ' Reload the form with the selected settings
            Call SetFormSize()

        End If
    End Sub

    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        'rs.ResizeAllControls(Me)
    End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' rs.FindAllControls(Me)
    End Sub

    Private Sub ResetDownloadChecksumToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetDownloadChecksumToolStripMenuItem.Click
        Call File.Delete(UserApplicationSettings.DownloadFolderPath & "\" & "checksum")
        Call MsgBox("Database Checksum file deleted.", vbInformation, Application.ProductName)
    End Sub
End Class

' For updating the data grid view
Public Class ProgressColumn
    Inherits DataGridViewColumn

    Public Sub New()
        MyBase.New(New ProgressCell())
    End Sub

    Public Overrides Property CellTemplate() As DataGridViewCell
        Get
            Return MyBase.CellTemplate
        End Get
        Set(ByVal Value As DataGridViewCell)
            ' Ensure that the cell used for the template is a ProgressCell.
            If Value IsNot Nothing And TypeOf (Value) IsNot ProgressCell Then
                Throw New InvalidCastException("Must be a ProgressCell")
            End If
            MyBase.CellTemplate = Value
        End Set
    End Property

End Class

Public Class ProgressCell
    Inherits DataGridViewImageCell
    Protected Overrides Function GetFormattedValue(ByVal value As Object, ByVal rowIndex As Integer, ByRef cellStyle As DataGridViewCellStyle,
                                                   ByVal valueTypeConverter As System.ComponentModel.TypeConverter,
                                                   ByVal formattedValueTypeConverter As System.ComponentModel.TypeConverter,
                                                   ByVal context As DataGridViewDataErrorContexts) As Object
        ' Create bitmap.
        Dim bmp As New Bitmap(Me.Size.Width, Me.Size.Height)

        Using g As Graphics = Graphics.FromImage(bmp)

            If Not IsNothing(Me.Value) Then
                ' Percentage.
                Dim percentage As Double = 0
                Double.TryParse(Me.Value.ToString(), percentage)
                Dim text As String = percentage.ToString() + " %"

                ' Get width and height of text.
                Dim f As New Font("Microsoft Sans Serif", 8.5, FontStyle.Regular)
                Dim w As Integer = CType(g.MeasureString(text, f).Width, Integer)
                Dim h As Integer = CType(g.MeasureString(text, f).Height, Integer)

                ' Draw pile - build a white box first to cover the value in the grid so it doesn't overlap
                g.DrawRectangle(Pens.Black, 1, 1, Me.Size.Width - 6, Me.Size.Height - 6)
                g.FillRectangle(Brushes.White, 2, 2, CInt((Me.Size.Width - 7)), CInt(Me.Size.Height - 7))
                ' Draw the green progress rectangle based on the number
                g.DrawRectangle(Pens.Black, 1, 1, Me.Size.Width - 6, Me.Size.Height - 6)
                g.FillRectangle(Brushes.LimeGreen, 2, 2, CInt((Me.Size.Width - 7) * percentage / 100), CInt(Me.Size.Height - 7))

                Dim rect As New RectangleF(0, 3, bmp.Width, bmp.Height)
                Dim sf As New StringFormat With {
                    .Alignment = StringAlignment.Center
                }
                g.DrawString(text, f, Brushes.Black, rect, sf)
            End If
        End Using

        Return bmp
    End Function

End Class
