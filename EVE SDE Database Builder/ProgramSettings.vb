﻿Imports System.Xml
Imports System.IO

Public Class ProgramSettings

    Public FullAppSettingsFileName As String
    Private Const AppSettingsFileName As String = "ApplicationSettings"
    Private Const XMLfileType As String = ".xml"

    Private Const DefaultSelectedDB As String = "SQLite"
    Private Const DefaultSelectedLanguage As String = "English"
    Private Const DefaultpostgreSQLPort As String = "5432"
    Private Const DefaultMySQLPort As String = "3306"
    Private Const DefaultEUCheck As Boolean = False
    Private Const DefaultUseLargerVersion As Boolean = False

    ' Local version of settings
    Private AppSettings As ApplicationSettings

    Public Sub New()
        AppSettings = Nothing
        FullAppSettingsFileName = AppSettingsFileName & XMLfileType
    End Sub

    ''' <summary>
    ''' Writes the sent settings to the sent file name
    ''' </summary>
    ''' <param name="FileName">FileName without XML extension</param>
    ''' <param name="Settings">Settings to save</param>
    ''' <param name="RootName">Root name of the XML file</param>
    Private Sub WriteSettingsToFile(FileName As String, Settings As Setting(), RootName As String)
        Dim i As Integer
        Dim TempFileName As String = FileName & XMLfileType

        ' Create XmlWriterSettings.
        Dim XMLSettings As New XmlWriterSettings With {
            .Indent = True
        }

            ' Delete and make a fresh copy
        If File.Exists(TempFileName) Then
            File.Delete(TempFileName)
        End If

        ' Loop through the settings sent and output each name and value
        Using writer As XmlWriter = XmlWriter.Create(TempFileName, XMLSettings)
            writer.WriteStartDocument()
            writer.WriteStartElement(RootName) ' Root.

            ' Main loop
            For i = 0 To Settings.Count - 1
                writer.WriteElementString(Settings(i).Name, Settings(i).Value)
            Next

            ' End document.
            writer.WriteEndDocument()
        End Using

    End Sub

    ''' <summary>
    ''' Gets a value from a referenced XML file by searching for it
    ''' </summary>
    ''' <param name="FileName">Filename to search</param>
    ''' <param name="ObjectType">Type of the setting we are looking for</param>
    ''' <param name="RootElement">Root element to search for the setting</param>
    ''' <param name="ElementString">The name of the setting in the XML file</param>
    ''' <param name="DefaultValue">If not found, what value to assign for the setting</param>
    ''' <returns></returns>
    Private Function GetSettingValue(ByRef FileName As String, ObjectType As SettingTypes, RootElement As String, ElementString As String, DefaultValue As Object) As Object
        Dim m_xmld As New XmlDocument
        Dim m_nodelist As XmlNodeList

        Dim TempValue As String

        'Load the Xml file
        m_xmld.Load(FileName & XMLfileType)

        'Get the settings

        ' Get the cache update
        m_nodelist = m_xmld.SelectNodes("/" & RootElement & "/" & ElementString)

        If Not IsNothing(m_nodelist.Item(0)) Then
            ' Should only be one
            TempValue = m_nodelist.Item(0).InnerText

            ' If blank, then return default
            If TempValue = "" Then
                Return DefaultValue
            End If

            If TempValue = "False" Or TempValue = "True" Then
                ' Change to type boolean
                ObjectType = SettingTypes.TypeBoolean
            End If

            ' Found it, return the cast
            Select Case ObjectType
                Case SettingTypes.TypeBoolean
                    Return CBool(TempValue)
                Case SettingTypes.TypeDouble
                    Return CDbl(TempValue)
                Case SettingTypes.TypeInteger
                    Return CInt(TempValue)
                Case SettingTypes.TypeString
                    Return CStr(TempValue)
                Case SettingTypes.TypeLong
                    Return CLng(TempValue)
            End Select

        Else
            ' Doesn't exist, use default
            Return DefaultValue
        End If

        Return Nothing

    End Function

    ''' <summary>
    ''' Just checks if the file exists or not so we don't have to mess with file names
    ''' </summary>
    ''' <param name="FileName">Filename to search</param>
    ''' <returns>True if found, false if not</returns>
    Private Function FileExists(FileName As String) As Boolean

        If File.Exists(FileName & XMLfileType) Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Structure Setting
        Dim Name As String
        Dim Value As String

        Public Sub New(inName As String, inValue As String)
            Name = inName
            Value = inValue
        End Sub

    End Structure

    Private Enum SettingTypes
        TypeInteger = 1
        TypeDouble = 2
        TypeString = 3
        TypeBoolean = 4
        TypeLong = 5
    End Enum

