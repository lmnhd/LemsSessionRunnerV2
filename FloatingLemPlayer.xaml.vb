Public Class FloatingLemPlayer
    Private isPlaying As Boolean = False
    Private isLive As Boolean = False
    Private trackList As List(Of IO.FileInfo)
    Private currentArtist As Artist
    Private trackNum As Int32 = 0
    Private cancel As System.Threading.CancellationTokenSource
    Private token As System.Threading.CancellationToken
    Private playImmediately As Boolean = False
    ReadOnly Property Is_Playing As Boolean
        Get
            Return isPlaying
        End Get
    End Property


    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        isLive = True
        cancel = New System.Threading.CancellationTokenSource()
        token = cancel.Token
        Me.ui_AritstSelect.DataContext = Application.ArtistList
        trackList = New List(Of IO.FileInfo)

        SelectArtist()
        Me.Show()
        ' Add any initialization after the InitializeComponent() call.

    End Sub



    Public Sub play(path As String, Optional internal As Boolean = False)
        ' ui_LemPlayer.playTrack(path)
        Me.Show()
        ui_LemPlayer.play(path)
        isPlaying = True
        If Not internal Then
            'ui_TrackList.Visibility = Windows.Visibility.Collapsed
        Else
            Title = New IO.FileInfo(path).Name
        End If

    End Sub


    Public Sub stopPlayer()
        ui_LemPlayer.stopPlayer()

    End Sub

    'Public Function writeWave(path As String) As Boolean
    '    If (isPlaying) Then
    '        Return ui_LemPlayer.WriteWave(path)
    '    End If
    '    Return False

    'End Function

    'Public Sub addSongToBurner(path As String)
    '    ui_LemPlayer.addToBurnList(path)

    'End Sub


    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        ui_LemPlayer.stopPlayer()
        ' ui_LemPlayer.disposePlayer()


        'ui_LemPlayer = Nothing
        isLive = False
        MyBase.OnClosing(e)




        ' ui_LemPlayer.Dispatcher.InvokeShutdown()

    End Sub
    ReadOnly Property Is_Live As Boolean
        Get
            Return isLive
        End Get
    End Property

    Public Sub SelectAndPlay(art As Artist)
        'If (token.CanBeCanceled) Then
        '    cancel.Cancel()
        'End If
        playImmediately = True

        trackNum = 0
        SelectArtist(art)
        'play(trackList.Item(trackNum).FullName)


    End Sub
    'Private Sub NextTrack() Handles ui_LemPlayer.SongStopped
    '    trackNum += 1
    '    If (trackNum = trackList.Count) Then
    '        trackNum = 0
    '    End If
    '    play(trackList.Item(trackNum).FullName)
    'End Sub

    'Public Sub SelectArtistAndPlay(art As Artist)
    '    SelectArtist(art)
    '    play(trackList.Item(trackNum).FullName)

    'End Sub

    Private Sub ui_TrackList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim list = TryCast(sender, ListBox)

        Dim fi As IO.FileInfo = TryCast(list.SelectedItem, IO.FileInfo)
        If Not (fi Is Nothing) Then
            play(fi.FullName, True)
        End If

    End Sub


    Private Sub SelectArtist(Optional art As Artist = Nothing)
        currentArtist = art
        Dim filters = New List(Of String)({"PreviousVersions", "MixTapeTags", "bounce_temp", "tstInternet Beats", "Cc"})
        trackList.Clear()
        ui_TrackList.DataContext = Nothing
        ui_TrackList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()

        Dim mp3path = Application.MP3_PATH
        If Not (art Is Nothing) Then
            mp3path = art.MP3Path
        End If
        Dim getTracks As Task(Of Tuple(Of String, List(Of System.IO.FileInfo))) = Task(Of Tuple(Of String, List(Of System.IO.FileInfo))).Factory.StartNew(Function()
                                                                                                                                                              Try


                                                                                                                                                                  Dim thisName = "all"
                                                                                                                                                                  If Not art Is Nothing Then
                                                                                                                                                                      thisName = art.My_Name
                                                                                                                                                                  End If



                                                                                                                                                                  For Each Path As String In IO.Directory.GetFiles(mp3path, "*.mp3", IO.SearchOption.AllDirectories)
                                                                                                                                                                      Dim skip = False


                                                                                                                                                                      If (art Is Nothing) Then
                                                                                                                                                                          For Each outname As String In filters
                                                                                                                                                                              If (System.IO.Directory.GetParent(Path).Name = outname) Then
                                                                                                                                                                                  skip = True
                                                                                                                                                                                  Continue For


                                                                                                                                                                              End If

                                                                                                                                                                          Next

                                                                                                                                                                      End If

                                                                                                                                                                      If Not skip Then
                                                                                                                                                                          trackList.Add(New IO.FileInfo(Path))

                                                                                                                                                                      End If


                                                                                                                                                                  Next
                                                                                                                                                                  Try
                                                                                                                                                                      trackList.Sort(Function(a, b) -a.LastWriteTime.CompareTo(b.LastWriteTime))

                                                                                                                                                                      Dim result As Tuple(Of String, List(Of System.IO.FileInfo)) = New Tuple(Of String, List(Of IO.FileInfo))(thisName, trackList)

                                                                                                                                                                      Return result
                                                                                                                                                                  Catch ex As Exception
                                                                                                                                                                      Return Nothing

                                                                                                                                                                  End Try


                                                                                                                                                              Catch ex As Exception
                                                                                                                                                                  Return Nothing

                                                                                                                                                              End Try
                                                                                                                                                          End Function, Nothing, token)
        Dim finalizeGetTracks = getTracks.ContinueWith(Sub(trackList)
                                                           Dim go = True
                                                           If (currentArtist Is Nothing And Not (trackList.Result Is Nothing)) Then
                                                               If (trackList.Result.Item1 = "all") Then
                                                                   go = True
                                                               Else
                                                                   go = False
                                                               End If
                                                           ElseIf Not (currentArtist Is Nothing) Then
                                                               If (currentArtist.My_Name = trackList.Result.Item1) Then

                                                                   go = True
                                                               Else
                                                                   go = False

                                                               End If
                                                           Else
                                                               go = False
                                                           End If


                                                           If (go) Then
                                                               Try
                                                                   ui_TrackList.DataContext = trackList.Result.Item2
                                                                   ui_TrackList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()

                                                                   ui_TrackList.Visibility = Windows.Visibility.Visible
                                                                   If (playImmediately) Then
                                                                       play(trackList.Result.Item2.Item(trackNum).FullName)
                                                                       playImmediately = False
                                                                   End If

                                                               Catch ex As Exception

                                                               End Try


                                                           End If




                                                       End Sub, token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())


    End Sub
    Private Sub ui__SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If (trackList Is Nothing) Then
            trackList = New List(Of IO.FileInfo)
        End If
        ui_TrackList.DataContext = Nothing
        'ui_TrackList.Items().Clear()
        Dim cb As ComboBox = TryCast(sender, ComboBox)
        Dim artName As String = cb.SelectedValue.ToString()
        Dim art As Artist = Application.ArtistList.Find(Function(a) a.My_Name = artName)
        SelectArtist(art)
    End Sub
End Class
