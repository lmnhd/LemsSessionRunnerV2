Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Timers
Imports AxCCLEMMUSICPLAYERACTIVEXV11Lib




Imports DxVBLibA





Public Class LemPlayer
    Private Shared medplayer As Windows.Media.MediaPlayer
   
    Private Shared player As AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11

    Private Shared host As System.Windows.Forms.Integration.WindowsFormsHost

    Private Shared currentFile As String

    Private Shared isScrewed As Boolean


    Shared isPlaying As Boolean

    Private Shared timer As Timer

    Private Shared pb As positionBinder




    Public Shared Event ScrewedChangedEvent()


    Shared Sub test()
        'player.addSongToBurner("K:\wamp\www\mp3\Hump\Nodoubtaboutit.mp3")



    End Sub


    Public Shared Function getPlayPosition() As Double
        Dim pos As Double = 0.0

        If (isPlaying) Then

            ' pos = player.
            pos = player.getCurrentPosition





        End If
        Return pos
    End Function

    Shared Sub New()

        medplayer = New Windows.Media.MediaPlayer()

        player = New AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11


        Dim meta As FrameworkPropertyMetadata = New FrameworkPropertyMetadata(False)
        meta.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged

        host = New System.Windows.Forms.Integration.WindowsFormsHost()


        ' isScrewdProperty = DependencyProperty.Register("isScrewed", GetType(Boolean), GetType(LemPlayer), meta)

        timer = New Timer(1000)
        ' AddHandler timer.Elapsed, AddressOf updateSlider

        pb = New positionBinder


    End Sub

    Structure positionBinder
        Property sliderValue As Double


    End Structure
    ''' <summary>
    ''' Call this to add player to UI
    ''' ie
    ''' "grid1.Children().Add(GetPlayerHost)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetPlayerHost() As System.Windows.Forms.Integration.WindowsFormsHost

        host.Child = player
        Return host



    End Function


    Public cb As PropertyChangedCallback




    Public Shared Property SongPosition As Double


    Public Shared Property Screwed As Boolean
        Get
            Return isScrewed
        End Get
        Set(value As Boolean)
            isScrewed = value

        End Set
    End Property



    Public Shared Function play(Optional ByRef songPath As String = "") As Boolean

        If Not (songPath = "") Then
            currentFile = songPath

        End If
        If Not (currentFile = "") Then
            Try
                player.playTrack(currentFile)
                isPlaying = True
                timer.Start()
            Catch ex As Exception
                isPlaying = False
            End Try

            Return True


        End If

        Return False

    End Function



    Public Shared Function GetTimer() As Timer
        Return timer
    End Function

    Public Shared Function create() As Boolean
        Return True
    End Function

    Public Shared Function stopPlaying() As Boolean

        player.stop()
        isPlaying = False
        timer.Stop()
        Return True
    End Function

    Private Shared Sub updateSlider()
        pb.sliderValue = getPlayPosition()

    End Sub



    Public Shared Function FLA_Style() As Boolean

        If (isScrewed) Then
            player.pitch = 1
            isScrewed = False

            RaiseEvent ScrewedChangedEvent()
        Else

            player.pitch = 0.7
            isScrewed = True
            RaiseEvent ScrewedChangedEvent()

        End If

        Return True
    End Function

    Public Shared Function resetPitch() As Boolean
        player.pitch = 1
        isScrewed = False
        RaiseEvent ScrewedChangedEvent()


        Return True
    End Function












End Class
