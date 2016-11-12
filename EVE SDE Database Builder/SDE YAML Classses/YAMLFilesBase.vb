Imports YamlDotNet.Serialization

' Base class for initializing and closing YAML file process - all other classes will inherit this so put common functions here
Public Class YAMLFilesBase

    Protected YAMLFile As String ' Name of the file we are importing
    Protected TableName As String ' Name of the table (based on file name) we will insert data into
    Protected UpdateDB As Object ' The database object for the class used to insert data into the database
    Protected Translator As YAMLTranslations ' Ref for storing translation data in the trn tables

    Public NullValue As String = "null"

    Public Const MaxFieldLen As Integer = -1

    ' DatabaseLocation is the file path or the instance name
    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DBConnectionRef As Object, ByRef TranslationData As YAMLTranslations)

        ' Set the Database
        UpdateDB = DBConnectionRef

        ' Save the location and name of the file we are processing
        YAMLFile = YAMLFilePath & YAMLFileName

        ' Save the translation class ref
        Translator = TranslationData

        ' Table name will be the file name in almost all cases - update if needed
        If YAMLFileName <> "" Then
            TableName = YAMLFileName.Substring(0, InStr(YAMLFileName, ".") - 1)
        End If

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    ' Parameters to import a yaml file
    Public Structure ImportParameters
        Dim RowLocation As Integer
        Dim InsertRecords As Boolean ' For skipping inserts and just building the tables
        Dim ImportLanguageCode As LanguageCode ' Language code for importing translated text (e.g. categoryIDs.yaml)
        Dim ReturnList As Boolean ' if we want to return the data back
    End Structure

End Class