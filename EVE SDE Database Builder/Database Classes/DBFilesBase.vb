Imports System.Security.AccessControl
Imports System.IO

''' <summary>
''' Base class for all database classes, which stores common functions and variables
''' </summary>
Public Class DBFilesBase

    Protected Const SPACE As String = " "
    Protected Const COMMA As String = ","
    Protected Const SEMICOLON As String = ";"
    Protected Const NULL As String = "NULL"

    Protected MainDatabase As String
    Private SelectedDBType As DatabaseType
    Protected CSVDirectory As String ' For bulk inserts

    ''' <summary>
    ''' Creates a new 
    ''' </summary>
    ''' <param name="DatabaseName"></param>
    ''' <param name="CurrentDBType"></param>
    Public Sub New(ByVal DatabaseName As String, CurrentDBType As DatabaseType)
        MainDatabase = DatabaseName
        SelectedDBType = CurrentDBType
    End Sub

    ''' <summary>
    ''' Strips the last character from the text sent
    ''' </summary>
    ''' <param name="Text">Text to strip last character from</param>
    ''' <returns>Modified text</returns>
    Protected Function StripLastCharacter(Text As String) As String
        ' Strip the last char
        If Text.Length > 0 Then
            Text = Text.Substring(0, Len(Text) - 1)
        End If

        Return Text
    End Function

    ''' <summary>
    ''' Provides a valid insert value for SQL strings and updates things like quotes and apostrophies so they won't error on insert
    ''' </summary>
    ''' <param name="CheckString">String to check and modify if needed</param>
    ''' <returns>Updated string for use.</returns>
    Protected Function BuildSQLInsertStringValue(ByVal CheckString As String) As String
        If CheckString.ToUpper = NULL Then
            Return NULL
        Else
            If SelectedDBType = DatabaseType.CSV Then
                If CheckString.Contains("""") Then
                    CheckString = CheckString.Replace("""", "'") ' Replace all double quotes in text with single quotes so it doesn't hose up the text delimiter when importing
                End If
                Return """" & Trim(CheckString) & """" ' Add quotes and format it for proper insert
            Else
                ' Anything with quote mark in name it won't correctly load - need to replace with double quotes
                If InStr(CheckString, "'") Then
                    CheckString = Replace(CheckString, "'", "''")
                End If
                ' Add quotes around for insert
                Return String.Format("'{0}'", Trim(CheckString)) ' Add quotes and format it for proper insert
            End If
        End If
    End Function

    ''' <summary>
    ''' Sets the CSV Directory for bulk inserts. Will create directory if it doesn't exist and give it full read permissions.
    ''' </summary>
    ''' <param name="FileDirectory">Directory where the bulk insert CSV files will be stored.</param>
    Public Sub SetCSVDirectory(ByRef FileDirectory As String)

        If Directory.Exists(FileDirectory) Then
            Call Directory.Delete(FileDirectory, True)
        End If

        Dim DS As New DirectorySecurity
        DS.AddAccessRule(New FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
        Directory.CreateDirectory(FileDirectory, DS)
        CSVDirectory = FileDirectory

    End Sub

    ''' <summary>
    ''' Formats data sent for correct inserts into the tables to include the correct type and null values
    ''' </summary>
    ''' <param name="FieldName">Field Name to insert</param>
    ''' <param name="FieldValue">Value of the field for insert</param>
    ''' <param name="FieldDataType">Datatype of the field</param>
    ''' <returns></returns>
    Public Function BuildDatabaseField(ByVal FieldName As String, ByVal FieldValue As Object, FieldDataType As FieldType) As DBField
        Dim ReturnFieldValue As String = ""

        ' Format the field value and return a DB field
        If IsNothing(FieldValue) Or IsDBNull(FieldValue) Then
            ' This is null
            ReturnFieldValue = NULL
        Else
            If Trim(CStr(FieldValue)) <> "" Then
                Select Case FieldDataType
                    Case FieldType.char_type, FieldType.nchar_type, FieldType.ntext_type, FieldType.nvarchar_type, FieldType.text_type, FieldType.varchar_type
                        ReturnFieldValue = BuildSQLInsertStringValue(CStr(FieldValue))
                    Case FieldType.bit_type
                        ' Boolean values need to be properly formatted to 1/0
                        ReturnFieldValue = CStr(CInt(CBool(FieldValue)) * -1)
                    Case Else
                        If CStr(FieldValue) = "false" Or CStr(FieldValue) = "true" Then
                            ' Assume it needs to be converted into a 1 or a 0
                            ReturnFieldValue = CStr(CInt(CBool(FieldValue)))
                        Else
                            ' Convert numbers to strings
                            ReturnFieldValue = CStr(FieldValue)
                        End If
                End Select
            Else
                ' Set empty strings to null as well
                ReturnFieldValue = NULL
            End If
        End If

        If ReturnFieldValue = NULL And SelectedDBType = DatabaseType.CSV Then
            ' Just set to empty string - formatted with the text delimiter
            ReturnFieldValue = ""
        End If

        Return New DBField(FieldName, ReturnFieldValue, FieldDataType)

    End Function

End Class

''' <summary>
''' Common Structure for a Database Field
''' </summary>
Public Structure DBField
    Public FieldName As String
    Public FieldValue As String
    Public FieldType As FieldType

    Public Sub New(_FieldName As String, _FieldValue As String, _FieldType As FieldType)
        FieldName = _FieldName
        FieldValue = _FieldValue
        FieldType = _FieldType
    End Sub

End Structure

''' <summary>
''' A list of acceptable field types for all databases, based on SQLServer data fields. 
''' DB Classes will determine which types apply for their own data types
''' </summary>
Public Enum FieldType
    bit_type = 1
    tinyint_type = 2
    smallint_type = 3
    int_type = 4
    bigint_type = 5

    float_type = 6
    double_type = 7
    real_type = 8

    char_type = 9
    varchar_type = 10
    text_type = 11

    nchar_type = 12
    nvarchar_type = 13
    ntext_type = 14
End Enum

''' <summary>
''' Structure to build a database field
''' </summary>
Public Structure DBTableField
    Public FieldName As String
    Public FieldType As FieldType
    Public FieldLength As Integer
    Public IsNull As Boolean
    Public IsPrimaryKey As Boolean

    Public Sub New(_FieldName As String, _FieldType As FieldType, _FieldLength As Integer, _IsNull As Boolean, Optional _IsPrimaryKey As Boolean = False)
        FieldName = _FieldName
        FieldType = _FieldType
        FieldLength = _FieldLength ' length of string or if -1, then 'max' - all else use 0
        IsNull = _IsNull
        IsPrimaryKey = _IsPrimaryKey
    End Sub

End Structure

''' <summary>
''' Type of database this base file is using
''' </summary>
Public Enum DatabaseType
    SQLite = 1
    SQLServer = 2
    MSAccess = 3
    CSV = 4
    MySQL = 5
    PostgreSQL = 6
End Enum
