
''' <summary>
''' Class to process import languages for the EVE SDE
''' </summary>
Public Class ImportLanguage

    ' Language Consts
    Public Const ENGLISH As String = "en"
    Public Const SPANISH As String = "es" ' Not used
    Public Const GERMAN As String = "de"
    Public Const FRENCH As String = "fr"
    Public Const ITALIAN As String = "it" ' Not used
    Public Const JAPANESE As String = "ja"
    Public Const RUSSIAN As String = "ru"
    Private Const CHINESE As String = "zh"
    Private Const KOREAN As String = "ko"

    Private ReadOnly SelectedLanguageCode As String

    Public Sub New(SentCode As LanguageCode)

        SelectedLanguageCode = GetLanguageStringCode(SentCode)

    End Sub

    ''' <summary>
    ''' Returns a string for the data set sent of the language set in constructor. Returns english if selected code not found.
    ''' </summary>
    ''' <param name="AllData">Translations class with all YAML translations (e.g. EN, DE, etc.)</param>
    ''' <returns></returns>
    Public Function GetLanguageTranslationData(AllData As Translations) As String
        Dim Translation As Object = Nothing

        If Not IsNothing(AllData) Then
            With AllData
                Select Case SelectedLanguageCode
                    Case ENGLISH
                        Translation = .en
                    Case GERMAN
                        Translation = .de
                    Case FRENCH
                        Translation = .fr
                    Case JAPANESE
                        Translation = .ja
                    Case RUSSIAN
                        Translation = .ru
                    Case CHINESE
                        Translation = .zh
                    Case SPANISH
                        Translation = .es
                    Case ITALIAN
                        Translation = .it
                    Case KOREAN
                        Translation = .ko
                    Case Else
                        Translation = .en
                End Select

                If IsNothing(Translation) Then
                    Translation = .en
                End If
            End With

            Return CStr(Translation)

        Else
            Return ""
        End If

    End Function

    ''' <summary>
    ''' Gets a list of all translations with their language codes
    ''' </summary>
    ''' <param name="AllData">Translations class with all YAML translations (e.g. EN, DE, etc.)</param>
    ''' <returns></returns>
    Public Function GetAllTranslations(AllData As Translations) As List(Of TranslationData)
        Dim TempTranslationData As TranslationData
        Dim ReturnList As New List(Of TranslationData)

        If Not IsNothing(AllData) Then
            ' Build the list of the 6 languages
            For i = 1 To 6
                With TempTranslationData
                    .TranslationCode = GetLanguageStringCode(i)
                    Select Case i
                        Case LanguageCode.English
                            .Translation = AllData.en
                        Case LanguageCode.German
                            .Translation = AllData.de
                        Case LanguageCode.French
                            .Translation = AllData.fr
                        Case LanguageCode.Japanese
                            .Translation = AllData.ja
                        Case LanguageCode.Russian
                            .Translation = AllData.ru
                        Case LanguageCode.Chinese
                            .Translation = AllData.zh
                        Case LanguageCode.Korean
                            .Translation = AllData.ko
                        Case Else
                            .Translation = AllData.en ' Spanish and Italian don't have complete data, so use English 
                    End Select

                    ' Add the data
                    ReturnList.Add(TempTranslationData)
                End With
            Next
        End If

        Return ReturnList

    End Function

    ''' <summary>
    ''' Gets the Current language ID set for the class
    ''' </summary>
    ''' <returns>Current language ID as a string</returns>
    Public Function GetCurrentLanguageID() As String
        Return SelectedLanguageCode
    End Function

    ''' <summary>
    ''' Gets the language string constant from the enumerated type
    ''' </summary>
    ''' <param name="Code">Enumeraged Language Code</param>
    ''' <returns>String language code</returns>
    Private Function GetLanguageStringCode(Code As LanguageCode) As String
        Dim ReturnCode As String

        Select Case Code
            Case LanguageCode.English
                ReturnCode = ENGLISH
            Case LanguageCode.German
                ReturnCode = GERMAN
            Case LanguageCode.French
                ReturnCode = FRENCH
            Case LanguageCode.Japanese
                ReturnCode = JAPANESE
            Case LanguageCode.Russian
                ReturnCode = RUSSIAN
            Case LanguageCode.Chinese
                ReturnCode = CHINESE
            Case LanguageCode.Spanish
                ReturnCode = SPANISH
            Case LanguageCode.Italian
                ReturnCode = ITALIAN
            Case LanguageCode.Korean
                ReturnCode = KOREAN
            Case Else
                ReturnCode = ENGLISH
        End Select

        Return ReturnCode
    End Function

End Class

Public Enum LanguageCode
    English = 1
    German = 2
    French = 3
    Japanese = 4
    Russian = 5
    Chinese = 6
    Spanish = 7
    Italian = 8
    Korean = 9
End Enum

Public Class Translations
    Public Property de As Object
    Public Property en As Object
    Public Property es As Object
    Public Property fr As Object
    Public Property it As Object
    Public Property ja As Object
    Public Property ru As Object
    Public Property zh As Object
    Public Property ko As Object
End Class

Public Structure TranslationData
    Dim TranslationCode As String
    Dim Translation As String
End Structure