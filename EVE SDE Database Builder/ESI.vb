Imports Newtonsoft.Json
Imports System.Net
Imports System.Text

Public Class ESI

    ''' <summary>
    ''' Gets the name from universe/names public ESI call
    ''' </summary>
    ''' <param name="ItemID">Single itemID to look up</param>
    ''' <returns>Returns the name if found or empty string if not</returns>
    Public Function GetItemName(ByVal ItemID As Integer) As String
        Dim PublicData As String
        Dim ESIData As New List(Of ESINameData)

        PublicData = GetPublicData("https://esi.evetech.net/latest/" & "universe/names/" & "?datasource=tranquility", "[" & CStr(ItemID) & "]")

        If Not IsNothing(PublicData) Then
            ESIData = JsonConvert.DeserializeObject(Of List(Of ESINameData))(PublicData)
            For Each Record In ESIData
                ' Just return the name
                Return Record.name
            Next
        End If

        Return ""

    End Function

    ''' <summary>
    ''' Queries the server for public data for the URL sent. If not found, returns nothing
    ''' </summary>
    ''' <param name="URL">Full public data URL as a string</param>
    ''' <returns>Byte Array of response or nothing if call fails</returns>
    Public Function GetPublicESIData(ByVal URL As String, ByRef CacheDate As Date, Optional BodyData As String = "") As String
        Dim Response As String = ""
        Dim WC As New WebClient
        Dim myWebHeaderCollection As New WebHeaderCollection
        Dim Expires As String = Nothing
        Dim Pages As Integer = Nothing

        Try

            WC.Proxy = Nothing

            If BodyData <> "" Then
                Response = Encoding.UTF8.GetString(WC.UploadData(URL, Encoding.UTF8.GetBytes(BodyData)))
            Else
                Response = WC.DownloadString(URL)
            End If

            ' Get the expiration date for the cache date
            myWebHeaderCollection = WC.ResponseHeaders
            Expires = myWebHeaderCollection.Item("Expires")
            Pages = CInt(myWebHeaderCollection.Item("X-Pages"))

            If Not IsNothing(Expires) Then
                CacheDate = CDate(Expires.Replace("GMT", "").Substring(InStr(Expires, ",") + 1)) ' Expiration date is in GMT
            Else
                CacheDate = #1/1/2200#
            End If

            If Not IsNothing(Pages) Then
                If Pages > 1 Then
                    Dim TempResponse As String = ""
                    For i = 2 To Pages
                        TempResponse = WC.DownloadString(URL & "&page=" & CStr(i))
                        ' Combine with the original response - strip the end and leading brackets
                        Response = Response.Substring(0, Len(Response) - 1) & "," & TempResponse.Substring(1)
                    Next
                End If
            End If

            Return Response

        Catch ex As WebException

            ' Retry call
            If CType(ex.Response, HttpWebResponse).StatusCode >= 500 And Not RetryCall Then
                RetryCall = True
                ' Try this call again after waiting a second
                Threading.Thread.Sleep(1000)
                Return GetPublicESIData(URL, CacheDate, BodyData)
            End If

        Catch ex As Exception
            MsgBox("The request failed to get public data. " & ex.Message, vbInformation, Application.ProductName)
        End Try

        If Not IsNothing(Response) Then
            If Response <> "" Then
                Return Response
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Queries the server for public data for the URL sent. If not found, returns nothing
    ''' </summary>
    ''' <param name="URL">Full public data URL as a string</param>
    ''' <returns>Byte Array of response or nothing if call fails</returns>
    Private Function GetPublicData(ByVal URL As String, Optional BodyData As String = "") As String
        Dim Response As String = ""
        Dim WC As New WebClient
        Dim ErrorCode As Integer = 0
        Dim ErrorResponse As String = ""

        WC.Proxy = Nothing

        Try

            If BodyData <> "" Then
                Response = Encoding.UTF8.GetString(WC.UploadData(URL, Encoding.UTF8.GetBytes(BodyData)))
            Else
                Response = WC.DownloadString(URL)
            End If

            Return Response
        Catch
            Call OutputMsgtoLog(CStr(BodyData))
        End Try

        If Response <> "" Then
            Return Response
        Else
            Return Nothing
        End If

    End Function

    Public Class ESINameData
        <JsonProperty("category")> Public category As String '[ alliance, character, constellation, corporation, inventory_type, region, solar_system, station ]
        <JsonProperty("id")> Public id As Integer
        <JsonProperty("name")> Public name As String
    End Class

End Class
