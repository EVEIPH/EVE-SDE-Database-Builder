
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLnpcCorporations
    Inherits YAMLFilesBase

    Public Const npcCorporationsFile As String = "npcCorporations.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = npcCorporationsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, crpNPCCorporation)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False, True),
            New DBTableField("tickerName", FieldType.nvarchar_type, 5, True),
            New DBTableField("corporationName", FieldType.nvarchar_type, 100, True),
            New DBTableField("corporationDescription", FieldType.nvarchar_type, 1500, True),
            New DBTableField("uniqueName", FieldType.bit_type, 0, True),
            New DBTableField("taxRate", FieldType.real_type, 0, True),
            New DBTableField("memberLimit", FieldType.int_type, 0, True),
            New DBTableField("hasPlayerPersonnelManager", FieldType.bit_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("ceoID", FieldType.int_type, 0, True),
            New DBTableField("deleted", FieldType.bit_type, 0, True),
            New DBTableField("extent", FieldType.char_type, 1, True),
            New DBTableField("friendID", FieldType.int_type, 0, True),
            New DBTableField("enemyID", FieldType.int_type, 0, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("stationID", FieldType.int_type, 0, True),
            New DBTableField("minSecurity", FieldType.real_type, 0, True),
            New DBTableField("minimumJoinStanding", FieldType.real_type, 0, True),
            New DBTableField("publicShares", FieldType.bigint_type, 0, True),
            New DBTableField("shares", FieldType.bigint_type, 0, True),
            New DBTableField("initialPrice", FieldType.real_type, 0, True),
            New DBTableField("mainActivityID", FieldType.int_type, 0, True),
            New DBTableField("secondaryActivityID", FieldType.int_type, 0, True),
            New DBTableField("size", FieldType.char_type, 1, True),
            New DBTableField("sizeFactor", FieldType.real_type, 0, True),
            New DBTableField("raceID", FieldType.int_type, 0, True),
            New DBTableField("sendCharTerminationMessage", FieldType.bit_type, 0, True),
            New DBTableField("url", FieldType.nvarchar_type, 1000, True),
            New DBTableField("iconID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim CorporationAllowedMemberRacesTableName As String = "corporationAllowedMemberRaces"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("memberRace", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationAllowedMemberRacesTableName, Table)

        Dim CorporationTradesTableName As String = "corporationTrades"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("value", FieldType.real_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationTradesTableName, Table)

        Dim CorporationDivisionsTableName As String = "corporationDivisions"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("divisionID", FieldType.int_type, 0, False),
            New DBTableField("divisionNumber", FieldType.int_type, 0, True),
            New DBTableField("leaderID", FieldType.int_type, 0, True),
            New DBTableField("size", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationDivisionsTableName, Table)

        Dim CorporationExchangeRatesTableName As String = "corporationExchangeRates"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("exchangeID", FieldType.int_type, 0, True),
            New DBTableField("exchangeRate", FieldType.real_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationExchangeRatesTableName, Table)

        Dim CorporationInvestorsTableName As String = "corporationInvestors"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("investorID", FieldType.int_type, 0, True),
            New DBTableField("shares", FieldType.real_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationInvestorsTableName, Table)

        Dim CorporationLPOffersTableName As String = "corporationLPOffers"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("lpOfferTableID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationLPOffersTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, crpNPCCorporation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("tickerName", .tickerName, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationDescription", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", .uniqueName, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("taxRate", .taxRate, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("memberLimit", .memberLimit, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hasPlayerPersonnelManager", .hasPlayerPersonnelManager, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ceoID", .ceoID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("deleted", .deleted, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("extent", .extent, FieldType.char_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("friendID", .friendID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("enemyID", .enemyID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("stationID", .stationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("minSecurity", .minSecurity, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("minimumJoinStanding", .minimumJoinStanding, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("publicShares", .publicShares, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shares", .shares, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("initialPrice", .initialPrice, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("mainActivityID", .mainActivityID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("secondaryActivityID", .secondaryActivityID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("size", .size, FieldType.char_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", .sizeFactor, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sendCharTerminationMessage", .sendCharTerminationMessage, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("url", .url, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "corporationID", "corporationName", TableName, NameTranslation.GetAllTranslations(.nameID))
                Call Translator.InsertTranslationData(DataField.Key, "corporationID", "corporationDescription", TableName, NameTranslation.GetAllTranslations(.descriptionID))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Add other tables now
            If Not IsNothing(DataField.Value.allowedMemberRaces) Then
                For Each AMR In DataField.Value.allowedMemberRaces
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("memberRace", AMR, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationAllowedMemberRacesTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.corporationTrades) Then
                For Each CT In DataField.Value.corporationTrades
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("typeID", CT.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("value", CT.Value, FieldType.real_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationTradesTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.divisions) Then
                For Each CD In DataField.Value.divisions
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("divisionID", CD.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("divisionNumber", CD.Value.divisionNumber, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("leaderID", CD.Value.leaderID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("size", CD.Value.size, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationDivisionsTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.exchangeRates) Then
                For Each ER In DataField.Value.exchangeRates
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("exchangeID", ER.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("exchangeRate", ER.Value, FieldType.real_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationExchangeRatesTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.investors) Then
                For Each CI In DataField.Value.investors
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("investorID", CI.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("shares", CI.Value, FieldType.real_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationInvestorsTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.lpOfferTables) Then
                For Each LPO In DataField.Value.lpOfferTables
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("lpOfferTableID", LPO, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationLPOffersTableName, DataFields)
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class crpNPCCorporation
    Public Property allowedMemberRaces As List(Of Object)
    Public Property ceoID As Object
    Public Property corporationTrades As Dictionary(Of Long, Double) ' new table
    Public Property deleted As Object
    Public Property descriptionID As Translations
    Public Property divisions As Dictionary(Of Long, corporationDivisions) ' new table 
    Public Property extent As Object
    Public Property enemyID As Object
    Public Property exchangeRates As Dictionary(Of Long, Double) ' long is a list of corp IDs - new table
    Public Property factionID As Object
    Public Property friendID As Object
    Public Property hasPlayerPersonnelManager As Object
    Public Property iconID As Object
    Public Property initialPrice As Object
    Public Property investors As Dictionary(Of Long, Long) ' new table
    Public Property lpOfferTables As List(Of Object) ' new table
    Public Property mainActivityID As Object
    Public Property memberLimit As Object
    Public Property minSecurity As Object
    Public Property minimumJoinStanding As Object
    Public Property nameID As Translations
    Public Property publicShares As Object
    Public Property raceID As Object
    Public Property secondaryActivityID As Object
    Public Property sendCharTerminationMessage As Object
    Public Property shares As Object
    Public Property size As Object
    Public Property sizeFactor As Object
    Public Property solarSystemID As Object
    Public Property stationID As Object
    Public Property taxRate As Object
    Public Property tickerName As Object
    Public Property uniqueName As Object
    Public Property url As Object
End Class

Public Class corporationDivisions
    Public Property divisionNumber As Object
    Public Property leaderID As Object
    Public Property size As Object
End Class