#Region "Application Settings"

    ' Loads the settings for the user from the DB (for now) for the whole program
    Public Function LoadApplicationSettings() As ApplicationSettings
        Dim TempSettings As ApplicationSettings = Nothing

        Try
            If FileExists(AppSettingsFileName) Then

                'Get the settings
                With TempSettings
                    .SelectedDB = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SelectedDB", DefaultSelectedDB))
                    .SDEDirectory = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SDEDirectory", ""))
                    .DatabaseName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "DatabaseName", ""))
                    .FinalDBPath = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "FinalDBPath", ""))
                    .DownloadFolderPath = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "DownloadFolderPath", ""))
                    .SQLConnectionString = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SQLConnectionString", ""))
                    .SQLPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SQLPassword", ""))
                    .SQLUserName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SQLUserName", ""))
                    .AccessPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "AccessPassword", ""))
                    .PostgreSQLConnectionString = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLConnectionString", ""))
                    .PostgreSQLUserName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLUserName", ""))
                    .PostgreSQLPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLPassword", ""))
                    .PostgreSQLPort = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLPort", DefaultpostgreSQLPort))
                    .MySQLConnectionString = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLConnectionString", ""))
                    .MySQLUserName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLUserName", ""))
                    .MySQLPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLPassword", ""))
                    .MySQLPort = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLPort", DefaultMySQLPort))
                    .CSVEUCheck = CBool(GetSettingValue(AppSettingsFileName, SettingTypes.TypeBoolean, AppSettingsFileName, "CSVEUCheck", DefaultEUCheck))
                    .SelectedLanguage = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SelectedLanguage", DefaultSelectedLanguage))
                    .UseLargerVersion = CBool(GetSettingValue(AppSettingsFileName, SettingTypes.TypeBoolean, AppSettingsFileName, "UseLargerVersion", DefaultUseLargerVersion))
                End With

            Else
                ' Load defaults 
                TempSettings = SetDefaultApplicationSettings()
            End If

        Catch ex As Exception
            MsgBox("An error occured when loading Application Settings. Error: " & Err.Description & vbCrLf & "Default settings were loaded.", vbExclamation, Application.ProductName)
            ' Some other error occured Load defaults 
            TempSettings = SetDefaultApplicationSettings()
        End Try

        ' Save them locally and then export
        AppSettings = TempSettings

        Return TempSettings

    End Function

    ' Loads the defaults
    Public Function SetDefaultApplicationSettings() As ApplicationSettings
        Dim TempSettings As ApplicationSettings

        With TempSettings
            ' Load default settings
            .SelectedDB = DefaultSelectedDB
            .SDEDirectory = ""
            .DatabaseName = ""
            .FinalDBPath = ""
            .SQLConnectionString = ""
            .SQLPassword = ""
            .SQLUserName = ""
            .AccessPassword = ""
            .PostgreSQLConnectionString = ""
            .PostgreSQLUserName = ""
            .PostgreSQLPassword = ""
            .PostgreSQLPort = DefaultpostgreSQLPort
            .MySQLConnectionString = ""
            .MySQLUserName = ""
            .MySQLPassword = ""
            .MySQLPort = DefaultMySQLPort
            .CSVEUCheck = DefaultEUCheck
            .UseLargerVersion = DefaultUseLargerVersion
            .SelectedLanguage = DefaultSelectedLanguage
            .DownloadFolderPath = ""
        End With

        ' Save locally
        AppSettings = TempSettings
        Return TempSettings

    End Function

    ' Saves the application settings to XML
    Public Sub SaveApplicationSettings(SentSettings As ApplicationSettings)
        Dim ApplicationSettingsList(19) As Setting

        Try
            With SentSettings
                ApplicationSettingsList(0) = New Setting("SDEDirectory", .SDEDirectory)
                ApplicationSettingsList(1) = New Setting("DatabaseName", .DatabaseName)
                ApplicationSettingsList(2) = New Setting("FinalDBPath", .FinalDBPath)
                ApplicationSettingsList(3) = New Setting("SQLConnectionString", .SQLConnectionString)
                ApplicationSettingsList(4) = New Setting("SQLPassword", .SQLPassword)
                ApplicationSettingsList(5) = New Setting("SQLUserName", .SQLUserName)
                ApplicationSettingsList(6) = New Setting("AccessPassword", .AccessPassword)
                ApplicationSettingsList(7) = New Setting("PostgreSQLConnectionString", .PostgreSQLConnectionString)
                ApplicationSettingsList(8) = New Setting("PostgreSQLUserName", .PostgreSQLUserName)
                ApplicationSettingsList(9) = New Setting("PostgreSQLPassword", .PostgreSQLPassword)
                ApplicationSettingsList(10) = New Setting("PostgreSQLPort", .PostgreSQLPort)
                ApplicationSettingsList(11) = New Setting("MySQLConnectionString", .MySQLConnectionString)
                ApplicationSettingsList(12) = New Setting("MySQLUserName", .MySQLUserName)
                ApplicationSettingsList(13) = New Setting("MySQLPassword", .MySQLPassword)
                ApplicationSettingsList(14) = New Setting("MySQLPort", .MySQLPort)
                ApplicationSettingsList(15) = New Setting("CSVEUCheck", .CSVEUCheck)
                ApplicationSettingsList(16) = New Setting("SelectedLanguage", .SelectedLanguage)
                ApplicationSettingsList(17) = New Setting("SelectedDB", .SelectedDB)
                ApplicationSettingsList(18) = New Setting("DownloadFolderPath", .DownloadFolderPath)
                ApplicationSettingsList(19) = New Setting("UseLargerVersion", .UseLargerVersion)
            End With

            Call WriteSettingsToFile(AppSettingsFileName, ApplicationSettingsList, AppSettingsFileName)

        Catch ex As Exception
            MsgBox("An error occured when saving Application Settings. Error: " & Err.Description & vbCrLf & "Settings not saved.", vbExclamation, Application.ProductName)
        End Try

    End Sub

    ' Returns the application settings
    Public Function GetApplicationSettings() As ApplicationSettings
        Return AppSettings
    End Function

#End Region

End Class

' For general program settings
Public Structure ApplicationSettings
    Dim SelectedDB As String

    ' The same for all dbs
    Dim SDEDirectory As String
    Dim DatabaseName As String

    ' Used for all Access, SQLite, and CSV dbs
    Dim FinalDBPath As String

    ' Where we download the SDE to 
    Dim DownloadFolderPath As String

    Dim SQLConnectionString As String
    Dim SQLUserName As String
    Dim SQLPassword As String

    Dim AccessPassword As String

    Dim PostgreSQLConnectionString As String
    Dim PostgreSQLUserName As String
    Dim PostgreSQLPassword As String
    Dim PostgreSQLPort As String

    Dim MySQLConnectionString As String
    Dim MySQLUserName As String
    Dim MySQLPassword As String
    Dim MySQLPort As String

    Dim CSVEUCheck As Boolean

    Dim UseLargerVersion As Boolean

    Dim SelectedLanguage As String

End Structure