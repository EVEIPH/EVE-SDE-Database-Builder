Imports System.Xml
Imports System.IO

Public Class ProgramSettings

    Public FullAppSettingsFileName As String
    Private Const AppSettingsFileName As String = "ApplicationSettings"
    Private Const XMLfileType As String = ".xml"

    Private Const DefaultSelectedDB As String = "SQLite"
    Private Const DefaultSelectedLanguage As String = "English"
    Private Const DefaultpostgreSQLPort As String = "5432"
    Private Const DefaultEUCheck As Boolean = False

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
        Dim XMLSettings As XmlWriterSettings = New XmlWriterSettings()
        XMLSettings.Indent = True

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
                    .SQLServerName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SQLServerName", ""))
                    .AccessPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "AccessPassword", ""))
                    .PostgreSQLServerName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLServerName", ""))
                    .PostgreSQLUserName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLUserName", ""))
                    .PostgreSQLPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLPassword", ""))
                    .PostgreSQLPort = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "PostgreSQLPort", DefaultpostgreSQLPort))
                    .MySQLServerName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLServerName", ""))
                    .MySQLUserName = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLUserName", ""))
                    .MySQLPassword = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "MySQLPassword", ""))
                    .CSVEUCheck = CBool(GetSettingValue(AppSettingsFileName, SettingTypes.TypeBoolean, AppSettingsFileName, "CSVEUCheck", DefaultEUCheck))
                    .SelectedLanguage = CStr(GetSettingValue(AppSettingsFileName, SettingTypes.TypeString, AppSettingsFileName, "SelectedLanguage", DefaultSelectedLanguage))
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
            .SQLServerName = ""
            .AccessPassword = ""
            .PostgreSQLServerName = ""
            .PostgreSQLUserName = ""
            .PostgreSQLPassword = ""
            .PostgreSQLPort = DefaultpostgreSQLPort
            .MySQLServerName = ""
            .MySQLUserName = ""
            .MySQLPassword = ""
            .CSVEUCheck = DefaultEUCheck
            .SelectedLanguage = DefaultSelectedLanguage
        End With

        ' Save locally
        AppSettings = TempSettings
        Return TempSettings

    End Function

    ' Saves the application settings to XML
    Public Sub SaveApplicationSettings(SentSettings As ApplicationSettings)
        Dim ApplicationSettingsList(14) As Setting

        Try
            With SentSettings
                ApplicationSettingsList(0) = New Setting("SDEDirectory", .SDEDirectory)
                ApplicationSettingsList(1) = New Setting("DatabaseName", .DatabaseName)
                ApplicationSettingsList(2) = New Setting("FinalDBPath", .FinalDBPath)
                ApplicationSettingsList(3) = New Setting("SQLServerName", .SQLServerName)
                ApplicationSettingsList(4) = New Setting("AccessPassword", .AccessPassword)
                ApplicationSettingsList(5) = New Setting("PostgreSQLServerName", .PostgreSQLServerName)
                ApplicationSettingsList(6) = New Setting("PostgreSQLUserName", .PostgreSQLUserName)
                ApplicationSettingsList(7) = New Setting("PostgreSQLPassword", .PostgreSQLPassword)
                ApplicationSettingsList(8) = New Setting("PostgreSQLPort", .PostgreSQLPort)
                ApplicationSettingsList(9) = New Setting("MySQLServerName", .MySQLServerName)
                ApplicationSettingsList(10) = New Setting("MySQLUserName", .MySQLUserName)
                ApplicationSettingsList(11) = New Setting("MySQLPassword", .MySQLPassword)
                ApplicationSettingsList(12) = New Setting("CSVEUCheck", .CSVEUCheck)
                ApplicationSettingsList(13) = New Setting("SelectedLanguage", .SelectedLanguage)
                ApplicationSettingsList(14) = New Setting("SelectedDB", .SelectedDB)
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

    Dim SQLServerName As String

    Dim AccessPassword As String

    Dim PostgreSQLServerName As String
    Dim PostgreSQLUserName As String
    Dim PostgreSQLPassword As String
    Dim PostgreSQLPort As String

    Dim MySQLServerName As String
    Dim MySQLUserName As String
    Dim MySQLPassword As String

    Dim CSVEUCheck As Boolean

    Dim SelectedLanguage As String

End Structure