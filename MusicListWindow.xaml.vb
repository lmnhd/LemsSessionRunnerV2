Imports System.IO
Public Class MusicListWindow

    Private sources As Dictionary(Of String, List(Of FileInfo))
    Private sourceNames As List(Of String)
    Private currentSource As Integer
    Private myMusicLists As List(Of MusicList)
    Private myList As MusicList

    Private currentArtist As Artist
    Private currentUserTrackSelection As MusicList.MusicListItem
    Private currentMainListSelection As FileInfo

    Private NAME_CHECK As Boolean


    Private MyDrive As String
    Private app As Application = Application.Current
    Delegate Sub PlayTrack(song As String, title As String)


    Public Property CopyFirst As Boolean

    Private Sub ui_Buttons_ShowMainList_Click(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_ShowMainList.Click
        If ui_MainList.Visibility = Windows.Visibility.Visible Then
            ui_MainList.Visibility = Windows.Visibility.Collapsed
        Else
            ui_MainList.Visibility = Windows.Visibility.Visible
        End If
    End Sub

    Sub New(availableLists As Dictionary(Of String, List(Of FileInfo)), type As MusicList.MusicListType, Optional art As Artist = Nothing, Optional drive As String = "")

        ' This call is required by the designer.
        InitializeComponent()
        ui_UserList.Visibility = Windows.Visibility.Collapsed
        ui_Buttons_UserListButtonPanel1.Visibility = Windows.Visibility.Collapsed
        ui_Buttons_UserListMoveTrackPositionButtonsGrid.Visibility = Windows.Visibility.Collapsed
        sources = New Dictionary(Of String, List(Of FileInfo))
        If Not art Is Nothing Then
            currentArtist = art
            myMusicLists = MusicList.GetAllLists(art)
            If Not myMusicLists Is Nothing Then
                If myMusicLists.Count > 0 Then
                    ui_Buttons_MyListSelector.DataContext = myMusicLists
                    ui_Buttons_MyListSelector.SelectedIndex = myMusicLists.Count - 1



                    myList = myMusicLists(0)
                    ui_UserList.DataContext = myList.TheList

                    ui_UserListLabel.Text = myList.Title
                    ui_UserList.Visibility = Windows.Visibility.Visible



                    ui_Buttons_UserListButtonPanel1.Visibility = Windows.Visibility.Visible
                    ui_Buttons_UserListMoveTrackPositionButtonsGrid.Visibility = Windows.Visibility.Visible

                End If
            Else
                initializeMyList(art, type)
            End If

            

        End If

        If Not drive = "" Then
            MyDrive = drive
            Dim mysongs As List(Of FileInfo) = New List(Of FileInfo)
            For Each _file As String In Directory.GetFiles(MyDrive, "*", SearchOption.AllDirectories)
                If _file.ToLower.EndsWith(".mp3") Then
                    Dim temp As FileInfo = New FileInfo(_file)

                    mysongs.Add(temp)

                End If
            Next
            If mysongs.Count > 0 Then
                Dim tex As String = "My Device"
                If Not currentArtist Is Nothing Then
                    tex = currentArtist.My_Name & "'s Device"

                End If
                sources.Add(tex, mysongs)

            End If
        End If
        ' Add any initialization after the InitializeComponent() call.

        sources = availableLists

        If sources.Count > 0 Then
            If sourceNames Is Nothing Then
                sourceNames = New List(Of String)
            End If
            For Each item As KeyValuePair(Of String, List(Of FileInfo)) In availableLists
                sourceNames.Add(item.Key)

            Next
        End If

        currentSource = 0
        ui_MainListLabel.Text = sourceNames(currentSource)
        ui_MainList.DataContext = sources.Item(sourceNames(currentSource))
        ui_MainListBorder.Visibility = Windows.Visibility.Visible





    End Sub

    Private Sub initializeMyList(art As Artist, type As MusicList.MusicListType)
        myMusicLists = New List(Of MusicList)
        myList = New MusicList(art, "Untitled List" & Now, MusicList.MusicListType.GeneralPlayList)
        ui_UserList.DataContext = myList
        myMusicLists.Add(myList)
        ui_Buttons_MyListSelector.DataContext = myMusicLists
        ui_Buttons_MyListSelector.Items.Refresh()
        ui_UserList.Visibility = Windows.Visibility.Visible
        ui_Buttons_UserListButtonPanel1.Visibility = Windows.Visibility.Visible
        ui_Buttons_UserListMoveTrackPositionButtonsGrid.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub ToggleSourceList(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_ToggleSource.Click

        currentSource += 1

        If currentSource > sourceNames.Count - 1 Then
            currentSource = 0

        End If

        ui_MainList.DataContext = sources.Item(sourceNames(currentSource))
        ui_MainListLabel.Text = sourceNames.Item(currentSource)


        



    End Sub


    Private Sub ui_UserList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_UserList.SelectionChanged
        currentUserTrackSelection = CType(ui_UserList.SelectedItem, MusicList.MusicListItem)
        MsgBox("")
    End Sub

    Private Sub ui_MainList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_MainList.SelectionChanged
        If (ui_MainList.SelectedIndex = -1) Then
            Return
        End If
        currentMainListSelection = sources(sourceNames(currentSource))(ui_MainList.SelectedIndex)
        Dim play As PlayTrack = New PlayTrack(AddressOf Application.PlayFloater)
        Application.PlayFloater(currentMainListSelection.FullName, currentMainListSelection.Name)


    End Sub

    Private Sub ui_Buttons_Remove_Song_Click(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_Remove_Song.Click
        If Not currentUserTrackSelection.SongTitle Is Nothing Then
            ui_UserList.Items.Remove(currentUserTrackSelection)
            currentUserTrackSelection = Nothing
        End If
    End Sub

    Private Async Function nameThisList() As Task(Of String)

        MainGrid.Visibility = Windows.Visibility.Collapsed
        NameListGrid.Visibility = Windows.Visibility.Visible

        NAME_CHECK = False
        While NAME_CHECK = False
            Await Task.Delay(1000)
        End While
        If ui_NameListText.Text = "" Or ui_NameListText.Text = "Please Name Properly" Then
            ui_NameListText.Text = "Please Name Properly"
            Await nameThisList()

        End If
        NAME_CHECK = False


        MainGrid.Visibility = Windows.Visibility.Visible
        NameListGrid.Visibility = Windows.Visibility.Collapsed

        Return ui_NameListText.Text

    End Function

    Private Async Function SaveMyList() As Task
        If Not myList.Artist Is Nothing And myList.Title.StartsWith("Untitled") Then
            'Rename



            Dim newname As String = Await nameThisList()

            If newname = "" Then
                newname = myList.Title & Now


            End If
            For Each listName As String In MusicList.GetListNames(myList.Artist)
                If newname = listName Then

                    newname = listName & Now

                End If
            Next

            myList.UpdateTitle(newname)
        End If


        If CopyFirst Then
            myList = MusicList.MovetracksToLocalSpace(myList)


        End If
        myList.SaveAll()
        If myList.Artist Is Nothing Then
            MsgBox("Saved to General Beat Location")
        Else
            MsgBox("Saved " & myList.Count.ToString & " tracks to " & vbNewLine & myList.Artist.My_Name & "s Private Beats Location")
        End If
    End Function

    Private Sub ui_Buttons_Add_Song_Click(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_Add_Song.Click
        If Not currentMainListSelection Is Nothing Then
            e.Handled = True

            For Each item As MusicList.MusicListItem In myList
                If item.trackFileInfo.FullName = currentMainListSelection.FullName Then

                    MsgBox("Already in list")
                    Return

                End If
            Next
            myList.addSong(New MusicList.MusicListItem(currentMainListSelection, 0), CopyFirst)
            ui_UserList.DataContext = myList.TheList
            ui_Buttons_SaveList.Visibility = Windows.Visibility.Visible
            'myList.SaveAll()
            ui_UserList.Items.Refresh()


        End If
    End Sub

    Private Sub ui_Buttons_MyListSelector_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_Buttons_MyListSelector.SelectionChanged
        ui_UserList.DataContext = ui_Buttons_MyListSelector.SelectedItem
    End Sub

    Private Sub ui_Buttons_SaveList_Click(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_SaveList.Click
        SaveMyList()
        ui_Buttons_SaveList.Visibility = Windows.Visibility.Collapsed
        e.Handled = True
    End Sub

    Private Sub NameListGrid_DoneBtn_Click_1(sender As Object, e As RoutedEventArgs)
        If ui_NameListText.Text = "" Then
            Return
        End If
        NAME_CHECK = True
    End Sub

    Private Sub NewSessionClicked(sender As Object, e As RoutedEventArgs) Handles ui_Buttons_NewSession.Click
        e.Handled = True
        If (currentMainListSelection Is Nothing) Then
            MsgBox("Select something first")
            Return
        End If
        If (MsgBox("Start A Session with this mp3?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes) Then
            Dim sesswin As SessionWindow = SessionWindow.CURRENT_INSTANCE()
            Dim del As SessionWindow.StartNewSessionRemotelyDelegate = New SessionWindow.StartNewSessionRemotelyDelegate(AddressOf sesswin.StartNewSessionRemotely)
            sesswin.Dispatcher.Invoke(del, {currentMainListSelection.FullName})
            Me.Close()
        End If

    End Sub

    Private Sub ui_Buttons_NewSession_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If (currentMainListSelection Is Nothing) Then

            Return
        End If
        If (MsgBox("Start A Session with this mp3?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes) Then
            Dim sesswin As SessionWindow = SessionWindow.CURRENT_INSTANCE()
            Dim del As SessionWindow.StartNewSessionRemotelyDelegate = New SessionWindow.StartNewSessionRemotelyDelegate(AddressOf sesswin.StartNewSessionRemotely)
            sesswin.Dispatcher.Invoke(del, {currentMainListSelection.FullName})
            Me.Close()
        End If
    End Sub
End Class
