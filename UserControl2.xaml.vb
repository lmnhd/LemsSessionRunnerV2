Public Class UserControl2
    Private testTrack As String = "K:\wamp\www\mp3\Hump\6 In The Morning.mp3"
    ' Private lemPlayer As AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11
    Public WithEvents mediaPlayer As AxWMPLib.AxWindowsMediaPlayer
    'Private WithEvents mediaPlayerMultiCaster As AxWMPLib.AxWindowsMediaPlayerEventMulticaster
    'Private WithEvents mediaPlayerEvent As AxWMPLib._WMPOCXEvents_EndOfStreamEventHandler
    Private host As System.Windows.Forms.Integration.WindowsFormsHost = New Forms.Integration.WindowsFormsHost()
    Private USE_LEMPLAYER As Boolean = Application.UseLemPlayerInsteadOfMediaPlayer
    Private IS_PLAYING As Boolean = False
    Private CURRENT_PLAYING_TRACK As String = ""



End Class
