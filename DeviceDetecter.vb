Imports Dolinay
Imports System.IO
Public Class DeviceDetecter

    Private Shared thisInstance As DeviceDetecter
    Private WithEvents m_Detector As DriveDetector
    Private m_currentDrive As String
    Private m_TrackList As List(Of System.IO.FileInfo)
    Private m_RingToneList As List(Of System.IO.FileInfo)
    Private m_PhotoList As List(Of FileInfo)
    Private app As Application
    Private FLAG_CANCEL_WAIT_FOR_DRIVE As Boolean
    Private CANCEL_WAIT_FOR_DRIVE_LETTER As String
    Private IS_WAITING As Boolean = False
    Public Shared IS_EXTERNAL_DRIVE As Boolean
    Private log As lemLog = New lemLog(AddressOf Application.LemLog)
    Private checkCancel As CheckForCancel = New CheckForCancel(AddressOf Me.CheckForCancelWait)


    Public Shared Function GetInstance() As DeviceDetecter
        Return thisInstance

    End Function

    Sub New()
        m_Detector = New DriveDetector()
        app = Application.Current
        thisInstance = Me
    End Sub

    Public Function CheckForCancelWait(drive As String) As Boolean
        If CANCEL_WAIT_FOR_DRIVE_LETTER Is Nothing Then
            Return False
        End If
        If CANCEL_WAIT_FOR_DRIVE_LETTER = drive Then
            If FLAG_CANCEL_WAIT_FOR_DRIVE Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Delegate Function CheckForCancel(drive As String)

    Private Async Function WaitForDriveToRespond(drive As String) As Task(Of Boolean)

       
        Dim dd As DeviceDetecter = DeviceDetecter.GetInstance
        Await app.Dispatcher.BeginInvoke(log, {"Waiting for " & drive & " to be ready!!!!", "Device Detected"})
        Dim counter As Integer = 0


        While Not (My.Computer.FileSystem.DirectoryExists(drive) And counter < 60)

            counter += 1
            Await Task.Delay(1000)
            If dd.CheckForCancelWait(drive) Then

                app.Dispatcher.Invoke(log, {"Device Removed", "Welcome"})

                Exit While
                Return False
            End If
            Await WaitForDriveToRespond(drive)


        End While
        If (counter >= 59) Then
            app.Dispatcher.Invoke(log, {"Scanning media canceled"})
            Return False
        End If
        Await app.Dispatcher.BeginInvoke(log, {"Scanning Drive", "Device Detected"})
        Return True
    End Function

    Public Sub CDInserted()
        Application.LemLog("CD Detected ")
    End Sub

    Public Async Sub Drivedetected(sender As Object, e As DriveDetectorEventArgs) Handles m_Detector.DeviceArrived
        If (IS_WAITING) Then
            Return

        End If
        If Not (LoginData.GetAppModeFromString(LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.mode)) = LoginData.AppMode.loggedIn) Then
            IS_WAITING = True
            MsgBox("Disconnect yo shit... Login first!... then plug it back up")
            IS_WAITING = False
            Return

        End If




        e.Cancel = True
        If e.Drive.StartsWith(Application.LOCAL_CD_DRIVE.First()) Then
            CDInserted()
            Return
        End If
        
        m_currentDrive = e.Drive
        IS_EXTERNAL_DRIVE = True


        FLAG_CANCEL_WAIT_FOR_DRIVE = False
        'CANCEL_WAIT_FOR_DRIVE_LETTER = Nothing
        Application.LemLog("Current Device is channeling through " & e.Drive)
        Dim dic As Dictionary(Of String, List(Of FileInfo)) = New Dictionary(Of String, List(Of FileInfo))
        Dim tempList As List(Of FileInfo) = New List(Of FileInfo)

        'close autoplay window
        IS_WAITING = True
        Dim ready = Await WaitForDriveToRespond(m_currentDrive)
        IS_WAITING = False
        If Not CANCEL_WAIT_FOR_DRIVE_LETTER Is Nothing Then
            If CANCEL_WAIT_FOR_DRIVE_LETTER = e.Drive Then
                If FLAG_CANCEL_WAIT_FOR_DRIVE Then
                    Return
                End If
            End If
        End If

        If ready Then
            dic.Add("My Device", MusicSource.GetList(m_currentDrive))
            dic.Add("My Tracks", MusicSource.GetList(app.GetCurrentArtist.MP3Path))




            If app.GetCurrentArtist Is Nothing Then
                Dim tempartist As Artist = New Artist(Application.MASTER_LOGIN, True)


            End If
            MusicList.DisplaySoloWindow(dic, MusicList.MusicListType.GeneralPlayList, True, app.GetCurrentArtist, m_currentDrive, "Files Found")
        End If




        'call findandsort









    End Sub

    Private Sub DeviceRemoved(sender As Object, e As DriveDetectorEventArgs) Handles m_Detector.DeviceRemoved
        e.Cancel = True
        Application.LemLog(e.Drive & " Was Removed")
        m_currentDrive = "*"
        If IS_WAITING Then
            CANCEL_WAIT_FOR_DRIVE_LETTER = e.Drive
            FLAG_CANCEL_WAIT_FOR_DRIVE = True
        End If
        If MusicList.LiveMusicListWindow Then
            MusicList.CloseListWindow()

        End If

        If Application.IsFloaterPlaying Then
            Application.ResetFloater()

        End If
        IS_EXTERNAL_DRIVE = False


    End Sub

    Private Function gatherandsort(drive As String) As List(Of FileInfo)()


    End Function

    ''' <summary>
    ''' Handler sub for adding beats/ringtones to a playlist
    ''' that can be passed to MovetracksToLocalSpace
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub handleAddToMyBeats(sender As Object, e As EventArgs)

    End Sub
    Public Delegate Sub lemLog(message As String, overallStat As String)


    ''' <summary>
    ''' Send in a FileInfo or a list of fileinfo
    ''' </summary>
    ''' <param name="trackOrTracks"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    'Private Function MovetracksToLocalSpace(trackOrTracks As MusicList) As List(Of String)
    '    Dim result As List(Of String) = New List(Of String)

    '    Dim LemLog As lemLog = New lemLog(AddressOf app.LemLog)
    '    If Not trackOrTracks.Type = MusicList.MusicListType.Relocation Or trackOrTracks.Artist Is Nothing Then
    '        For Each track As MusicList.MusicListItem In trackOrTracks.TheList

    '            app.Dispatcher.BeginInvoke(LemLog, {"Copying " & track.trackFileInfo.Name, "Moving " & trackOrTracks.TheList.Count & " Tracks to Computer "})

    '            Dim newLocation As String = Application.BASE_PATH & "AquiredBeats\" & track.trackFileInfo.Name & track.trackFileInfo.Extension

    '            My.Computer.FileSystem.CopyFile(track.trackFileInfo.FullName, newLocation)
    '            result.Add(newLocation)




    '        Next
    '        Return result
    '    End If




    '    Try


    '        For Each track As MusicList.MusicListItem In trackOrTracks.TheList
    '            app.Dispatcher.BeginInvoke(LemLog, {"Copying " & track.trackFileInfo.Name, "Moving " & trackOrTracks.TheList.Count & " Tracks to " & trackOrTracks.Artist.My_Name & "s Personal Location"})

    '            Dim newLoc As String = trackOrTracks.Artist.RecordingPath & "Private Tracks\" & track.trackFileInfo.Name & track.trackFileInfo.Extension

    '            My.Computer.FileSystem.CopyFile(track.trackFileInfo.FullName, newLoc)
    '            result.Add(newLoc)


    '        Next

    '    Catch ex As Exception
    '        Console.WriteLine(ex.StackTrace)

    '    End Try
    '    app.Dispatcher.BeginInvoke(LemLog, {"Copying Completed", "Welcome "})

    '    Return result
    'End Function





End Class
