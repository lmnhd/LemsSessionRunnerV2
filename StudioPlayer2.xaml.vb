Public Class StudioPlayer2
    '' Private testTrack As String = "K:\wamp\www\mp3\Hump\6 In The Morning.mp3"
    '' Private lemPlayer As AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11
    ''Public WithEvents mediaPlayer As AxWMPLib.AxWindowsMediaPlayer
    ''Private WithEvents mediaPlayerMultiCaster As AxWMPLib.AxWindowsMediaPlayerEventMulticaster
    ''Private WithEvents mediaPlayerEvent As AxWMPLib._WMPOCXEvents_EndOfStreamEventHandler
    '' Private host As System.Windows.Forms.Integration.WindowsFormsHost = New Forms.Integration.WindowsFormsHost()
    'Private USE_LEMPLAYER As Boolean = Application.UseLemPlayerInsteadOfMediaPlayer
    ''Private IS_PLAYING As Boolean = False
    '' Private CURRENT_PLAYING_TRACK As String = ""
    '' Public Event SongStopped()





    'Public Sub New()

    '    ' This call is required by the designer.
    '    InitializeComponent()

    '    ' Add any initialization after the InitializeComponent() call.

    'End Sub
    'Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
    '    If (USE_LEMPLAYER) Then
    '        loadLemPlayer()
    '    Else
    '        loadMediaPlayer()
    '    End If
    'End Sub

    ''Private Sub TrackStopped(sender As Object, e As AxWMPLib._WMPOCXEvents_EndOfStreamEvent)
    ''    ' RaiseEvent SongStopped()
    ''End Sub
    'Private Sub loadLemPlayer()

    '    ' lemPlayer = New AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11()


    '    ' host.Child = lemPlayer

    '    '  grid1.Children.Add(host)

    '    ' lemPlayer.playTrack(testTrack)


    'End Sub

    'Private Sub loadMediaPlayer()
    '    '  mediaPlayer = New AxWMPLib.AxWindowsMediaPlayer()

    '    '   host.Child = mediaPlayer

    '    '   grid1.Children.Add(host)

    '    'mediaPlayerMultiCaster = New AxWMPLib.AxWindowsMediaPlayerEventMulticaster(mediaPlayer)




    '    'mediaPlayer.URL = testTrack



    'End Sub



    'Public Sub play(path As String)
    '    If Not (System.IO.File.Exists(path)) Then
    '        Return
    '    ElseIf Not (path.ToLower().Contains(".mp3")) Then
    '        Return
    '    End If
    '    ' If (USE_LEMPLAYER) Then
    '    'lemPlayer.playTrack(path)
    '    '   Else
    '    '  MediaPlayer.URL = path
    '    '    End If
    '    '    CURRENT_PLAYING_TRACK = path
    '    '    IS_PLAYING = True

    'End Sub

    'Public Sub stopPlayer()
    '    ' If (IS_PLAYING) Then
    '    Try
    '        'If (USE_LEMPLAYER) Then
    '        'lemPlayer.stop()
    '        '      Else
    '        ' mediaPlayer.Ctlcontrols.stop()

    '        '     End If
    '    Catch ex As Exception

    '    End Try

    '    ' End If
    '    ' IS_PLAYING = False
    '    ' CURRENT_PLAYING_TRACK = ""
    'End Sub

    ''Public Function GetCurrentPlayingTrackPath() As String
    ''    Return CURRENT_PLAYING_TRACK
    ''End Function
    ''Public Function GetCurrentPlayingTrackTitle() As String
    ''    If Not (CURRENT_PLAYING_TRACK = "") Then
    ''        If (CURRENT_PLAYING_TRACK.ToLower().Contains(".mp3")) Then
    ''            Return System.IO.Path.GetFileNameWithoutExtension(CURRENT_PLAYING_TRACK)

    ''        End If
    ''    End If
    ''    Return ""
    ''End Function

    ''Public Sub addToBurnList(path As String)


    ''    ' ui_LemPlayer.check2(1)

    ''    'ui_LemPlayer.addTrackToBurnList("ff")

    ''    'ui_LemPlayer.burnCD(True)


    ''    ' Dim stat = ui_LemPlayer.getCDBurnerStatus()

    ''    'ui_LemPlayer.playCDTrack(0)


    ''    'ui_LemPlayer.openCDTray()



    ''    'ui_LemPlayer.ripCDTrack(0, "dkfjdkfj")



    ''End Sub
End Class
