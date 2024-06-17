
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLtypes
    Inherits YAMLFilesBase

    Public Const typesFile As String = "types.yaml"

    Private Const crtMasteries_Table As String = "crtMasteries"
    Private Const invTraits_Table As String = "invTraits"
    Private Const invTypes_Table As String = "invTypes"

    Private BonusIDCounter As Integer = 0 ' For assigning unique bonusIDs in traits but linking to translation tables

    Private ReadOnly PackagedItems As List(Of PackagedItem)
    Private PackagedItemtoFind As PackagedItem
    Private ReadOnly PackagedGroups As List(Of PackagedGroup)
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
        If Item.TypeID = PackagedItemtoFind.TypeID Then
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
        PackagedGroups = New List(Of PackagedGroup) From {
            New PackagedGroup With {.GroupID = 25, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 26, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 27, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 28, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 29, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 30, .PackagedVolume = 10000000},
            New PackagedGroup With {.GroupID = 31, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 237, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 324, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 358, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 380, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 381, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 419, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 420, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 463, .PackagedVolume = 3750},
            New PackagedGroup With {.GroupID = 485, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 513, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 540, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 541, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 543, .PackagedVolume = 3750},
            New PackagedGroup With {.GroupID = 547, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 659, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 830, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 831, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 832, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 833, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 834, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 883, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 893, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 894, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 898, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 900, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 902, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 906, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 941, .PackagedVolume = 500000},
            New PackagedGroup With {.GroupID = 963, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1022, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 1201, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 1202, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 1283, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 1305, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1527, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 1534, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1538, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 1972, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 2001, .PackagedVolume = 2500}
        }

        ' Now add the container data
        PackagedItems = New List(Of PackagedItem) From {
            New PackagedItem With {.TypeID = 3293, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 3296, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 3297, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 3465, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 3466, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 3467, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 11488, .PackagedVolume = 150},
            New PackagedItem With {.TypeID = 11489, .PackagedVolume = 300},
            New PackagedItem With {.TypeID = 17363, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 17364, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 17365, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 17366, .PackagedVolume = 10000},
            New PackagedItem With {.TypeID = 17367, .PackagedVolume = 50000},
            New PackagedItem With {.TypeID = 17368, .PackagedVolume = 100000},
            New PackagedItem With {.TypeID = 24445, .PackagedVolume = 1200},
            New PackagedItem With {.TypeID = 33003, .PackagedVolume = 2500},
            New PackagedItem With {.TypeID = 33005, .PackagedVolume = 5000},
            New PackagedItem With {.TypeID = 33007, .PackagedVolume = 1000},
            New PackagedItem With {.TypeID = 33009, .PackagedVolume = 500},
            New PackagedItem With {.TypeID = 33011, .PackagedVolume = 100}
        }

    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = typesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, typeID)
        Dim DataFields As List(Of DBField)
        Dim CategoryName As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long

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
                DataFields.Add(UpdateDB.BuildDatabaseField("mass", .mass, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("volume", .volume, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("packagedVolume", GetPackagedVolume(DataField.Key, .groupID, .volume), FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("capacity", .capacity, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("portionSize", .portionSize, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("basePrice", .basePrice, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("marketGroupID", .marketGroupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", .graphicID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("soundID", .soundID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofFactionName", .sofFactionName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofMaterialSetID", .sofMaterialSetID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("metaGroupID", .metaGroupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("variationparentTypeID", .variationParentTypeID, FieldType.int_type))

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

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub BuildInventoryTypesTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("groupID", FieldType.int_type, 0, True),
            New DBTableField("typeName", FieldType.nvarchar_type, 500, True),
            New DBTableField("description", FieldType.text_type, MaxFieldLen, True),
            New DBTableField("mass", FieldType.real_type, 0, True),
            New DBTableField("volume", FieldType.real_type, 0, True),
            New DBTableField("packagedVolume", FieldType.real_type, 0, True),
            New DBTableField("capacity", FieldType.real_type, 0, True),
            New DBTableField("portionSize", FieldType.int_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("raceID", FieldType.tinyint_type, 0, True),
            New DBTableField("basePrice", FieldType.real_type, 0, True),
            New DBTableField("published", FieldType.bit_type, 0, True),
            New DBTableField("marketGroupID", FieldType.int_type, 0, True),
            New DBTableField("graphicID", FieldType.int_type, 0, True),
            New DBTableField("radius", FieldType.real_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("soundID", FieldType.int_type, 0, True),
            New DBTableField("sofFactionName", FieldType.nvarchar_type, 100, True),
            New DBTableField("sofMaterialSetID", FieldType.int_type, 0, True),
            New DBTableField("metaGroupID", FieldType.int_type, 0, True),
            New DBTableField("variationparentTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(invTypes_Table, Table)

        ' Create indexes
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(invTypes_Table, "IDX_" & invTypes_Table & "_TID", IndexFields)

        IndexFields = New List(Of String) From {
            "groupID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_GID", IndexFields)

        IndexFields = New List(Of String) From {
            "marketGroupID"
        }
        Call UpdateDB.CreateIndex(invTypes_Table, "IDX_" & invTypes_Table & "_MGID", IndexFields)

    End Sub

    Private Sub BuildInventoryTraitsTable()
        Dim IndexFields As List(Of String)
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("bonusID", FieldType.int_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("skilltypeID", FieldType.int_type, 0, True),
            New DBTableField("bonus", FieldType.real_type, 0, True),
            New DBTableField("bonusText", FieldType.text_type, MaxFieldLen, True),
            New DBTableField("importance", FieldType.int_type, 0, True),
            New DBTableField("nameID", FieldType.int_type, 0, True),
            New DBTableField("unitID", FieldType.int_type, 0, True),
            New DBTableField("isPositive", FieldType.bit_type, 0, True)
        }

        Call UpdateDB.CreateTable(invTraits_Table, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(invTraits_Table, "IDX_" & invTraits_Table & "_TID", IndexFields)

        IndexFields = New List(Of String) From {
            "bonusID"
        }
        Call UpdateDB.CreateIndex(invTraits_Table, "IDX_" & invTraits_Table & "_BID", IndexFields)

    End Sub

    Private Sub BuildCertificateMasteriesTable()
        Dim IndexFields As List(Of String)
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("masteryLevel", FieldType.tinyint_type, 0, True),
            New DBTableField("masteryRecommendedTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(crtMasteries_Table, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(crtMasteries_Table, "IDX_" & crtMasteries_Table & "_TID", IndexFields)

    End Sub

    Private Sub InsertMasteries(ByVal TypeID As Integer, ByVal SentMasteries As Dictionary(Of Long, Object))
        Dim DataFields As List(Of DBField)

        If Not IsNothing(SentMasteries) Then
            For Each Mastery In SentMasteries
                For Each ID In Mastery.Value
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("masteryLevel", Mastery.Key + 1, FieldType.tinyint_type),
                        UpdateDB.BuildDatabaseField("masteryRecommendedTypeID", ID, FieldType.int_type)
                    }

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
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("iconID", Traits.iconID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("skilltypeID", -1, FieldType.int_type), ' -1 for role bonuses that don't have skills associated with them
                        UpdateDB.BuildDatabaseField("bonus", R_bonus.bonus, FieldType.real_type),
                        UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(R_bonus.bonusText), FieldType.text_type),
                        UpdateDB.BuildDatabaseField("importance", R_bonus.importance, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("nameID", R_bonus.nameID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("unitID", R_bonus.unitID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("isPositive", R_bonus.isPositive, FieldType.int_type)
                    }

                    Call UpdateDB.InsertRecord(invTraits_Table, DataFields)

                    ' Insert translation field data
                    Call Translator.InsertTranslationData(TypeID, "bonusID", "bonusText", invTraits_Table, NameTranslation.GetAllTranslations(R_bonus.bonusText))

                Next
            End If

            ' Misc bonuses
            If Not IsNothing(Traits.miscBonuses) Then
                BonusIDCounter += 1
                For Each M_bonus In Traits.miscBonuses
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("iconID", Traits.iconID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("skilltypeID", -1, FieldType.int_type), ' -1 for misc bonuses that don't have skills associated with them
                        UpdateDB.BuildDatabaseField("bonus", M_bonus.bonus, FieldType.real_type),
                        UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(M_bonus.bonusText), FieldType.text_type),
                        UpdateDB.BuildDatabaseField("importance", M_bonus.importance, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("nameID", M_bonus.nameID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("unitID", M_bonus.unitID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("isPositive", M_bonus.isPositive, FieldType.int_type)
                    }

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
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("bonusID", BonusIDCounter, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("iconID", Traits.iconID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("skilltypeID", TypeTrait.Key, FieldType.int_type), ' -1 for misc bonuses that don't have skills associated with them
                            UpdateDB.BuildDatabaseField("bonus", S_bonus.bonus, FieldType.real_type),
                            UpdateDB.BuildDatabaseField("bonusText", NameTranslation.GetLanguageTranslationData(S_bonus.bonusText), FieldType.text_type),
                            UpdateDB.BuildDatabaseField("importance", S_bonus.importance, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("nameID", S_bonus.nameID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("unitID", S_bonus.unitID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("isPositive", S_bonus.isPositive, FieldType.int_type)
                        }

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
            PackagedGrouptoFind = New PackagedGroup With {.GroupID = GroupID, .PackagedVolume = 0}
            FoundGroup = PackagedGroups.Find(AddressOf FindPackagedVolumebyGroup)

            If FoundGroup IsNot Nothing Then
                Return FoundGroup.PackagedVolume
            Else
                Return DefaultVolume
            End If
        End If

    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
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
    Public Property metaGroupID As Object
    Public Property variationParentTypeID As Object

    Public Property masteries As Dictionary(Of Long, Object)
    Public Property traits As itemTraits

    Public Class itemTraits
        Public Property iconID As Object
        Public Property roleBonuses As List(Of bonus)
        Public Property types As Dictionary(Of Long, List(Of bonus)) ' for skill bonuses to ships
        Public Property miscBonuses As List(Of bonus) ' For things like T3 destroyers
    End Class

    Public Class bonus
        Public Property bonus As Object
        Public Property bonusText As Translations
        Public Property importance As Object
        Public Property isPositive As Object
        Public Property nameID As Object
        Public Property unitID As Object
    End Class
End Class


