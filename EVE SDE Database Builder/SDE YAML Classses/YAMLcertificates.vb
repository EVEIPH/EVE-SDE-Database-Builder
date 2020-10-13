
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLcertificates
    Inherits YAMLFilesBase

    Public Const certificatesFile As String = "certificates.yaml"

    Private Const crtCertificates_Table As String = "crtCertificates"
    Private Const crtCertificateSkills_Table As String = "crtCertificateSkills"
    Private Const crtRecommendedTypes_Table As String = "crtRecommendedTypes"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, certificate)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build all the tables to insert certificate data into. This includes the following tables:
        ' - crtCertificates
        ' - crtCertificateSkills
        ' - crtRecommendedTypes
        ' Note: crtCertificateMasteries is loaded from the blueprints.yaml
        Call BuildCertificatesTable()
        Call BuildCertificateSkillsTable()
        Call BuildCertificateRecTypesTable()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, certificate))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list for certificates
                DataFields.Add(UpdateDB.BuildDatabaseField("certificateID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", .groupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", .name, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.text_type))


                Call UpdateDB.InsertRecord(crtCertificates_Table, DataFields)

                ' Insert skills and required types
                Call InsertCertificateSkills(DataField.Key, .skillTypes)
                Call InsertCertificateRecommendedTypes(DataField.Key, .recommendedFor)

            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub BuildCertificatesTable()

        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("certificateID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("name", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.text_type, MaxFieldLen, True))

        Call UpdateDB.CreateTable(crtCertificates_Table, Table)

    End Sub

    Private Sub BuildCertificateSkillsTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("certificateID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("masteryLevel", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("masteryText", FieldType.varchar_type, 10, True))
        Table.Add(New DBTableField("skillTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("requiredSkillLevel", FieldType.tinyint_type, 0, True))

        Call UpdateDB.CreateTable(crtCertificateSkills_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("certificateID")
        Call UpdateDB.CreateIndex(crtCertificateSkills_Table, "IDX_" & crtCertificateSkills_Table & "_CID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("skillTypeID")
        Call UpdateDB.CreateIndex(crtCertificateSkills_Table, "IDX_" & crtCertificateSkills_Table & "_SID", IndexFields)

    End Sub

    Private Sub BuildCertificateRecTypesTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("certificateID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(crtRecommendedTypes_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("typeID")
        IndexFields.Add("certificateID")
        Call UpdateDB.CreateIndex(crtRecommendedTypes_Table, "IDX_" & crtRecommendedTypes_Table & "_TID_CID", IndexFields)

    End Sub

    Private Sub InsertCertificateSkills(ByVal CertID As Integer, ByVal Skills As Dictionary(Of Long, certificate.certificateMastery))
        Dim DataFields As List(Of DBField)

        Dim SkillTypeID As Integer
        Dim MasteryLevel As Integer
        Dim MasteryText As String
        Dim ReqSkillLevel As Integer

        If Not IsNothing(Skills) Then
            For Each Skill In Skills
                With Skill.Value
                    ' Build the insert list for certificate skills
                    For i = 0 To 4 ' 5 loops for the masteries
                        DataFields = New List(Of DBField)
                        DataFields.Add(UpdateDB.BuildDatabaseField("certificateID", CertID, FieldType.int_type))
                        SkillTypeID = Skill.Key
                        MasteryLevel = i + 1
                        MasteryText = ""
                        ReqSkillLevel = 0
                        ' Just select these based off of the loop
                        Select Case i
                            Case 0 ' basic
                                MasteryText = "Basic"
                                ReqSkillLevel = Skill.Value.basic
                            Case 1 ' basic
                                MasteryText = "Standard"
                                ReqSkillLevel = Skill.Value.standard
                            Case 2 ' basic
                                MasteryText = "Improved"
                                ReqSkillLevel = Skill.Value.improved
                            Case 3 ' basic
                                MasteryText = "Advanced"
                                ReqSkillLevel = Skill.Value.advanced
                            Case 4 ' basic
                                MasteryText = "Elite"
                                ReqSkillLevel = Skill.Value.elite
                        End Select

                        If MasteryText <> "" Then
                            DataFields.Add(UpdateDB.BuildDatabaseField("masteryLevel", MasteryLevel, FieldType.tinyint_type))
                            DataFields.Add(UpdateDB.BuildDatabaseField("masteryText", MasteryText, FieldType.varchar_type))
                            DataFields.Add(UpdateDB.BuildDatabaseField("skillTypeID", SkillTypeID, FieldType.int_type))
                            DataFields.Add(UpdateDB.BuildDatabaseField("requiredSkillLevel", ReqSkillLevel, FieldType.tinyint_type))
                        End If

                        Call UpdateDB.InsertRecord(crtCertificateSkills_Table, DataFields)

                    Next
                End With
            Next
        End If

    End Sub

    Private Sub InsertCertificateRecommendedTypes(ByVal CertID As Integer, ByVal TypeList As List(Of Object))
        Dim DataFields As List(Of DBField)

        If Not IsNothing(TypeList) Then
            For Each recType In TypeList
                DataFields = New List(Of DBField)

                ' Build the insert list for recomended types
                DataFields.Add(UpdateDB.BuildDatabaseField("certificateID", CertID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", recType, FieldType.int_type))

                Call UpdateDB.InsertRecord(crtRecommendedTypes_Table, DataFields)
            Next
        End If
    End Sub

End Class

Public Class certificate
    Public Property description As Object
    Public Property groupID As Object
    Public Property name As Object
    Public Property recommendedFor As List(Of Object)
    ' key will be the skill ID and each mastery after that
    Public Property skillTypes As Dictionary(Of Long, certificateMastery)

    Public Class certificateMastery
        Public Property advanced As Object
        Public Property basic As Object
        Public Property elite As Object
        Public Property improved As Object
        Public Property standard As Object
    End Class

End Class

