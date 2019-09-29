
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLblueprints
    Inherits YAMLFilesBase

    Public Const blueprintsFile As String = "blueprints.yaml"

    ' These are hard coded here but can be looked up in ramActivities
    Private Const ActivityManufacturing As Integer = 1
    Private Const ActivityResearchTime As Integer = 3
    Private Const ActivityResearchMaterial As Integer = 4
    Private Const ActivityCopying As Integer = 5
    Private Const ActivityReverseEngineering As Integer = 7
    Private Const ActivityInvention As Integer = 8
    Private Const ActivityReaction As Integer = 11

    Private Const industryBlueprints_Table As String = "industryBlueprints"
    Private Const industryActivities_Table As String = "industryActivities"
    Private Const industryActivityMaterials_Table As String = "industryActivityMaterials"
    Private Const industryActivityProducts_Table As String = "industryActivityProducts"
    Private Const industryActivitySkills_Table As String = "industryActivitySkills"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        Dim DSB = New DeserializerBuilder()
        DSB.IgnoreUnmatchedProperties()
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, Blueprint)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build all the tables to insert blueprint data into. This includes the following tables:
        ' - industryBlueprints
        ' - industryActivities
        ' - industryActivityMaterials
        ' - industryActivityProducts (includes probability for invention)
        ' - industryActivitySkills
        Call BuildIndustryBlueprintsTable()
        Call BuildIndustryActivitiesTable()
        Call BuildIndustryActivityMaterialsTable()
        Call BuildIndustryActivityProductsTable()
        Call BuildIndustryActivitySkillsTable()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, Blueprint))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Insert blueprint record first
                DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maxProductionLimit", .maxProductionLimit, FieldType.int_type))
                Call UpdateDB.InsertRecord(industryBlueprints_Table, DataFields)

                ' Now upload activities
                If Not IsNothing(.activities.copying) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityCopying, .activities.copying.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.copying.materials, ActivityCopying)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.copying.skills, ActivityCopying)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.copying.products, ActivityCopying)
                End If

                If Not IsNothing(.activities.invention) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityInvention, .activities.invention.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.invention.materials, ActivityInvention)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.invention.skills, ActivityInvention)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.invention.products, ActivityInvention)
                End If

                If Not IsNothing(.activities.manufacturing) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityManufacturing, .activities.manufacturing.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.manufacturing.materials, ActivityManufacturing)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.manufacturing.skills, ActivityManufacturing)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.manufacturing.products, ActivityManufacturing)
                End If

                If Not IsNothing(.activities.research_material) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityResearchMaterial, .activities.research_material.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.research_material.materials, ActivityResearchMaterial)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.research_material.skills, ActivityResearchMaterial)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.research_material.products, ActivityResearchMaterial)
                End If

                If Not IsNothing(.activities.research_time) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityResearchTime, .activities.research_time.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.research_time.materials, ActivityResearchTime)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.research_time.skills, ActivityResearchTime)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.research_time.products, ActivityResearchTime)
                End If

                If Not IsNothing(.activities.reaction) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityReaction, .activities.reaction.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.reaction.materials, ActivityReaction)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.reaction.skills, ActivityReaction)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.reaction.products, ActivityReaction)
                End If
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub InsertBlueprintActivity(ByVal BPID As Integer, ByVal ActivityID As Integer, ByVal ActivityTime As Integer)
        Dim DataFields As New List(Of DBField)

        DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.tinyint_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("time", ActivityTime, FieldType.tinyint_type))

        Call UpdateDB.InsertRecord(industryActivities_Table, DataFields)

    End Sub

    Private Sub InsertBlueprintMaterials(ByVal BPID As Integer, ByVal Materials As List(Of Blueprint.Material), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Materials) Then
            For Each mat In Materials
                DataFields = New List(Of DBField)

                ' Insert material record into industryActivityMaterials
                DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("materialTypeID", mat.typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("quantity", mat.quantity, FieldType.tinyint_type))

                Call UpdateDB.InsertRecord(industryActivityMaterials_Table, DataFields)
            Next
        End If

    End Sub

    Private Sub InsertBlueprintSkills(ByVal BPID As Integer, ByVal Skills As List(Of Blueprint.Skill), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Skills) Then
            For Each Skill In Skills
                DataFields = New List(Of DBField)

                ' Insert material record into industryActivityMaterials
                DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("skillID", Skill.typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("level", Skill.level, FieldType.tinyint_type))

                Call UpdateDB.InsertRecord(industryActivitySkills_Table, DataFields)
            Next
        End If

    End Sub

    Private Sub InsertBlueprintProducts(ByVal BPID As Integer, ByVal Products As List(Of Blueprint.Product), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Products) Then
            For Each Product In Products
                DataFields = New List(Of DBField)

                ' Insert material record into industryActivityMaterials
                DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("productTypeID", Product.typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("quantity", Product.quantity, FieldType.int_type))
                If IsNothing(Product.probability) Then
                    ' Set it to 1 for 100% probability
                    Product.probability = 1
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("probability", Product.probability, FieldType.float_type))

                Call UpdateDB.InsertRecord(industryActivityProducts_Table, DataFields)
            Next
        End If

    End Sub

    Private Sub BuildIndustryBlueprintsTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("blueprintTypeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("maxProductionLimit", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(industryBlueprints_Table, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("blueprintTypeID")
        Call UpdateDB.CreateIndex(industryBlueprints_Table, "IDX_" & industryBlueprints_Table & "_BPID", IndexFields)

    End Sub

    Private Sub BuildIndustryActivitiesTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("blueprintTypeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("time", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(industryActivities_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("activityID")
        Call UpdateDB.CreateIndex(industryActivities_Table, "IDX_" & industryActivities_Table & "_AID", IndexFields)

    End Sub

    Private Sub BuildIndustryActivityMaterialsTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("blueprintTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("materialTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("quantity", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(industryActivityMaterials_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("blueprintTypeID")
        IndexFields.Add("activityID")
        Call UpdateDB.CreateIndex(industryActivityMaterials_Table, "IDX_" & industryActivityMaterials_Table & "_TID_AID", IndexFields)

    End Sub

    Private Sub BuildIndustryActivitySkillsTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("blueprintTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("skillID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("level", FieldType.tinyint_type, 0, True))

        Call UpdateDB.CreateTable(industryActivitySkills_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("blueprintTypeID")
        IndexFields.Add("activityID")
        Call UpdateDB.CreateIndex(industryActivitySkills_Table, "IDX_" & industryActivitySkills_Table & "_TID_AID", IndexFields)

    End Sub

    Private Sub BuildIndustryActivityProductsTable()
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("blueprintTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("productTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("quantity", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("probability", FieldType.float_type, 0, True))

        Call UpdateDB.CreateTable(industryActivityProducts_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("blueprintTypeID")
        IndexFields.Add("activityID")
        Call UpdateDB.CreateIndex(industryActivityProducts_Table, "IDX_" & industryActivityProducts_Table & "_TID_AID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("productTypeID")
        Call UpdateDB.CreateIndex(industryActivityProducts_Table, "IDX_" & industryActivityProducts_Table & "_PTID", IndexFields)

    End Sub

End Class

' Class to parse the blueprints.yaml file
Public Class Blueprint

    Public Property activities As BlueprintActivities
    Public Property blueprintTypeID As Object
    Public Property maxProductionLimit As Object

    Public Class BlueprintActivities
        Public Property copying As CopyingActivity
        Public Property invention As InventionActivity
        Public Property manufacturing As ManufacturingActivity
        Public Property research_material As ResearchMaterialActivity
        Public Property research_time As ResearchTimeActivity
        Public Property reaction As ReactionActivity
    End Class

    Public Class CopyingActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Object
    End Class

    Public Class InventionActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Object
    End Class

    Public Class ManufacturingActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Object
    End Class

    Public Class ResearchMaterialActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Long
    End Class

    Public Class ResearchTimeActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Object
    End Class

    Public Class ReactionActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Object
    End Class

    Public Class Material
        Public Property quantity As Object
        Public Property typeID As Object
    End Class

    Public Class Skill
        Public Property level As Integer
        Public Property typeID As Long
    End Class

    Public Class Product
        Public Property probability As Object
        Public Property quantity As Object
        Public Property typeID As Object
    End Class

End Class