
Imports System.Data.SqlClient

''' <summary>
''' Class to create a Microsoft SQL Server DB and insert data into it.
''' </summary>
Public Class msSQLDB
    Inherits DBFilesBase

    ' Save the database name and instance for later
    Private DBServerName As String
    Private TempDB As New LocalDatabase ' for doing bulk inserts

    ''' <summary>
    ''' Constructor class for a Microsoft SQL Server database. Class connects to the server name sent.
    ''' If the database name sent does not exist, will create the database on the server.
    ''' </summary>
    ''' <param name="DatabaseName">Name of the database to open or create.</param>
    ''' <param name="InstanceName">Name of the Microsoft SQL Server (e.g. localhost')</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseName As String, ByVal InstanceName As String, ByRef Success As Boolean)
        MyBase.New(DatabaseName, DatabaseType.SQLServer)

        Dim Conn As New SqlConnection

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try
            ' Set the DB Instance and Name data for later use
            MainDatabase = DatabaseName
            DBServerName = InstanceName

            Conn = New SqlConnection(String.Format("Server={0}\{1};Trusted_Connection=True; Initial Catalog=master; Integrated Security=True;Connection Timeout=60;",
                                             Environment.MachineName, InstanceName))
            SqlConnection.ClearAllPools()
            Conn.Open()

            ' If the database doesn't exist, then create it
            Dim Command As New SqlCommand(String.Format("SELECT * FROM sys.databases WHERE name='{0}'", DatabaseName), Conn)

            If IsNothing(Command.ExecuteScalar) Then
                ' Create the database
                Command.CommandText = String.Format("CREATE DATABASE [{0}]", DatabaseName)
                Command.ExecuteNonQuery()
                Command.Dispose()
            End If

            Call CloseDB(Conn)

            Success = True

        Catch ex As Exception
            Call CloseDB(Conn)
            Call ShowErrorMessage(ex)
            Success = False
        End Try

        Call ClearMainProgressBar()

    End Sub

    ''' <summary>
    ''' Opens a database connection to a MS SQL server for
    ''' the database name given for use with the class.
    ''' </summary>
    ''' <returns>Returns a SqlConnection for use.</returns>
    Private Function DBConnectionRef() As SqlConnection

        ' Open the connection for reference
        Dim DBRef As New SqlConnection(String.Format("Server={0}\{1};Database={2};Trusted_Connection=True;Connection Timeout=600;",
                                             Environment.MachineName, DBServerName, MainDatabase))
        DBRef.Open()

        Return DBRef

    End Function

    ''' <summary>
    ''' Closes and disposes the referenced database.
    ''' </summary>
    ''' <param name="RefDB">The database to close.</param>
    Public Sub CloseDB(ByRef RefDB As SqlConnection)
        On Error Resume Next
        RefDB.Close()
        RefDB.Dispose()
        On Error GoTo 0
    End Sub

    ''' <summary>
    ''' Executes the sent SQL on the main database for the class.
    ''' </summary>
    ''' <param name="SQL">SQL query to execute.</param>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Dim DBRef As New SqlConnection
        DBRef = DBConnectionRef()
        Dim Command As New SqlCommand(SQL, DBRef)
        Command.ExecuteNonQuery()
        Command.Dispose()
        DBRef.Close()
        DBRef.Dispose()
    End Sub

    ''' <summary>
    ''' Begins an SQL transaction on the server
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Call ExecuteNonQuerySQL("BEGIN TRANSACTION;")
    End Sub

    ''' <summary>
    ''' Commits an SQL transaction on the server
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Call ExecuteNonQuerySQL("COMMIT TRANSACTION;")
    End Sub

    ''' <summary>
    ''' Rollsback an SQL transaction on the server
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Call ExecuteNonQuerySQL("ROLLBACK TRANSACTION;")
    End Sub

    ''' <summary>
    ''' Drops the tablename sent, if it exists on the database.
    ''' </summary>
    ''' <param name="TableName">Table you want to drop</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim SQL As String = ""
        Dim FieldLength As String = "0" ' For char types
        Dim PKFields As New List(Of String)

        ' Now build the SQL and execute
        SQL = String.Format("If OBJECT_ID('{0}') IS NOT NULL " +
                            "   DROP TABLE {0} " +
                            "   CREATE TABLE {0} (", TableName)

        ' Add fields
        For Each Field In TableStructure
            With Field
                ' Add field name
                SQL &= .FieldName & SPACE

                ' Set field length
                If .FieldLength = -1 Then
                    FieldLength = "max"
                Else
                    FieldLength = CStr(.FieldLength)
                End If

                Select Case .FieldType
                    ' Format any strings
                    Case FieldType.nchar_type
                        SQL &= "nchar(" & FieldLength & ")"
                    Case FieldType.nvarchar_type
                        SQL &= "nvarchar(" & FieldLength & ")"
                    Case FieldType.ntext_type
                        SQL &= "ntext(" & FieldLength & ")"
                    Case FieldType.char_type
                        SQL &= "char(" & FieldLength & ")"
                    Case FieldType.varchar_type
                        SQL &= "varchar(" & FieldLength & ")"
                    Case FieldType.text_type
                        SQL &= "text"
                    Case FieldType.float_type, FieldType.double_type
                        SQL &= "float"
                    Case FieldType.real_type
                        SQL &= "real"
                    Case FieldType.bit_type
                        SQL &= "bit"
                    Case FieldType.tinyint_type
                        SQL &= "tinyint"
                    Case FieldType.smallint_type
                        SQL &= "smallint"
                    Case FieldType.int_type
                        SQL &= "int"
                    Case FieldType.bigint_type
                        SQL &= "bigint"
                End Select

                If Not .IsNull Then
                    SQL &= SPACE & "NOT NULL" & COMMA
                Else
                    SQL &= COMMA
                End If

                ' Save the PK fields for adding at end
                If .IsPrimaryKey Then
                    PKFields.Add(.FieldName)
                End If

            End With
        Next

        If PKFields.Count > 0 Then
            ' Create PK constraint here
            SQL &= String.Format("Constraint {0}_PK PRIMARY KEY CLUSTERED (", TableName)
            For Each PK In PKFields
                SQL &= PK & COMMA
            Next
            ' Strip the final comma
            SQL = StripLastCharacter(SQL) & ")"
        Else
            ' Strip the final comma
            SQL = StripLastCharacter(SQL)
        End If

        SQL &= ")"

        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Creates an index on the table sent using the fields sent.
    ''' </summary>
    ''' <param name="TableName">Table the index will apply to.</param>
    ''' <param name="IndexName">Unique name of the index on the database.</param>
    ''' <param name="IndexFields">Fields that make up the index.</param>
    ''' <param name="Unique">If the index is unique.</param>
    ''' <param name="Clustered">Optional - If the index is clustered or unclustered (not used).</param>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Dim SQL As String = ""

        SQL = "CREATE" & SPACE

        ' Unique index
        If Unique Then
            SQL &=  "UNIQUE" & SPACE
        End If

        ' Cluster type
        If Clustered Then
            SQL &=  "CLUSTERED" & SPACE
        Else
            SQL &=  "NONCLUSTERED" & SPACE
        End If

        SQL &=  String.Format("INDEX {0} ON {1} (", IndexName, TableName)

        ' Build index fields
        For Each Field In IndexFields
            SQL &=  Field & COMMA
        Next

        ' Strip the last comman
        SQL = StripLastCharacter(SQL) & ")"

        ' Create index
        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Inserts the list of record values (fields) into the table name provided.
    ''' </summary>
    ''' <param name="TableName">Table to insert records.</param>
    ''' <param name="Record">List of table fields that make up the record.</param>
    ''' <param name="ImmediateInsert">If we immediately insert the record or do a bulk insert.</param>
    Public Sub InsertRecord(ByVal TableName As String, ByVal Record As List(Of DBField), Optional ByVal ImmediateInsert As Boolean = False)

        ' If it's a translation table, then save it to the database directly - no bulk insert
        If ImmediateInsert Then
            Dim Fields As String = ""
            Dim FieldValues As String = ""

            For Each Field In Record
                Fields = Fields & Field.FieldName & COMMA
                FieldValues = FieldValues & Field.FieldValue & COMMA
            Next

            ' Strip the last commas
            Fields = StripLastCharacter(Fields)
            FieldValues = StripLastCharacter(FieldValues)

            Call ExecuteNonQuerySQL(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", TableName, Fields, FieldValues))

        Else
            ' Save locally for bulk insert
            Call TempDB.InsertRecord(TableName, Record)
        End If

    End Sub

    ''' <summary>
    ''' Finalizes the data import. If translation tables were used, they will be imported here.
    ''' </summary>
    ''' <param name="Translator">YAMLTranslations object to get stored tables from.</param>
    ''' <param name="TranslationTableImportList">List of translation tables to import.</param>
    Public Sub FinalizeDataImport(ByRef Translator As YAMLTranslations, ByVal TranslationTableImportList As List(Of String))
        Dim DBREf As SqlConnection = DBConnectionRef()
        Dim Tables As New List(Of DataTable)

        Tables = New List(Of DataTable)
        Tables = Translator.TranslationTables.GetTables

        Call InitalizeMainProgressBar(Tables.Count, "Importing Translation data...")

        ' Import the translation tables only if they were selected - otherwise skip
        For i = 0 To Translator.TranslationTables.GetTables.Count - 1
            If TranslationTableImportList.Contains(Tables(i).TableName) Then
                Using copy As New SqlBulkCopy(DBREf)
                    Call UpdateMainProgressBar(i, "Importing " & Tables(i).TableName)
                    ' Set up options
                    copy.BulkCopyTimeout = 0 ' Don't timeout
                    copy.BatchSize = 1000 ' 1000 rows at a time
                    ' Inserts should map directly to table
                    For j = 0 To Tables(i).Columns.Count - 1
                        copy.ColumnMappings.Add(j, j)
                    Next
                    copy.DestinationTableName = Tables(i).TableName
                    copy.WriteToServer(Tables(i))

                End Using
            End If
        Next

        Call ClearMainProgressBar()

        Tables = New List(Of DataTable)
        Tables = TempDB.GetTables
        Call InitalizeMainProgressBar(Tables.Count, "Importing Bulk data...")

        ' Now insert each table from files
        For i = 0 To Tables.Count - 1
            Using copy As New SqlBulkCopy(DBREf)
                Call UpdateMainProgressBar(i, "Importing " & Tables(i).TableName)
                ' Set up options
                copy.BulkCopyTimeout = 0 ' Don't timeout
                copy.BatchSize = 1000 ' 1000 rows at a time
                ' Inserts should map directly to table
                For j = 0 To Tables(i).Columns.Count - 1
                    copy.ColumnMappings.Add(j, j)
                Next
                copy.DestinationTableName = Tables(i).TableName
                copy.WriteToServer(Tables(i))

            End Using
        Next

        Call CloseDB(DBREf)
        Call ClearMainProgressBar()

    End Sub

End Class