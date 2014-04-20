Imports System.IO
Imports System.Collections.ObjectModel




Public Class SetupSession
    Inherits DependencyObject

    Private SessionList As ObservableCollection(Of Session)

    Private LocalPriorities As SongData.SongPriority




    Private currentLoginResource As XMLResources

    Private currentArtist As Artist

    Private m_resources As XMLResources

    Private currentSession As Session

    Private musicSource As SessionMusicSource_Type = SessionMusicSource_Type.none

    Private defaultSongName As String = "Unfinishedsong_" & Now.ToString

    Private statBar As Integer = 0

    Public Property New_Song_Name As String
       


    Sub New()
        ' TODO: Complete member initialization 
    End Sub

    Public Function setArtist(ByRef artist As Artist) As Boolean
        currentArtist = artist

        If artist.My_Name = "MasterLoginSession" Then

            ' app.Dispatcher.Invoke(_statProg, {20, bar})

            '  App.Dispatcher.Invoke(_statText, "Master Session!!!")

            Return True
        End If


        m_resources = New XMLResources(Me)


        Return True
    End Function

    Public Function GetSongPrioritys() As SongData.SongPriority
        If LocalPriorities Is Nothing Then
            LocalPriorities = SongData.SongPriority.Retrieve(currentArtist.My_Name & "Songs", currentArtist)

        End If
        Return LocalPriorities

    End Function

    Delegate Sub StatWinText(message As String)
    Delegate Sub StatWinProg(progress As Integer, barnum As Integer)
    Public Delegate Function StatWinNewBar(junk As String)
    Delegate Sub StatWinCloseBar(barnum As Integer)


    'Delegate Sub getStatBarDelegate(barnum As Integer)



    Public Sub New(ByVal Artist As Artist)
        currentArtist = Artist




        If Artist.My_Name = "MasterLoginSession" Then


            Exit Sub
        End If







        m_resources = New XMLResources(Me)

        LocalPriorities = GetSongPrioritys()



        XMLResources.SubmitResources()

        'GetSessions()





    End Sub


    Public Enum SessionMusicSource_Type

        SoundClick = 0
        DeviceBeat = 1
        EmailBeat = 2
        FlLive = 3
        FlBounced = 4
        CD = 5
        FlashDrive = 6
        BlueTooth = 7
        Other = 8
        none = 9



    End Enum

    Public Shared Function GetSessionMusicSourceType(ByRef typeAsString As String) As SessionMusicSource_Type

        Select Case typeAsString
            Case "SoundClick"
                Return SessionMusicSource_Type.SoundClick
            Case "DeviceBeat"
                Return SessionMusicSource_Type.DeviceBeat
            Case "EmailBeat"
                Return SessionMusicSource_Type.EmailBeat
            Case "FlLive"
                Return SessionMusicSource_Type.FlLive
            Case "FlBounced"
                Return SessionMusicSource_Type.FlBounced
            Case "CD"
                Return SessionMusicSource_Type.CD
            Case "FlashDrive"
                Return SessionMusicSource_Type.FlashDrive
            Case "BlueTooth"
                Return SessionMusicSource_Type.BlueTooth
            Case "Other"
                Return SessionMusicSource_Type.Other
            Case "none"
                Return SessionMusicSource_Type.none


        End Select
        Return SessionMusicSource_Type.none
    End Function

    Public Shared Function CreatesessionForFeature(songName As String, songGID As String, artistGID As String) As Session
        Dim sess As Session = New Session(songName, songGID, artistGID)
        Return sess




    End Function

    Public Class SessionListComparer
        Implements IComparer(Of Session)


        Public Function Compare(x As Session, y As Session) As Integer Implements IComparer(Of Session).Compare
            Dim up As Integer = 1
            Dim down As Integer = -1
            Dim same As Integer = 0

            If (x.Is_Priority) Then
                If (y.Is_Priority) Then
                    If (x.LocalPriority > y.LocalPriority) Then
                        Return down
                    ElseIf (x.LocalPriority < y.LocalPriority) Then
                        Return up
                    End If
                    Return same
                End If
                Return up

            End If
            If (y.Is_Priority) Then
                Return down
            End If
            Dim result As Integer = Date.Compare(Date.Parse(x.LastOpened), Date.Parse(y.LastOpened))

            Return result

        End Function
    End Class

    Public Sub SortSessions()

        If (SessionList Is Nothing) Then
            SessionList = GetCurrentSessions()

        End If

        Dim sortedlilst As List(Of Session) = SessionList.ToList()


        sortedlilst.Sort(New SessionListComparer)
        sortedlilst.Reverse()
        SessionList.Clear()

        For Each sess As Session In sortedlilst
            SessionList.Add(sess)
        Next

        ' SessionList.Reverse()





    End Sub

    Public Async Function NewSession(baseTrack As BaseTrackInfoWrapper) As Task(Of Session)

        musicSource = SessionMusicSource_Type.SoundClick

        'CREATE NEW SESSION FOLDER IN ARTIST NUENDO PATH AND STORE THE NAME
        ' Dim scfileinfo = baseTrack.m_TrackFileInfo

        Dim tempname = String.Format("{0}-{1}-{2}", "Unfinishedsong_", DateTime.Now.Date.ToShortDateString.Replace("/", "-").Replace("\", "-"), Path.GetFileNameWithoutExtension(baseTrack.m_TrackFileInfo.Name))


        currentSession = New Session(Me, True, songName:=tempname)

        'If Not (currentSession.Song_Object.Initialized) Then
        '    Dim init As Boolean = Await currentSession.intitalize(newSession:=True, uniqueName:=tempname)
        'End If


        'currentSession.Set_Session_Path(tempname)
        Dim blocker As BlockingWindow = New BlockingWindow()
        Application.BlockerWindow()


        If (MacroShop.NewSession(currentSession, baseTrack.BounceLocation, baseTrack)) Then
            'should set current tracksource in session before returning

            Dim result = Await StartAndReturnLiveSession(currentSession)


            Application.BlockerWindow(False)
            Return currentSession

        End If
        Application.BlockerWindow(False)
        Return currentSession

    End Function

    ''' <summary>
    ''' Deletes the specified session from this context
    ''' if softdelete is false, will hard delete all traces of this session, ie: "mp3,logoninfo",session and song gid"
    ''' </summary>
    ''' <param name="session"></param>
    ''' <param name="softdelete"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Delete_Session(session As Session, Optional softdelete As Boolean = True) As Boolean

        Dim todelete As Session = SessionList.Where(Function(s) s.LOCAL_ID = session.LOCAL_ID).FirstOrDefault()

        If Not (todelete Is Nothing) Then
            SessionList.Remove(todelete)

            'update sessionproperties datacontext


            'check to update prioritys

            If (session.Is_Priority) Then
                LocalPriorities.RemovePriority(session.Song_Object.GLOBAL_ID)

            End If


            todelete.DeleteThis()

            If Not softdelete Then
                todelete.DeleteSessionPath()
            Else
                Dim path As String = Application.BASE_PATH & Get_Current_Artist.My_Name & "\" & todelete.Song_Object.Title
                Dim destinationPath As String = Application.BASE_PATH & "Administrator\KeepTheFuckOut\Deleted\" & todelete.Artist_Name & "\" & todelete.Song_Object.Title & "_deleted" + DateTime.Now.Ticks.ToString()

                'If Not (Directory.Exists(destinationPath)) Then
                '    Directory.CreateDirectory(destinationPath)


                'End If


                If (Directory.Exists(path)) Then

                    Try
                        My.Computer.FileSystem.MoveDirectory(path, destinationPath)
                    Catch ex As Exception
                        Application.LEM_ERR(ex.Message)
                        Return False
                    End Try


                End If

               






            End If



            Dim songtodelete As SongData = todelete.Song_Object
            songtodelete.DeleteThis()

            songtodelete.DeleteAllTracesOfSong()
            Return True

        End If



        Return False



    End Function


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="session"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Async Function Open_Previous_Session(ByVal session As Session) As Task(Of Boolean)

        currentSession = session
        Dim args As Session.NewSongArgs = New Session.NewSongArgs
        args.artist = Get_Current_Artist()
        args.song = session.Title
        session.Set_Live(True)
        Dim result = True
        'If Not (session.Song_Object.Initialized) Then
        '    Application.LemLog("Initializing Session...", "")
        '    result = session.intitalizeSync(args, False, session.Song_Object.GLOBAL_ID)
        'End If

        If (result) Then
            session = Await StartAndReturnLiveSession(session)
            If (result) Then


                Return True
            End If
        End If








        Return False


    End Function


    ''' <summary>
    ''' 
    ''' </summary>
    ''' 
    ''' <param name="BeatPath"></param>
    ''' <param name="bouncedBeat">Whether or not the BeatPath param is to and mp3 or FLStudio File</param>
    ''' <remarks></remarks>
    Public Sub NewFLStudoiBeatSession(ByRef BeatPath As String, ByVal bouncedBeat As Boolean)

        musicSource = SessionMusicSource_Type.FlBounced
        currentSession = New Session(Me, True, songName:=defaultSongName)




    End Sub

    Public Function NewFLStudioLiveSession(ByRef session As Session)

        musicSource = SessionMusicSource_Type.FlLive
        Dim started As Boolean = MacroShop.StartNewFLStudioSession(session)
        If (started) Then
            Return True

        End If
        Return False

    End Function
    ''' <summary>
    ''' Called to add new song to login and update every thing
    ''' as a song is closed
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CloseCurrentSession() As Boolean



        If Not (currentSession.Title = currentSession.Song_Object.Title) Then
            currentSession.UpdateName(currentSession.Song_Object.Title, False)

        End If

        Try
            If (m_resources.SongClosing()) Then

                If (StoreData()) Then

                    UpdateAllXML()

                    Return True

                End If

            End If

        Catch
            Throw New Exception("Problem with saving one of the xmls")

            Return False
        End Try


        'currentArtist.StoreAllData()

        ' currentArtist.





        Return False



    End Function

    Private Async Function StartAndReturnLiveSession(ByVal liveSession As Session) As Task(Of Session)

        Dim originalName As String = liveSession.Song_Object.Title
        Dim originalPath As String = liveSession.Final_Recording_Path

        Dim nowOpen As Boolean = Await MacroShop.OpenSession(liveSession)

        If nowOpen Then
            Dim args As Session.NewSongArgs = New Session.NewSongArgs With {.artist = liveSession.GetArtist(), .song = liveSession.SongName}

            'Await liveSession.intitalize(newArgs:=args)


            'Await Task.Delay(2000)
            Dim closeSessionWindow As Appbar.MainWindow = New Appbar.MainWindow
            Application.BlockerWindow(False)
            MacroShop.MaximizeNuendo()


            

            If (Appbar.MainWindow.showNow) Then

                MacroShop.MinimizeNuendo()


                Await Task.Delay(2000)



                Dim choose As CloseSessionOptions = CloseSessionDialog.showCloseSessionDialogue(liveSession.SongName, liveSession)




                Select Case choose.m_Choice
                    Case CloseSessionOptionsChoice.BounceSession
                        Application.NEW_BOUNCE = True


                        'If Not (liveSession.Song_Object.Initialized) Then
                        '    App.Dispatcher.BeginInvoke(Function() liveSession.intitalize(newArgs:=args))

                        'End If

                        Application.BlockerWindow()

                        liveSession.GetMother().New_Song_Name = choose.SongTitle


                      




                        'BEGIN-CORE-BOUNCE
                        Dim result = Await MacroShop.BounceSession(liveSession)


                        If (result = 1) Then
                            'END-CORE-BOUNCE
                            'liveSession.UpdateName(choose.SongTitle)
                            'Await Task.Delay(2000)


                            CloseCurrentSession()
                            Application.BlockerWindow(False)
                            Return liveSession


                        ElseIf (result = 0) Then

                            CloseCurrentSession()
                            Application.BlockerWindow(False)
                            Return liveSession

                        Else

                            CloseCurrentSession()
                            Application.BlockerWindow(False)
                            Return liveSession
                        End If


                    Case CloseSessionOptionsChoice.CloseSessionNoBounce

                        Dim result = Await MacroShop.closeSessionQuick()
                        liveSession.GetMother().New_Song_Name = choose.SongTitle
                        If (result) Then
                            result = Await MacroShop.WaitForMacrosToComplete()
                            CloseCurrentSession()
                            If (result) Then
                                Return liveSession
                            End If
                        End If



                    Case Else
                        MacroShop.Close_Session(liveSession)
                        CloseCurrentSession()
                        Return liveSession



                End Select


            End If
        End If


        'CALL UPDATE FOLDER NAME
        'Process.Start(Application.DOUBLE_CHECK_LAST_SONGFOLDER_RENAME)





        MacroShop.killAllMacros()
        Dim result2 = Await MacroShop.WaitForMacrosToComplete
        If (result2) Then
            result2 = Await MacroShop.closeSessionQuick()

            If (result2) Then
                If Not (originalName = liveSession.Song_Object.Title) Then
                    Utils.writeSongFolderName(originalPath, liveSession.SongName, True)
                    Return liveSession
                End If
            End If
        End If

        Return liveSession


    End Function

    Public Function GetResources() As XMLResources
        Return m_resources
    End Function

    Public Async Function BounceClosedSession(ByVal session As Session) As Task(Of String)

        MacroShop.killAllMacros()

        Dim wait As Task(Of Boolean) = MacroShop.WaitForMacrosToComplete()
        Dim result As Boolean
        MsgBox("try to close session", MsgBoxStyle.SystemModal)
        Dim closed As Task(Of Boolean) = MacroShop.closeSessionQuick()

        'Close all nuendo sessions that may be open


        Dim isClosed As Boolean = Await closed

        If (isClosed) Then
            MsgBox("sessions closed", MsgBoxStyle.SystemModal)
            Dim wait3 As Task(Of Boolean) = MacroShop.WaitForMacrosToComplete()

            result = Nothing
            result = Await wait3
            If (result) Then



                'Open session

                Dim ready As Task(Of Boolean) = MacroShop.OpenSession(session)
                Dim setgo As Boolean = Await ready
                MsgBox("session to be bounced should be open now", MsgBoxStyle.SystemModal)
                result = Nothing
                If (setgo) Then
                    result = Await wait
                End If

                'wait

                If (result) Then


                    'Close extra windows
                    Dim wait2 As Task(Of Boolean) = MacroShop.WaitForMacrosToComplete()
                    Dim result2 As Boolean
                    MacroShop.Close_Extra_Nuendo_Child_Windows()

                    result2 = Await wait2

                    'wait


                    If (result2) Then
                        MsgBox("first call to bounce session", MsgBoxStyle.SystemModal)

                        'bounce session

                        Dim tryBounce As Task(Of Integer) = MacroShop.BounceSession(session)







                        Dim bounced = Await tryBounce

                        MsgBox("try bounce returned " & bounced, MsgBoxStyle.SystemModal)
                        If (bounced = 0) Then
                            Return "Bounce UnSuccessful"
                        ElseIf (bounced = 1) Then
                            Return "Bounce Successful"
                        Else
                            Return "Probably Empty Session"

                        End If



                    Else
                        Return "no"
                    End If

                End If

            End If


        End If


        Return "no"
    End Function





    Class Session
        Inherits Identity




        Private artistOBJ As Artist
        Private songOBJ As SongData
        Private engineer As Artist
        Private songAsID As Int32
        Private artistName As String

        Private mother As SetupSession

        Private sessionPath As String = ""
        Private isNewSession As Boolean = True

        Private musicSource As SetupSession.SessionMusicSource_Type
        Private trackInfo As BaseTrackInfoWrapper
        Private sourceTrackName As String
        Private sourceTrackLocation As String
        Private m_isLive As Boolean = False
        Private basePath As String = Application.BASE_PATH

        Friend Start_Time As Date
        Friend End_Time As Date
        Dim m_SongID As String


        Public ReadOnly Property Is_Live As Boolean
            Get
                Return m_isLive
            End Get



        End Property

        Public Overrides Sub OnRebuild(ByRef GlobalID As Identity.GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")

            If (GlobalID.GLOBAL_ID = m_GlobalID.GLOBAL_ID) Then

                isNewSession = False

                Application.LEM_ERR("Rebuild session Not Implemented yet")
                Throw New NotImplementedException


            End If

        End Sub

        Public Overrides Sub OnNameChanged(ByRef newName As String, ByRef globalId As Identity.GlobalID, Optional ByRef data As Object = "")
            MyBase.OnNameChanged(newName, globalId)


            Try
                ChangeNameForBounce = Convert.ToBoolean(data)


            Catch
                '   ChangeNameForBounce = True
            End Try


            If (Song_Object.Title = Old_Name) Then
                Song_Object.UpdateName(newName, ChangeNameForBounce)

                ' artistOBJ.StoreAllData()

            End If


        End Sub

        Public Structure NewSongArgs
            Property artist As Artist
            Property song As String
        End Structure

        Public Sub New(SongName As String, songGID As String, artistLID As String)
            Dim art As Artist = Nothing
            If (songGID.Contains("UNT-")) Then
                Dim myname As String = Utils.GetNameByGlobalID(artistLID)
                art = Identity.MakeNew(ClassCode.Artist, myname)
            Else

                art = Identity.MakeNew(ClassCode.Artist, Utils.GetNameByGlobalID("UNT-12000-" & artistLID))
            End If





            If art Is Nothing Then
                Return
            End If

            Dim song As SongData = New SongData(art, SongName, songGID, art)
            Song_Object = song
            m_isLive = False
            Me.Start_Time = Now




        End Sub

        Public Delegate Function CreateSongDelegate(theArtist As Artist, song As String)
        ''' <summary>
        ''' Sessions have 2 places to get their data. First it needs to gather all info from the directory it represents, 2nd
        ''' if a song has been bounced it can gather more info from that as well as at the time it was bounced
        ''' </summary>
        ''' <param name="motherSession"></param>
        ''' <param name="Live"></param>
        ''' <param name="songName"></param>
        ''' <param name="SongGlobID"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef motherSession As SetupSession, ByVal Live As Boolean, ByVal songName As String, Optional ByVal SongGlobID As String = "")


            MyBase.New(ClassCode.Session, songName)

            Dim app As Application = Application.Current
            If (m_GlobalID.IS_NEW) Then
                isNewSession = True
            Else
                isNewSession = False
            End If

            m_isLive = Live
            artistOBJ = motherSession.Get_Current_Artist
            mother = motherSession




            If (m_isLive) Then
                Start_Time = Now
            End If

            Dim newArgs As NewSongArgs = New NewSongArgs
            newArgs.song = songName
            newArgs.artist = artistOBJ











            m_SongID = SongGlobID

            songOBJ = New SongData(newArgs.artist, newArgs.song, noInit:=True)


        End Sub

        Private Async Function LoadSong(newArgs As NewSongArgs) As Task(Of SongData)


            Dim songTask = Await Task(Of SongData).Factory.StartNew(Function(state)
                                                                        Dim _app As Application = Application.Current
                                                                        Dim createNewSong As CreateSongDelegate = New CreateSongDelegate(AddressOf SongData.CreateAndReturn)
                                                                        Dim args As NewSongArgs = CType(state, NewSongArgs)
                                                                        Dim art As Artist = args.artist
                                                                        Dim song As String = args.song
                                                                        Dim temp As SongData = _app.Dispatcher.Invoke(createNewSong, {art, song})
                                                                        Return temp
                                                                        'Return Nothing
                                                                    End Function, newArgs)
            Return songTask
        End Function
        Private Function LoadSongSync(newArgs As NewSongArgs) As SongData


                                               Dim _app As Application = Application.Current
                                                                        Dim createNewSong As CreateSongDelegate = New CreateSongDelegate(AddressOf SongData.CreateAndReturn)

            Dim art As Artist = newArgs.artist
            Dim song As String = newArgs.song
                                                                        Dim temp As SongData = _app.Dispatcher.Invoke(createNewSong, {art, song})
                                                                        Return temp

                                                                    End 
        End Function
        Public Function Reinitialize() As Boolean
            isNewSession = Old_Name.Contains("Unfinishedsong")

            If (Song_Object.ReInitialize()) Then
                Return UpdateSessionPath()


            End If
            Return False
        End Function
        Public Async Function intitalize(Optional newArgs As NewSongArgs = Nothing, Optional ByVal newSession As Boolean = False, Optional uniqueName As String = "", Optional globID As String = "") As Task(Of Boolean)

            If (globID.Contains("UNT-")) Then
                m_SongID = globID
            End If
            If (artistOBJ.AS_Engineer) Then
                engineer = artistOBJ

            End If





            If (Is_Live) Then

                If (newSession) Then


                    If Not (uniqueName = "") Then
                        songOBJ = New SongData(artistOBJ, uniqueName)

                    Else
                        songOBJ = New SongData(artistOBJ, "UnFinishedSong_" & DateTime.UtcNow.Date.ToShortDateString.Replace("/", "-"))
                    End If


                    'logins.Add(motherSession.currentLogin)
                    songAsID = Utils.ExtracLocalFromGlobal(songOBJ.GLOBAL_ID)
                    artistName = artistOBJ.My_Name

                    sessionPath = artistOBJ.RecordingPath & songOBJ.Title & "\"
                    isNewSession = True

                    musicSource = mother.Get_Music_Source
                    Return True

                Else

                    songOBJ = Await LoadSong(newArgs)

                    Return True






                End If


            Else

                songOBJ = New SongData(newArgs.artist, newArgs.song)
                m_SongID = songOBJ.GLOBAL_ID
                If Not (SongName = "") And (m_SongID.Contains("UNT-")) Then

                    If (newArgs.artist Is Nothing) Or (newArgs.song = String.Empty) Then
                        Return False
                    End If

                    songOBJ = Await LoadSong(newArgs)




                    songAsID = songOBJ.LOCAL_ID



                    musicSource = songOBJ.Music_Source



                    artistName = artistOBJ.My_Name

                    sessionPath = artistOBJ.RecordingPath & songOBJ.Title & "\"

                    'Song Has Already been created
                    'Dim waitForSongInit = songOBJ.WaitforInitialization


                    'If (waitForSongInit.Result = True) Then



                    '    songAsID = songOBJ.LOCAL_ID

                    '    musicSource = songOBJ.Music_Source

                    '    artistName = artistOBJ.My_Name



                    'Else
                    '    Throw New Exception("trying to instantiate old song named with missing name and/or gid")


                End If




                'Return True


            End If






            'Else
            '    Throw New Exception("Creating A New Dead Session Unexpectedly")


            'End If
            Return False
        End Function
        Public Function intitalizeSync(Optional newArgs As NewSongArgs = Nothing, Optional ByVal newSession As Boolean = False, Optional uniqueName As String = "", Optional globID As String = "") As Boolean

            If (globID.Contains("UNT-")) Then
                m_SongID = globID
            End If
            If (artistOBJ.AS_Engineer) Then
                engineer = artistOBJ

            End If





            If (Is_Live) Then

                If (newSession) Then


                    If Not (uniqueName = "") Then
                        songOBJ = New SongData(artistOBJ, uniqueName)

                    Else
                        songOBJ = New SongData(artistOBJ, "UnFinishedSong_" & DateTime.UtcNow.Date.ToShortDateString.Replace("/", "-"))
                    End If


                    'logins.Add(motherSession.currentLogin)
                    songAsID = Utils.ExtracLocalFromGlobal(songOBJ.GLOBAL_ID)
                    artistName = artistOBJ.My_Name

                    sessionPath = artistOBJ.RecordingPath & songOBJ.Title & "\"
                    isNewSession = True

                    musicSource = mother.Get_Music_Source
                    Return True

                Else

                    songOBJ = LoadSongSync(newArgs)

                    Return True






                End If


            Else

                songOBJ = New SongData(newArgs.artist, newArgs.song)
                m_SongID = songOBJ.GLOBAL_ID
                If Not (SongName = "") And (m_SongID.Contains("UNT-")) Then

                    If (newArgs.artist Is Nothing) Or (newArgs.song = String.Empty) Then
                        Return False
                    End If

                    songOBJ = LoadSongSync(newArgs)




                    songAsID = songOBJ.LOCAL_ID



                    musicSource = songOBJ.Music_Source



                    artistName = artistOBJ.My_Name

                    sessionPath = artistOBJ.RecordingPath & songOBJ.Title & "\"

                    'Song Has Already been created
                    'Dim waitForSongInit = songOBJ.WaitforInitialization


                    'If (waitForSongInit.Result = True) Then



                    '    songAsID = songOBJ.LOCAL_ID

                    '    musicSource = songOBJ.Music_Source

                    '    artistName = artistOBJ.My_Name



                    'Else
                    '    Throw New Exception("trying to instantiate old song named with missing name and/or gid")


                End If




                'Return True


            End If






            'Else
            '    Throw New Exception("Creating A New Dead Session Unexpectedly")


            'End If
            Return False
        End Function

        Private Function UpdateSessionPath() As Boolean
            sessionPath = artistOBJ.RecordingPath & songOBJ.Title & "\"
            Return True
        End Function

        Public Function StoreSessionData() As Boolean




            Return songOBJ.StoreData()







        End Function

        Private ChangeNameForBounce As Boolean = False

        Public Function UpdateName(ByVal newName As String, forBounce As Boolean) As String
            If (forBounce) Then
                If (m_name = newName) Then
                    If (System.IO.File.Exists(Song_Object.MP3_File_Path)) Then
                        System.IO.File.Delete(Song_Object.MP3_File_Path)


                    End If

                    If (Not Song_Object.MP3_File_Path = "Not Bounced") Then
                        Application.TEMP_BOUNCE_PATH = Song_Object.MP3_File_Path
                    End If

                    Return m_name
                End If
            End If
            ChangeNameForBounce = forBounce
            Old_Name = m_name
            Return m_GlobalID.UpdateName(newName, data:=forBounce)



        End Function

        Public Function GetMother() As SetupSession

            Return mother


        End Function

        Public Function GetSong() As SongData
            Return Song_Object

        End Function

        Public Function GetArtist() As Artist
            Return artistOBJ
        End Function

        Public Function CreateNotification(ByVal type As Notification.NotificationType, ByRef toWhom As Artist, ByVal message As String, ByVal atLogin As Boolean, ByRef callBack As Func(Of Boolean)) As Int16



            Return 0
        End Function
        Public Function DeleteSessionPath() As Boolean
            If (Directory.Exists(Final_Recording_Path)) Then
                My.Computer.FileSystem.DeleteDirectory(Final_Recording_Path, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If

            Return True

        End Function
        Public Function Set_SourceTrack_Info(ByRef trackName As String, ByRef trackLocation As String)
            sourceTrackLocation = trackLocation
            sourceTrackName = trackName
            Return True
        End Function

        Public Function Set_Session_Path(ByRef newPath As String) As String
            sessionPath = newPath
            Return sessionPath
        End Function

        'Private Function GetPreviousLogin()


        '    Dim loginNode As XElement = Application.g






        'End Function


        Public Property SongName As String
            Get
                If Not songOBJ Is Nothing Then
                    Return songOBJ.Title
                End If

                Return ""
            End Get
            Set(value As String)
                If Not (value = Song_Object.Title) Then
                    If Not (value = "") And Not (value.Contains(".mp3")) Then
                        UpdateName(value, False)

                    End If
                End If
            End Set

        End Property

        Public Property LocalPriority As Int32
            Get

                Dim result = mother.GetSongPrioritys().GetPriority(songOBJ.GLOBAL_ID)

                Return result




            End Get
            Set(value As Int32)

            End Set

        End Property
        Public ReadOnly Property PriorityCount As Int32
            Get
                Return mother.PriorityCount
            End Get
        End Property
        Property Song_Object As SongData
            Get
                Return songOBJ
            End Get
            Set(value As SongData)
                songOBJ = value
            End Set
        End Property

        Property Song_As_ID As Int32
            Get
                Return songAsID
            End Get
            Set(value As Int32)

            End Set
        End Property

        Property Is_New_Song As Boolean
            Get
                Return isNewSession
            End Get
            Set(value As Boolean)
                isNewSession = value
            End Set
        End Property

        ReadOnly Property Artist_Name As String
            Get
                Return mother.Get_Current_Artist.My_Name
            End Get
           
        End Property

        Public ReadOnly Property Has_Comments As Boolean
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Has_Features As Boolean
            Get
                Return Song_Object.Has_Features
            End Get
        End Property

        ReadOnly Property Is_Priority As Boolean
            Get
                Return LocalPriority > -1
            End Get
        End Property


        ReadOnly Property Final_Recording_Path As String
            Get
                If (sessionPath = "") Then
                    sessionPath = artistOBJ.RecordingPath & Title & "\"
                End If

                Return sessionPath
            End Get

        End Property

        ReadOnly Property LastOpened As DateTime
            Get
                Return songOBJ.ModifiedDate
            End Get

        End Property

        Property Bounced As String
            Get
                If (Song_Object.IsBounced) Then
                    Return "Yes"
                End If
                Return "No"
            End Get
            Set(value As String)


            End Set
        End Property



        Property Featuring As String
            Get
                If (Has_Features) Then
                    Dim nameList As List(Of String) = New List(Of String)
                    For Each feat As SongData.FeatureInfo In Song_Object.Get_Features_List
                        nameList.Add(feat.Artist_Name)

                    Next
                    Return Utils.CombineNames(nameList)
                End If
                Return "none"
            End Get
            Set(value As String)

            End Set
        End Property

        Property Mix_Ready As String
            Get
                Dim result As String = "No"
                If (songOBJ.MixReady) Then
                    result = "Yes"
                End If
                Return result
            End Get
            Set(value As String)
                If (value = "Yes") Then
                    songOBJ.MixReady = True
                End If
                songOBJ.MixReady = False

            End Set
        End Property

        ReadOnly Property Feature_Count As Int32
            Get
                Return songOBJ.Get_Features_List.Count
            End Get

        End Property

        ReadOnly Property Music_Source As String
            Get
                Return musicSource.ToString
            End Get
        End Property

        Sub Set_Live(ByVal value As Boolean)
            m_isLive = value

            If (m_isLive) Then
                Start_Time = Now
            End If
        End Sub


        Public ReadOnly Property Title
            Get
                Return songOBJ.Title
            End Get
        End Property



    End Class

    Public Delegate Function utilsFunc(text As String)

    Public Delegate Function GetCurrentArtist()

    Public Delegate Function GetCurrentSetupSession()

    Public Function GetSessions(songs As List(Of String)) As Boolean

        If (SessionList Is Nothing) Then
            SessionList = New ObservableCollection(Of Session)
        End If


        Dim app As Application = Application.Current
        Dim updateText As StatWinText = New StatWinText(AddressOf Application.StatwinText)

        Dim addsessiondel = New Application.AddSessionDelegate(AddressOf Application.AddSessionToCurrentSessionWindow)

        'Dim refreshSessionsWindowDel = New Application.RefreshSessionWindowSessionDelegate(AddressOf app.RefreshSessionWindowSession)

        For Each song As String In songs
            Dim temp As Session = New Session(Me, False, song)
            'Dim args = New Session.NewSongArgs
            'args.artist = currentArtist
            'args.song = song
            ' Dim ready = Await temp.intitalize(args)

            app.Dispatcher.BeginInvoke(addsessiondel, temp)
            'app.Dispatcher.BeginInvoke(refreshSessionsWindowDel, temp)



            'SessionList.Add(temp)
            'app.Dispatcher.Invoke(updateText, temp.SongName & " loaded")
            'Await Task.Delay(10)

        Next


        'app.Dispatcher.Invoke(updateText, "")
        'Await Task.Delay(10)





        ' Next
        If (Application.IdentityHasChanged) Then
            SongData.Update_Songs_XML()
            Application.IdentityHasChanged = False
        End If
        XMLResources.SubmitResources()
        Return True


    End Function

    Public Function GetSongSessionList(ByRef Artist As Artist) As List(Of String)

        Dim FinalList As List(Of String) = New List(Of String)
        'Dim _session As SetupSession = New SetupSession(Artist_Name)
        ' Dim songList = SetupSessi


        ' ListBox1.Items.Clear()
        ' ListBox1.Items.Add("Start NEW Session")
        Dim skip(0 To 2) As String
        skip(0) = "Audio"
        skip(1) = "Images"
        skip(2) = "Edits"
        Dim count As Integer = 0
        Dim willSkip As Boolean
        Dim songList As List(Of String) = Directory.GetDirectories(Artist.RecordingPath).ToList
        'songList = sortSongList(songList)

        For Each songFolder As String In songList
            willSkip = False
            For Each unwantedName As String In skip
                If (songFolder.Contains(unwantedName)) Then
                    willSkip = True



                End If

            Next
            If (willSkip) Then
                Continue For
            Else
                count += 1



                Dim fnl = Split(songFolder, "\")

                FinalList.Add(removeFeatureText(fnl.GetValue(fnl.Count - 1)))
            End If


            ' ListBox1.Items.Add(count.ToString() & ". " & Path.GetFileName(songFolder))



        Next

        Return FinalList


    End Function

    Public Overrides Function ToString() As String
        Dim result = ""
        Dim curr = Get_Current_Song
        If Not curr Is Nothing Then
            result = curr.Title
        End If
        Return result
    End Function

    Public Function removeFeatureText(ByVal textToClean As String) As String
        If (textToClean.Contains("-feat")) Then
            textToClean = Split(textToClean, "-feat").GetValue(0)

        End If
        Return textToClean
    End Function

    Public Sub UpdateAllXML()

        If (Application.IdentityHasChanged) Then
            SongData.Update_Songs_XML()
            Identity.GlobalID.CommitAllData()
            'Song_Object.SubmitChanges()
            Application.IdentityHasChanged = False
        End If

    End Sub

    Public Function GetSessionCount() As Int32
        If (SessionList Is Nothing) Then
            SessionList = New ObservableCollection(Of Session)
        End If
        Return SessionList.Count
    End Function

    Public ReadOnly Property Get_Session_Count As Int32
        Get
            If (SessionList Is Nothing) Then
                SessionList = New ObservableCollection(Of Session)
            End If
            Return SessionList.Count
        End Get
    End Property

    Public ReadOnly Property Get_Login_ID As String
        Get
            Return m_resources.Login_ID.GLOBAL_ID


        End Get
    End Property

    Public ReadOnly Property Get_Login_GLOBALID As Identity.GlobalID
        Get
            Return m_resources.Login_ID
        End Get
    End Property
    'Public ReadOnly Property Has_Comments As Boolean
    '    Get
    '        For Each song As Session In SessionList
    '            If song.Has_Comments Then
    '                Return True
    '            End If
    '        Next
    '        Return False
    '    End Get
    'End Property

    'Public ReadOnly Property Has_Features As Boolean
    '    Get
    '        For Each song As Session In SessionList
    '            If song.Has_Features Then
    '                Return True
    '            End If
    '        Next
    '        Return False
    '    End Get
    'End Property

    'Public ReadOnly Property Needs_Features As Int32
    '    Get
    '        Dim count = 0
    '        For Each song As Session In SessionList
    '            If song.Song_Object.Needs_Feature Then
    '                count = count + 1
    '            End If
    '        Next
    '        Return count
    '    End Get
    'End Property
    Property Session_List As ObservableCollection(Of Session)
        Get
            If (SessionList Is Nothing) Then

                SessionList = New ObservableCollection(Of Session)

            ElseIf (SessionList.Count <= 0) Then
                SessionList = GetCurrentSessions()
            End If
            Return SessionList

        End Get
        Set(value As ObservableCollection(Of Session))

        End Set
    End Property



    Public ReadOnly Property Get_Session(ByVal name As String) As SetupSession.Session
        Get
            For Each ssn As Session In SessionList
                If (ssn.Song_Object.Title = name) Then
                    Return ssn
                End If
            Next
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property Artist_Has_Priorites() As Boolean
        Get
            For Each ssn As Session In SessionList
                If (ssn.Song_Object.Is_Priority) Then
                    Return True
                End If
            Next
            Return False
        End Get
    End Property

    Public ReadOnly Property Artist_Has_Comments() As Boolean
        Get
            For Each ssn As Session In SessionList
                If (ssn.Song_Object.Has_Comments) Then
                    Return True
                End If
            Next
            Return False
        End Get
    End Property



    Public ReadOnly Property PriorityCount
        Get
            Return LocalPriorities.Count

        End Get
    End Property

    Public ReadOnly Property Get_PriorityList As List(Of SongData.SongPriority)
        Get
            Dim list As List(Of SongData.SongPriority) = New List(Of SongData.SongPriority)
            For Each ssn As Session In SessionList
                If (ssn.Song_Object.Is_Priority) Then
                    'list.Add(ssn.Song_Object.)


                End If
            Next
            Return list
        End Get
    End Property

    Public Property App As Application





    Public ReadOnly Property Get_Current_Artist As Artist
        Get
            Return currentArtist
        End Get
    End Property

    Public ReadOnly Property Get_Current_Song As SongData
        Get

            If (currentSession Is Nothing) Then
                If (Session_List.Count > 0) Then
                    currentSession = Session_List.Item(0)
                Else
                    Return Nothing
                End If

            End If
            Return currentSession.Song_Object
        End Get
    End Property

    Public ReadOnly Property Get_Current_Session As SetupSession.Session
        Get
            If (currentSession Is Nothing) Then
                Dim tempname = String.Format("{0}-{1}", "Empty-", DateTime.Now.Ticks.ToString())


                currentSession = New Session(Me, True, songName:=tempname)
            End If
            Return currentSession
        End Get
    End Property

    Public ReadOnly Property Get_Music_Source
        Get
            Return musicSource
        End Get
    End Property

    Public Function GetCurrentSessions() As ObservableCollection(Of Session)

        Return GetSessions()

    End Function

    Private Function GetSessions() As ObservableCollection(Of Session)
        If SessionList Is Nothing Then
            SessionList = New ObservableCollection(Of Session)
            SortSessions()

        End If
        Return SessionList

    End Function
    Public Delegate Sub UpdateSessionListDelegate(session As Session)
    Public Sub UpdateSessionList(session As Session)

        Dim existing As Session = Session_List.Where(Function(s) s.Final_Recording_Path = session.Final_Recording_Path).FirstOrDefault()
        If (existing Is Nothing) Then
            Session_List.Add(session)
        Else
            Dim index = Session_List.IndexOf(existing)

            Session_List.Item(index) = session
            'Session_List.Add(session)


        End If




    End Sub


    Public Function StoreData() As Boolean


        'For Each ssn As Session In SessionList
        '    ssn.StoreSessionData()
        'Next
        Dim i = 0
        For Each ses As Session In GetSessions()
            If (ses.StoreSessionData()) Then
                i += 1
            End If


        Next

        Return i = GetSessions().Count
    End Function

    Public Function GetSessionBySongID(id As Integer) As Session
        Dim sess As Session = Session_List.FirstOrDefault(Function(s) s.Song_Object.LOCAL_ID = id)
        Return sess
    End Function

    Shared Function GetSessionSetup(ByVal artist As Artist) As SetupSession
        Dim result As SetupSession = New SetupSession(artist)

        Return result
    End Function







End Class