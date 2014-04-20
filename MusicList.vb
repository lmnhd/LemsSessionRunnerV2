Imports System.IO
Public Class MusicList
    Inherits List(Of MusicListItem)
    Implements IComparer(Of MusicListItem)

    Private app As Application = Application.Current




    Private m_Title As String
    Private m_Artist As Artist

    Private m_Type As MusicListType
    Private Shared window As MusicListWindow
    Private Shared LiveWindow As Boolean = False


    Public Structure MusicListItem
        Private m_SongTitle As String


        Property trackFileInfo As FileInfo
        Property index As Integer

        Property SongTitle As String
            Set(value As String)

            End Set
            Get
                If m_SongTitle Is Nothing Then
                    Return trackFileInfo.Name
                End If
                Return m_SongTitle
            End Get
        End Property
        ''' <summary>
        ''' if _index is 0 song will be added to the end of the list
        ''' </summary>
        ''' <param name="fileNFO"></param>
        ''' <param name="_index"></param>
        ''' <remarks></remarks>
        Sub New(fileNFO As FileInfo, _index As Integer)
            trackFileInfo = fileNFO
            index = _index
        End Sub
    End Structure

    Enum MusicListType
        GeneralPlayList
        CDBurnList
        AlbumArrangeMent
        MixTapeArrangeMent
        Relocation
        BeatList
    End Enum
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="artist"></param>
    ''' <param name="title"></param>
    ''' <param name="type"></param>
    ''' <remarks></remarks>
    Public Sub New(artist As Artist, title As String, type As MusicListType)

        m_Artist = artist
        m_Title = title
        m_Type = type


    End Sub

    Sub New()

    End Sub

    ''' <summary>
    ''' Returns the priority level of the given item
    ''' </summary>
    ''' <param name="song"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function addSong(song As MusicListItem, copyFirst As Boolean) As Integer


        If copyFirst Then

            If Not Application.CurrentArtist Is Nothing Then
                'copy to artist location
            Else
                'copy to base location
            End If


        End If


        If (Me.Count = 0) Then
            Add(song)
            song.index = 1

            Return song.index

        End If


        If song.index = 0 Then
            'means it has no priority
            song.index = Me.Count + 1
            Add(song)
        Else
            For Each item As MusicListItem In Me
                If item.index = song.index Then
                    item.index = 0
                    Add(song)
                    For Each afterItem As MusicListItem In Me
                        If afterItem.index > song.index Then
                            afterItem.index += 1

                        End If
                        item.index = song.index + 1
                    Next
                    Return song.index



                End If
                Add(song)
                Return song.index


            Next
        End If
        'Me.Sort()


        Return Nothing
    End Function

    Public Function removeSong(index As Integer) As Boolean
        If Me.Count > 0 Then

            RemoveAt(index)
            Dim num = 1
            For Each Item As MusicListItem In Me
                Item.index = num
                num += 1
            Next


        End If
        Return True
    End Function
    Public Delegate Sub lemLog(message As String, overallStat As String)

    Public Shared Function MovetracksToLocalSpace(trackOrTracks As MusicList) As MusicList

        Dim app As Application = Application.Current
        Dim newList As MusicList = New MusicList(trackOrTracks.Artist, trackOrTracks.Title, trackOrTracks.Type)

        Dim LemLog As lemLog = New lemLog(AddressOf app.LemLog)
        If Not trackOrTracks.Artist Is Nothing Then
            For Each track As MusicList.MusicListItem In trackOrTracks.TheList

                app.Dispatcher.BeginInvoke(LemLog, {"Copying " & track.trackFileInfo.Name, "Moving " & trackOrTracks.TheList.Count & " Tracks to Computer "})

                Dim newLocation As String = Application.BASE_PATH & "AquiredBeats\" & track.trackFileInfo.Name


                If Not My.Computer.FileSystem.FileExists(newLocation) Then
                    My.Computer.FileSystem.CopyFile(track.trackFileInfo.FullName, newLocation)
                End If



                Dim newInfo As FileInfo = New FileInfo(newLocation)

                Dim newTrack As MusicListItem = New MusicListItem
                newTrack.trackFileInfo = newInfo
                newTrack.index = track.index

                newList.addSong(newTrack, False)





            Next

            Return newList
        End If




        Try


            For Each track As MusicList.MusicListItem In trackOrTracks.TheList
                app.Dispatcher.BeginInvoke(LemLog, {"Copying " & track.trackFileInfo.Name, "Moving " & trackOrTracks.TheList.Count & " Tracks to " & trackOrTracks.Artist.My_Name & "s Personal Location"})


                Dim newLoc As String = trackOrTracks.Artist.RecordingPath & "Private Tracks\" & track.trackFileInfo.Name

                track.trackFileInfo = New FileInfo(newLoc)
                My.Computer.FileSystem.CopyFile(track.trackFileInfo.FullName, newLoc)



            Next

        Catch ex As Exception
            Console.WriteLine(ex.StackTrace)

        End Try
        app.Dispatcher.BeginInvoke(LemLog, {"Copying Completed", "Welcome "})

        Return trackOrTracks
    End Function

    Private Sub swapItems(ByRef index1 As MusicListItem, ByRef index2 As MusicListItem)

        If index1.index = index2.index Then
            Return
        End If

        Dim temp = index2.index
        index2.index = index1.index
        index1.index = temp

        Sort()

    End Sub

    Public Function Compare(x As MusicListItem, y As MusicListItem) As Integer Implements IComparer(Of MusicListItem).Compare
        If x.index > y.index Then
            Return 1
        ElseIf x.index < y.index Then
            Return -1

        End If
        Return 0
    End Function

    ReadOnly Property Title As String
        Get
            Return m_Title
        End Get
    End Property

    ReadOnly Property TheList As List(Of MusicListItem)
        Get
            Return Me
        End Get
    End Property

    ReadOnly Property Artist As Artist
        Get
            Return m_Artist
        End Get
    End Property

    ReadOnly Property Type
        Get
            Return m_Type
        End Get
    End Property

    Private Function StoreData() As Boolean


        'MusicList Node
        Dim motherNode As XElement = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="MusicList")
        Dim myNode As XElement = Nothing
        'aritstName node
        Dim result = From node As XElement In motherNode.Elements
                     Where node.Name.LocalName = m_Artist.GLOBAL_ID
                     Select node


        If Not result.Any Then
            myNode = New XElement(m_Artist.GLOBAL_ID, New XAttribute("count", "0"))
            motherNode.Add(myNode)

        Else
            myNode = result.First
            'myNode.RemoveNodes()

        End If
        'somgnodesById
        Dim myList As XElement = New XElement(m_Type.ToString, New XAttribute("id", "1"), New XAttribute("title", m_Title))
        For Each item As MusicListItem In Me
            Dim songNode As XElement = New XElement("Song", New XAttribute("title", Path.GetFileNameWithoutExtension(item.SongTitle)), New XAttribute("path", item.trackFileInfo.FullName), New XAttribute("index", item.index))
            myList.Add(songNode)


        Next
        myNode.Add(myList)
        myList.Attribute("id").Value = myNode.Elements.Count
        myNode.Attribute("count").Value = myNode.Elements.Count
        motherNode.Attribute("count").Value = motherNode.Elements.Count
        Application.ResourcesHaveChanged = True
        XMLResources.SubmitResources()






        Return True






    End Function


    Public Shared Function GetTypeByString(type As String) As MusicListType
        Select Case type
            Case "AlbumArrangeMent"
                Return MusicListType.AlbumArrangeMent
            Case "BeatList"
                Return MusicListType.BeatList
            Case "CDBurnList"
                Return MusicListType.CDBurnList
            Case "GeneralPlayList"
                Return MusicListType.GeneralPlayList
            Case "MixTapeArrangeMent"
                Return MusicListType.MixTapeArrangeMent
            Case "Relocation"
                Return MusicListType.Relocation
        End Select
        Return Nothing
    End Function


    Public Shared Function GetAllLists(art As Artist) As List(Of MusicList)

        Dim count = GetListCount(art)
        If count > 0 Then

            Dim result As List(Of MusicList) = New List(Of MusicList)
            For Each playlist As XElement In GetArtistMusicListNode(art).Elements
                Dim temp As MusicList = GetSavedList(art, GetTypeByString(playlist.Name.LocalName), playlist.Attribute("id").Value)

                result.Add(temp)
            Next
            Return result
        End If

        Return Nothing

    End Function

    Public Overrides Function ToString() As String
        Return Title

    End Function

    Public Sub UpdateTitle(newName As String)
        m_Title = newName
    End Sub

    Public Shared Function GetArtistMusicListNode(art As Artist) As XElement
        For Each node As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="MusicList").Elements
            If node.Name = art.GLOBAL_ID Then

                Return node
            End If
        Next
        Return Nothing
    End Function

    Public Shared Function GetListNames(art As Artist) As List(Of String)

        Dim result As List(Of String) = New List(Of String)

        For Each node As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="MusicList").Elements
            If node.Name.LocalName = art.GLOBAL_ID Then

                For Each lstnd As XElement In node.Elements
                    result.Add(lstnd.Attribute("title").Value)

                Next
                Return result

            End If
        Next
        Return result
    End Function




    Public Shared Function GetListCount(art As Artist) As Integer


        For Each node As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="MusicList").Elements
            If node.Name.LocalName = art.GLOBAL_ID Then

                Return node.Elements.Count
            End If





        Next
        Return 0
    End Function

    Public Shared Function GetSavedList(artist As Artist, type As MusicListType, id As Integer) As MusicList

        For Each artistNode As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="MusicList").Elements
            If artistNode.Name.LocalName = artist.GLOBAL_ID Then
                Dim savedresult = From listNode In artistNode.Elements
                             Where listNode.Attribute("id").Value = id.ToString
                             Select listNode

                If savedresult.Any Then
                    Dim listnode As XElement = savedresult.First
                    Dim finalList As MusicList = New MusicList(artist, listnode.Attribute("title").Value, type)
                    For Each songNode As XElement In listnode.Elements
                        Dim temp As MusicListItem = New MusicListItem(New FileInfo(songNode.Attribute("path").Value), Integer.Parse(songNode.Attribute("index").Value))

                        finalList.addSong(temp, False)


                    Next
                    Return finalList
                End If
            End If

        Next
        Return Nothing
    End Function


    Public Function SaveAll() As Boolean



        Return StoreData()


    End Function

    Public Shared ReadOnly Property LiveMusicListWindow As Boolean
        Get
            If window Is Nothing Then
                Return False

            End If
            Return LiveWindow
        End Get
    End Property

    Public Shared Sub CloseListWindow()
        If Not window Is Nothing Then
            Try
                window.Close()
                window = Nothing
                LiveWindow = False
            Catch ex As Exception
                Console.WriteLine(ex.StackTrace)

            End Try
        End If
    End Sub

    Public Shared Sub DisplaySoloWindow(sourceLists As Dictionary(Of String, List(Of FileInfo)), type As MusicListType, copyFirst As Boolean,
 Optional art As Artist = Nothing, Optional drive As String = "", Optional windowtitle As String = "")





        window = New MusicListWindow(sourceLists, type, art, drive)
        window.Show()
        window.CopyFirst = copyFirst
        window.Title = windowtitle
        window.Topmost = True
        LiveWindow = True
    End Sub


End Class
