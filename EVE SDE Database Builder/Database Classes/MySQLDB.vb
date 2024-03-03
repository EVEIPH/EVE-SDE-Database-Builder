
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Collections.Concurrent

' Class to support MySQL Server Database
''' <summary>
''' Class to create a MySQL Database and insert data into it.
''' </summary>
Public Class MySQLDB
    Inherits DBFilesBase

    Private ReadOnly BulkInsertTablesData As ConcurrentQueue(Of BulkInsertData)

    ' Save the database information for later connections
    Private ReadOnly DBServerName As String
    Private ReadOnly DBUserID As String
    Private ReadOnly DBUserPassword As String

    Private Const DBConnectionString As String = "Server={0}; userid={1}; password={2}; pooling=false; default command timeout=600; AllowLoadLocalInfile=true;"

    ' For doing bulk data inserts
    Private Structure BulkInsertData
        Dim BulkImportSQL As String
        Dim TableName As String
    End Structure

    ''' <summary>
    ''' Constructor class for a MySQL database. Class connects to the default
    ''' MySQL server with root UserID and password. If the database name sent 
    ''' does not exist, will create the database on the server.
    ''' </summary>
    ''' <param name="DatabaseName">Name of the database to create.</param>
    ''' <param name="ServerName">Name of the postgreSQL server (e.g. localhost')</param>
    ''' <param name="UserID">Root user name or user name with account access to create databases, tables, etc.</param>
    ''' <param name="Password">Root password or password of account with access to create databases, tables, etc.</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseName As String, ByVal ServerName As String, ByVal UserID As String,
                   ByVal Password As String, ByVal Port As String, ByRef Success As Boolean)
        MyBase.New(DatabaseName, DatabaseType.MySQL)

        Dim DB As New MySqlConnection

        BulkInsertTablesData = New ConcurrentQueue(Of BulkInsertData)
        CSVDirectory = ""

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try
            ' Set the DB Instance and Name data for later use
            MainDatabase = DatabaseName
            DBServerName = ServerName
            DBUserID = UserID
            DBUserPassword = Password

            If Port <> "" Then
                Port = " port=" & Port & ";"
            End If

            DB = New MySqlConnection(String.Format(DBConnectionString, DBServerName, DBUserID, DBUserPassword) & Port)
            DB.Open()

            ' See if the Databae exists first and create if not
            Dim Command As New MySqlCommand(String.Format("SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", DatabaseName), DB)

            If IsNothing(Command.ExecuteScalar) Then
                Command = New MySqlCommand("CREATE DATABASE " & MainDatabase, DB)
                Command.ExecuteNonQuery()
                Command.Dispose()
            End If

            Call CloseDB(DB)
            Success = True

        Catch ex As Exception
            Call CloseDB(DB)
            Call ShowErrorMessage(ex)
            Success = False
        End Try

        Call ClearMainProgressBar()

    End Sub

    ''' <summary>
    ''' Opens a new database connection for reference and returns it.
    ''' </summary>
    ''' <returns>Returns a MySqlConnection for use.</returns>
    Private Function DBConnectionRef() As MySqlConnection

        ' Open the connection for reference
        Dim DBRef As New MySqlConnection(String.Format(DBConnectionString, DBServerName, DBUserID, DBUserPassword))

        DBRef.Open()


        ' Set the database
        DBRef.ChangeDatabase(MainDatabase)

        Return DBRef

    End Function

    ''' <summary>
    ''' Closes and disposes the referenced database.
    ''' </summary>
    ''' <param name="RefDB">The database to close.</param>
    Public Sub CloseDB(ByRef RefDB As MySqlConnection)
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
        Dim DBRef As MySqlConnection = DBConnectionRef()
        Dim Command As MySqlCommand
        Command = New MySqlCommand(SQL, DBRef)
        Command.ExecuteNonQuery()
        Command.Dispose()
        DBRef.Close()

    End Sub

    ''' <summary>
    ''' Gets the CSV Directory for bulk inserts. MYSQL has a specific upload folder to use
    ''' </summary>
    Public Function GetCSVDirectory() As String
        Dim DBRef As MySqlConnection = DBConnectionRef()
        Dim Command As New MySqlCommand("SELECT @@global.secure_file_priv", DBRef)

        CSVDirectory = Command.ExecuteScalar().ToString()

        DBRef.Close()

        Return CSVDirectory

    End Function

    ''' <summary>
    ''' Begins an SQL transaction on the server
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Call ExecuteNonQuerySQL("START TRANSACTION;")
    End Sub

    ''' <summary>
    ''' Commits an SQL transaction on the server
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Call ExecuteNonQuerySQL("COMMIT;")
    End Sub

    ''' <summary>
    ''' Rollsback an SQL transaction on the server
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Call ExecuteNonQuerySQL("ROLLBACK;")
    End Sub

    ''' <summary>
    ''' Drops the tablename sent, if it exists on the database.
    ''' </summary>
    ''' <param name="TableName">Table you want to drop</param>
    Private Sub DropTable(TableName As String)
        Call ExecuteNonQuerySQL("DROP TABLE IF EXISTS " & TableName)
    End Sub

    ''' <summary>
    ''' Creates a table on the server for the sent table name in the structure sent by table structure
    ''' </summary>
    ''' <param name="TableName">Name of the table to create.</param>
    ''' <param name="TableStructure">List of table fields that define the table.</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim SQL As String
        Dim FieldLength As String = "0" ' For char types
        Dim PKFields As New List(Of String)

        Dim FieldList As String = ""

        ' Drop table first
        Call DropTable(TableName)

        ' Now build the SQL and execute
        SQL = String.Format("CREATE TABLE {0} (", TableName)

        ' Add fields
        For Each Field In TableStructure
            With Field
                ' Add field name
                SQL &= .FieldName & SPACE

                ' Set field length
                If .FieldLength <> -1 Then
                    FieldLength = CStr(.FieldLength)
                End If

                Select Case .FieldType
                    ' Format any strings
                    Case FieldType.char_type, FieldType.nchar_type
                        SQL &= "CHAR(" & FieldLength & ")"
                    Case FieldType.varchar_type, FieldType.nvarchar_type
                        SQL &= "VARCHAR(" & FieldLength & ")"
                    Case FieldType.text_type, FieldType.ntext_type
                        SQL &= "LONGTEXT"
                    Case FieldType.double_type
                        SQL &= "DOUBLE"
                    Case FieldType.float_type, FieldType.real_type
                        SQL &= "FLOAT"
                    Case FieldType.tinyint_type, FieldType.bit_type
                        SQL &= "TINYINT UNSIGNED"
                    Case FieldType.smallint_type
                        SQL &= "SMALLINT"
                    Case FieldType.int_type
                        SQL &= "INT"
                    Case FieldType.bigint_type
                        SQL &= "BIGINT"
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

                FieldList = FieldList & .FieldName & COMMA

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

        SQL &= ") ENGINE=MyISAM;" ' use MyISAM for faster bulk uploads

        Call ExecuteNonQuerySQL(SQL)

        ' Strip comma
        FieldList = "(" & StripLastCharacter(FieldList) & ")"

        ' Insert the bulk data insert string for bulk insert later
        Dim TempData As BulkInsertData
        TempData.BulkImportSQL = String.Format("LOAD DATA LOCAL INFILE '{0}{1}.csv' INTO TABLE {1} " +
                                 "FIELDS TERMINATED BY ',' ENCLOSED BY '""' " +
                                 "LINES TERMINATED BY '\r\n' IGNORE 1 LINES {2}",
                                 CSVDirectory.Replace("\", "/"), TableName, FieldList)
        TempData.TableName = TableName

        BulkInsertTablesData.Enqueue(TempData)

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
        Dim SQL As String

        SQL = "CREATE" & SPACE

        ' Unique index
        If Unique Then
            SQL &= "UNIQUE" & SPACE
        End If

        SQL &= String.Format("INDEX {0} ON {1} (", IndexName, TableName)

        ' Build index fields
        For Each Field In IndexFields
            SQL &= Field & COMMA
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
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Dim SQL As String
        Dim Fields As String = ""
        Dim FieldValues As String = ""

        For Each Field In Record
            Fields = Fields & Field.FieldName & COMMA
            FieldValues = FieldValues & Field.FieldValue & COMMA
        Next

        ' Strip the last commas
        Fields = Fields.Substring(0, Len(Fields) - 1)
        FieldValues = FieldValues.Substring(0, Len(FieldValues) - 1)

        SQL = String.Format("INSERT DELAYED INTO {0} ({1}) VALUES ({2})", TableName, Fields, FieldValues)

        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Reads the list of tables created in Create Table method and does a CSV insert into the database.
    ''' </summary>
    Public Sub FinalizeDataImport(ByRef Translator As YAMLTranslations, ByVal TranslationTableImportList As List(Of String))

        Call InitalizeMainProgressBar(BulkInsertTablesData.Count, "Importing Bulk Data...")

        For i = 0 To BulkInsertTablesData.Count - 1
            Call UpdateMainProgressBar(i, "Importing " & BulkInsertTablesData(i).TableName & "...")
            Application.DoEvents()
            Call BeginSQLTransaction()
            Call ExecuteNonQuerySQL("SET GLOBAL local_infile=1;") ' Server setting to allow local uploads of data
            Call ExecuteNonQuerySQL(String.Format("ALTER TABLE {0} DISABLE KEYS", BulkInsertTablesData(i).TableName))
            Call ExecuteNonQuerySQL("SET autocommit = 1;set unique_checks = 0;set foreign_key_checks = 0;set sql_log_bin=0;")
            Call ExecuteNonQuerySQL(BulkInsertTablesData(i).BulkImportSQL)
            Call ExecuteNonQuerySQL(String.Format("ALTER TABLE {0} ENABLE KEYS", BulkInsertTablesData(i).TableName))
            Call CommitSQLTransaction()

            ' Since we are done, delete the csv file we just imported
            Call File.Delete(CSVDirectory & BulkInsertTablesData(i).TableName & ".csv")

        Next

        Call ClearMainProgressBar()

    End Sub

End Class