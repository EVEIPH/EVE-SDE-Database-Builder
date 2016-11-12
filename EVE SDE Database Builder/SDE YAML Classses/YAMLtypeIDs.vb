
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLtypeIDs
    Inherits YAMLFilesBase

    Public Const typeIDsFile As String = "typeIDs.yaml"

    Private Const crtMasteries_Table As String = "crtMasteries"
    Private Const invTraits_Table As String = "invTraits"
    Private Const invTypes_Table As String = "invTypes"

    Private BonusIDCounter As Integer = 0 ' For assigning unique bonusIDs in traits

    Private PackagedItems As List(Of PackagedItem)
    Private PackagedItemtoFind As PackagedItem
    Private PackagedGroups As List(Of PackagedGroup)
    Private PackagedGrouptoFind As PackagedGroup

    Private Class PackagedItem
        Public TypeID As Integer
        Public PackagedVolume As Double
    End Class

    Private Class PackagedGroup
        Public GroupID As Integer
        Public PackagedVolume As Double
    End Class

    ' Predicate for finding a packaged category record
    Private Function FindPackagedVolumebyTypeID(ByVal Item As PackagedItem) As Boolean
        If Item.typeID = PackagedItemtoFind.typeID Then
            Return True
        Else
            Return False
        End If
    End Function

    ' Predicate for finding a packaged category record
    Private Function FindPackagedVolumebyGroup(ByVal Item As PackagedGroup) As Boolean
        If Item.GroupID = PackagedGrouptoFind.GroupID Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)

        ' Reset table name
        TableName = "invTypes"

        ' Load up the packaged groups for ships
        PackagedGroups = New List(Of PackagedGroup)
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 25, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 26, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 27, .PackagedVolume = 50000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 28, .PackagedVolume = 20000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 29, .PackagedVolume = 500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 30, .PackagedVolume = 10000000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 31, .PackagedVolume = 500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 237, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 324, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 358, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 380, .PackagedVolume = 20000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 381, .PackagedVolume = 50000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 419, .PackagedVolume = 15000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 420, .PackagedVolume = 5000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 463, .PackagedVolume = 3750})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 485, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 513, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 540, .PackagedVolume = 15000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 541, .PackagedVolume = 5000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 543, .PackagedVolume = 3750})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 547, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 659, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 830, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 831, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 832, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 833, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 834, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 883, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 893, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 894, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 898, .PackagedVolume = 50000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 900, .PackagedVolume = 50000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 902, .PackagedVolume = 1300000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 906, .PackagedVolume = 10000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 941, .PackagedVolume = 500000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 963, .PackagedVolume = 5000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 1022, .PackagedVolume = 500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 1201, .PackagedVolume = 15000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 1202, .PackagedVolume = 20000})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 1283, .PackagedVolume = 2500})
        PackagedGroups.Add(New PackagedGroup With {.GroupID = 1305, .PackagedVolume = 5000})

        ' Now add the container data
        PackagedItems = New List(Of PackagedItem)
        PackagedItems.Add(New PackagedItem With {.typeID = 3293, .PackagedVolume = 33})
        PackagedItems.Add(New PackagedItem With {.typeID = 3296, .PackagedVolume = 65})
        PackagedItems.Add(New PackagedItem With {.typeID = 3297, .PackagedVolume = 10})
        PackagedItems.Add(New PackagedItem With {.typeID = 3465, .PackagedVolume = 65})
        PackagedItems.Add(New PackagedItem With {.typeID = 3466, .PackagedVolume = 33})
        PackagedItems.Add(New PackagedItem With {.typeID = 3467, .PackagedVolume = 10})
        PackagedItems.Add(New PackagedItem With {.typeID = 11488, .PackagedVolume = 150})
        PackagedItems.Add(New PackagedItem With {.typeID = 11489, .PackagedVolume = 300})
        PackagedItems.Add(New PackagedItem With {.typeID = 17363, .PackagedVolume = 10})
        PackagedItems.Add(New PackagedItem With {.typeID = 17364, .PackagedVolume = 33})
        PackagedItems.Add(New PackagedItem With {.typeID = 17365, .PackagedVolume = 65})
        PackagedItems.Add(New PackagedItem With {.typeID = 17366, .PackagedVolume = 10000})
        PackagedItems.Add(New PackagedItem With {.typeID = 17367, .PackagedVolume = 50000})
        PackagedItems.Add(New PackagedItem With {.typeID = 17368, .PackagedVolume = 100000})
        PackagedItems.Add(New PackagedItem With {.typeID = 24445, .PackagedVolume = 1200})
        PackagedItems.Add(New PackagedItem With {.typeID = 33003, .PackagedVolume = 2500})
        PackagedItems.Add(New PackagedItem With {.typeID = 33005, .PackagedVolume = 5000})
        PackagedItems.Add(New PackagedItem With {.typeID = 33007, .PackagedVolume = 1000})
        PackagedItems.Add(New PackagedItem With {.typeID = 33009, .PackagedVolume = 500})
        PackagedItems.Add(New PackagedItem With {.typeID = 33011, .PackagedVolume = 100})

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

        Dim YAMLRecords As New Dictionary(Of Long, typeID)
        Dim DataFields As List(Of DBField)
        Dim CategoryName As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build all the tables to insert inventory data into. This includes the following tables:
        ' - invTypes
        ' - invTraits
        ' - crtMasteries
        Call BuildInventoryTypesTable()
        Call BuildInventoryTraitsTable()
        Call BuildCertificateMasteriesTable()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, typeID))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", .groupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeName", NameTranslation.GetLanguageTranslationData(.name), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", NameTranslation.GetLanguageTranslationData(.description), FieldType.text_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("mass", .mass, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("volume", .volume, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("packagedVolume", GetPackagedVolume(DataField.Key, .groupID, .volume), FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("capacity", .capacity, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("portionSize", .portionSize, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("basePrice", .basePrice, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("marketGroupID", .marketGroupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", .graphicID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("soundID", .soundID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofFactionName", .sofFactionName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofMaterialSetID", .sofMaterialSetID, FieldType.int_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "typeID", "typeName", invTypes_Table, NameTranslation.GetAllTranslations(.name))
                Call Translator.InsertTranslationData(DataField.Key, "typeID", "description", invTypes_Table, NameTranslation.GetAllTranslations(.description))

                ' Insert the traits and masteries
                Call InsertTraits(DataField.Key, .traits, Params.ImportLanguageCode)
                Call InsertMasteries(DataField.Key, .masteries)

            End With

            Call UpdateDB.InsertRecord(invTypes_Table, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub BuildInventoryTypesTable()
        Dim IndexFields As List(Of String)
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.text_type, MaxFieldLen, True))
        Table.Add(New DBTableField("mass", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("volume", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("packagedVolume", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("capacity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("portionSize", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("raceID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("basePrice", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("published", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("marketGroupID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("soundID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("sofFactionName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("sofMaterialSetID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(invTypes_Table, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("groupID")
        Call UpdateDB.CreateIndex(invTypes_Table, "IDX_" & invTypes_Table & "_GID", IndexFields)

    End Sub

    Private Sub BuildInventoryTraitsTable()
        Dim IndexFields As List(Of String)
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("bonusID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("skilltypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("bonus", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("bonusText", FieldType.text_type, MaxFieldLen, True))
        Table.Add(New DBTableField("importance", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("nameID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("unitID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(invTraits_Table, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("typeID")
        Call UpdateDB.CreateIndex(invTraits_Table, "IDX_" & invTraits_Table & "_TID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("bonusID")
        Call UpdateDB.CreateIndex(invTraits_Table, "IDX_" & invTraits_Table & "_BID", IndexFields)

    End Sub

    Private Sub BuildCertificateMasteriesTable()
        Dim IndexFields As List(Of String)
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("masteryLevel", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("masteryRecommendedTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(crtMasteries_Table, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("typeID")
        Call UpdateDB.CreateIndex(crtMasteries_Table, "IDX_" & crtMasteries_Table & "_TID", IndexFields)

    End Sub

    Private Sub InsertMasteries(ByVal TypeID As Integer, ByVal SentMasteries As Dictionary(Of Long, Object))
        Dim DataFields As List(Of DBField)

        If Not IsNothing(SentMasteries) Then
            For Each Mastery In SentMasteries
                For Each ID In Mastery.Value
                    DataFields = New List(Of DBField)

                    DataFields.Add(UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("masteryLevel", Mastery.Key + 1, FieldType.tinyint_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("masteryRecommendedTypeID", ID, FieldType.int_type))

                    Call UpdateDB.InsertRecord(crtMasteries_Table, DataFields)
                Next
            Next
        End If

    End Sub

    Private Sub InsertTraits(ByVal TypeID As Integer, ByVal Traits As typeID.itemTraits, Language As LanguageCode)
        Dim DataFields As List(Of DBField)

        Dim NameTranslation As New ImportLanguage(Language)

        If Not IsNothing(Traits) Then
            ' Role bonuses
            If Not IsNothing(Traits.roleBonuses) Then
                BonusIDCounter += 1
                For Each R_bonus In Traits.roleBonuses
                    DataFields = New List(Of DBField)

                    DataFields.Add(UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skilltypeID", -1, FieldType.int_type)) ' -1 for role bonuses that don't have skills associated with them
                    DataFields.Add(UpdateDB.BuildDatabaseField("bonus", R_bonus.bonus, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(R_bonus.bonusText), FieldType.text_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("importance", R_bonus.importance, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("nameID", R_bonus.nameID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("unitID", R_bonus.unitID, FieldType.int_type))

                    Call UpdateDB.InsertRecord(invTraits_Table, DataFields)

                    ' Insert translation field data
                    Call Translator.InsertTranslationData(TypeID, "bonusID", "bonusText", invTraits_Table, NameTranslation.GetAllTranslations(R_bonus.bonusText))

                Next
            End If

            ' Misc bonuses
            If Not IsNothing(Traits.miscBonuses) Then
                BonusIDCounter += 1
                For Each M_bonus In Traits.miscBonuses
                    DataFields = New List(Of DBField)

                    DataFields.Add(UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skilltypeID", -1, FieldType.int_type)) ' -1 for misc bonuses that don't have skills associated with them
                    DataFields.Add(UpdateDB.BuildDatabaseField("bonus", M_bonus.bonus, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(M_bonus.bonusText), FieldType.text_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("importance", M_bonus.importance, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("nameID", M_bonus.nameID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("unitID", M_bonus.unitID, FieldType.int_type))

                    Call UpdateDB.InsertRecord(invTraits_Table, DataFields)

                    ' Insert translation field data
                    Call Translator.InsertTranslationData(TypeID, "bonusID", "bonusText", invTraits_Table, NameTranslation.GetAllTranslations(M_bonus.bonusText))

                Next
            End If

            ' Skill bonuses
            If Not IsNothing(Traits.types) Then
                BonusIDCounter += 1
                For Each TypeTrait In Traits.types
                    For Each S_bonus In TypeTrait.Value
                        DataFields = New List(Of DBField)

                        DataFields.Add(UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("skilltypeID", TypeTrait.Key, FieldType.int_type)) ' -1 for misc bonuses that don't have skills associated with them
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonus", S_bonus.bonus, FieldType.float_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(S_bonus.bonusText), FieldType.text_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("importance", S_bonus.importance, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("nameID", S_bonus.nameID, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("unitID", S_bonus.unitID, FieldType.int_type))

                        Call UpdateDB.InsertRecord(invTraits_Table, DataFields)

                        ' Insert translation field data
                        Call Translator.InsertTranslationData(TypeID, "bonusID", "bonusText", invTraits_Table, NameTranslation.GetAllTranslations(S_bonus.bonusText))

                    Next
                Next
            End If
        End If
    End Sub

    Private Function GetPackagedVolume(ByVal TypeID As Integer, ByVal GroupID As Integer, ByVal DefaultVolume As Object) As Object
        Dim FoundItem As PackagedItem = Nothing
        Dim FoundGroup As New PackagedGroup

        If IsNothing(DefaultVolume) Then
            Return Nothing
        End If

        ' Look up typeID first
        PackagedItemtoFind = New PackagedItem With {.TypeID = TypeID, .PackagedVolume = 0}
        FoundItem = PackagedItems.Find(AddressOf FindPackagedVolumebyTypeID)

        If FoundItem IsNot Nothing Then
            Return FoundItem.PackagedVolume
        Else
            ' Look up group
            PackagedGrouptoFind = New PackagedGroup With {.GroupID = TypeID, .PackagedVolume = 0}
            FoundGroup = PackagedGroups.Find(AddressOf FindPackagedVolumebyGroup)

            If FoundGroup IsNot Nothing Then
                Return FoundGroup.PackagedVolume
            Else
                Return DefaultVolume
            End If
        End If

    End Function
End Class

Public Class typeID
    Public Property groupID As Object
    Public Property name As Translations
    Public Property description As Translations
    Public Property mass As Object
    Public Property volume As Object
    Public Property capacity As Object
    Public Property portionSize As Object
    Public Property factionID As Object
    Public Property raceID As Object
    Public Property basePrice As Object
    Public Property published As Object
    Public Property marketGroupID As Object
    Public Property graphicID As Object
    Public Property radius As Object
    Public Property iconID As Object
    Public Property soundID As Object
    Public Property sofFactionName As Object
    Public Property sofMaterialSetID As Object

    Public Property masteries As Dictionary(Of Long, Object)
    Public Property traits As itemTraits

    Public Class itemTraits
        Public Property roleBonuses As List(Of bonus)
        Public Property types As Dictionary(Of Long, List(Of bonus)) ' for skill bonuses to ships
        Public Property miscBonuses As List(Of bonus) ' For things like T3 destroyers
    End Class

    Public Class bonus
        Public Property bonus As Object
        Public Property bonusText As Translations
        Public Property importance As Object
        Public Property nameID As Object
        Public Property unitID As Object
    End Class
End Class


