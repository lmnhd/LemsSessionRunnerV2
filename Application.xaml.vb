Imports System.Collections
Imports System.Configuration
Imports System.Data
Imports System.Linq
Imports System.Windows
Imports System.Timers
Imports System.Text
Imports System.IO
Imports System.Xml
Imports System.Threading
Imports System.Globalization
'Imports System.Threading.Tasks.Dataflow
Imports COMHelpers.RegisterDLL
Imports NAudio.Wave
Imports WindowUtils
Imports LemsWebCallCenter.CallCenter
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports LemsUTools.WaveAndMp3Manipulation

'quick find debugging points using finder
'BEGIN-CORE-?
'END-CORE-?


Public Class Application
    Public Shared LEM_MESSAGES As String = ""
    Public Shared LOCAL_HOST As Boolean = True
    Public Shared useMacros As Boolean = True
    Public Shared UseTEST_MESS = False
    Public Shared DisAbleFloater = False
    Public Shared SERVER_ADDRESS = ""
    Public Shared SERVER_LOGIN = ""
    Public Shared SERVER_PASS = ""
    Public Shared UPLOAD_FINALMIX_TO_SERVER = True
    Public Shared DISABLE_LOGINS = False

    Public Shared RETRY_SAVE_CANLOGINXML = False
    Public Shared MASTER_CANLOGINDOC As XDocument
    Public Shared SAVE_CANLOGINXML_TIMER As System.Timers.Timer
    Public Shared SAVE_CANLOGINXML_TRY_COUNT = 0


    Public Shared UseLemPlayerInsteadOfMediaPlayer As Boolean = False
    Public Shared CheckModeAtStartUp = True
    Public Shared PreUpdateUsedBeats As Boolean = False
    Public Shared UpdateSongInfoXML = False
    Public Shared UpdateLoginInfoXML = False

    Public Shared statWin As ProgressModalWindow
    Public Shared TEMPORARY_FILENAME_HOLDER As String = ""

    Public Shared XML_PATH As String = "xml\"
    Public Shared IDENTITY_PATH As String = "xml\Identity.xml"
    Public Shared PASSWORDS_PATH As String = "xml\passwords.xml"
    Public Shared RESOURCES_PATH As String = "xml\resources.xml"
    Public Shared LOGIN_HANDLE As String = "xml\LoginInfo.xml"
    Public Shared SessionRunning As Boolean = False

    Public Shared GLOBAL_ARTIST As String = "GLOBAL_ARTIST"
    Public Shared Current_Booking As StudioBCallCenter.HTTPConnect.BookingProxy
    Public Shared UNTAMEHTTP As StudioBCallCenter.HTTPConnect
    Public Shared UNTAMEFTP As StudioBCallCenter.FTPConnect





    Public Shared CurrentState As Integer = 0
    Public Shared selectedSong As SongData
    Public Shared data As List(Of LoginData)


    Public Shared TEMP_BOUNCE_PATH As String = ""
    Public Shared propsLoaded As Boolean = False

    Private Shared artistRecordingBasePath As String
    Private Shared artistRecordingSongPath As String
    Private Shared listMode As Integer = 0
    Private Shared index As Integer = 0
    Private Shared sessionCount As Integer = 0
    Private Shared SongList As List(Of String)
    Private Shared CurrentSessions As SetupSession
    Private Shared m_CurrentArtist As Artist = Nothing

    Private Shared WithEvents m_BackGroundWorker As BackgroundWorker


    Private Shared m_Session_Window As SessionWindow


    Private Shared AppBar As Appbar.MainWindow

    Public Shared ArtistList As List(Of Artist)

    Private Shared thisApp As Application


    Public Shared IdentityHasChanged As Boolean = False

    Public Shared ResourcesHaveChanged As Boolean = False

    Private Shared mw As MainWindow

    Public Shared statWinBars As List(Of ProgressBar)
    Public Shared SoundClickList As List(Of SessionWindow.TrackWrapper) = New List(Of SessionWindow.TrackWrapper)


    Public Shared AllProducersAndBeats As List(Of Tuple(Of String, List(Of SessionWindow.TrackWrapper))) = New List(Of Tuple(Of String, List(Of SessionWindow.TrackWrapper)))


    Public Shared MASTER_LOGIN As String = "MasterLoginSession"
    Public Shared MP3_PATH As String
    Public Shared INTERNET_BEATS_DIRECTORY As String
    Public Shared INHOUSE_LOCATION As String
    Public Shared PRODUCER_BEATS_FOLDER As String
    Public Shared BOUNCE_SESSION_SCRIPT As String
    Public Shared START_SESSION_AUDITION_BEATS As String
    Public Shared START_SESSION_WITH_BEAT As String
    Public Shared CLEAN_CLOSE_SESSION As String
    Public Shared MAXNUENDO As String
    Public Shared MINNUENDO As String
    Public Shared TEMPLATE_FILE As String
    Public Shared TEMPLATE_DIRECTORY As String
    Public Shared CLOSE_THIS As String
    Public Shared CLOSE_SELECTED_DIRECTORY_WINDOW As String
    Public Shared BRING_TO_FRONT As String
    Public Shared MACROS_DIRECTORY As String
    Public Shared LOCAL_CD_DRIVE As String
    Public Shared DOUBLE_CHECK_LAST_SONGFOLDER_RENAME As String
    Public Shared OPEN_NEWLY_CREATED_SESSION As String
    Public Shared GET_SCHEDULE As String
    Public Shared LOCAL_HOST_ADDRESS As String
    Public Shared USE_SCHEDULE As Boolean
    Public Shared INTRO_MESSAGE As String
    Public Shared INTRO_MESSAGE2 As String


    Public Shared CURRENT_SESSION As SetupSession.Session
    Private Shared WithEvents FloatingPlayer As FloatingLemPlayer
    Private CREATED As Boolean = False
    Private Shared FLAG_LOGIN_INFO_INITIALIZED = False
    Private Shared FLAG_UPDATING_LOGINS = False
    Public Shared BASE_PATH As String
    Private songInfoUpdater As SongData

    Public Shared NEW_BOUNCE = False
    Private Shared cleanupXMLforDebug As Boolean = False
    Friend Shared MWHANDLE As LemsSessionRunnerV2.MainWindow

    Public Shared ARTIST_RELEVANCE_OBJECT As GLOBALARTISTRELEVANCE

    Public Shared CURRENT_SESSION_LIST As List(Of SetupSession.Session)
    Public Shared LEMS_BACKDOOR As String = "Lem"
    Public Shared LEMS_BACKDOOR_NO_XML As String = "LemTest"
    Public Shared LEMTESTING As Boolean = False

    Public Shared NAMESKIPARRAY(1) As String

    Public Shared TEMP_SESSION_FOLDER_PATH_OLD As String = ""
    Public Shared TEMP_SESSION_FOLDER_NAME_NEW As String = ""
    Public Shared SONG_INFO_XML As SongInfoXML = New SongInfoXML()
    Public Shared XMLRESOURCES As XMLResources = XMLRESOURCES.GET_XMLResources()

    Public Shared SERVER_ERROR As Boolean = False


    Public Shared MASTER_CANCEL As CancellationToken = New CancellationToken

    Private Shared Property PRODUCER_BEATS_LOADED As Boolean

    Private Shared Property SOUNDCLICK_LIST_LOADED As Boolean



    Public Shared Sub UpdateSessionFolderName()



        If Not (TEMP_SESSION_FOLDER_PATH_OLD = "") Then

            If Not (TEMP_SESSION_FOLDER_NAME_NEW = "") Then
                Dim parentPath = Path.GetDirectoryName(TEMP_SESSION_FOLDER_PATH_OLD)
                Dim newPath = parentPath + "\" + TEMP_SESSION_FOLDER_NAME_NEW
                If (My.Computer.FileSystem.DirectoryExists(TEMP_SESSION_FOLDER_PATH_OLD)) Then

                    Dim directoryname = My.Computer.FileSystem.GetName(TEMP_SESSION_FOLDER_PATH_OLD)

                    If Not (directoryname = TEMP_SESSION_FOLDER_NAME_NEW) Then
                        If (Directory.Exists(newPath)) Then
                            Return
                            'TEMP_SESSION_FOLDER_NAME_NEW = TEMP_SESSION_FOLDER_NAME_NEW + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.TimeOfDay.TotalMilliseconds.ToString()

                        End If
                        My.Computer.FileSystem.RenameDirectory(TEMP_SESSION_FOLDER_PATH_OLD, TEMP_SESSION_FOLDER_NAME_NEW)
                    Else
                        'TEST_MESS("Problem Renaming folder_APPLICATION.UpdateSessionFolderName")
                        'Application.LEM_ERR("Problem Renaming folder_APPLICATION.UpdateSessionFolderName")
                    End If
                End If
            End If
        End If

    End Sub

    Public Shared Async Sub SendSongToDataBase(song As SongData)
        If (LOCAL_HOST) Then
            ' Return
        End If
        If Not (song.IsBounced) Then
            Return
        End If
        Try
            Dim relativeUrl = song.Artist_Name + "/" + Utils.addFeatureText(song.Title, song.Get_Features_List) + ".mp3"
            If Not (song.GetArtist().Is_Untame_Artist) Then
                relativeUrl = "Outhouse/" + song.Artist_Name + "/" + Utils.addFeatureText(song.Title, song.Get_Features_List) + ".mp3"
            End If
            Dim movetoserver As Task = Task.Factory.StartNew(Sub()
                                                                 Dim prox = New StudioBCallCenter.HTTPConnect.SongProxy()
                                                                 prox.SessionRunnerLocalID = song.LOCAL_ID
                                                                 prox.Title = song.Title
                                                                 prox.Date = song.ModifiedDate
                                                                 prox.Features = Utils.GetFeaturesStringList(song.Get_Features_List)
                                                                 prox.LengthInMillis = song.LengthInMillis
                                                                 prox.RelativeUrl = relativeUrl
                                                                 prox.ArtistName = song.Artist_Name


                                                                 Try
                                                                     If (Application.UPLOAD_FINALMIX_TO_SERVER) Then
                                                                         UNTAMEFTP.MoveToServer(song.MP3_File_Path, prox.RelativeUrl)

                                                                     End If

                                                                     prox = UNTAMEHTTP.SongRequest(prox, Application.LOCAL_HOST)
                                                                     Dim app As Application = Application.Current
                                                                     Dim logdel As Application.LemLogDel = New Application.LemLogDel(AddressOf Application.LemLog)


                                                                     If (prox.Stat = StudioBCallCenter.HTTPConnect.ServerStatMessageHolder.Status.OK) Then
                                                                         app.Dispatcher.BeginInvoke(logdel, {String.Format("{0} was sent to the cloud with no issues. Song id is {1}- {2}", prox.Title, prox.ContextSongID, DateTime.Now.ToShortTimeString()), "Song Uploaded to Server."})


                                                                     Else
                                                                         app.Dispatcher.BeginInvoke(logdel, {String.Format("{0} encounterd errors while sending to the main server, Error : {1}- {2}", prox.Title, prox.Message, DateTime.Now.ToShortTimeString()), "Problem sending song to database..."})




                                                                     End If
                                                                 Catch ex As Exception

                                                                     Application.LemLog(ex.Message, "ERROR!!!")

                                                                 End Try




                                                             End Sub)
            Await movetoserver

        Catch ex As Exception
            Dim message = "Problem moving file to server ..."
            TEST_MESS(message)
            LEM_ERR(message)
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try




    End Sub




    Public Shared Function TrimAndConvertFinalBounceAndRenameSessionFolder(liveSession As SetupSession.Session) As SetupSession.Session
        Dim logdel = New LemLogDel(AddressOf Application.LemLog)
        Dim app = Application.Current
        Dim waveman = New LemsUTools.WaveAndMp3Manipulation()
        Dim waveFile = liveSession.GetArtist().RecordingPath + liveSession.GetMother().New_Song_Name + ".wav"
        If (Not System.IO.File.Exists(waveFile)) Then
            Dim finalName = liveSession.UpdateName(liveSession.GetMother().New_Song_Name, False)
            Return liveSession
        End If
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.loggedIn.ToString())
        Dim finalMP3 = liveSession.Song_Object.MP3_Location + Utils.addFeatureText(liveSession.GetMother().New_Song_Name, liveSession.Song_Object.Get_Features_List) + ".mp3"
        If (System.IO.File.Exists(finalMP3)) Then
            System.IO.File.Delete(finalMP3)
        End If

        app.Dispatcher.Invoke(logdel, {"Trimming final mp3 file...", "Finalizing " + finalMP3})
        Dim workPath As String = "\wave_edits"
        'Dim waveFile2 = waveman.CleanAndFade(waveFile, TimeSpan.Zero, 0, False, useThisPathForEdits:=workPath)
        'Dim waveFile2 = workPath + "\final_wave\" + "1.wav"
        Dim readytomove = MacroShop.Find_Previous_Version_Of_Same_Song(Directory.GetParent(finalMP3).FullName, liveSession)

        Dim mm = New MasterMix()
        Dim done = mm.ConvertAndTrimAudioFile(waveFile, finalMP3, waveman, workPath)
        If (done > 0) Then
            app.Dispatcher.Invoke(logdel, {finalMP3 + " is " + TimeSpan.FromMilliseconds(Convert.ToDouble(done)).ToString().Substring(0, 8) + " long", "Final MP3 Edit"})

            liveSession.Song_Object.LengthInMillis = done
            If (File.Exists(waveFile)) Then
                File.Delete(waveFile)
            End If
            app.Dispatcher.Invoke(logdel, {"Updating Session Name to " + liveSession.GetMother().New_Song_Name, ""})
            liveSession.Song_Object.FLAG_POSTBOUNCE_UPDATE = True
            Dim finalName = liveSession.UpdateName(liveSession.GetMother().New_Song_Name, False)


            app.Dispatcher.Invoke(logdel, {"Finalizing Bounce and Preparing to play", "Bounce Complete!!!"})
        End If
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.newBounceName, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.oldBounceName, "")

        Return liveSession
    End Function
    Public Shared Sub TEST_MESS(message As String)
        If (UseTEST_MESS) Then
            MsgBox(message)
        End If
    End Sub

    Public Shared ReadOnly Property CLEANUPXMLFORDEBUGENABLED As Boolean
        Get
            Return cleanupXMLforDebug
        End Get
    End Property

    Public Enum sessionState
        started = 1
        running = 2
        closing = 3
        closed = 0
    End Enum

    Public Shared ReadOnly Property Main_Window As LemsSessionRunnerV2.MainWindow
        Get
            Return MWHANDLE
        End Get
    End Property
    Public Shared Function RegisterLemPlayerNow() As Boolean
        Dim playerPath As String = "CCLEMMUSICPLAYERACTIVEXV1.1.ocx"

        Return RegisterNow({"/r", playerPath})

    End Function

    Public Sub New()





        If (UpdateLoginInfoXML) Then
            Task.Factory.StartNew(Function() LoginData.UpdateLoginIDs())

        End If
        If (UpdateSongInfoXML) Then
            LoginData.UpdateSongInfoXML()

        End If

        ARTIST_RELEVANCE_OBJECT = New GLOBALARTISTRELEVANCE
        statWin = New ProgressModalWindow


        statWinBars = New List(Of ProgressBar)


        If (cleanupXMLforDebug) Then
            cleanUPXMLs()
        Else
            If (loadProperties()) Then






                getArtistList()
                If Not (DisAbleFloater) Then
                    Application.ResetFloater()
                End If
            Else
                Throw New Exception("Error loading props...")
            End If

            Dim startGetLogins As Task(Of List(Of LoginData)) = Task(Of List(Of LoginData)).Factory.StartNew(Function()
                                                                                                                 Return LoginData.LoginHistory(DateTime.Now.Day)
                                                                                                             End Function, Nothing, MASTER_CANCEL)
            Dim finalize As Task = startGetLogins.ContinueWith(Sub(list) FinalizeLoginHistory(list.Result))







        End If

        If (CheckModeAtStartUp) Then
            Dim mode As LoginData.AppMode = LoginData.GetAppModeFromString(LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.mode))
            Dim user As Artist = ArtistList.Find(Function(a) a.My_Name = LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.currentUser))
            Dim songLocId = LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.currentSong)
            Dim loggedIn = LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.loggedIn) = "true"
            Dim bookeUntil As DateTime
            DateTime.TryParse(LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil), bookeUntil)

            If (loggedIn And Not (user Is Nothing)) Then
                If (bookeUntil < DateTime.Now) Then

                Else
                    Select Case mode
                        Case LoginData.AppMode.bounceSession
                            If (Login_As_Artist(user, True)) Then
                                Dim bounceTask As Task(Of String) = Task(Of String).Factory.StartNew(Function()
                                                                                                         Return SessionWindow.GetSetupSession().BounceClosedSession(SessionWindow.GetSetupSession().GetSessionBySongID(Convert.ToInt32(songLocId))).Result
                                                                                                     End Function)

                            End If
                        Case LoginData.AppMode.loggedIn
                            If (Login_As_Artist(user, True)) Then

                            End If
                    End Select
                End If
                Return

            End If




        End If

        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.startUp.ToString())
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.loggedIn, 0)
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.currentSong, 0)
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.currentUser, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookingDuration, 0)
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.newBounceName, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.oldBounceName, "")



    End Sub

    Private Sub FinalizeLoginHistory(list As List(Of LoginData))
        Try
            If Not (FLAG_UPDATING_LOGINS) Then
                FLAG_UPDATING_LOGINS = True
                data = list
                'data.Sort()
                If (Application.Main_Window Is Nothing) Then
                    Return
                End If
                Dim app = Application.Current
                Dim mainWin As MainWindow = Application.Main_Window
                Dim updateLogindel = New MainWindow.UpdateLoginsDelegate(AddressOf mainWin.updateLogins)
                mainWin.Dispatcher.BeginInvoke(updateLogindel)
                FLAG_UPDATING_LOGINS = False
            End If
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try



    End Sub
    Private Shared Async Function REWRITELOGINANDSONGINFOXMLS() As Task(Of Boolean)
        Dim result = Utils.UpdateSongInfoXML()
        If (result) Then
            Await LoginData.UpdateLoginIDs()
        End If


        Return True
    End Function

    Public Shared ReadOnly Property Login_Data As List(Of LoginData)
        Get
            Return data
        End Get
    End Property


    Public ReadOnly Property SessionWindow As SessionWindow
        Get
            Return m_Session_Window
        End Get
    End Property

    Protected Overrides Sub OnExit(e As ExitEventArgs)
        MyBase.OnExit(e)
        ' FloatingPlayer.Close()
        ' FloatingPlayer = Nothing


    End Sub


    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)



        If Not (CREATED) Then

            If (IdentityHasChanged) Then
                Identity.GlobalID.CommitAllData()
                IdentityHasChanged = False
            End If




            statWin = New ProgressModalWindow()


            CREATED = True


        End If







    End Sub
    Public Delegate Sub RefreshSessionWindowSessionDelegate(sess As SetupSession.Session)
    Public Sub RefreshSessionWindowSession(sess As SetupSession.Session)

        m_Session_Window.RefreshSessions(sess)
    End Sub

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)


        thisApp = Me
        'statWin = New ProgressModalWindow

    End Sub

    Private Async Sub cleanUPXMLs()
        Dim ld As LoginData = New LoginData()

        Dim result As Boolean = ld.UpdateSongInfoXML()
        If (result) Then
            Await LoginData.UpdateLoginIDs()
        End If
        MainWindow.Close()
    End Sub

    Public Shared Function Floating_Player() As FloatingLemPlayer
        If (DisAbleFloater) Then
            Return Nothing
        End If
        If (FloatingPlayer Is Nothing) Then
            ResetFloater()
        End If
        Return FloatingPlayer
    End Function

    Public Sub getArtistList()

        ArtistList = New List(Of Artist)
        Dim xmlDoc As New XmlDocument

        xmlDoc.Load(Application.PASSWORDS_PATH)


        Dim root As XmlNode = xmlDoc.DocumentElement()
        For Each artistNode As XmlNode In root.ChildNodes
            Dim ar As Artist = New Artist(artistNode.Attributes.ItemOf(0).Value, False)

            ArtistList.Add(ar)

        Next









    End Sub


    Public Structure ArtistLoginArgs

        Public artist As Artist
        Public sw As SessionWindow
        Public statusText As String
        Public session As SetupSession
        Property window As ProgressModalWindow



    End Structure

    Public Delegate Function GetArtistMp3ListDelegate(artistName As String) As String()
    Public Shared Function GetArtistMp3List() As String
        ' Dim artistOBJ = ArtistList.Where(Function(a) a.My_Name = artistName).FirstOrDefault()
        ' Dim artistOBJ As Artist
        'Dim songs() = IO.Directory.GetFiles(artistOBJ.MP3Path, "*.mp3")

        'Return songs

        Return ""
    End Function

    Public Shared Function test1(artistName As String, val2 As String) As String
        Dim result As String = ""

        Dim artistOBJ = ArtistList.Where(Function(a) a.My_Name = artistName).FirstOrDefault()

        Dim songs() = Directory.GetFiles(artistOBJ.MP3Path, "*.mp3")

        ''Dim done = Await Task(Of String()).Factory.StartNew(Function()
        ''                                                        Dim getsongsdel = New Application.GetArtistMp3ListDelegate(AddressOf Application.GetArtistMp3List)
        ''                                                        Dim app As Application = Application.Current
        ''                                                        Return app.Dispatcher.Invoke(getsongsdel, artistName)


        ''                                                    End Function)
        ''songs = done.Result


        'Dim temp As String = ""
        'For Each song As String In songs
        '    temp = song

        '    temp = System.IO.Path.GetFileNameWithoutExtension(temp)

        '    If (songName = temp) Then
        '        result = song
        '        Exit For
        '    Else
        '        temp = Utils.removeFeatureText(temp)
        '        If (songName = temp Or songName.Contains(temp) Or temp.Contains(songName) Or Utils.removeSpaces(songName).ToLower = Utils.removeSpaces(temp).ToLower) Then
        '            result = song
        '            Exit For
        '        End If
        '    End If



        'Next
        'Return result
        Return ""
    End Function




    Public Shared Function Get_Bounced_Song_Path(ByVal songName As String, artistName As String) As String
        Dim result As String = ""
        Dim artistOBJ As Artist = Nothing
        Try
            artistOBJ = ArtistList.Where(Function(a) a.My_Name = artistName).FirstOrDefault()
        Catch ex As Exception
            Throw New Exception()
        End Try


        Dim songs() = System.IO.Directory.GetFiles(artistOBJ.MP3Path, "*.mp3")

        'Dim done = Await Task(Of String()).Factory.StartNew(Function()
        '                                                        Dim getsongsdel = New Application.GetArtistMp3ListDelegate(AddressOf Application.GetArtistMp3List)
        '                                                        Dim app As Application = Application.Current
        '                                                        Return app.Dispatcher.Invoke(getsongsdel, artistName)


        '                                                    End Function)
        'songs = done.Result


        Dim temp As String = ""
        For Each song As String In songs
            temp = song

            temp = System.IO.Path.GetFileNameWithoutExtension(temp)

            If (songName = temp) Then
                result = song
                Exit For
            Else
                temp = Utils.removeFeatureText(temp)
                If (songName = temp Or songName.Contains(temp) Or temp.Contains(songName) Or Utils.removeSpaces(songName).ToLower = Utils.removeSpaces(temp).ToLower) Then
                    result = song
                    Exit For
                End If
            End If



        Next
        Return result
    End Function

    Public Function ShowPasswordBox(message As String) As String
        Return PassWordWindow.ShowDialoque(message)

    End Function
    Private Shared Blocker As BlockingWindow
    Private Shared BLOCKER_OPEN As Boolean
    Public Shared Sub BlockerWindow(Optional open As Boolean = True)
        Return

        If (Blocker Is Nothing) Then
            Blocker = New BlockingWindow()

        End If
        If (open) Then
            If (BLOCKER_OPEN) Then
                Return
            Else
                Blocker.Show()
                BLOCKER_OPEN = True
            End If
        Else
            If (BLOCKER_OPEN) Then
                Blocker.Close()
                BLOCKER_OPEN = False
            Else
                Return
            End If

        End If
    End Sub

    Public Sub StartLoadProducerBeats()

        If (PreUpdateUsedBeats) Then
            LemLog("Updating Beats information, " & vbNewLine & "This may take a minute...")
            UsedBeatTracker.UpdateTracksNow()
            LemLog("All Beats Updated!", "Completed Track Scan")
        End If



        If (AllProducersAndBeats Is Nothing) Then
            AllProducersAndBeats = New List(Of Tuple(Of String, List(Of SessionWindow.TrackWrapper)))()

        End If
        For Each beatsfolder As String In Directory.GetDirectories(Application.PRODUCER_BEATS_FOLDER)
            Dim producerName = New DirectoryInfo(beatsfolder).Name
            Dim getBeats = Task(Of List(Of SessionWindow.TrackWrapper)).Factory.StartNew(Function()
                                                                                             Return LoadProducerBeats(producerName, BaseTrackInfo.ProductionCompany.UntameMusicBrickLyfe)

                                                                                         End Function)
            Dim updateAfterBeatLoad = getBeats.ContinueWith(Sub(list)
                                                                Application.AllProducersAndBeats.Add(New Tuple(Of String, List(Of SessionWindow.TrackWrapper))(producerName, list.Result))


                                                            End Sub, MASTER_CANCEL, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext)
        Next
        Application.PRODUCER_BEATS_LOADED = True
        RaiseEvent AllProducerBeatsLoaded()
        StartLoadSCList()

    End Sub

    Public Sub StartLoadSCList()
        Dim scThread As Task(Of List(Of SessionWindow.TrackWrapper)) = Task(Of List(Of SessionWindow.TrackWrapper)).Factory.StartNew(AddressOf LoadSoundClickList)


        Dim cntu As Task = scThread.ContinueWith(Sub(x)
                                                     If x.IsCompleted And Not x.IsFaulted Then
                                                         FinishLoadSoundClickList(x.Result)
                                                     Else
                                                         Dim logdel As LemLogDel = New LemLogDel(AddressOf LemLog)
                                                         Dispatcher.BeginInvoke(logdel, {"Loading beats caused and error...Probably need to clean songInfoXml....", ""})
                                                     End If


                                                 End Sub)

    End Sub
    Public Function GetOrCreateProducerBrand(producerName As String, Optional company As BaseTrackInfo.ProductionCompany = BaseTrackInfo.ProductionCompany.UntameMusicBrickLyfe, Optional tagPath As String = "", Optional allowNonExclusiveDefault As Boolean = False) As BaseTrackInfo.ProducerBrand
        Dim result As BaseTrackInfo.ProducerBrand = New BaseTrackInfo.ProducerBrand(producerName, LemsSessionRunnerV2.XMLResources.GetProducersResourceNode())
        If Not result.IsInitialized Then
            result = BaseTrackInfo.ProducerBrand.CreateNew(company, producerName, LemsSessionRunnerV2.XMLResources.GetProducersResourceNode(), False, allowNonExclusiveDefault, tagPath, 5.0)
        End If
        Return result
    End Function
    'Slab Projektz
    Public Function LoadProducerBeats(producerName As String, Optional company As BaseTrackInfo.ProductionCompany = BaseTrackInfo.ProductionCompany.UntameMusicBrickLyfe) As List(Of SessionWindow.TrackWrapper)
        Dim result As List(Of SessionWindow.TrackWrapper) = New List(Of SessionWindow.TrackWrapper)

        If Not Directory.Exists(Application.PRODUCER_BEATS_FOLDER & producerName) Then
            Return Nothing
        End If
        Dim brand As BaseTrackInfo.ProducerBrand = GetOrCreateProducerBrand(producerName, company)

        For Each trkPath As String In Directory.GetFiles(Application.PRODUCER_BEATS_FOLDER & producerName, "*.mp3")

            Dim btio As BaseTrackInfoWrapper = New BaseTrackInfoWrapper(trkPath, "none", SetupSession.SessionMusicSource_Type.FlBounced, XMLRESOURCES, Nothing, brand, New FileInfo(trkPath))

            Dim wrapper As SessionWindow.TrackWrapper = New SessionWindow.TrackWrapper(btio.BounceLocation, btio)
            result.Add(wrapper)

        Next
        result.Sort()
        Return result


    End Function
    Private Function LoadSoundClickList() As List(Of SessionWindow.TrackWrapper)
        Dim app As Application = Application.Current
        Dim log As Application.LemLogDel = New Application.LemLogDel(AddressOf Application.LemLog)


        app.Dispatcher.BeginInvoke(log, {"", "Loading SoundClickList"})

        SoundClickList = New List(Of SessionWindow.TrackWrapper)()


        For Each filePath As String In Directory.GetFiles(Application.INTERNET_BEATS_DIRECTORY)

            If (Path.GetExtension(filePath).ToLower = ".mp3") Then
                'create new trackinfo and trackwrapper
                Dim btiw As BaseTrackInfoWrapper = New BaseTrackInfoWrapper(filePath, "none", SetupSession.SessionMusicSource_Type.SoundClick, XMLRESOURCES, Nothing, Nothing, (New FileInfo(filePath)), False)

                'If (btiw.ID > 0) Then



                Dim wrapper As SessionWindow.TrackWrapper = New SessionWindow.TrackWrapper(btiw.BounceLocation, btiw)



                SoundClickList.Add(wrapper)
                ' app.Dispatcher.BeginInvoke(log, {wrapper.BeatName, "Loading SoundClickList"})



            End If
            ' End If

        Next
        SoundClickList.Sort()


        Return SoundClickList
        'UsedSongList.DataContext = SoundClickList

    End Function
    Public Sub FinalizeLoadSoundClickList(list As List(Of SessionWindow.TrackWrapper))
        SoundClickList = list


        Application.ResourcesHaveChanged = True
        XMLRESOURCES.SubmitResources()


    End Sub
    Public Delegate Sub FinalizeLoadSoundClickDelegate(list As List(Of SessionWindow.TrackWrapper))

    Private Sub FinishLoadSoundClickList(list As List(Of SessionWindow.TrackWrapper))
        Dim app As Application = Application.Current
        Dim logdel As Application.LemLogDel = New Application.LemLogDel(AddressOf Application.LemLog)


        Dim finish As FinalizeLoadSoundClickDelegate = New FinalizeLoadSoundClickDelegate(AddressOf FinalizeLoadSoundClickList)

        app.Dispatcher.Invoke(finish, list)
        Application.SOUNDCLICK_LIST_LOADED = True
        RaiseEvent SoundClickListLoaded()

        Application.ResourcesHaveChanged = True
        LemsSessionRunnerV2.XMLResources.SubmitResources()
        app.Dispatcher.BeginInvoke(logdel, {String.Format("{0} SC Beats Loaded...", SoundClickList.Count), "Load beats completed with no problems."})
    End Sub
    Public Delegate Sub AddSessionDelegate(sess As SetupSession.Session)
    Public Shared Sub AddSessionToCurrentSessionWindow(sess As SetupSession.Session)
        If (m_Session_Window Is Nothing) Then
            Return
        End If
        m_Session_Window.GetSetupSession().Session_List.Add(sess)
        m_Session_Window.RefreshSessions(sess)

        ' m_Session_Window.ui_sessionsList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()
    End Sub
    Public Shared Sub LoginAndReturnSessionWindow(sender As Object, e As DoWorkEventArgs)
        Dim statText As StatWinTextDelegate = New StatWinTextDelegate(AddressOf Application.StatwinText)
        Dim statProg As StatWinProgressDelegate = New StatWinProgressDelegate(AddressOf Application.StatWinProgress)
        ' Dim statStart As StatwinStartDelegate = New StatwinStartDelegate(AddressOf Application.StatWinStart)
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
        worker.ReportProgress(10)

        Dim app As Application = Application.Current
        Dim args As ArtistLoginArgs = e.Argument
        Dim artist As Artist = args.artist
        'm_Session_Window = args.sw
        MacroShop.killAllMacros()
        If (useMacros) Then
            worker.ReportProgress(20)
            app.Dispatcher.Invoke(statText, "Checking Last song folder rename")
            Process.Start(DOUBLE_CHECK_LAST_SONGFOLDER_RENAME)
            ' LemLog("Checking Last song folder rename")
        End If
        worker.ReportProgress(30)
        ' artist.init(sess)
        worker.ReportProgress(50)
        If (artist.Is_Logged_In) Then


            worker.ReportProgress(70)





            args.artist = artist
            e.Result = args



        End If
        worker.ReportProgress(90)
    End Sub
    Public Function GetMainWindow() As MainWindow
        Return Main_Window
    End Function

    Public Sub finalizeInit()



        m_Session_Window = New SessionWindow(m_CurrentArtist.Get_Sessions)
        '  loginargs.sw = m_Session_Window
        '  m_BackGroundWorker.RunWorkerAsync(loginargs)



        m_Session_Window.DataContext = m_CurrentArtist.SessionList



        StatWinClose()
        m_Session_Window.Show()
    End Sub

    Public Function GetCurrentArtist() As Artist
        Return CurrentArtist
    End Function

    Delegate Function statWin2Check(message As String)
    Private Shared Sub BookingResultHandler(br As StudioBCallCenter.HTTPConnect.BookingProxy)
        Dim result = ""

        If (br.Canceled) Then
            result = br.ArtistName + "'s Booking was Canceled. "
        ElseIf (br.TimesUP) Then
            result = "You have used over 4 hours today..."
        ElseIf (br.invalid) Then
            result = "Error booking " + br.ArtistName + "."

        ElseIf (br.IsBooked) Then
            result = "Studio allready booked by " + br.ArtistName + " until " + br.when.AddHours(br.scheduledDuration).ToString("hh:mm tt")
            Dim minDiff = br.when.TimeOfDay.Add(TimeSpan.FromMinutes(30)).TotalMinutes - DateTime.Now.TimeOfDay.TotalMinutes
            If (minDiff > 1 And minDiff < 30) Then
                result += vbNewLine + " If " + br.ArtistName + " doesn't log in in the next " + Math.Floor(minDiff).ToString() + " minutes, you can override that booking...."
            End If

        ElseIf (br.scheduledDuration > 0) Then
            result = "You are now booked until " + DateTime.Now.Add(TimeSpan.FromHours(br.scheduledDuration)).ToString("hh:mm tt")
        ElseIf (br.StudioClosed) Then
            result = "No bookings can be made after 12am, and no bookings can be scheduled between closing hours(4am-10am)"

        ElseIf (br.durationDiscrepency > 0) Then
            result = "Requested duration of " + br.scheduledDuration + " hours overlaps another session booked by " + br.ArtistName
        End If
        If Not (result = "") Then
            MsgBox(result)
            LemLog(result, "Welfare Studio Booking...")
        End If
    End Sub
    ''' <summary>
    ''' Simply checks if artist is loggedin and if it is opens session window
    ''' </summary>
    ''' <param name="artist"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Structure LemRemoteMessage
        Public QuickAnswer As Boolean
        Public Messages As List(Of String)
    End Structure
    Private Shared Function GetOKFromLem(artist As Artist) As LemRemoteMessage
        Try
            Dim doc As XDocument = XDocument.Load(LEM_MESSAGES)
            Dim changed As Boolean = False
            Dim canlogins As XElement = doc.Element("canlogins")
            Dim mymessage As XElement = canlogins.Elements.Where(Function(e) e.Attribute("name").Value = artist.My_Name).FirstOrDefault()

            Dim canlogin As Boolean = Convert.ToBoolean(mymessage.Attribute("value").Value)

            Dim messages As List(Of String) = New List(Of String)
            Dim indexes = New List(Of XElement)()
            Dim removed = False
            If (mymessage.Elements.Count() > 0) Then
                For Each mess As XElement In mymessage.Elements
                    messages.Add(mess.Value)
                    mess.Attribute("hasbeenread").Value = True
                    changed = True
                    Dim delete As Boolean = Convert.ToBoolean(mess.Attribute("deleteafterread").Value)
                    If (delete) Then
                        removed = True
                        mess.Remove()

                    End If

                Next
                If (indexes.Count > 0) Then


                End If
            End If
            If (removed) Then
                Try
                    doc.Save(LEM_MESSAGES)

                Catch ex As Exception
                    RETRY_SAVE_CANLOGINXML = True
                    MASTER_CANLOGINDOC = doc
                    SAVE_CANLOGINXML_TIMER = New Timers.Timer()
                    SAVE_CANLOGINXML_TIMER.Interval = 60000
                    SAVE_CANLOGINXML_TIMER.Start()
                    SAVE_CANLOGINXML_TRY_COUNT = 0

                End Try

            End If
            
            Return New LemRemoteMessage With {.Messages = messages, .QuickAnswer = canlogin}
        Catch ex As Exception
            LemLog("error on GETOK!!!", "")
            Return New LemRemoteMessage With {.QuickAnswer = True, .Messages = New List(Of String)}

        End Try



    End Function
    Public Shared Sub HandleMasterCanloginTimerEvent(e As System.Timers.ElapsedEventArgs)
        If (RETRY_SAVE_CANLOGINXML) Then
            Try
                If (SAVE_CANLOGINXML_TRY_COUNT > 4) Then
                    RETRY_SAVE_CANLOGINXML = False
                Else

                End If
                If Not (MASTER_CANLOGINDOC Is Nothing) Then

                    MASTER_CANLOGINDOC.Save(LEM_MESSAGES)
                    MASTER_CANLOGINDOC = Nothing
                    RETRY_SAVE_CANLOGINXML = False

                End If



            Catch ex As Exception
                RETRY_SAVE_CANLOGINXML = True
                SAVE_CANLOGINXML_TRY_COUNT += 1
            End Try
        End If
    End Sub
    Public Shared Function Login_As_Artist(artist As Artist, Optional relogin As Boolean = False) As Boolean
        statWin = Nothing
        Dim finalmessage = ""

        'If (RETRY_SAVE_CANLOGINXML) Then
        '    Try
        '        If Not (MASTER_CANLOGINDOC Is Nothing) Then

        '            MASTER_CANLOGINDOC.Save(LEM_MESSAGES)
        '            MASTER_CANLOGINDOC = Nothing
        '            RETRY_SAVE_CANLOGINXML = False

        '        End If



        '    Catch ex As Exception
        '        RETRY_SAVE_CANLOGINXML = True
        '    End Try
        'End If
        Try
            Dim ok As LemRemoteMessage = GetOKFromLem(artist)
            If (ok.Messages.Count > 0) Then

                For Each mess As String In ok.Messages
                    finalmessage &= String.Format("{0}{1}", mess, vbNewLine)


                Next
            End If
            If (ok.QuickAnswer) Then
                If (ok.Messages.Count > 0) Then
                    MsgBox(finalmessage)
                End If
            Else
                If (ok.Messages.Count > 0) Then
                    MsgBox(finalmessage)
                Else
                    finalmessage = "Naw!"
                    MsgBox(finalmessage)
                End If

                Return False
            End If
        Catch ex As Exception

        End Try
        

        If (DISABLE_LOGINS) Then
            Dim logdel = New Application.LemLogDel(AddressOf Application.LemLog)
            Dim app As Application = Application.Current
            app.Dispatcher.Invoke(logdel, {"LOGINS DISABLED TEMPORARILY", "UNDER MAITNANCE!!!"})

            Return False
        End If


        m_Session_Window = Nothing
        MacroShop.killAllMacros()


        statWin = New ProgressModalWindow
        Dim pass As Admin.PassWord = New Admin.PassWord(artist.My_Name, False)
        If Not (relogin) Then
            If (USE_SCHEDULE) Then
                LemLog("Checking Schedule...")
                'CHECK SCHEDULE
                Dim go = True
                If (LOCAL_HOST) Then
                    LemLog("using local host...", "Checking Schedule...")
                    If (DateTime.Now.TimeOfDay >= TimeSpan.FromHours(4) And DateTime.Now.TimeOfDay <= TimeSpan.FromHours(9)) Then

                        go = False

                    End If
                Else
                    LemLog("Calling Untame Music Dot Com!!!", "Checking Schedule...")
                End If
                If (go) Then

                    If (DateTime.Now.TimeOfDay >= TimeSpan.FromHours(4) And DateTime.Now.TimeOfDay <= TimeSpan.FromHours(9)) Then
                        MsgBox("Welfare Studio Closed till 10am....")
                        LemLog("Closed until 10", "CLOSED!")
                        Return False

                    End If
                    Try
                        Application.Current_Booking = UNTAMEHTTP.QuickCheckSchedule(Application.LOCAL_HOST, LOCAL_HOST_ADDRESS)
                    Catch ex As Exception
                        LemLog(ex.Message, "ERROR...Get Schedule")
                        SERVER_ERROR = True
                    End Try


                    If (Current_Booking Is Nothing) Then
                        MsgBox("Cant connect to UntameMusic.com for some reason, Scheduling Disabled, booking mode is WHILE OUT until 4am ....")
                        LemLog("Error communicating with server...", "SCHEDULE TEMPORARILY DISABLED")
                        SERVER_ERROR = True


                        'IF NO BOOKINGS AND ARTIST HAS TIME ASK HOW MANY HOURS
                        'AND BOOK

                    ElseIf (Current_Booking.IsBooked) Then
                        SERVER_ERROR = False
                        If (Current_Booking.ArtistName = artist.My_Name) Then

                            LemLog(artist.My_Name & " booked until " & Current_Booking.when.Add(TimeSpan.FromHours(Current_Booking.scheduledDuration)).TimeOfDay.ToString())

                        ElseIf (Current_Booking.minutesLeftInCurrentBooking < 1) Then

                            Application.Current_Booking = UNTAMEHTTP.CancelBooking(Current_Booking, LOCAL_HOST, LOCAL_HOST_ADDRESS)
                            BookingResultHandler(Current_Booking)
                            Application.Current_Booking = UNTAMEHTTP.QuickBookStudio(artist.My_Name, 4, LOCAL_HOST, LOCAL_HOST_ADDRESS)
                            BookingResultHandler(Current_Booking)

                        ElseIf ((DateTime.Now.TimeOfDay >= Current_Booking.when.TimeOfDay.Add(TimeSpan.FromMinutes(30))) Or LOCAL_HOST) Then
                            Dim response = MsgBox("This will cancel " + Current_Booking.ArtistName + "'s booking...", MsgBoxStyle.OkCancel, "Cancel Current Booking?")
                            If (response = MsgBoxResult.Ok) Then
                                Application.Current_Booking = UNTAMEHTTP.CancelBooking(Current_Booking, LOCAL_HOST, LOCAL_HOST_ADDRESS)
                                BookingResultHandler(Current_Booking)
                                Application.Current_Booking = UNTAMEHTTP.QuickBookStudio(artist.My_Name, 4, LOCAL_HOST, LOCAL_HOST_ADDRESS)
                                BookingResultHandler(Current_Booking)
                            Else
                                BookingResultHandler(Current_Booking)
                                Return False
                            End If


                        Else

                            BookingResultHandler(Current_Booking)
                            Return False
                        End If




                    Else
                        SERVER_ERROR = False

                        Current_Booking = UNTAMEHTTP.QuickBookStudio(artist.My_Name, 4, LOCAL_HOST, LOCAL_HOST_ADDRESS)
                        If (Current_Booking.scheduledDuration > 0) Then
                            BookingResultHandler(Current_Booking)

                        ElseIf (Current_Booking.TimesUP) Then
                            BookingResultHandler(Current_Booking)

                            Dim mess = ""

                            If (Current_Booking.canceledDate.Add(TimeSpan.FromHours(2)) > DateTime.Now()) Then
                                mess = "Your Time Today Has Been Exhausted..." & vbNewLine & "See you tommorow " & artist.My_Name & vbNewLine & "GO HOME BRUH!"
                                Return False
                            Else
                                mess = "You have been booked for over 4 hours now..." & vbNewLine
                                Dim checkNextBooking As StudioBCallCenter.HTTPConnect.BookingProxy = UNTAMEHTTP.QuickCheckSchedule(Application.LOCAL_HOST)

                                If (checkNextBooking.IsBooked) Then
                                    If (checkNextBooking.when.Subtract(DateTime.Now()).TotalMinutes < 5) Then
                                        mess &= checkNextBooking.ArtistName & " is Scheduled for " & checkNextBooking.when.TimeOfDay.ToString & " in " & Math.Floor(checkNextBooking.minutesUntil).ToString() & " minutes!"
                                    Else
                                        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil, Math.Floor(checkNextBooking.minutesUntil).ToString())
                                        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.overTime, "true")

                                        mess &= vbNewLine & "You are in UnScheduled mode until " & checkNextBooking.when.TimeOfDay.ToString()
                                    End If
                                End If
                            End If



                        Else
                            BookingResultHandler(Current_Booking)
                            LemLog("Not Booked...", "Error, Please try later")
                            Return False

                        End If


                    End If




                    'IF BOOKED AND ARTIST IS NOT LOGGED IN , AND ITS 30 MIN PAST BOOKING, CANCEL BOOKING AND TRY TO BOOK THIS ARTIST


                End If
            Else
                'not using schedule
            End If


        ElseIf (relogin) Then
            LemLog("You are still booked until " & Current_Booking.when.Add(TimeSpan.FromHours(Current_Booking.scheduledDuration)).TimeOfDay.ToString(), "Relogging in " & artist.My_Name)
        End If



        If (relogin Or pass.TryLogin(artist)) Then
            If Not (relogin) Then
                Application.LemLog("Welcome to StudioB( Welfare Studio ) " & artist.My_Name & "...")
            End If
            If (useMacros) Then
                MacroShop.Clean_Close_Session()
            End If
            ' MacroShop.Clean_Close_Session()
            artist.SetPasswordOwner(pass)

            CurrentSessions = New SetupSession(artist)
            artist.init(CurrentSessions)
            m_CurrentArtist = artist

            If Utils.QuickCheckIfArtistHasSessions(artist) Then




                Dim cts As CancellationTokenSource = New CancellationTokenSource




            Else
                m_Session_Window = New SessionWindow(m_CurrentArtist.Get_Sessions)
                m_Session_Window.Show()
                LemLog("Welcome to Untames Welfare Unit!!!", "No Sessions Found yet...")
                Return True
            End If



        Else
            LemLog("Problem logging in ", "Error...")
            Return False

        End If




        m_Session_Window = New SessionWindow(m_CurrentArtist.Get_Sessions)
        '  loginargs.sw = m_Session_Window
        '  m_BackGroundWorker.RunWorkerAsync(loginargs)

        m_CurrentArtist.SortSessions()


        m_Session_Window.DataContext = m_CurrentArtist.SessionList
        LemLog(m_CurrentArtist.Get_Sessions.Session_List.Count.ToString() & " sessions found...", "Finding Available Sessions...")
        ' statWin.setProgress(100, 0)
        ' Await Task.Delay(200)
        ' statWin.Close()
        Application.Main_Window.Hide()
        m_Session_Window.Show()
        If Not (Current_Booking Is Nothing) Then
            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil, Current_Booking.when.Add(TimeSpan.FromHours(Current_Booking.scheduledDuration)))
        Else
            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil, DateTime.Now.Add(TimeSpan.FromHours(4)))



        End If

        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.currentUser, m_CurrentArtist.My_Name)
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.loggedIn, 1)
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.loggedIn.ToString())
        Application.LemLog("What's Up " & m_CurrentArtist.My_Name & "!")

        Application.Floating_Player().SelectAndPlay(m_CurrentArtist)

        Return True




        Return False


    End Function
    Public Sub BeginLogOutArtist(setupSess As SetupSession)

        If (
         setupSess.Get_Current_Artist.StoreAllData()) Then
            Main_Window.Show()
            'Dim artistNode As XElement = setupSess.GetResources.ArtistLoginNode
            'If (setupSess.Get_Current_Artist.Resources.LoggingOut()) Then
            '    Try
            '        If Not (Application.data Is Nothing) Then

            '            Dim ld = New LoginData(artistNode, setupSess.GetResources.Login_ID.GLOBAL_ID)

            '            Application.data.Add(ld)
            '        End If


            '        Dim app As Application = Application.Current


            '        Dim mainwin As LemsSessionRunnerV2.MainWindow = app.GetMainWindow()
            '        Dim updateLogDel = New MainWindow.UpdateLoginsDelegate(AddressOf mainwin.updateLogins)



            '        Application.Main_Window.Dispatcher.Invoke(updateLogDel)

            '        ' Main_Window.updateLogins()


            '    Catch ex As Exception
            '        'Application.LemLog(ex.Message, "Error_Update_LoginInfo")
            '    End Try


            '    '  LemPlayer.destroy()
            'End If

            Dim finalizeLogout = Task.Factory.StartNew(Sub()

                                                       End Sub, MASTER_CANCEL, TaskCreationOptions.AttachedToParent, TaskScheduler.FromCurrentSynchronizationContext()
).ContinueWith(Sub() Log_Out_Artist(), MASTER_CANCEL, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext)


        End If
    End Sub
    'Public Shared Function runStatusWindow(ByVal sender As Object, ByVal e As DoWorkEventArgs) As Boolean Handles m_BackGroundWorker.DoWork


    '    Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
    '    Dim args As ArtistLoginArgs = e.Argument

    '    ' Dim artist As Artist = args.artist

    '    Dim statWin As ProgressModalWindow = args.window


    '    statWin.ShowNow()


    '    ' 

    '    Return True
    'End Function

    Public Shared Sub updateProgress(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles m_BackGroundWorker.ProgressChanged
        statWin.setProgress(e.ProgressPercentage)

    End Sub
    Public Delegate Function getLemPlayer()
    Public Delegate Sub releaseLemPlayer()
    Public Delegate Sub setLemPlayer(player As AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11)

    Public Shared Function ResetFloater(Optional show As Boolean = False) As Boolean
        If (DisAbleFloater) Then
            Return True
        End If
        If Not FloatingPlayer Is Nothing Then

            FloatingPlayer.stopPlayer()
            FloatingPlayer.Hide()
            'FloatingPlayer.Close()
            'FloatingPlayer = Nothing
            'FloatingPlayer = New FloatingLemPlayer()

            If Not Last_Played_Song Is Nothing Then
                Current_Playing_Song = Last_Played_Song
            Else
                Current_Playing_Song = Nothing
            End If
        Else
            FloatingPlayer = New FloatingLemPlayer()

        End If
        If (show) Then
            Try
                FloatingPlayer.Show()
            Catch ex As Exception
                FloatingPlayer = Nothing
                FloatingPlayer = New FloatingLemPlayer()
                FloatingPlayer.Show()
            End Try

        Else
            FloatingPlayer.Hide()
        End If
        Return True
    End Function
    Public Shared Sub StopFloater(hide As Boolean)
        If (FloatingPlayer.Is_Playing Or FloatingPlayer.Is_Live) Then
            FloatingPlayer.stopPlayer()
            If (hide) Then
                FloatingPlayer.Hide()
            End If
        End If
    End Sub
    Public Sub AddArtistToGroup(artist As Artist, group As Artist)
        group.AddArtistToThisGroup(artist.GLOBAL_ID, artist.My_Name)

    End Sub

    Public Delegate Sub PlayFloaterDelegate(songPath As String, songName As String)
    Public Sub PlayFloaterLocal(songPath As String, songName As String)
        PlayFloater(songPath, songName)
    End Sub

    Public Shared Sub PlayFloater(songPath As String, songName As String)
        If (DisAbleFloater) Then
            Return
        End If

        Try

            If Not (File.Exists(songPath)) Then
                Return
            End If


            If Not (FloatingPlayer Is Nothing) Then
                If Not FloatingPlayer.Is_Live Then
                    FloatingPlayer = Nothing
                Else
                    FloatingPlayer.play(songPath)
                    Last_Played_Song = Current_Playing_Song
                    Current_Playing_Song = songPath
                    FloatingPlayer.Title = "LemPlayer : " & songName
                    FloatingPlayer.Show()
                End If

                'Return
            End If



            If FloatingPlayer Is Nothing Then

                FloatingPlayer = New FloatingLemPlayer
                FloatingPlayer.Title = "LemPlayer : " & songName
                FloatingPlayer.Show()
                'FloatingPlayer.Top = 600

                FloatingPlayer.play(songPath)
                Current_Playing_Song = songPath
            End If



        Catch ex As Exception
            If (UseLemPlayerInsteadOfMediaPlayer) Then
                MsgBox("Attempting to register player...")

                If (RegisterLemPlayerNow()) Then
                    MsgBox("Registered Successfully")
                Else
                    MsgBox("Problem registering player...")
                End If
            End If
            MsgBox(ex.Message)
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try


    End Sub



    Public Shared Function IsFloaterPlaying() As Boolean
        Return FloatingPlayer.Is_Playing
    End Function






    Public Sub Log_Out_Artist()
        Dim timeLeft As DateTime
        Dim minutesLeft As Int32
        Dim isParsed As Boolean = DateTime.TryParse(LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil), timeLeft)
        If (isParsed) Then
            minutesLeft = timeLeft.Subtract(DateTime.Now).TotalMinutes
        Else
            LemLog("Problem parsing bookeduntil time...Artist will be logged out...")
        End If


        If (Application.USE_SCHEDULE And Not Application.Current_Booking Is Nothing And Not Application.SERVER_ERROR) Then
            If (minutesLeft > 10) Then
                If (MsgBox(String.Format("Do you want to cancel your remaining {0} minutes so you can use it to book later?", minutesLeft), MsgBoxStyle.YesNo, "Cancel Booking?") = MsgBoxResult.Yes) Then
                    Try
                        Current_Booking = UNTAMEHTTP.CancelBooking(Current_Booking, Application.LOCAL_HOST)
                        Application.LemLog(m_CurrentArtist.My_Name & " was logged out at " & DateTime.Now.TimeOfDay.ToString() & vbNewLine & "You have " & TimeSpan.FromHours(4).Subtract(TimeSpan.FromMinutes(Current_Booking.TotalMinutesUsedToday)).TotalHours.ToString() & " hours left for today...")
                    Catch ex As Exception
                        Application.LemLog("Booking not canceled...Try from the web site...", "ERROR COMMUNICATING WITH SERVER...")
                    End Try

                Else
                    Try
                        Current_Booking = UNTAMEHTTP.CancelBooking(Current_Booking, Application.LOCAL_HOST)
                        Application.LemLog(m_CurrentArtist.My_Name & " was logged out at " & DateTime.Now.TimeOfDay.ToString() & vbNewLine & "You have " & TimeSpan.FromHours(4).Subtract(TimeSpan.FromMinutes(Current_Booking.TotalMinutesUsedToday)).TotalHours.ToString() & " hours left for today...")
                    Catch ex As Exception
                        Application.LemLog("Booking not canceled...Try from the web site...", "ERROR COMMUNICATING WITH SERVER...")
                    End Try
                End If
            End If

        End If





        m_CurrentArtist.LogOut()
        Application.LemLog(m_CurrentArtist.My_Name & " was logged out at " & DateTime.Now.TimeOfDay.ToString())
        m_CurrentArtist = Nothing



        ' mw.Show()
        'Try
        '    If Not (m_Session_Window Is Nothing) Then
        '        If (m_Session_Window.IsActive) Then
        '            m_Session_Window.Close()
        '        End If

        '    End If
        'Catch ex As Exception

        'End Try



        'm_Session_Window = Nothing

        LEMTESTING = False

        ' data = LoginData.LoginHistory()


        MacroShop.killAllMacros()

    End Sub

    Public Shared Function TryUpdateXMLInfos()

        Try
            SongData.Update_Songs_XML()
            LoginData.UpdateLoginIDs()

            Return True


        Catch ex As Exception
            Application.LemLog("Problem occured during update...", "Done.")
            Throw New Exception("Global ID Missing, Run SongData.UpdateSongInfo and/or LogingData.UpdateLoginData")

            Return False

        End Try
    End Function

    Public Shared Function loadProperties() As Boolean

        UNTAMEHTTP = New StudioBCallCenter.HTTPConnect()

        If (propsLoaded) Then
            Return True
        End If

        NAMESKIPARRAY(0) = "MasterLoginSession"

        Application.LemLog("", "LOADING SETTINGS...")


        Dim xmlDoc As New XmlDocument
        Try
            xmlDoc.Load(XML_PATH & "props.xml")
        Catch ex As Exception
            LemLog(ex.Message, "ERROR!!!")
            MsgBox("weirdness = " & ex.Message())
        End Try

        Try
            Dim root As XmlNode = xmlDoc.DocumentElement()
            Dim links As XmlNode = root.Item("links")
            GET_SCHEDULE = links.Item("GET_SCHEDULE").GetAttribute("address")




            Dim scripts As XmlNode = root.Item("scripts")
            If (LOCAL_HOST) Then

                LOCAL_HOST_ADDRESS = scripts.Item("LOCAL_HOST_ADDRESS").GetAttribute("address")
            Else

                LOCAL_HOST_ADDRESS = ""
            End If
            If (scripts.Item("USE_LOCAL_HOST").GetAttribute("value") = "true") Then
                LOCAL_HOST = True
                Application.LemLog("Using Local Host...")
            Else
                LOCAL_HOST = False
                Application.LemLog("Using Untame Music Server...")
            End If
            If (scripts.Item("USE_SCHEDULE").GetAttribute("value") = "true") Then
                USE_SCHEDULE = True
                Application.LemLog("Using Schedule...")
            Else
                USE_SCHEDULE = False
                Application.LemLog("Schedule Disabled...")
            End If

            DISABLE_LOGINS = Convert.ToBoolean(scripts.Item("DISABLE_LOGINS").GetAttribute("value"))

            LEM_MESSAGES = scripts.Item("LEM_MESSAGES").GetAttribute("address")


            UPLOAD_FINALMIX_TO_SERVER = Convert.ToBoolean(scripts.Item("UPLOAD_FINALMIX_TO_SERVER").GetAttribute("value"))

            SERVER_ADDRESS = scripts.Item("SERVER_ADDRESS").GetAttribute("address")
            SERVER_LOGIN = scripts.Item("SERVER_LOGIN").GetAttribute("name")
            SERVER_PASS = scripts.Item("SERVER_PASS").GetAttribute("value")

            If (UPLOAD_FINALMIX_TO_SERVER) Then
                UNTAMEFTP = New StudioBCallCenter.FTPConnect(SERVER_ADDRESS, SERVER_LOGIN, SERVER_PASS)
            End If


            INHOUSE_LOCATION = scripts.Item("INHOUSE_LOCATION").GetAttribute("address")
            Application.LemLog(INHOUSE_LOCATION, "Loading...")

            PRODUCER_BEATS_FOLDER = INHOUSE_LOCATION & "Beats\"
            If Not Directory.Exists(PRODUCER_BEATS_FOLDER) Then
                Directory.CreateDirectory(PRODUCER_BEATS_FOLDER)
            End If

            INTRO_MESSAGE = scripts.Item("IntroMessage1").GetAttribute("value")
            If (DISABLE_LOGINS) Then
                Application.LemLog("Logins Disabled", "MAITNANCE MODE")
            Else
                Application.LemLog(INTRO_MESSAGE, "Loading...")
            End If

            Dim masterpass As String = scripts.Item("MASTER_PASSWORDS_LOCATION").GetAttribute("address")
            If Not masterpass = "" Then
                PASSWORDS_PATH = masterpass
            End If


            INTRO_MESSAGE2 = scripts.Item("IntroMessage2").GetAttribute("value")
            Application.LemLog(INTRO_MESSAGE2, "Loading...")

            MAXNUENDO = scripts.Item("MAXNUENDO").GetAttribute("address")
            Application.LemLog(MAXNUENDO, "Loading...")

            MINNUENDO = scripts.Item("MINNUENDO").GetAttribute("address")
            Application.LemLog(MINNUENDO, "Loading...")

            BOUNCE_SESSION_SCRIPT = scripts.Item("BOUNCE_SESSION_SCRIPT").GetAttribute("address")
            Application.LemLog(BOUNCE_SESSION_SCRIPT, "Loading...")

            START_SESSION_AUDITION_BEATS = scripts.Item("START_SESSION_AUDITION_BEATS").GetAttribute("address")

            Application.LemLog(START_SESSION_AUDITION_BEATS, "Loading...")

            CLOSE_THIS = scripts.Item("CLOSE_THIS").GetAttribute("address")
            Application.LemLog(CLOSE_THIS, "Loading...")

            CLEAN_CLOSE_SESSION = scripts.Item("CLEAN_CLOSE_SESSION").GetAttribute("address")
            Application.LemLog(CLEAN_CLOSE_SESSION, "Loading...")

            BRING_TO_FRONT = scripts.Item("BRING_TO_FRONT").GetAttribute("address")
            Application.LemLog(BRING_TO_FRONT, "Loading...")

            MACROS_DIRECTORY = scripts.Item("MACROS_DIRECTORY").GetAttribute("address")
            Application.LemLog(MACROS_DIRECTORY, "Loading...")

            DOUBLE_CHECK_LAST_SONGFOLDER_RENAME = scripts.Item("DOUBLE_CHECK_LAST_SONGFOLDER_RENAME").GetAttribute("address")
            Application.LemLog(DOUBLE_CHECK_LAST_SONGFOLDER_RENAME, "Loading...")

            BASE_PATH = scripts.Item("NUENDO_PATH").GetAttribute("address")
            Application.LemLog(BASE_PATH, "Loading...")

            MP3_PATH = scripts.Item("MP3_PATH").GetAttribute("address")
            Application.LemLog(MP3_PATH, "Loading...")

            TEMPLATE_FILE = scripts.Item("TEMPLATE_FILE").GetAttribute("address")
            Application.LemLog(TEMPLATE_FILE, "Loading...")


            CLOSE_SELECTED_DIRECTORY_WINDOW = scripts.Item("CLOSE_SELECTED_DIRECTORY_WINDOW").GetAttribute("address")
            Application.LemLog(CLOSE_SELECTED_DIRECTORY_WINDOW, "Loading...")

            OPEN_NEWLY_CREATED_SESSION = scripts.Item("OPEN_NEWLY_CREATED_SESSION").GetAttribute("address")
            Application.LemLog(OPEN_NEWLY_CREATED_SESSION, "Loading...")

            INTERNET_BEATS_DIRECTORY = scripts.Item("INTERNET_BEATS_DIRECTORY").GetAttribute("address")
            Application.LemLog(INTERNET_BEATS_DIRECTORY, "Loading...")

            TEMPLATE_DIRECTORY = scripts.Item("TEMPLATE_DIRECTORY").GetAttribute("address")
            Application.LemLog(TEMPLATE_DIRECTORY, "Loading...")

            LOCAL_CD_DRIVE = scripts.Item("LocalCDDriveLetter").GetAttribute("path")
            Application.LemLog(LOCAL_CD_DRIVE, "Loading...")


            TEMP_BOUNCE_PATH = MP3_PATH + "bounce_temp\1.mp3"
            If (Not Directory.Exists(MP3_PATH + "bounce_temp")) Then
                Directory.CreateDirectory(MP3_PATH + "bounce_temp")
            End If
            propsLoaded = True
            Application.LemLog("Settings Loaded", "FINSHED LOADING PROPERTIES...")
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try


        Return propsLoaded
    End Function



    Public Shared Property _SongPath As String
        Get
            Return artistRecordingSongPath
        End Get
        Set(value As String)
            artistRecordingSongPath = value
        End Set
    End Property

    Public Shared Property _List_Mode As Integer
        Get
            Return listMode
        End Get
        Set(value As Integer)
            listMode = value
        End Set
    End Property

    Public Shared Property _Session_Count As Integer
        Get
            Return sessionCount
        End Get
        Set(value As Integer)
            sessionCount = value
        End Set
    End Property

    Public Shared Property Current_Index As Integer
        Get
            Return index
        End Get
        Set(value As Integer)
            index = value
        End Set
    End Property

    Public Shared Property Current_Playing_Song As String
    Public Shared Property Last_Played_Song As String



    Public Shared ReadOnly Property CurrentArtist() As Artist
        Get

            Return m_CurrentArtist
        End Get

    End Property

    Shared Sub LEM_ERR(ByVal MESSAGE As String)
        MsgBox(MESSAGE, MsgBoxStyle.Exclamation, "Lem Error")
        ' Throw New NotImplementedException
    End Sub
    Public Shared Event LemLogEvent(message As String, overallStatus As String)
    Public Delegate Sub LemLogDel(message As String, overallStatus As String)

    Public Shared Event SoundClickListLoaded()
    Public Shared Event AllProducerBeatsLoaded()

    Public Shared Sub LemLog(p1 As String, Optional overallStatus As String = "")

        'If (Not statWin Is Nothing) Then
        '    If (statWin.IsActive) Then
        '        statWin.setText(p1)
        '    End If


        'End If
        RaiseEvent LemLogEvent(p1, overallStatus)

        ' Throw New NotImplementedException
    End Sub

    Public Sub FinalizeLoadSoundClick(list As List(Of SessionWindow.TrackWrapper))
        If m_Session_Window Is Nothing Then
            Return
        End If
        FinalizeLoadSoundClickList(list)

    End Sub


    Public Shared Function getBackGroundWorker() As BackgroundWorker

        If (m_BackGroundWorker Is Nothing) Then
            m_BackGroundWorker = New BackgroundWorker
        End If
        Return m_BackGroundWorker
    End Function

    Shared Sub StatWincloseBar(ByVal bar As Integer)
        statWin.closeBar(bar)
    End Sub
    Delegate Function SetTempFileLocDelegate(_set As Boolean, val As String)
    Shared Function SetTempFileLoc(_set As Boolean, val As String) As String
        If (_set) Then
            TEMPORARY_FILENAME_HOLDER = val
        Else
            Return TEMPORARY_FILENAME_HOLDER
        End If
        Return ""
    End Function

    Delegate Function StatwinStartDelegate()

    Shared Function StatWinStart() As Boolean

        If (statWin Is Nothing) Then
            statWin = New ProgressModalWindow()

        End If
        If Not (statWin.IsActive) Then

            statWin.ShowNow()


        End If

        Return True
    End Function

    Delegate Sub StatWinCloseDelegate()

    Shared Function StatWinClose() As Boolean

        If (statWin Is Nothing) Then
            Return True

        End If

        If (statWin.IsActive) Then
            statWin.Close()
            Return True
        End If
        Return False
    End Function

    Delegate Sub StatWinGetBarDelegate()

    Public Function StatWinGetBar() As Integer
        Return 0
        If Not statWin Is Nothing Then
            Return statWin.addNewProgressBar()

        End If
        Return Nothing


    End Function


    Delegate Sub StatWinProgressDelegate(progress As Integer, barNum As Integer)

    Shared Sub StatWinProgress(p1 As Integer, barNum As Integer)

        If Not statWin Is Nothing And statWin.IsLive Then
            statWin.setProgress(p1, barNum)
        End If
    End Sub

    Delegate Sub StatWinTextDelegate(message As String)

    Shared Async Sub StatwinText(message As String)
        Dim update As StatWinTextDelegate = New StatWinTextDelegate(AddressOf statWin.setText)


        If Not statWin Is Nothing And statWin.IsLive Then
            statWin.Dispatcher.Invoke(update, message)
            Await Task.Delay(10)


        End If
    End Sub





End Class

Public Class GLOBALARTISTRELEVANCE

    Private globalList As List(Of KeyValuePair(Of Artist, Integer)) = New List(Of KeyValuePair(Of Artist, Integer))


    Public Function updateRelevance(ByRef artist As Artist) As Boolean
        If (globalList.Count = 0) Then
            Dim kv As KeyValuePair(Of Artist, Integer) = New KeyValuePair(Of Artist, Integer)(artist, 1)


            globalList.Add(kv)

        Else

            For Each kv As KeyValuePair(Of Artist, Integer) In globalList

                If (kv.Key.My_Name = artist.My_Name) Then
                    Dim count As Integer = kv.Value + 1
                    Dim art As Artist = kv.Key

                    globalList.Remove(kv)
                    Dim updatedKVofThisRelevence As KeyValuePair(Of Artist, Integer) = New KeyValuePair(Of Artist, Integer)(art, count)
                    globalList.Add(updatedKVofThisRelevence)

                    Return True



                End If



            Next


            Dim newKV As KeyValuePair(Of Artist, Integer) = New KeyValuePair(Of Artist, Integer)(artist, 1)
            globalList.Add(newKV)

        End If


        Return True





    End Function

    Public Function getRelevance(ByRef artist As Artist) As Integer

        For Each kv As KeyValuePair(Of Artist, Integer) In globalList
            If (kv.Key.My_Name = artist.My_Name) Then
                Return kv.Value
            End If
        Next
        Return 0
    End Function

    Public Function updateRelevance(featureName As String, songCount As String) As Boolean

        Dim ftKV As KeyValuePair(Of Artist, Integer)

        If (globalList Is Nothing) Then
            Dim art As Artist = New Artist(featureName, False)
            updateRelevance(art)





            Return True
        End If

        If Not (globalList.Count = 0) Then


            For Each kv As KeyValuePair(Of Artist, Integer) In globalList
                If (kv.Key.My_Name = featureName) Then
                    Dim foundArtist As Artist = kv.Key
                    Dim count As Integer = kv.Value + 1
                    globalList.Remove(kv)

                    ftKV = New KeyValuePair(Of Artist, Integer)(foundArtist, count)
                    globalList.Add(ftKV)


                    Return True
                End If
            Next


            Dim art As Artist = New Artist(featureName, False)
            ftKV = New KeyValuePair(Of Artist, Integer)(art, 1)
            globalList.Add(ftKV)


        End If

        Return True

    End Function



End Class

Public Class IsBouncedLabelConverter
    Implements IValueConverter

    Property IsBounced As Boolean
    Property IsTrue As String
    Property IsFalse As String


    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If (value = True) Then
            Return IsTrue
        Else
            Return IsFalse
        End If

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class


Public Class IsValueConverter
    Implements IValueConverter



    Property IsBounced As String
    Property IsTrue As Brush
    Property IsFalse As Brush

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert

        Dim str As String = TryCast(value, String)


        If Not (str Is Nothing) Then
            If value = "Yes" Then


                Return IsTrue


            Else
                Return IsFalse

            End If
        Else
            If (value) Then
                Return IsTrue

            Else
                Return IsFalse
            End If
        End If


    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack




        Return Nothing
    End Function
End Class


Public Class SessionSongValueConverter
    Implements IValueConverter



    Property IsBounced As String
    Property IsTrue As Brush
    Property IsFalse As Brush


    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert

        If TryCast(value, String) = "Yes" Then


            Return IsTrue
        Else
            Return IsFalse

        End If

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack




        Return Nothing
    End Function
End Class

Public Interface ObjectLister

End Interface





Public Class Admin


    Private Shared PASS_PATH = Application.PASSWORDS_PATH
    Private Shared doc As XDocument
    Private Shared root As XElement

    Public Class Administrator
        Inherits Identity

        'Private MySelf As Artist
        'Private Shadows m_name As String
        Private m_stat As AdministratorType
        ' Private m_pass As PassWord
        Private m_image As Image

        Private m_myNode As XElement
        Private m_myNameAtt As XAttribute
        Private m_myPassAtt As XAttribute

        Public Sub New(clscd As Identity.ClassCode, ByVal _name As String)

            MyBase.New(ClassCode.Artist, _name)

        End Sub

        Public Sub New(ByVal _name As String)

        End Sub



        Public Sub New()

        End Sub






        Public Enum AdministratorType
            CEO = 0
            LEM = 1
            ARTIST = 2


        End Enum

        Public Sub New(ByVal type As AdministratorType)

            m_stat = type




            '            For Each artistnode As XElement In root.Elements
            '                If (artistnode.Attribute("name").Value = name) Then
            '                    'found it

            '                    myNode = artistnode
            '                    myNameAtt = myNode.Attribute("name")
            '                    myPassAtt = myNode.Attribute("pass")

            '                End If

            '                If (myNode Is Nothing) Then
            '                    myNode = New XElement("artist", New XAttribute("name", artist.My_Name), New XAttribute("pass", "temp"))
            '                    myNameAtt = myNode.Attribute("name")
            '                    myPassAtt = myNode.Attribute("pass")
            '                End If

            'Next

        End Sub

        'Public Function LoginAsArtist(ByVal ArtistName As String) As Boolean








        '    Return True

        'End Function


        Public Shared Function ConvertAdminType(ByRef AdministratorType As AdministratorType, Optional ByRef typeAsString As String = "String")


            Select Case typeAsString.ToUpper
                Case "STRING"

                    Select Case AdministratorType
                        Case Administrator.AdministratorType.ARTIST
                            Return "ARTIST"
                        Case Administrator.AdministratorType.CEO
                            Return "CEO"
                        Case Administrator.AdministratorType.LEM
                            Return "LEM"

                    End Select


                Case "INT" Or "INT32" Or "INTEGER"

                    Select Case AdministratorType
                        Case Administrator.AdministratorType.ARTIST
                            Return 2
                        Case Administrator.AdministratorType.CEO
                            Return 0
                        Case Administrator.AdministratorType.LEM
                            Return 1

                    End Select

            End Select


            Return Nothing
        End Function
        Public Shared Function GetAdminTypeFromString(ByVal type As String) As AdministratorType


            Select Case type
                Case "CEO"
                    Return AdministratorType.CEO
                Case "LEM"
                    Return AdministratorType.LEM


            End Select
            Return AdministratorType.ARTIST
        End Function

    End Class
    Public Class PassWord




        'TO DO//// WRITE PREVS TO XML

        Private Current As String = "temp"
        Private Prev As String = ""
        Private Prev2 As String = ""
        Private Prev3 As String = ""

        Private isSet As Boolean = False
        Private Owner As Artist
        Private initialized As Boolean = False

        Private myNode As XElement
        Private myNameAtt As XAttribute
        Private myPassAtt As XAttribute

        Private name As String

        Public RememberMe As Boolean
        Public isLoggedIn As Boolean

        Private backDoorOwner As Boolean
        Dim XMLinitialized As Boolean

        Private Function SetUp() As String

            If (backDoorOwner) Then
                Return True

            End If

            Dim val1 = PassWordWindow.ShowDialoque("Pass not set, Enter New Pass")
            If (val1.ToLower = Application.LEMS_BACKDOOR.ToLower) Then
                Return val1
            End If
            If (val1 = "") Or (val1 = "cancelthisshit") Then
                MsgBox("Cancelled!!")
                Return ""
            End If

            Dim val2 = PassWordWindow.ShowDialoque("One Mo Gen!")

            If (val1 = val2) Then
                createPassWord(val1)


                ' Current = val1
                'Submit()
                MsgBox("PassWord Set, Welcome to Untame " & name & "!")
                Return val1
            End If
            MsgBox("No Match")
            Return ""

        End Function

        Public Function VerifyNameExists() As Boolean

            For Each node As XElement In root.Elements
                If (node.Attribute("name").Value = name) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Delegate Function PassWordWindowDelegate(message As String)



        Public Function TryLogin(artistLoggingIn As Artist) As Boolean




            If (Not Is_Logged_In) Then



                If (Current = "temp") Then
                    Current = SetUp()
                    ' Return False
                    If (Current = "") Then
                        Return False
                    ElseIf Current = Application.LEMS_BACKDOOR_NO_XML Or Current = Application.LEMS_BACKDOOR Then
                        Application.LEMTESTING = True
                        isLoggedIn = True
                        Return True
                    End If

                End If

                Dim arr(4) As String
                arr.SetValue("Please Enter PassWord", 0)
                arr.SetValue("Nope, Try that shit again", 1)
                arr.SetValue("Havin Problems? CHECK YA CAPS!!", 2)
                arr.SetValue("I don't think you know it bruh, Check with Lem", 3)


                Dim app As Application = Application.Current
                Dim pwd As PassWordWindowDelegate = New PassWordWindowDelegate(AddressOf app.ShowPasswordBox)


                For i As Int32 = 0 To 3
                    Dim result As String = app.Dispatcher.Invoke(pwd, arr.GetValue(i))
                    'Select Case PassWordWindow.ShowDialoque(arr.GetValue(i))
                    Select Case result

                        Case Current
                            isLoggedIn = True
                            Return True

                        Case "cancelthisshit"
                            Return False

                        Case Application.LEMS_BACKDOOR
                            isLoggedIn = True
                            Return True
                        Case Application.LEMS_BACKDOOR_NO_XML
                            isLoggedIn = True
                            Application.LEMTESTING = True
                            Return True



                        Case Else
                            ' Return False
                            If Not (artistLoggingIn.GroupMembers Is Nothing) Then
                                Dim q = From art In artistLoggingIn.GroupMembers
                                        Where art.GetMyPass(artistLoggingIn) = result
                                        Select art

                                If Not q.FirstOrDefault Is Nothing Then
                                    Return True
                                End If




                            End If

                    End Select
                    'If (InputBox(arr.GetValue(i), MsgBoxStyle.RetryCancel) = Current) Then
                    '    isLoggedIn = True
                    '    Return True


                    'End If
                Next
            Else
                Return True



            End If


            Return False





        End Function

        Public ReadOnly Property Is_Logged_In As Boolean
            Get
                Return isLoggedIn
            End Get
        End Property

        Public Sub New(ByRef ownerOfPass As String, Optional ByVal _initialize As Boolean = True)
            name = ownerOfPass
            If (doc Is Nothing Or root Is Nothing) Then
                doc = XDocument.Load(PASS_PATH)
                root = doc.Root
            End If

            If Not XMLinitialized Then
                initializeNodes()
            End If
            If (ownerOfPass = "BackDoor") Then
                isSet = True
                isLoggedIn = True
                backDoorOwner = True

            End If




            If (_initialize) Then
                init()
            End If
        End Sub


        Public Sub Set_Owner_Reference(ByRef _owner As Artist)

            Owner = _owner

        End Sub



        Private Sub Submit()

            If Not (backDoorOwner) Then
                doc.Save(PASS_PATH)
            End If



        End Sub

        Private Function init() As Boolean


            Dim isThere = False

            For Each artistnode As XElement In root.Elements
                If (artistnode.Attribute("name").Value = name) Or (artistnode.Attribute("name").Value.ToLower = name.ToLower) Then
                    'found it
                    isThere = True
                    myNode = artistnode
                    myNameAtt = myNode.Attribute("name")
                    myPassAtt = myNode.Attribute("pass")

                    If Not (artistnode.Attribute("pass").Value = "temp") Then
                        'pass is good

                        Current = myPassAtt.Value

                        isSet = True

                        initialized = True


                    Else
                        'pass= temp
                        'add pass
                        Select Case SetUp()
                            Case "Lem"
                                Return True
                            Case Application.LEMS_BACKDOOR_NO_XML
                                Return True
                            Case ""
                                MsgBox("Password not set, Please try again or Contact Lem")
                                Return False
                        End Select

                        Return False
                    End If


                End If

            Next

            ' see if its there
            If Not (isThere) Then
                'must be a new name


                If Not SetUp() Then
                    MsgBox("Password not set, Please try again or Contact Lem")
                End If
                Return False




            End If




            Return False
        End Function


        Public Function AddNewUserToXML(ByVal pass As String) As Boolean

            If (backDoorOwner) Then
                Return True
            End If


            If Not (VerifyNameExists()) Then


                myNode = New XElement("artist", New XAttribute("name", name), New XAttribute("pass", pass))
                root.Add(myNode)

                Submit()
                isSet = True

                myNameAtt = myNode.Attribute("artist")
                myPassAtt = myNode.Attribute("pass")

                Return True
            Else
                MsgBox("Use another Name or Change the spelling slightly")
                Return False

            End If
        End Function

        Private Function createPassWord(ByVal pass As String) As Boolean
            Dim _init = False
            If (backDoorOwner) Then
                Return True
            End If
            If Not XMLinitialized Then
                _init = initializeNodes()
            End If

            If (_init) Then
                myPassAtt.Value = pass
                Current = pass
                isSet = True
                Submit()
            End If


            Return True

        End Function

        Private Function initializeNodes() As Boolean

            If XMLinitialized Then
                Return True
            End If
            For Each artistnode As XElement In root.Elements
                If (artistnode.Attribute("name").Value = name) Or (artistnode.Attribute("name").Value.ToLower = name.ToLower) Then


                    myNode = artistnode
                    myNameAtt = myNode.Attribute("name")
                    myPassAtt = myNode.Attribute("pass")

                    If Not (artistnode.Attribute("pass").Value = "temp") Then
                        'pass is good

                        Current = myPassAtt.Value

                        isSet = True

                        XMLinitialized = True


                    Else

                    End If
                End If
            Next
            Return True
        End Function
        ' Protected Sub OnNameChanged()


        Public Function UpdatePassWord() As Boolean

            If (backDoorOwner) Then
                Return True
            End If

            If (isSet) Then
                If (InputBox("Enter Old PassWord") = Current) Then


                    Dim new1 = InputBox("Enter New Pass").ToLower
                    Dim new2 = InputBox("Re-enter Pass").ToLower

                    If (new1 = new2) Then
                        Prev3 = Prev2
                        Prev2 = Prev
                        Prev = Current
                        Current = new2

                        myPassAtt.Value = Current
                        Submit()
                        MsgBox("Done")
                        Return True

                    Else
                        MsgBox("Try again playa")
                        Return False
                    End If



                Else
                    MsgBox("Wrong PassWord")
                    Return False
                End If


            Else




            End If
            Return False

        End Function
        Public ReadOnly Property Is_Set As Boolean
            Get
                Return isSet
            End Get
        End Property

        Public ReadOnly Property PassWord As String
            Get
                Return Current
            End Get
        End Property
        Public WriteOnly Property Update_Name As String
            Set(value As String)
                myNameAtt.Value = value
                Submit()
            End Set
        End Property

        Private Function Match(ByRef try_text As String) As Boolean

            If (Current = try_text) Then
                Return True
            End If

            Return False
        End Function



        Public Sub OnNameChanged(ByRef newName As String, ByRef globalId As Identity.GlobalID)

            If (Owner.GLOBAL_ID = globalId.GLOBAL_ID) Then
                Update_Name = newName


            End If
        End Sub
    End Class

    'Public Class Administrator

    '    Private MySelf As Artist
    '    Private name As String
    '    Private stat As AdministratorType
    '    Private pass As PassWord
    '    Private image As Image

    '    Private myNode As XElement
    '    Private myNameAtt As XAttribute
    '    Private myPassAtt As XAttribute




    '    Public Enum AdministratorType
    '        CEO = 0
    '        LEM = 1
    '        ARTIST = 2


    '    End Enum

    '    Public Sub New(ByVal type As AdministratorType, ByRef artist As Artist)

    '        stat = type
    '        MySelf = artist



    '        '            For Each artistnode As XElement In root.Elements
    '        '                If (artistnode.Attribute("name").Value = name) Then
    '        '                    'found it

    '        '                    myNode = artistnode
    '        '                    myNameAtt = myNode.Attribute("name")
    '        '                    myPassAtt = myNode.Attribute("pass")

    '        '                End If

    '        '                If (myNode Is Nothing) Then
    '        '                    myNode = New XElement("artist", New XAttribute("name", artist.My_Name), New XAttribute("pass", "temp"))
    '        '                    myNameAtt = myNode.Attribute("name")
    '        '                    myPassAtt = myNode.Attribute("pass")
    '        '                End If

    '        'Next

    '    End Sub

    '    'Public Function LoginAsArtist(ByVal ArtistName As String) As Boolean








    '    '    Return True

    '    'End Function


    '    Public Shared Function ConvertAdminType(ByRef AdministratorType As AdministratorType, Optional ByRef typeAsString As String = "String")


    '        Select Case typeAsString.ToUpper
    '            Case "STRING"

    '                Select Case AdministratorType
    '                    Case Administrator.AdministratorType.ARTIST
    '                        Return "ARTIST"
    '                    Case Administrator.AdministratorType.CEO
    '                        Return "CEO"
    '                    Case Administrator.AdministratorType.LEM
    '                        Return "LEM"

    '                End Select


    '            Case "INT" Or "INT32" Or "INTEGER"

    '                Select Case AdministratorType
    '                    Case Administrator.AdministratorType.ARTIST
    '                        Return 2
    '                    Case Administrator.AdministratorType.CEO
    '                        Return 0
    '                    Case Administrator.AdministratorType.LEM
    '                        Return 1

    '                End Select

    '        End Select


    '        Return Nothing
    '    End Function
    '    Public Shared Function GetAdminTypeFromString(ByVal type As String) As AdministratorType


    '        Select Case type
    '            Case "CEO"
    '                Return AdministratorType.CEO
    '            Case "LEM"
    '                Return AdministratorType.LEM


    '        End Select
    '        Return AdministratorType.ARTIST
    '    End Function





    'End Class

    Shared Function CreateNewUntameRecordingArtist(ByRef ArtistName As String, Optional ByRef password As String = "", Optional ByVal returnType As String = "bool") As Array
        'Throw New NotImplementedException
        If (ArtistName Is Nothing) Then
            Return Nothing
        End If

        Dim artist As Artist = Nothing
        Dim result(2)
        result(0) = -1


        Dim pass As PassWord = New PassWord(Utils.converToTitleCase(ArtistName), False)

        If Not (password = "") Then
            If Not (pass.Is_Set) Then
                If Not (pass.AddNewUserToXML(password)) Then
                    Return result
                End If

            End If

        Else
            pass.AddNewUserToXML("temp")

        End If

        If (returnType = "bool") Then
            result(0) = 0
        Else
            result(0) = 1
            result(1) = New Artist(ArtistName, False)

        End If


        Return result
    End Function



End Class











































Public Class Identity



    Protected Shared m_GlobalIDList As List(Of GlobalID) = New List(Of GlobalID)
    Protected Shared m_listItems As IEnumerator(Of GlobalID) = m_GlobalIDList.GetEnumerator
    Protected Shared ClassGlobalCount As Int32


    'Protected ncl As GlobalID.NameChangedListener


    'Protected m_IsNewObject As Boolean = True
    Protected m_name As String
    Protected WithEvents m_GlobalID As GlobalID
    Protected m_LocalID As Int32


    Public Enum ClassCode

        Artist = 12000
        Comment = 12001
        FeatureInfo = 12002
        Session = 12003
        Song = 12004
        LoginData = 12005

    End Enum

    Public Function DeleteThis() As Boolean
        m_GlobalID.UnRegisterID()
        Return True
    End Function

    Public Class GlobalID

        Private Shared identityPath As String = Application.IDENTITY_PATH
        Private m_base As String = "UNT"
        Private m_classCode As ClassCode
        Private m_localID As Int32
        Private m_name As String
        Private m_isNew As Boolean = True
        Private BrandNewName As Boolean = True
        'Private ncl As nameChangedListener
        Private rbl As RebuildReciever
        Private mom As Identity

        Private Shared handle As XDocument
        Private Shared root As XElement
        Private Shared rootCount As XAttribute

        Private classNode As XElement
        Private classIDAtt As XAttribute
        Private classCount As XAttribute

        Private localNode As XElement
        Private localIDAtt As XAttribute
        Private localNameAtt As XAttribute
        Private _glbid As String

        Sub New(glbid As String, ByRef parent As Identity)
            ' TODO: Complete member initialization 
            ExtractID(glbid)
            mom = parent
            m_isNew = False
            m_name = FindNameByGlobalID(glbid)
            BrandNewName = False



            classNode = root.Element(Identity.GetClassCodeType(m_classCode) & "s")
            classIDAtt = classNode.Attribute("id")
            classCount = classNode.Attribute("count")
            classIDAtt.Value = CLASS_CODE_INT


        End Sub

        ' Public WithEvents ncListener As NameChangedListener

        Public Event NameChanged(ByRef newName As String, ByRef globalID As Identity.GlobalID, ByRef data As Object)

        Public Event StartRebuildObject(ByRef globalID As GlobalID, ByRef parent As Identity, ByRef returnObject As Object)



        'Public Interface nameChangedListener
        '    Sub OnNameChanged(ByRef newName As String, ByRef globalId As Identity.GlobalID)
        'End Interface


        'Public Class NameChangedListener

        '    Public Event NameChanged(ByRef newName As String, ByRef globalID As Identity.GlobalID)

        '    Public Sub OnNameChanged(ByRef newName As String, ByRef globalID As Identity.GlobalID)

        '        RaiseEvent NameChanged(newName, globalID)

        '    End Sub


        'End Class

        Public Interface RebuildReciever
            Sub OnRebuild(ByRef GlobalID As GlobalID)
        End Interface

        Private Sub ExtractID(ByVal ID As String)

            Dim arr = Split(ID, "-")
            m_localID = arr.GetValue(2)
            m_classCode = arr.GetValue(1)


        End Sub
        Public Shared Function FindGlobalIDByName(ByVal name As String) As String
            For Each _classNode As XElement In root.Elements
                For Each _sunNode As XElement In _classNode.Elements
                    If Not (_sunNode.Attributes.FirstOrDefault(Function(att) att.Value() = "deleted") Is Nothing) Then
                        Return ""
                    End If


                    If (_sunNode.Attribute("name").Value = name) Then
                        Return _sunNode.Attribute("id").Value
                    End If
                Next

            Next
            Return ""
        End Function

        Public Shared Function CheckForExistingID(GID As String) As Boolean
            For Each id As GlobalID In m_GlobalIDList
                If (id.GLOBAL_ID = GID) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function GetExistingIDObject(GID As String) As GlobalID
            For Each id As GlobalID In m_GlobalIDList
                If (id.GLOBAL_ID = GID) Then
                    Return id
                End If
            Next
            Throw New Exception("Global ID Doesnt exist, Check for this before calling me")
            Return Nothing
        End Function

        Public Shared Function FindNameByGlobalID(ByVal GlobalID As String) As String

            Dim gidSplit As Array = GlobalID.Split("-")
            Dim cc As Int16 = Convert.ToInt16(gidSplit.GetValue(1))
            Dim local As Int16 = Convert.ToInt16(gidSplit.GetValue(2))



            For Each _classNode As XElement In root.Elements
                If (_classNode.Attribute("id").Value = cc) Then
                    If (_classNode.HasElements) Then
                        For Each locnode As XElement In _classNode.Elements

                            If (locnode.Attribute("id").Value = local) Then
                                Dim deletedAtt = locnode.Attributes.FirstOrDefault(Function(att) att.Name = "deleted")
                                If Not (deletedAtt Is Nothing) Then
                                    If (deletedAtt.Value = "true") Then

                                        Return ""
                                    End If

                                End If
                                Return locnode.Attribute("name").Value
                            End If

                        Next
                    Else
                        If (cc = 12005) Or (cc = 12003) Then
                            Return ""
                        End If

                        Return ""
                    End If



                End If
            Next
            If (cc = 12005) Or (cc = 12003) Then
                Return ""
            End If
            Dim app As Application = Application.Current
            Dim logdel = New Application.LemLogDel(AddressOf Application.LemLog)
            app.Dispatcher.BeginInvoke(logdel, {"Global ID Missing, Attempting to update Song and Login data...", "Please Wait"})
            'Application.LemLog("Global ID Missing, Attempting to update Song and Login data...", "Please Wait")
            Try
                SongData.Update_Songs_XML()
                LoginData.UpdateLoginIDs()


            Catch ex As Exception
                Application.LemLog("Problem occured during update...", "Done.")
                Throw New Exception("Global ID Missing, Run SongData.UpdateSongInfo and/or LogingData.UpdateLoginData")

            End Try
            Return ""
        End Function

        Public Function UpdateName(ByVal newName As String, Optional BrandNew As Boolean = False, Optional data As Object = "") As String


            'find id and change name, set m_name, return name, notify parent of change

            localNode.Attribute("name").Value = newName


            If Not (BrandNew) Then
                m_name = newName


                RaiseEvent NameChanged(newName, Me, data)
            End If


            Application.IdentityHasChanged = True
            Return newName

        End Function

        Public Sub RebuildObject(ByVal copy As GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")


            RaiseEvent StartRebuildObject(copy, parent, returnOBJ)


        End Sub




        Public Sub New(ByRef ClassCode As ClassCode, ByRef Parent As Identity, Optional ByRef name As String = "", Optional ByRef GlobID As String = "", Optional ByRef returnObject As Object = Nothing)


            If (name.Contains("UNT-")) Then
                Throw New Exception("NO NO NO")

            End If
            mom = Parent
            mom.m_GlobalID = Me
            m_classCode = ClassCode



            'setting shared fields
            If (handle Is Nothing Or root Is Nothing) Then



                handle = XDocument.Load(identityPath)
                root = handle.Root
                rootCount = root.Attribute("count")




            End If

            classNode = root.Element(Identity.GetClassCodeType(ClassCode) & "s")
            classIDAtt = classNode.Attribute("id")
            classCount = classNode.Attribute("count")
            classIDAtt.Value = CLASS_CODE_INT


            'if gid
            If (GlobID.Contains("UNT-") And classNode.Elements.Count >= Utils.ExtracLocalFromGlobal(GlobID) - classCount.Value) Then
                m_localID = Utils.ExtracLocalFromGlobal(GlobID)
                mom.m_LocalID = m_localID
                m_isNew = False
                For Each node As XElement In classNode.Elements
                    If (Convert.ToInt32(node.Attribute("id").Value = m_localID)) Then
                        If Not (node.Attributes.FirstOrDefault(Function(att) att.Name = "deleted") Is Nothing) Then
                            If node.Attribute("deleted").Value = "true" Then
                                Exit For
                            End If

                        End If
                        localNode = node
                        localNameAtt = localNode.Attribute("name")
                        localIDAtt = localNode.Attribute("id")
                        m_name = localNameAtt.Value
                        mom.m_name = m_name





                        RebuildObject(AdministerIDList(Me), mom, returnObject)
                        Exit For
                    Else
                        ' GlobID = ""
                        'Application.LEM_ERR(Identity.GetClassCodeType(m_classCode) & " Id is missing from xml for" & GlobID)
                        'Throw New Exception(Identity.GetClassCodeType(m_classCode) & " Id is missing from xml for" & GlobID)
                    End If
                Next

            Else
                'incrementClassCount()
                ' localNode = New XElement(Identity.GetClassCodeType(ClassCode), New XAttribute("id", classCount), New XAttribute("name", ""))
            End If





            'THIS MAY NOT BE GETTING CALLED
            'If (m_GlobalIDList.Count > 0) Then
            '    For Each gid In m_GlobalIDList
            '        If (GlobID.Contains("UNT-")) Then
            '            If (gid.GLOBAL_ID = GlobID) Then
            '                mom = gid.GET_OBJECT
            '                Exit Sub

            '            End If
            '            'ElseIf (gid.GLOBAL_ID = Parent.GLOBAL_ID) Then
            '            '    ff()
            '        End If

            '    Next
            'End If

            If (GlobID.Contains("UNT-")) Then
                Dim newOBJ = New GlobalID()
                newOBJ.m_classCode = Utils.ExtractClassCodeFromGlobalID(GlobID)
                newOBJ.m_localID = Utils.ExtracLocalFromGlobal(GlobID)

                RegisterID(gid:=GlobID)


                newOBJ.m_name = localNameAtt.Value
                newOBJ.m_isNew = False

                AdministerIDList(newOBJ)
                mom = Parent



                RebuildObject(newOBJ, mom, returnObject)
                Exit Sub

            End If

            'no glid

            'classNode = root.Element(Identity.GetClassCodeType(m_classCode) & "s")
            'classIDAtt = classNode.Attribute("id")
            'classCount = classNode.Attribute("count")





            'if there is a name 
            If Not (name = "") Then
                'than either find id against name  or create new id

                RegisterID(Utils.converToTitleCase(name, True))


            ElseIf (GlobID.Contains("UNT-") And name = "") Then

                RegisterID(gid:=GlobID)



            Else

                'has to be a new creation so create new name and id
                RegisterID()
            End If



        End Sub





        Public Sub New()

        End Sub



        Public ReadOnly Property ID_NAME As String
            Get
                Return m_name
            End Get
        End Property

        Public ReadOnly Property IS_NEW As Boolean
            Get
                Return m_isNew
            End Get
        End Property

        Public ReadOnly Property GLOBAL_ID As String
            Get
                Return m_base & "-" & m_classCode & "-" & m_localID
            End Get
        End Property

        Public ReadOnly Property LOCAL_ID As Int32
            Get
                Return m_localID
            End Get
        End Property

        Public ReadOnly Property CLASS_CODE As ClassCode
            Get
                Return m_classCode
            End Get
        End Property

        Public ReadOnly Property CLASS_CODE_STRING As String
            Get
                Return Identity.GetClassCodeType(m_classCode)
            End Get
        End Property

        Public ReadOnly Property CLASS_CODE_INT As Int32
            Get
                Return Convert.ToInt32(m_classCode)
            End Get
        End Property

        Public ReadOnly Property CLASS_COUNT As Int32
            Get
                Return Convert.ToInt32(classCount.Value)
            End Get
        End Property

        Public ReadOnly Property GET_OBJECT As Identity
            Get
                Return mom

            End Get
        End Property


        Public WriteOnly Property UpDate_Name As String
            Set(value As String)
                UpdateName(Utils.converToTitleCase(value, True))
            End Set
        End Property

        Private Function AdministerIDList(ByRef globalID As GlobalID) As GlobalID

            If (m_GlobalIDList.Count > 0) Then

                For Each gi As GlobalID In m_GlobalIDList

                    If (gi.GLOBAL_ID = GLOBAL_ID) Then

                        mom = gi.GET_OBJECT
                        mom.m_GlobalID = Me
                        'm_isNew = False

                        'mom.m_IsNewObject = False
                        Return gi
                    End If



                Next



            End If
            'mom.m_IsNewObject = True
            mom.m_GlobalID = Me
            ' m_isNew = True
            m_GlobalIDList.Add(Me)
            Return Me



        End Function

        ''' <summary>
        ''' Retrieves the local Identity Node for this GlobalID Object
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub queryMyNode()

            If Not (classNode Is Nothing) Then
                Dim queryMyNode =
                                From childNode As XElement In classNode.Elements
                                Where childNode.Attribute("id").Value = m_localID
                                Select childNode

                localNode = queryMyNode.First
                localNameAtt = localNode.Attribute("name")
                localIDAtt = localNode.Attribute("id")

                m_name = localNameAtt.Value
            Else
                Throw New Exception(Identity.GetClassCodeType(m_classCode) & " node is null")


            End If


        End Sub

        ''' <summary>
        ''' Checks for a registerd name if one is provided, if not will create a new id
        ''' </summary>
        ''' <param name="name"></param>
        ''' <remarks></remarks>
        Private Sub RegisterID(Optional ByRef name As String = "", Optional ByVal gid As String = "")

            Dim elname = Identity.GetClassCodeType(m_classCode)
            If name.Contains("UNT-") Then
                Throw New Exception("NO No NO")
            End If
            ' if name
            If Not (name = "") Then
                'temporarily reset isnew
                ' m_isNew = True
                '-----look for name in list
                For Each locNode As XElement In classNode.Elements
                    'check each node


                    If (Utils.SpaceSafeCompareSongName(locNode.Attribute("name").Value, name)) Then
                        'found it
                        m_isNew = False
                        If Not (locNode.Attribute("deleted") Is Nothing) Then
                            If (locNode.Attribute("deleted").Value = "true") Then
                                BrandNewName = True
                                Exit For
                            End If
                        End If
                        m_name = name
                        BrandNewName = False
                        mom.m_name = m_name
                        ' mom.m_IsNewObject = False


                        localNode = locNode
                        localIDAtt = localNode.Attribute("id")
                        localNameAtt = localNode.Attribute("name")
                        m_localID = localIDAtt.Value
                        mom.m_LocalID = m_localID
                        AdministerIDList(Me)

                        Exit For

                    End If



                Next

                If (BrandNewName) Then

                    'name no exist create whole new id

                    'but if existing globalID
                    If Not (m_isNew) And (gid.Contains("UNT-")) Then



                        queryMyNode()


                        Exit Sub
                    Else
                        m_name = name
                        incrementClassCount()

                        WriteNewNode()
                        Application.IdentityHasChanged = True

                        ' m_isNew = True
                        ' mom.m_IsNewObject = True

                    End If


                    m_localID = classCount.Value
                    AdministerIDList(Me)


                End If



            Else

                'if no name

                If (gid.Contains("UNT-")) Then
                    'set name
                    queryMyNode()

                Else
                    'create new name and id
                    incrementClassCount()
                    m_name = elname & "-" & Now
                    WriteNewNode()
                    m_localID = classCount.Value


                End If





                AdministerIDList(Me)



            End If

        End Sub



        Private Sub incrementClassCount()

            Dim clscnt = Convert.ToInt32(classCount.Value)
            clscnt = clscnt + 1
            classCount.Value = clscnt


        End Sub

        Private Sub decrementClassCount()

            Dim clscnt = Convert.ToInt32(classCount.Value)
            clscnt = clscnt - 1
            classCount.Value = clscnt

        End Sub

        Public Shared Sub CommitAllData()
            If (Application.IdentityHasChanged) Then
                Try
                    handle.Save(identityPath)
                    Application.IdentityHasChanged = False
                Catch ex As Exception
                    Application.LemLog(ex.Message)
                End Try
                
            End If


        End Sub

        Private Sub WriteNewNode()




            Dim elname = Identity.GetClassCodeType(m_classCode)
            localNode = New XElement(elname, New XAttribute("id", classCount.Value), New XAttribute("name", m_name))
            localIDAtt = localNode.Attribute("id")
            localNameAtt = localNode.Attribute("name")

            If Not (CLASS_CODE = ClassCode.FeatureInfo) Then
                classNode.Add(localNode)

                'Submit()

                Application.IdentityHasChanged = True
            End If






        End Sub
        Public Function GetGroupMembers() As List(Of Artist)
            Dim result As List(Of Artist) = New List(Of Artist)
            If Not (localNode.Attribute("group") Is Nothing) Then
                For Each member As XElement In localNode.Element("GroupMembers").Elements
                    Dim art As Artist = New Artist(member.Attribute("name"), False)
                    result.Add(art)

                Next
            End If
            Return result
        End Function


        Public Sub RegisterArtistToGroup(artistGID As String, artistName As String)






            For Each locNode As XElement In classNode.Elements
                'check each node


                If (Utils.SpaceSafeCompareSongName(locNode.Attribute("name").Value, m_name
)) Then
                    'found it
                    ' Dim groupAtt As XAttribute = New XAttribute("group", True)
                    Dim groupNode As XElement = Nothing
                    If (locNode.Attribute("group") Is Nothing) Then
                        groupNode = New XElement("GroupMembers")
                    Else
                        groupNode = locNode.Element("GroupMembers")
                        groupNode.Remove()
                    End If

                    groupNode.Add(New XElement("GroupMember", New XAttribute("id", artistGID), New XAttribute("name", artistName)))


                    locNode.Add(groupNode)
                    locNode.SetAttributeValue("group", True)

                    Exit For


                End If
            Next

        End Sub


        Public Sub UnRegisterID()

            ' decrementClassCount()

            For Each locnode In classNode.Elements
                If (Convert.ToInt32(locnode.Attribute("id").Value) = m_localID) Then
                    If (locnode.Attribute("deleted") Is Nothing) Then
                        locnode.Add(New XAttribute("deleted", True))
                    Else
                        locnode.Attribute("deleted").Value = "true"
                    End If


                    Dim toremove As GlobalID = m_GlobalIDList.Find(Function(gid) gid.GLOBAL_ID = GLOBAL_ID)

                    m_GlobalIDList.Remove(toremove)



                    Application.IdentityHasChanged = True

                    GlobalID.CommitAllData()

                End If

            Next
            'Submit()

        End Sub

        Public Overrides Function ToString() As String
            Return m_base & "-" & m_classCode & "-" & m_localID
        End Function

    End Class




    Public Shared Function GetClassCodeType(ByVal Cclass As ClassCode) As String
        Dim result As String = "N\A"

        Select Case Cclass
            Case ClassCode.Artist
                result = "Artist"
            Case ClassCode.Comment
                result = "Comment"
            Case ClassCode.FeatureInfo
                result = "FeatureInfo"
            Case ClassCode.LoginData
                result = "LoginData"
            Case ClassCode.Session
                result = "Session"
            Case ClassCode.Song
                result = "Song"
        End Select


        Return result





    End Function

    Public Shared Function GetClassCodeFromInt(ByRef int As Int32) As ClassCode

        Dim result As ClassCode

        Select Case int
            Case 12000
                result = ClassCode.Artist
            Case 12001
                result = ClassCode.Comment
            Case 12002
                result = ClassCode.FeatureInfo
            Case 12003
                result = ClassCode.Session
            Case 12004
                result = ClassCode.Song
            Case 12005
                result = ClassCode.LoginData
        End Select


        Return result
    End Function

    Public Function GetNew() As Object

        Dim result As Object = Me.MemberwiseClone

        Return result

    End Function



    Public Shared Function MakeNew(ByVal classCode As ClassCode, Optional ByVal name As String = "", Optional ByVal GlbID As String = "", Optional ByRef returnOBJ As Object = 0) As Identity


        If (GlbID = "") Then
            GlbID = Utils.GetGlobalIDByName(Identity.ClassCode.Artist, name)

        End If

        If (GlobalID.CheckForExistingID(GlbID)) Then
            Return GlobalID.GetExistingIDObject(GlbID).GET_OBJECT
        End If

        Dim result As Identity = New Identity(classCode, name, GlbID, returnOBJ)

        Return result





    End Function





    Public Sub New(ByVal classCode As ClassCode, Optional ByVal name As String = "", Optional ByRef GlbID As String = "", Optional ByRef returnOBJ As Object = 0)


        If name.Contains("UNT-") Then
            Throw New Exception("Using a GlobalID for a name!!!!!")

        End If
        'If (GlbID.Contains("UNT-")) Then
        '    Dim temp As GlobalID
        '    If Not (GetObjectByID(GlbID) Is Nothing) Then
        '        m_GlobalID =
        '    End If
        'End If
        If (classCode = Identity.ClassCode.Artist) Or (classCode = Identity.ClassCode.Song) Then
            For Each Str As String In Application.NAMESKIPARRAY
                If (Str = name) Then
                    Return
                End If
            Next
        End If


        m_GlobalID = New GlobalID(classCode, Me, name, GlbID, returnOBJ) ' if name is found will set the local id for this global id object


        If Not (IS_NEW) Then
            Exit Sub
        End If

        'ncl = m_GlobalID.ncListener
        m_LocalID = m_GlobalID.LOCAL_ID

        m_name = m_GlobalID.ID_NAME
        ClassGlobalCount = m_GlobalID.CLASS_COUNT
        Try
            Dim updateServerLSRID As Integer = Application.UNTAMEHTTP.UpdateArtistLSRID(m_name, m_LocalID, Application.LOCAL_HOST)
        Catch ex As Exception

        End Try




        'm_IsNewObject = m_GlobalID.IS_NEW

        ' m_GlobalIDList.Add(m_GlobalID)
        'm_listItems = m_GlobalIDList.GetEnumerator


    End Sub


    'Public Sub New(ByRef name As String, Optional ByVal GlobalID As String = "")



    'End Sub

    'Public Sub New()




    'End Sub

    'Public Overridable Sub CreateFromLocalId(ByRef Local As Int32)



    'End Sub

    'Protected Sub CoerceID(ByVal StringID As String)
    '    m_LocalId = Convert.ToInt32(Split(StringID, "-").GetValue(2))

    'End Sub


    'Private Function SetID() As Boolean

    '    Dim doc = XDocument.Load(Application.XML_PATH & "Identity.xml")
    '    Dim root = doc.Root
    '    Dim selection = From node As XElement In root.Elements
    '                      Where Convert.ToInt32(node.Attribute("id").Value) = m_ClassCode
    '                      Select node

    '    Dim parentNode = selection.First
    '    ClassGlobalCount = Convert.ToInt32(parentNode.Attribute("count").Value)

    '    'm_LocalId = CreateNewID(parentNode)




    '    selection = From node As XElement In parentNode.Elements
    '                    Where node.Attribute("name").Value = m_name
    '                    Select node

    '    Dim localNode As XElement = Nothing
    '    Try
    '        localNode = selection.First
    '    Catch ex As Exception

    '    End Try


    '    If (localNode Is Nothing) Then
    '        'If (m_name.Contains(GetClassCodeType(m_ClassCode))) Then
    '        '    m_name = m_name & GlobalID

    '        'End If
    '        'Dim tempId = CreateNewID(parentNode)
    '        m_LocalId = CreateNewID(parentNode)
    '        localNode = New XElement(GetClassCodeType(m_ClassCode), New XAttribute("id", m_LocalId), New XAttribute("name", m_name))




    '        parentNode.Add(localNode)
    '        doc.Save(Application.XML_PATH & "Identity.xml")
    '    Else

    '        m_LocalId = Convert.ToInt32(localNode.Attribute("id").Value)




    '    End If










    '    Return True



    'End Function


    Public Sub New()

    End Sub







    Public Shared Function GetObjectByID(ByVal GlobalID As String) As GlobalID ' HACK this may return null


        'first go through current list
        If (m_GlobalIDList.Count > 0) Then





            While Not (m_listItems.Current.GLOBAL_ID = GlobalID)
                m_listItems.MoveNext()

            End While




            Return m_listItems.Current

        End If
        Return Nothing
    End Function

    Public Overridable ReadOnly Property GLOBAL_ID As String
        Get
            Return m_GlobalID.GLOBAL_ID
        End Get
    End Property


    Public Overridable ReadOnly Property LOCAL_ID As Int32
        Get
            Return m_LocalID
        End Get
    End Property

    Public Overridable ReadOnly Property OBJECT_NAME As String
        Get
            Return m_name
        End Get
    End Property

    Public Overridable ReadOnly Property IS_NEW As Boolean
        Get
            Return m_GlobalID.IS_NEW
        End Get
    End Property

    Protected Property Old_Name As String





    Public Overridable Sub OnNameChanged(ByRef newName As String, ByRef globalId As GlobalID, Optional ByRef data As Object = "") Handles m_GlobalID.NameChanged
        Old_Name = m_name
        If (Old_Name = newName) Then
            Return
        End If
        If (GLOBAL_ID = globalId.GLOBAL_ID) Then
            m_name = newName
        End If
    End Sub


    Public Overridable Sub OnRebuild(ByRef glbID As GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "") Handles m_GlobalID.StartRebuildObject
        If (glbID.GLOBAL_ID = GLOBAL_ID And Not (m_GlobalID.IS_NEW)) Then


            m_LocalID = m_GlobalID.LOCAL_ID
            m_name = glbID.ID_NAME

        End If
    End Sub

    Shared Function CreateLoginID(ByRef name As String) As GlobalID
        Dim newLoginID As GlobalID = New GlobalID(ClassCode.LoginData, New Identity(), "LGD_" & name & "_" & Now)
        Application.IdentityHasChanged = True
        GlobalID.CommitAllData()
        Return newLoginID

    End Function

End Class

Public Class ArtistRelevanceComparer
    Inherits Comparer(Of Artist)

    Public Overrides Function Compare(x As Artist, y As Artist) As Integer

        Dim rel1 = Application.ARTIST_RELEVANCE_OBJECT.getRelevance(x)
        Dim rel2 = Application.ARTIST_RELEVANCE_OBJECT.getRelevance(y)

        If (rel1 < rel2) Then

            Return 1

        ElseIf (rel1 > rel2) Then
            Return -1


        Else

            Return 0

        End If


    End Function
End Class







Public Class LoginData
    Inherits Identity
    Implements IComparable(Of LoginData)




    Private Shared root As XElement = XMLResources.GetLoginXMLResourceRoot


    Private Shared loginHandle As XDocument

    Private Shared modeAttr As XAttribute
    Private Shared bookedUntilAttr As XAttribute


    ' Private liveSession As SetupSession

    'Private deadSession As SetupSession.Session
    Private isLive As Boolean = False
    Private currentArtist As String
    Private currentSongTitle As String
    Private currentSongGID As String
    Private features As Boolean
    Private songHasComments As Boolean
    Private nameVal As String
    Private isBounced As Boolean
    Private isMixReady As Boolean
    Private isPriority As Boolean
    Private musicSource As SetupSession.SessionMusicSource_Type
    Private NeedsFeature As Int32


    Private loginVal As DateTime
    Private logoutVal As DateTime
    Private lengthVal As String

    Private songVal As String



    Public artistLogin As XElement

    Public artistOBJ As Artist

    Public loginSongList As List(Of SongContainer)




    Public Structure SongContainer

        Private s_song As SongData

        Private s_opened As DateTime

        Private s_closed As DateTime

        Public WriteOnly Property DateClosed As DateTime
            Set(value As DateTime)
                s_closed = value
            End Set
        End Property
        Public WriteOnly Property DateOpened As DateTime
            Set(value As DateTime)
                s_opened = value
            End Set
        End Property


        Public Property Song As SongData
            Get
                Return s_song
            End Get
            Set(value As SongData)
                s_song = value
            End Set
        End Property

        Public ReadOnly Property Opened As String

            Get
                Return s_opened.ToShortTimeString
            End Get
        End Property


        Public ReadOnly Property Closed As String

            Get
                Return s_closed.ToShortTimeString
            End Get
        End Property

        Public Property DurationString As String
            Get
                Return String.Format("from {0} to {1}({2})", Opened, Closed, Utils.Get_Time_Duration(s_opened.ToString, s_closed.ToString))


            End Get
            Set(value As String)

            End Set
        End Property

    End Structure

    Sub New()
        ' TODO: Complete member initialization 
    End Sub


    Public Overrides Sub OnRebuild(ByRef GlobalID As Identity.GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")

        If (GlobalID.GLOBAL_ID = m_GlobalID.GLOBAL_ID) Then


            currentArtist = artistLogin.Attribute("name").Value
            currentSongTitle = artistLogin.Attribute("song").Value
            currentSongGID = Utils.GetGlobalIDByName(ClassCode.LoginData, artistLogin.Attribute("song").Value)

            features = artistLogin.Attribute("features").Value
            songHasComments = artistLogin.Attribute("songcomments").Value
            loginVal = Convert.ToDateTime(artistLogin.Attribute("loginTime").Value)
            logoutVal = Convert.ToDateTime(artistLogin.Attribute("logOutTime").Value)
            lengthVal = artistLogin.Attribute("duration").Value
            songVal = artistLogin.Attribute("song").Value
            nameVal = currentArtist



        End If

    End Sub


    Public Sub New(ByRef session As SetupSession, ByRef loginID As Identity.GlobalID) 'called when starting new live session
        MyBase.New()
        artistOBJ = session.Get_Current_Artist
        loginSongList = gatherSongsForThisLoggin()

        m_GlobalID = loginID
        m_LocalID = m_GlobalID.LOCAL_ID
        If (loginSongList Is Nothing) Then
            loginSongList = New List(Of SongContainer)
        End If
        For Each song As SongContainer In loginSongList

            If (song.Song.Has_Features) Then
                features = True
                Exit For
            End If

        Next


        nameVal = artistLogin.Attribute("name").Value

        'MyBase.New()
        'artistOBJ = session.Get_Current_Artist
        'm_GlobalID = loginID
        'm_LocalID = m_GlobalID.LOCAL_ID
        'm_name = "ArtistLogin--" & Now & "--" & session.Get_Current_Artist.My_Name
        'isLive = True
        'currentArtist = session.Get_Current_Artist.My_Name
        'currentSongTitle = session.Get_Current_Song.Title
        'currentSongGID = session.Get_Current_Song.GLOBAL_ID
        'features = session.Get_Current_Song.Has_Features
        'songHasComments = session.Get_Current_Song.Has_Comments
        'nameVal = currentArtist
        'isBounced = session.Get_Current_Song.IsBounced
        'isMixReady = session.Get_Current_Song.MixReady
        'isPriority = session.Get_Current_Song.Is_Priority
        'musicSource = session.Get_Current_Song.Music_Source
        'NeedsFeature = session.Get_Current_Song.Needs_Feature
        'loginVal = Convert.ToDateTime(session.GetResources.Logged_In)





    End Sub

    ''' <summary>
    ''' this is only used to update the logins if needed while debugging
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Async Function UpdateLoginIDs() As Task

        Dim count = 0
        Dim newParent As XElement = New XElement("LoginInfo", New XAttribute("currentUser", "Team Beaker"), New XAttribute("currentSong", ""), New XAttribute("LoginTime", Now), New XAttribute("loggedIn", 0), New XAttribute("count", 0))



        Dim doc = XDocument.Load(Application.XML_PATH & "LoginInfo.xml")
        Dim songDoc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")


        Dim root = doc.Root
        Dim songRoot = songDoc.Root

        For Each artistLogin As XElement In root.Elements

            Dim loging = Convert.ToDateTime(artistLogin.Attribute("loginTime").Value)
            Dim logout As DateTime


            'currentLogin.Attribute("duration").Value = length.Duration.ToString
            'Dim hours As Long = 0
            'Dim mins As Long = 0

            Try
                If (artistLogin.Attribute("logOutTime").Value = "null") Then
                    logout = loging
                Else

                    logout = Convert.ToDateTime(artistLogin.Attribute("logOutTime").Value)
                    ' hours = DateDiff(DateInterval.Hour, loging, logout)

                End If
            Catch ex As Exception
                Application.LemLog(ex.Message, "ERROR!!!")
            End Try

            Dim length As TimeSpan = logout - loging

            Dim sn As XElement = Nothing


            Dim newNode As XElement = New XElement("artistLogin", New XAttribute("name", Utils.removeNumbers(artistLogin.Attribute("name").Value)), New XAttribute("Identity", "UNT-12005-" & count), New XAttribute("loginTime", loging), New XAttribute("logOutTime", logout), New XAttribute("duration", length.ToString))

            Dim song As SongData = Nothing
            Dim foundSong As Boolean = False
            Dim art As Artist = New Artist(artistLogin.Attribute("name").Value, False)
            Dim tempsongname As String = Utils.converToTitleCase(Utils.removeNumbers(artistLogin.Attribute("song").Value))

            If Not (foundSong) Then

                song = New SongData(art, tempsongname)

                Dim result As Boolean = Await song.WaitforInitialization()

                If (result) Then

                Else
                    Throw New Exception

                End If




            End If

            'Dim newNode As XElement = New XElement("artistLogin", New XAttribute("name", Utils.removeNumbers(artistLogin.Attribute("name").Value)), New XAttribute("Identity", "UNT-12005-" & count), New XAttribute("loginTime", loging), New XAttribute("logOutTime", logout), New XAttribute("duration", mins & " minutes"), New XAttribute("features", "False"), New XAttribute("needsFeatures", False), New XAttribute("songcomments", "False"))





            Dim songNode As XElement = New XElement("song", New XAttribute("name", song.Title), New XAttribute("Identity", song.GLOBAL_ID), New XAttribute("opened", loging), New XAttribute("closed", logout), New XAttribute("workDuration", length))


            newNode.Add(songNode)


            newParent.Add(newNode)




            newParent.Attribute("count").Value = count

            If (song.StoreData()) Then
                count = count + 1
            End If



        Next
        root.ReplaceWith(newParent)


        doc.Save(Application.XML_PATH & "LoginInfo.xml")
        SongData.Update_Songs_XML()
        Identity.GlobalID.CommitAllData()
    End Function


    ''' <summary>
    ''' Called During Startup
    ''' </summary>
    ''' <param name="loginNode"></param>
    ''' <param name="glbid"></param>
    ''' <remarks></remarks>
    Public Sub New(ByRef loginNode As XElement, ByVal glbid As String) 'called during startup 
        MyBase.New()

        artistLogin = loginNode
        artistOBJ = New Artist(artistLogin.Attribute("name").Value, False)

        loginVal = Convert.ToDateTime(loginNode.Attribute("loginTime").Value)
        If (loginNode.Attribute("logOutTime").Value = "") Then
            logoutVal = DateTime.Parse("1/1/0001 12:00:00 AM")

        Else
            logoutVal = Convert.ToDateTime(loginNode.Attribute("logOutTime").Value)
        End If


        lengthVal = loginNode.Attribute("duration").Value
        'for backwards compatabilty
        If (loginNode.Attributes.Count = 8) Then



            currentSongTitle = loginNode.Elements.First.Attribute("name").Value
            currentSongGID = loginNode.Elements.First.Attribute("Identity").Value
            'loginSongList = gatherSongsForThisLoggin(currentSongTitle, currentSongGID)
            loginSongList = New List(Of SongContainer)()
            m_LocalID = Utils.ExtracLocalFromGlobal(glbid)
            isBounced = loginNode.Elements.First.Attribute("bounced").Value
            NeedsFeature = Convert.ToBoolean(loginNode.Attribute("needsFeatures").Value)

            features = loginNode.Attribute("features").Value
            songHasComments = loginNode.Attribute("songcomments").Value

            songVal = loginNode.Elements.First.Attribute("name").Value
            nameVal = currentArtist

        ElseIf (loginNode.Attributes.Count = 5) Then



            loginSongList = gatherSongsForThisLoggin()

            m_GlobalID = New GlobalID(glbid, Me)
            m_LocalID = m_GlobalID.LOCAL_ID
            For Each song As SongContainer In loginSongList

                If (song.Song.Has_Features) Then
                    features = True
                    Exit For
                End If

            Next


            nameVal = artistLogin.Attribute("name").Value





        End If

    End Sub


    Private Function gatherSongsForThisLoggin(currentSongTitle As String, currentSongGID As String, opened As Date, closed As Date) As List(Of SongContainer)

        Dim list As List(Of SongContainer) = New List(Of SongContainer)

        Dim artist As Artist = New Artist(currentArtist, False)


        Dim song As SongData = New SongData(artist, currentSongTitle, currentSongGID)

        Dim con As SongContainer = New SongContainer()
        con.Song = song
        con.DateOpened = opened
        con.DateClosed = closed
        list.Add(con)

        Return list


    End Function

    Private Function gatherSongsForThisLoggin() As List(Of SongContainer)
        If (artistLogin Is Nothing) Then
            artistLogin = artistOBJ.Resources.ArtistLoginNode

        End If
        Dim list As List(Of SongContainer) = New List(Of SongContainer)

        For Each node As XElement In artistLogin.Elements

            Dim gid As String = node.Attribute("Identity").Value
            Dim songName As String = Utils.GetNameByGlobalID(gid)
            Dim art As Artist = New Artist(artistLogin.Attribute("name").Value, False)


            If (songName = "") Then


            Else

                Dim song As SongData = New SongData(art, songName, gid, artistOBJ)
                '  song.ReInitialize()
                Dim con As SongContainer = New SongContainer()

                con.DateOpened = DateTime.Parse(node.Attribute("opened").Value)
                con.DateClosed = DateTime.Parse(node.Attribute("closed").Value)
                song.ParentSongContainer = con
                con.Song = song
                list.Add(con)

            End If



        Next

        Return list

    End Function

    Public Enum AppMode
        startUp
        loggedIn
        liveSession
        bounceSession


    End Enum
    Public Enum LoginInfoType
        currentUser
        currentSong
        loginTime
        loggedIn
        mode
        bookedUntil
        count
        newBounceName
        oldBounceName
        bookingDuration
        overTime
    End Enum
    Public Shared Sub UpdateCurrentLoginInfoValue(type As LoginInfoType, val As String)
        Dim info = Utils.getRootXMLNode("LoginInfo.xml")
        Select Case type
            Case LoginInfoType.bookedUntil
                info.Attribute("bookedUntil").Value = val

            Case LoginInfoType.currentSong
                info.Attribute("currentSong").Value = val
            Case LoginInfoType.currentUser
                info.Attribute("currentUser").Value = val
            Case LoginInfoType.loggedIn
                info.Attribute("loggedIn").Value = val
            Case LoginInfoType.loginTime
                info.Attribute("loginTime").Value = val
            Case LoginInfoType.mode
                info.Attribute("mode").Value = val
            Case LoginInfoType.count
                info.Attribute("count").Value = val
            Case LoginInfoType.count
                info.Attribute("count").Value = val
            Case LoginInfoType.newBounceName
                info.Attribute("newBounceName").Value = val
            Case LoginInfoType.oldBounceName
                info.Attribute("oldBounceName").Value = val
            Case LoginInfoType.bookingDuration
                info.Attribute("bookingDuration").Value = val
            Case LoginInfoType.overTime
                info.Attribute("overTime").Value = val
        End Select
        Utils.UpdateLogininfoNode()
    End Sub
    Public Shared Function GetCurrentLoginInfoValue(type As LoginInfoType) As String
        Dim info = Utils.getRootXMLNode("LoginInfo.xml")
        Select Case type
            Case LoginInfoType.bookedUntil
                Return info.Attribute("bookedUntil").Value

            Case LoginInfoType.currentSong
                Return info.Attribute("currentSong").Value
            Case LoginInfoType.currentUser
                Return info.Attribute("currentUser").Value
            Case LoginInfoType.loggedIn
                Return info.Attribute("loggedIn").Value
            Case LoginInfoType.loginTime
                Return info.Attribute("loginTime").Value
            Case LoginInfoType.mode
                Return info.Attribute("mode").Value
            Case LoginInfoType.count
                Return info.Attribute("count").Value
            Case LoginInfoType.oldBounceName
                Return info.Attribute("oldBounceName").Value
            Case LoginInfoType.newBounceName
                Return info.Attribute("newBounceName").Value
            Case LoginInfoType.bookingDuration
                Return info.Attribute("bookingDuration").Value
            Case LoginInfoType.overTime
                Return info.Attribute("overTime").Value

        End Select
        Return ""
    End Function

    Public Shared Function GetAppModeFromString(appModeAsString As String) As AppMode
        If (appModeAsString = "") Then
            Return AppMode.startUp
        End If
        Dim mode As AppMode = [Enum].Parse(GetType(LoginData.AppMode), appModeAsString)
        Return mode
    End Function

    Public Shared Function LoginHistory(Optional dayOfMonth As Integer = -1) As List(Of LoginData)

        Dim lInfo = Utils.getRootXMLNode("LoginInfo.xml")
        'Dim sinfo = Utils.getRootXMLNode("SongInfo.xml")
        Dim result As List(Of LoginData) = New List(Of LoginData)
        Dim counter = 1
        For Each loginnode As XElement In lInfo.Elements

            If Not dayOfMonth = -1 Then
                If Not (DateTime.Parse(loginnode.Attribute("loginTime").Value).Day = dayOfMonth) Then
                    Continue For
                End If
            End If
            If (loginnode.Attribute("logOutTime").Value = "") Then
                'loginnode.Remove()
                'counter += 1
                Continue For
            End If
            Dim ld = New LoginData(loginnode, loginnode.Attribute("Identity").Value)


            'create new ld without updating classcount
            If Not (ld.LogoutTime.Contains("Invalid LOGOUT")) Then

                result.Add(ld)


            Else
                ' loginnode.Remove()
                ' counter += 1


            End If

            Application.ARTIST_RELEVANCE_OBJECT.updateRelevance(ld.artistOBJ)





        Next
        For Each ftnode As XElement In SongData.Get_Global_Features_Node.Elements

            Application.ARTIST_RELEVANCE_OBJECT.updateRelevance(ftnode.Attribute("name").Value, ftnode.Attribute("songs").Value)




        Next
        If counter > 1 Then
            Application.IdentityHasChanged = True

        End If
        Return result

    End Function

    Public Function GetArtist() As Artist
        Return artistOBJ
    End Function

    Public Function GetSongGID()

        Return currentSongGID



    End Function

    Public Shared Function UpdateSongInfoXML() As Boolean

        '\\STUDIOB-PC\work\sessions
        Dim songList As List(Of Utils.songDate) = New List(Of Utils.songDate)




        Dim doc = XDocument.Load(Application.XML_PATH & "Identity.xml")
        Dim loginRoot = Utils.getRootXMLNode("LoginInfo.xml")
        Dim songsDoc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        ' Dim songsRoot = songsDoc.Root.Element("Songs")
        Dim lastmodlist = New Dictionary(Of String, DateTime)

        Dim songCount = 0





        Dim SongInfo = New XElement("SongInfo")
        Dim newSongs = New XElement("Songs", New XAttribute("songCount", 0), New XAttribute("commentCount", 0))
        Dim skip = False
        Dim replace = False

        Dim songobj As SongData
        Dim sd As Utils.songDate = New Utils.songDate
        Dim loginTime As DateTime

        For Each artistLogin As XElement In loginRoot.Elements

            Try



                If (artistLogin.HasAttributes) Then







                    If (songList.Count > 0) Then
                        skip = False
                        replace = False

                        Dim i = 0
                        For Each pair As Utils.songDate In songList
                            Dim xmltime = Convert.ToDateTime(artistLogin.Attribute("loginTime").Value)

                            If (Utils.SpaceSafeCompareSongName(SongData.RemoveMp3FromSongName(pair.song.Title), SongData.RemoveMp3FromSongName(artistLogin.Element("song").Attribute("name").Value))) Then
                                If (Convert.ToDateTime(songList.Item(i).logdate) < xmltime) Then
                                    'datelist greater/newer than xml 
                                    skip = True
                                    replace = True

                                Else
                                    ' dateList.RemoveAt(i)
                                    ' stringList.RemoveAt(i)



                                End If


                            End If
                            If Not (skip) Then
                                i = i + 1
                            End If

                        Next


                    End If



                    If Not (skip) Then


                        Dim atts = artistLogin.Attributes()



                        Dim name = atts.First.Value
                        Dim s_artistName = name

                        Dim artist = New Artist(name, False)

                        Dim identity = "UNT-" & 12004 & "-" & songCount

                        Dim m_LocalID = songCount
                        Dim s_id = m_LocalID

                        loginTime = Convert.ToDateTime(atts.ElementAt(1).Value)
                        Dim s_lastModified = loginTime


                        Dim song = Utils.removeNumbers(SongData.RemoveMp3FromSongName(artistLogin.Attribute("song").Value))

                        songobj = New SongData(artist, song)

                        Dim ready = songobj.WaitforInitialization
                        Dim s_hasFeatures As Boolean
                        Dim s_hasComments As Boolean


                        'sd.song = songobj
                        'sd.logdate = loginTime.ToString



                        songList.Add(sd)







                        'dateList.Add(loginTime)
                        ' Continue For






                        Dim bounced As Boolean

                        If (Convert.ToBoolean(ready)) Then
                            bounced = Utils.CheckIfSongIsBounced(songobj)
                        End If

                        'Dim features = atts.ElementAt(5).Value
                        s_hasFeatures = False

                        ' Dim songcomments = atts.ElementAt(7).Value
                        s_hasComments = False

                        'replace date 

                        If (replace) Then

                            'find songnode and update date att
                            Dim query = From sng As XElement In newSongs.Elements
                                        Where sng.Attribute("Identity").Value = identity
                                        Select sng


                            Dim existingNode = query.First()

                            existingNode.Attribute("lastModified").Value = loginTime






                        Else



                            'add node


                            Dim songnode = New XElement("song", New XAttribute("Identity", identity), New XAttribute("name", song), New XAttribute("mixReady", False), New XAttribute("features", False), New XAttribute("lastModified", loginTime), New XAttribute("artist", name), New XAttribute("bounced", bounced), New XAttribute("comments", False), New XAttribute("priority", False), New XAttribute("needsFeature", False), New XAttribute("musicSource", SetupSession.SessionMusicSource_Type.SoundClick))

                            newSongs.Add(songnode)



                            'write identity


                            Dim root = doc.Root
                            Dim sngs = root.Element("Songs")
                            sngs.Attribute("count").Value = songCount
                            newSongs.Attribute("songCount").Value = songCount
                            sngs.Add(New XElement("Song", New XAttribute("id", songCount), New XAttribute("name", song)))








                            songCount = songCount + 1





                            'end

                        End If






                        'end skip
                    End If

                End If

            Catch ex As Exception
                Dim app As Application = Application.Current
                Dim lemlog = New Application.LemLogDel(AddressOf Application.LemLog)
                app.Dispatcher.BeginInvoke(lemlog, {ex.Message, "ERROR!!!"})

                'Application.LemLog(ex.Message, "ERROR!!!")
                ' Throw New Exception
            End Try
        Next



        SongInfo.Add(newSongs)
        SongInfo.Save(Application.XML_PATH & "SongInfo.xml")
        doc.Save(Application.XML_PATH & "Identity.xml")


        Return True
    End Function


    'Public Function StoreLoginData() As Boolean

    '    ' Dim db2 As String = Application.XML_PATH & "SongInfo.xml"
    '    Dim success As Int32 = 0


    '    Dim loginNode As XElement = New XElement("artistLogin", New XAttribute("name", Name), New XAttribute("Identity", GLOBAL_ID), New XAttribute("loginTime", LoginTime), New XAttribute("logOutTime", LogoutTime), New XAttribute("duration", Duration), New XAttribute("features", features), New XAttribute("needsFeatures", liveSession.Get_Current_Song.Needs_Feature), New XAttribute("songcomments", liveSession.Get_Current_Song.Has_Comments))

    '    If (liveSession.Get_Current_Song.StoreData(loginNode)) Then
    '        loginHandle.Save(Application.XML_PATH & "LoginInfo.xml")

    '    End If




    '    'doc = XDocument.Load(db2)

    '    'root = doc.Root

    '    'root = m_Session.Song_Object.StoreSongNode(root)

    '    'doc.Save(db2)





    '    Return True




    'End Function




    Public Shared Property CurrentID As Int32
        Get

            Return Convert.ToInt32(Utils.getRootXMLNode("LoginInfo.xml").Attribute("count").Value)
        End Get
        Set(value As Int32)

        End Set

    End Property

    Public Shared Property CurrentUser As String
        Get
            Return Utils.getRootXMLNode("LoginInfo.xml").Attribute("currentUser").Value

        End Get
        Set(value As String)

        End Set
    End Property

    Public Shared ReadOnly Property CurrentSong As String
        Get
            Return Utils.getRootXMLNode("LoginInfo.xml").Attribute("currentSong").Value
        End Get

    End Property

    Public ReadOnly Property LoginSongCollection As List(Of SongContainer)
        Get
            Return loginSongList
        End Get
    End Property



    Public Shared Property CurrentLoginTime As DateTime
        Get
            Return Convert.ToDateTime(Utils.getRootXMLNode("LoginInfo.xml").Attribute("loginTime").Value)

        End Get
        Set(value As DateTime)

        End Set
    End Property

    Protected ReadOnly Property GetGlobalID As Identity.GlobalID
        Get
            Return m_GlobalID
        End Get
    End Property

    Public Overrides ReadOnly Property GLOBAL_ID As String
        Get
            Return ""
        End Get
    End Property








    Property ID As Int32
        Get
            Return m_LocalID
        End Get
        Set(value As Int32)

        End Set
    End Property

    Property Needs_Features
        Get
            If NeedsFeature Then
                Return "Yes"
            End If
            Return "No"
        End Get
        Set(value)
            NeedsFeature = value
        End Set
    End Property

    Property Artist As String
        Get
            Return nameVal
        End Get
        Set(value As String)
            nameVal = value
        End Set
    End Property


    Property LoginTime As DateTime
        Get
            Return loginVal
        End Get
        Set(value As DateTime)
            loginVal = value
        End Set
    End Property



    Property LogoutTime As String
        Get
            If (logoutVal = Convert.ToDateTime("1/1/0001 12:00:00 AM")) Then
                Return "   Invalid LOGOUT"
            End If
            Return logoutVal
        End Get
        Set(value As String)

            logoutVal = value
        End Set
    End Property





    Property Duration As String
        Get
            Return lengthVal
        End Get
        Set(value As String)
            lengthVal = value
        End Set
    End Property

    Property LoginString As String
        Get

            If (LogoutTime = "   Invalid LOGOUT") Then
                Return String.Format("{2}, {1} From {0}. InvalidLogout", DateTime.Parse(LoginTime).ToShortTimeString, DateTime.Parse(LoginTime).Date.ToShortDateString, DateTime.Parse(LoginTime).DayOfWeek.ToString())
            End If

            Return String.Format("{4}, {3} From {0} to {1} ({2})", DateTime.Parse(LoginTime).ToShortTimeString, DateTime.Parse(LogoutTime).ToShortTimeString, Duration, DateTime.Parse(LoginTime).Date.ToShortDateString, DateTime.Parse(LoginTime).DayOfWeek.ToString())



        End Get
        Set(value As String)

        End Set
    End Property

    Public Overrides Function ToString() As String
        Return songVal & " By " & currentArtist

    End Function










    Public Function CompareTo(other As LoginData) As Integer Implements IComparable(Of LoginData).CompareTo

        If (m_LocalID > other.m_LocalID) Then
            Return -1
        ElseIf (m_LocalID < other.m_LocalID) Then
            Return 1
        Else
            Return 0
        End If

    End Function

    Shared Function GetGlobalXMLResourceObject() As XElement

        Return XMLResources.GetLoginXMLResourceRoot


    End Function




End Class

'Class SessionList
'    Inherits List(Of LoginData)

'    Public Shared Function Load(ByRef Artist_Name As String) As SessionList

'        Dim list As SessionList = New SessionList
'        Dim doc = XDocument.Load("//STUDIOB-PC/Users/studioB/Documents/Visual Studio 2012/Projects/SessionStarter/SessionStarter/bin/Debug/xml/LoginInfo.xml")
'        Dim root As XElement = doc.Root
'        For Each node As XElement In root.Elements()

'            Dim data As LoginData = New LoginData(Artist_Name)
'            data.Name = node.Attribute("name").Value
'            data.Song = New SongData(removeNumbers(node.Attribute("song").Value))
'            Data.Id = Convert.ToInt32(node.Attribute("id").Value)
'            Data.LoginTime = Convert.ToDateTime(node.Attribute("loginTime").Value)
'            Data.LogoutTime = Convert.ToDateTime(node.Attribute("loginTime").Value)

'            If Not node.Attribute("logOutTime").Value = "null" Then
'                Data.LogoutTime = Convert.ToDateTime(node.Attribute("logOutTime").Value)


'            End If

'            list.Add(Data)



'        Next

'        Return list


'    End Function












''' <summary>
''' Use this to Manage All LoginInfo Nodes and general xml Resources
''' </summary>
''' <remarks></remarks>


Public MustInherit Class XMLBase



    Protected Shared resourcesPath As String
    Protected Shared handle As XDocument
    Protected Shared root As XElement
    Protected Shared rootCount As XAttribute

    Protected Shared Parent As XElement
    Protected Shared enumerator As IEnumerator(Of XAttribute)


    Protected localNode As XElement

    Public Sub New()

    End Sub

    Protected Overridable Sub Init()
        If (handle Is Nothing) Then
            handle = XDocument.Load(resourcesPath)
            root = handle.Root
        End If

        'rootCount = root.Attribute("count")

    End Sub

    Protected Overridable Sub Submit()

        If Application.LEMTESTING Then
            Return
        End If
        Try
            handle.Save(resourcesPath)
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try


    End Sub

    Protected Overridable Function updateCount(Optional ByVal up As Boolean = True) As Int32

        Dim count = Convert.ToInt32(rootCount.Value)
        If (up) Then
            count = count + 1
        Else
            count = count - 1
        End If

        rootCount.Value = count
        Return count
    End Function


End Class


















Public Class SongInfoXML
    Inherits XMLBase


    Private s_ParentCount As XAttribute
    Private s_CommentCount As XAttribute
    Private s_featureInfo As XElement

    Private Shared IdList As List(Of Int32) = New List(Of Integer)







    Public Sub New()

        If (handle Is Nothing) Then

            resourcesPath = Application.XML_PATH & "SongInfo.xml"
            handle = XDocument.Load(resourcesPath)
            root = handle.Root





            Parent = root.Element("Songs")
            rootCount = Parent.Attribute("songCount")
            s_ParentCount = rootCount
            s_CommentCount = Parent.Attribute("commentCount")

            If (root.Elements.Count > 1) Then
                s_featureInfo = root.Element("FeatureInfo")
            Else
                s_featureInfo = New XElement("FeatureInfo", New XAttribute("count", 1))

            End If

        End If

    End Sub


    Protected Overrides Sub Init()

        MyBase.Init()


    End Sub

    Public Sub UpdateData()

        If (Application.LEMTESTING) Then
            Return
        End If


        Submit()


    End Sub




    'Public Overrides Function Add_Data(ByRef Identity As String, ByRef name As String, ByRef mixReady As Boolean, ByRef features As Boolean, ByRef lastModified As DateTime, ByRef artist As String, ByRef bounced As Boolean, ByRef comments As Boolean) As Boolean

    '    Parent .Add(New XElement ("song",New XAttribute ("Identity",

    'End Function

    

    Public Function GetFeatureNodeForSong(songLocalID As Integer) As XElement
        For Each node As XElement In Parent.Elements
            If (node.Attribute("Identity").Value = "UNT-12004-" & songLocalID.ToString()) Then
                If (node.HasElements) Then
                    For Each el As XElement In node.Elements
                        If el.Name = "Features" Then
                            Return el
                        End If
                    Next
                End If

                Exit For
            End If
        Next
        Return Nothing
    End Function

    Public Overloads Function Get_Song_Node(ByRef ID As Int32,
                                            ByRef Identity As String,
                                            ByRef Title As String,
                                            ByRef mixReady As Boolean,
                                            ByRef features As Boolean,
                                            ByRef lastModified As DateTime,
                                            ByRef artist As String,
                                            ByRef bounced As Boolean,
                                            ByRef comments As Boolean,
                                            ByRef priority As Boolean,
                                            ByRef mp3 As String,
                                            ByRef featureRequests As Boolean,
                                            ByRef needsFeature As Int32,
                                            ByRef musicSource As String,
                                            ByRef trackInfoId As Int32,
                                            Optional ByVal updateThisNode As Boolean = False,
                                            Optional detachedFromSongInfo As Boolean = False) As XElement




        Dim songNode As XElement = Nothing
        Dim newsongNode = New XElement("song",
                                       New XAttribute("Identity", Identity),
                                       New XAttribute("name", Title),
                                       New XAttribute("mixReady", mixReady),
                                       New XAttribute("features", features),
                                       New XAttribute("lastModified", Now),
                                       New XAttribute("artist", artist),
                                       New XAttribute("bounced", bounced),
                                       New XAttribute("comments", comments),
                                       New XAttribute("priority", priority),
                                       New XAttribute("mp3", mp3),
                                       New XAttribute("needsFeature", 0),
                                       New XAttribute("musicSource", musicSource),
                                       New XAttribute("trackInfoId", trackInfoId))

        If Not detachedFromSongInfo Then
            'check xml for id 
            For Each node As XElement In Parent.Elements
                If (node.Attribute("Identity").Value = "UNT-12004-" & ID.ToString()) Then
                    songNode = node

                    Exit For
                End If
            Next

            If Not (songNode Is Nothing) Then
                'got it
                If (updateThisNode) Then
                    songNode.Attribute("name").Value = Title
                    songNode.Attribute("mixReady").Value = mixReady
                    songNode.Attribute("features").Value = features
                    songNode.Attribute("lastModified").Value = lastModified
                    songNode.Attribute("artist").Value = artist
                    songNode.Attribute("bounced").Value = bounced
                    songNode.Attribute("priority").Value = priority
                    songNode.Attribute("comments").Value = comments
                    songNode.Attribute("needsFeature").Value = needsFeature
                    songNode.Attribute("mp3").Value = mp3
                    songNode.Attribute("musicSource").Value = musicSource
                    Application.IdentityHasChanged = True
                    'Submit()

                End If

            Else


                songNode = newsongNode
                Parent.Add(songNode)
                updateCount(True)

                'If Not (songNode.Attribute("name").Value.Contains("Unfinishedsong_")) Then

                'End If



                'Submit()
                Application.IdentityHasChanged = True
                Return songNode

            End If
        Else



            Return newsongNode
        End If

        'create new xmlnode/add to parent and add to list
        Return songNode

    End Function

    Public Function Get_Feature_Info_Node() As XElement

        Return s_featureInfo

    End Function

    Public Shared Function Get_Root() As XElement

        If (handle Is Nothing) Then
            resourcesPath = Application.XML_PATH & "SongInfo.xml"
            handle = XDocument.Load(resourcesPath)
            root = handle.Root
        End If
        Return root
    End Function

    'Private Function IncrementSongCount() As Int32



    'End Function



End Class
























Public Class Utils
    'Private Sub closeSession()

    'If Not (Application.CurrentState = Application.sessionState.running) Then
    '    Return
    'End If
    ''Me.Controls.Remove(openSessionButton)
    'Application.CurrentState = Application.sessionState.closing

    'Dim nameOfTrack As String = ""
    '' Dim featuredArtists As String = ""
    '' Dim engineeredBy As String = ""
    'Dim bouncefolderName As String = ""
    'Dim finalPath As String = ""
    'Dim songName As String = ""
    'If (Application.selectedSong.Title.Contains(". ")) Then
    '    Dim cleanSelectedSongNameSplit = Split(Application.selectedSong.Title, ". ")
    '    Application.selectedSong = cleanSelectedSongNameSplit.GetValue(1)
    'End If



    'Select (ShowDialog("Close Session", "What's the hook gon be?", converToTitleCase(selectedSong), songName, "", "", False, artistName, featuredArtists))
    'Select Case ()

    '        Case 1


    '            nameOfTrack = songName
    '            'featuredArtists = converToTitleCase(CloseSessionDialog.FeaturingDDList.Text)

    '            For Each folder As String In My.Computer.FileSystem.GetDirectories(MP3_PATH, FileIO.SearchOption.SearchAllSubDirectories, artistName)

    '                If Not (My.Computer.FileSystem.GetParentPath(folder).Contains("Archives")) Then
    '                    bouncefolderName = folder
    '                End If
    '            Next


    '            'TRY TO MATCH NAME OF TRACK WITH SOMETHING IN ARTIST MP3 FOLDER




    '            If (bouncefolderName = "") Then
    '                bouncefolderName = MP3_PATH & "outhouse\" & artistName

    '                'MsgBox(bouncefolderName)
    '                Directory.CreateDirectory(bouncefolderName
    '                                           )

    '            Else

    '                Dim existingFile As String = ""
    '                Dim existingFileName As String = ""
    '                Dim currentDateStamp As Date = New Date()


    '                For Each songFile As String In My.Computer.FileSystem.GetFiles(bouncefolderName)


    '                    'remove spaces
    '                    Dim convertedSongName As String = Replace(Path.GetFileName(songFile).ToUpper(), " ", "")
    '                    Dim convertedChosenName As String = Replace(nameOfTrack.ToUpper(), " ", "")
    '                    If (convertedSongName.Contains(convertedChosenName)) Then
    '                        If (My.Computer.FileSystem.GetFileInfo(songFile).LastWriteTime > currentDateStamp) Then
    '                            currentDateStamp = My.Computer.FileSystem.GetFileInfo(songFile).LastWriteTime
    '                            existingFile = songFile
    '                            existingFileName = Path.GetFileName(songFile)
    '                        End If

    '                    End If

    '                Next


    '                If Not (existingFile = "") Then
    '                    Select Case (
    '                    MsgBox("should I replace the song titled " & existingFileName, MsgBoxStyle.YesNo, "Song May Already in DataBase!"))
    '                        Case MsgBoxResult.Yes
    '                            nameOfTrack = converToTitleCase(existingFileName)
    '                            finalPath = existingFile



    '                        Case MsgBoxResult.No

    '                            finalPath = bouncefolderName & "\" & songName & ".mp3"

    '                    End Select
    '                Else

    '                    finalPath = bouncefolderName & "\" & songName & ".mp3"





    '                End If
    '            End If






    '            While Not (Clipboard.GetText = finalPath)
    '                Process.Start(Application.BOUNCE_SESSION_SCRIPT)

    '                Try
    '                    Clipboard.SetText(finalPath, TextDataFormat.Text)
    '                Catch ex As Exception
    '                    Dim processes As Process() = Process.GetProcessesByName("bouncesession")
    '                    For Each process As Process In processes
    '                        process.Kill()

    '                    Next
    '                    'MsgBox("clipboard function failed")
    '                    System.Threading.Thread.Sleep(500)
    '                End Try
    '            End While






    '            Application._List_Mode = LISTMODE_ARTIST
    '            ' openSessionButton.BackColor = Color.SeaShell

    '            aTimer = New System.Timers.Timer(85000)
    '            AddHandler aTimer.Elapsed, sessionEndedHandler
    '            aTimer.Start()


    '            writeSongFolderName(nameOfTrack)
    '            Dim features As Boolean = featuredArtists.Count > 0


    '            ' Utils.XMLUtils.updateSongInfo(nameOfTrack, features, True, MIX_READY, featuredArtists)
    '            Utils.XMLUtils.updateLoginInfo(artistName, nameOfTrack, False)


















    '        Case 3
    '            currentState = sessionState.closing
    '            'If Not (songName = "TYPICAL NIGGAZ ENT PRESENTZ ANOTHER TURN THAT SHIT DOWN PRODUCTION") Then
    '            selectedSong = songName
    '            'End If
    '            'writeSongFolderName(converToTitleCase(selectedSong))
    '            If (selectedSong.ToUpper.Contains("UNFINISHEDSONG_")) Then
    '                Dim newName As String = InputBox("Name This???", "Should I Name this Session", selectedSong)
    '                ' If name was changed
    '                If (Not newName = "" And Not newName = selectedSong) Then
    '                    selectedSong = newName
    '                    writeSongFolderName(converToTitleCase(selectedSong))
    '                    killAllMacros()

    '                    closeSessionQuick()
    '                    'Dim mb As MessageBoxEx = New MessageBoxEx()


    '                    'renameDirectory()
    '                    System.Threading.Thread.Sleep(3000)
    '                    updateLoginInfo(artistName, selectedSong, False)
    '                    updateSongInfo(nameOfTrack, False)
    '                    Me.Close()
    '                    ' login(True)
    '                Else

    '                    closeSessionQuick()
    '                    updateLoginInfo(artistName, selectedSong, False)
    '                    updateSongInfo(nameOfTrack, False)
    '                    Me.Close()
    '                    currentState = sessionState.closed
    '                    System.Threading.Thread.Sleep(3000)
    '                    ' login(True)
    '                End If


    '            Else
    '                writeSongFolderName(converToTitleCase(selectedSong))
    '                updateLoginInfo(artistName, cleanSongName(nameOfTrack), False)
    '                updateSongInfo(cleanSongName(nameOfTrack), False)
    '                closeSessionQuick()
    '                Me.Close()
    '                currentState = sessionState.closed
    '                System.Threading.Thread.Sleep(3000)
    '                ' login(True)
    '            End If

    '            selectedSong = ""
    '            Application._Session_Count += 1



    '        Case 4
    '            ' openSessionButton.Text = "CLOSE SESSION"
    '            currentState = sessionState.running



    '            Return



    '    End Select


    ' updateLoginInfo(artistName, selectedSong, False, featuredArtists)



    'End Sub

    Private Shared s_artistName As String
    Private Shared m_LocalID As Integer
    Private Shared s_id As Integer
    Private Shared s_lastModified As Date
    Private Shared s_hasFeatures As Boolean
    Private Shared s_hasComments As Boolean

    Private Shared usr32 As User32 = New User32()


    ' Private Shared dateList As List(Of Date) = New List(Of DateTime)

    Private Sub renameDirectory()

        ' Process.Start(Application.DOUBLE_CHECK_LAST_SONGFOLDER_RENAME)


    End Sub

    Structure songDate
        Dim song As SongData
        Dim logdate As String

    End Structure
    Public Shared Function Get_Features_Count_Fresh(songLocalID As Integer) As Integer
        Dim songXML = Application.SONG_INFO_XML
        Dim featureInfoNode As XElement = songXML.GetFeatureNodeForSong(songLocalID)
        Dim count = 0
        If Not (featureInfoNode Is Nothing) Then
            count = Convert.ToInt16(featureInfoNode.Attribute("count").Value)
        End If
        Return count
    End Function
    Public Shared Function Get_Features_Fresh(song As SongData) As List(Of SongData.FeatureInfo)

        Dim result = New List(Of SongData.FeatureInfo)

        Dim songXML = Application.SONG_INFO_XML
        Dim featureInfoNode As XElement = songXML.GetFeatureNodeForSong(song.LOCAL_ID)
        If (featureInfoNode Is Nothing) Then
            Return result
        End If
        Dim songNode = featureInfoNode.Parent
        If Not (featureInfoNode Is Nothing) Then
            If featureInfoNode.HasElements Then
                For Each feat As XElement In featureInfoNode.Elements
                    Dim art = New Artist(feat.Attribute("name").Value, False)

                    result.Add(New SongData.FeatureInfo(art, song))


                Next

            End If
        End If


        Return result
    End Function
    Public Shared Function Get_Time_Duration(ByVal DateFrom As String, ByVal DateTo As String, Optional minsonly As Boolean = False) As String

        Dim dFrom As DateTime = New DateTime
        Dim dTo As DateTime = New DateTime
        If (DateTime.TryParse(DateFrom, dFrom) And DateTime.TryParse(DateTo, dTo)) Then
            Dim ts As TimeSpan = dTo.Subtract(dFrom)
            Dim hour As Int32 = ts.Hours
            Dim mins As Int32 = ts.Minutes
            Dim secs As Int32 = ts.Seconds

            ' Dim dur = Convert.ToDateTime(ts.ToString).ToShortTimeString
            If (minsonly) Then
                Return ts.Duration.Minutes.ToString() + " mins"
            End If

            Return hour.ToString("00") & "hrs and " & mins.ToString("00") & "mins"
        End If

        Return "not valid"


    End Function



    Public Shared Function removeSpaces(ByVal line As String) As String
        Dim tempArray = line.Split(" ")


        line = Join(tempArray, "")


        Return line
    End Function

    Private Function nameComparator(ByVal name1 As String, ByVal name2 As String) As Boolean
        name1 = removeSpaces(name1.ToLower)
        name2 = removeSpaces(name2.ToLower)
        Dim isTheSame As Boolean = name1 = name2

        Return isTheSame


    End Function
    Public Shared Function Find_Artist_Bounce_Directory(ByVal artist As Artist, Optional ByVal archives As Boolean = False) As String

        Dim result = Application.MP3_PATH & "\Outhouse\" & artist.My_Name

        'search and find artist bounce folder
        For Each artistfolder As String In My.Computer.FileSystem.GetDirectories(Application.MP3_PATH, FileIO.SearchOption.SearchAllSubDirectories, artist.My_Name)

            'found it now if we dont want any folders from archives
            If Not (archives) Then
                'make sure its not in the archives section
                If Not (My.Computer.FileSystem.GetParentPath(artistfolder).Contains("Archives")) Then



                    result = artistfolder
                    'found it now im leavin
                    Return result
                End If
                'skip if cuz 
                'there is no such folder with this artist name
                Directory.CreateDirectory(result)
                'created
            End If

        Next



        'return newly created folder name
        Return result
    End Function

    'Private Function CheckIfSongIsBounced(ByVal songName As String) As Boolean

    '    If s_artistOBJ.MP3Path Is Nothing Then
    '        If Not s_artistOBJ.gatherArtistStatus Then
    '            Return False
    '        End If
    '    End If

    '    Dim songs() = Directory.GetFiles(s_artistOBJ.MP3Path)



    '    For Each song As String In songs


    '        song = System.IO.Path.GetFileNameWithoutExtension(song)
    '        song = Utils.removeFeatureText(song)
    '        If (songName = song) Then
    '            Return True
    '        End If

    '    Next
    '    Return False
    'End Function

    Public Shared Function CheckIfSongIsBounced(_song As SongData) As Boolean


        Dim songs() = Directory.GetFiles(_song.MP3_Location)



        For Each song As String In songs


            Dim sng1 = System.IO.Path.GetFileNameWithoutExtension(song)
            ' song = Utils.removeFeatureText(song)
            If (Utils.addFeatureText(_song.Title, _song.Get_Features_List) = sng1) Then
                Return True
            End If

        Next
        Return False


    End Function

    Public Shared Function CheckIfWindowOpen(windowname As String) As Boolean

        If (usr32.Find(Nothing, windowname) = 0) Then
            Return False
        End If

        Return True
    End Function
    Public Shared Sub writeSongFolderName(ByVal artistSongRecordingSongPath As String, ByVal songTitle As String, Optional ByVal Now As Boolean = False)


        Dim folderNameFile As String = "C:\windows\names.txt"
        ' System.IO.File.Create(folderNameFile)

        If Not System.IO.File.Exists(folderNameFile) Then

            System.IO.File.Create(folderNameFile)

        Else
            ' MsgBox("Problem renaming folder, check to make sure Documents\Visual Studio 2012\Projects\UpdateSongFolder\UpdateSongFolder\names.txt Exists")
        End If

        If Not (Now) Then
            Dim objWriter As New System.IO.StreamWriter(folderNameFile)

            objWriter.Write(artistSongRecordingSongPath & "%" & songTitle)
            objWriter.Close()
            Application.TEMP_SESSION_FOLDER_PATH_OLD = artistSongRecordingSongPath
            Application.TEMP_SESSION_FOLDER_NAME_NEW = songTitle

        Else
            If (My.Computer.FileSystem.DirectoryExists(artistSongRecordingSongPath)) Then
                Try
                    Dim name = My.Computer.FileSystem.GetName(artistSongRecordingSongPath)
                    Dim name2 = New DirectoryInfo(artistSongRecordingSongPath).Name

                    If Not name2 = songTitle Then
                        Dim newfoldername As String = Path.Combine(Directory.GetParent(artistSongRecordingSongPath).FullName, songTitle)

                        If (Directory.Exists(newfoldername)) Then
                            Dim count = 0
                            For Each fname As String In Directory.GetDirectories(Directory.GetParent(newfoldername).FullName)
                                If (fname.Contains(songTitle)) Then
                                    count += 1
                                End If
                            Next
                            newfoldername &= "_" & count.ToString()
                            songTitle = New DirectoryInfo(newfoldername).Name

                        End If
                        My.Computer.FileSystem.RenameDirectory(artistSongRecordingSongPath, songTitle)
                    End If
                   
                Catch ex As Exception
                    Application.LemLog(ex.Message, "ERROR!!!")
                    TEST_MESS("Problem Renaming folder_Utils.writeSongFolderName   " + ex.Message)
                    Application.LEM_ERR("Folder was not renamed properly for some reason")
                End Try



            End If



        End If

    End Sub

    Private Shared Sub TEST_MESS(message As String)
        If (Application.UseTEST_MESS) Then
            MsgBox(message)
        End If
    End Sub
    Protected Const m_Base As String = "UNT"



    Public Shared Function GetGlobalCount(ByRef Cclass As Identity.ClassCode) As Int32
        Dim rootSongsNode = Utils.getRootXMLNode("Identity.xml", Identity.GetClassCodeType(Cclass) & "s")
        Return Convert.ToInt32(rootSongsNode.Attribute("count").Value)
    End Function

    Public Shared Function GetObjectByID(ByVal GlobalID As String) As Identity


        Dim cc = Utils.ExtractClassCodeFromGlobalID(GlobalID)

        Dim obj As Identity = New Identity(cc)
        'UNTCo-12001-12



        Dim root = Utils.getRootXMLNode("Identity.xml", Identity.GetClassCodeType(cc) & "s")

        'obj.CreateFromLocalId(GlobalID)


        Return obj
    End Function

    Public Shared Function CleanFolderPath(ByVal path As String) As String

        Dim arr = Split(path, "\")

        Return SongData.RemoveMp3FromSongName(arr.GetValue(arr.Count - 1))


    End Function
    'strip backslashes and get arr count minus 1
    'add songs to identity
    Public Shared Function GetGlobalIDByName(cc As Identity.ClassCode, ByRef ObjectName As String) As String
        Dim result As String = "N/A"
        Dim root = Utils.getRootXMLNode("Identity.xml")
        For Each node As XElement In root.Element(Identity.GetClassCodeType(cc) & "s").Elements

            If node.Attribute("name").Value = ObjectName Then
                result = "UNT-" & cc & "-" & node.Attribute("id").Value

                Exit For
            End If

        Next
        Return result
    End Function


    Public Shared Function GetNameByGlobalID(ByRef gid As String) As String

        If (gid.Contains("UNT-")) Then
            Return Identity.GlobalID.FindNameByGlobalID(gid)
        End If

        Return String.Empty
    End Function

    'Private Shared LOGIN_INFO_NODE As XElement
    'Private Shared LOGININFODOC As XDocument
    Public Shared Sub UpdateLogininfoNode()
       
        XMLResources.QuickUpdateLoginNode()

    End Sub


    Public Shared Function getRootXMLNode(ByVal xmlName As String, Optional ByVal nodeName As String = "null") As XElement
        Dim isloginnode = False
        If (nodeName = "null") Then
            If (xmlName.Contains("LoginInfo")) Then
                isloginnode = True
               
            End If

            Dim root = XMLResources.GetLoginRoot()
            
            Return root
        Else

            Dim root = XMLResources.GetLoginRoot()
            Dim finalNode = root.Element(nodeName)
            Return finalNode
        End If



    End Function
    Public Shared Function SaveToXML(ByRef newNode As XElement, ByVal xmlName As String, Optional ByVal AppendingNode As String = Nothing) As Int32

        If Not (AppendingNode = Nothing) Then
            Dim doc = XDocument.Load(Application.XML_PATH & xmlName)
            Dim root = doc.Root
            Dim nodeToAppendto = root.Element(AppendingNode)
            nodeToAppendto.Add(newNode)
            doc.Save(Application.XML_PATH & xmlName)
            Return 1
        Else
            Dim doc = XDocument.Load(Application.XML_PATH & xmlName)
            Dim root = doc.Root
            root.Add(newNode)
            doc.Save(Application.XML_PATH & xmlName)
        End If

        Return 0


    End Function

    Private Shared Function removeAllNonStandardCharacters(ByRef teststring As String) As String
        teststring = teststring.ToLower
        teststring = teststring.Replace(" ", "")
        teststring = teststring.Replace(",", "")
        teststring = teststring.Replace("'", "")
        teststring = teststring.Replace("-", "")
        teststring = teststring.Replace("-feat", "")
        teststring = teststring.Replace("unfinishedsong_", "")
        teststring = teststring.Replace("(", "")
        teststring = teststring.Replace(")", "")
        teststring = teststring.Replace(".", "")

        teststring = teststring.Replace(".mp3", "")

        Return teststring
    End Function

    Public Shared Function SpaceSafeCompareSongName(ByVal firstname As String, ByVal secondname As String) As Boolean

        Dim first = removeAllNonStandardCharacters(firstname)
        Dim second = removeAllNonStandardCharacters(secondname)


        If (first = second Or first.Contains(second) Or second.Contains(first)) Then
            Return True
        End If

        Return False

    End Function

    ''' <summary>
    ''' REturns  the path of a wave file in the provided artist's recording path that was
    ''' duplicated from a soundclick beat or and empty string
    ''' </summary>
    ''' <param name="ArtistRecordingPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CheckForUsedSoundClickBeat(ArtistRecordingPath As String) As String



        If Not (My.Computer.FileSystem.DirectoryExists(ArtistRecordingPath)) Then
            Return "STOPLOOKINGFORTHIS"
        End If

        For Each folder As String In Directory.GetDirectories(ArtistRecordingPath)

            Dim temp As String = Path.GetFileNameWithoutExtension(folder)
            Dim waveList As List(Of FileInfo) = New List(Of FileInfo)

            If (temp = "Audio") Then

                Dim beatinfotxt = folder & "\beatinfo.txt"
                If (File.Exists(beatinfotxt)) Then
                    Using reader As StreamReader = File.OpenText(beatinfotxt)
                        Dim val = reader.ReadToEnd()
                        Dim app As Application = Application.Current
                        Dim logdel = New Application.LemLogDel(AddressOf Application.LemLog)
                        app.Dispatcher.BeginInvoke(logdel, {Path.GetFileNameWithoutExtension(val) & " was used in " & ArtistRecordingPath})

                        Return val

                    End Using
                End If

                Dim collexon = My.Computer.FileSystem.GetFiles(folder)

                For Each wav As String In My.Computer.FileSystem.GetFiles(folder)
                    If (Path.GetFileNameWithoutExtension(wav).Length() > 35 Or New FileInfo(wav).Length > 80000000) Then
                        If (wav.Contains("artist_EAR_2_THA_BEAT_This_is_EAR2THABEAT_Your_Class_A_Hitfactory_ear2thabeat_crazyinforthekillfreedl")) Then
                            Continue For
                        End If
                        If (Path.GetExtension(wav).ToLower = ".wav") Then
                            waveList.Add(New FileInfo(wav))

                        End If

                    End If
                Next

            End If






            If (waveList.Count > 0) Then

                'look for soundclick beat
                Dim finalList As Dictionary(Of FileInfo, String) = New Dictionary(Of FileInfo, String)

                For Each wav As FileInfo In waveList

                    For Each mp3 As String In My.Computer.FileSystem.GetFiles(Application.INTERNET_BEATS_DIRECTORY)
                        If (Path.GetFileNameWithoutExtension(mp3) = Path.GetFileNameWithoutExtension(wav.FullName)) Then
                            finalList.Add(wav, mp3)

                        Else


                        End If
                    Next

                Next


                If Not (finalList.Count > 0) Then

                    'Nothing to return

                    Return ""
                End If

                Dim finalMP3 As String = ""
                Dim latesInfo As KeyValuePair(Of FileInfo, String) = Nothing
                Dim count = 0
                For Each info As KeyValuePair(Of FileInfo, String) In finalList
                    If (count = 0) Then
                        latesInfo = info
                    Else
                        If (info.Key.LastAccessTime > latesInfo.Key.LastAccessTime) Then
                            latesInfo = info
                        End If
                    End If

                Next

                finalMP3 = latesInfo.Value
                Application.LemLog(Path.GetFileNameWithoutExtension(finalMP3) & " was used in " & ArtistRecordingPath)
                Return finalMP3

            End If




        Next
        Return ""
    End Function


    Public Shared Function QuickCheckIfArtistHasSessions(art As Artist) As Boolean
        ' Dim root As XElement = SongInfoXML.Get_Root
        If Directory.Exists(art.RecordingPath) Then
            If Directory.EnumerateDirectories(art.RecordingPath).Count > 0 Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Shared Function GetSongSessionList(Artist As Artist) As List(Of String)

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
        Dim songList As List(Of String) = Directory.GetDirectories(
                                                                                                      Artist.RecordingPath).ToList





        'songList = sortSongList(songList)

        For Each songFolder As String In songList
           
            If songFolder.ToLower().Contains(".mp3") Or songFolder.ToLower.Contains(".wav") Then
                Dim newName = My.Computer.FileSystem.GetName(songFolder).Replace("'", "").Split(".").GetValue(0)

                If (Directory.Exists(Directory.GetParent(songFolder).FullName & "\" & newName)) Then
                    newName = songFolder = Directory.GetParent(songFolder).FullName & "\" & newName & "-" & DateTime.Now.Ticks
                    
              

                End If
                My.Computer.FileSystem.RenameDirectory(songFolder, newName)
                songFolder = Directory.GetParent(songFolder).FullName & "\" & newName
            End If

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

    Public Shared Function CombineNames(ByRef names As List(Of String)) As String


        Dim tempText As String = ""


        If (names.Count > 0) Then




            If (names.Count = 1 And names.Item(0) = "") Then
                Return ""
            End If

            If (names.Item(0) = "" And names.Count > 0) Then
                names.RemoveAt(0)
            End If

            Select Case names.Count
                Case 1


                    tempText += names.Item(0)

                Case 2
                    tempText += names.Item(0) & " and " & names.Item(1)
                Case 0
                    Return ""
                Case Else
                    Dim count As Integer = names.Count
                    For frontBatch As Integer = 0 To count - 3
                        tempText += converToTitleCase(names(frontBatch) & ",", True)
                    Next
                    tempText += converToTitleCase(names(count - 2), True)
                    tempText += " and " & converToTitleCase(names(count - 1), True)

            End Select
        End If
        Return tempText
    End Function

    Public Shared Function converToTitleCase(ByRef word As String, Optional ByRef keepSpaces As Boolean = True)
        If (word.StartsWith(" ")) Then
            word = word.Remove(0, 1)
        End If
        Dim pathArray() As String = Split(word.ToLower, " ")
        Dim i As Integer = 0
        For Each splitWord In pathArray
            'If Not (splitWord = "") Then
            Dim uc = Replace(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(splitWord), " ", "")
            pathArray(i) = uc

            i += 1
            ' Else
            ' Dim fff As String = ""
            ' End If


        Next


        If (keepSpaces) Then
            Dim str As String = Join(pathArray, " ")
            Return str
        End If


        Return Join(pathArray, "")

    End Function

    Public Shared Function removeFeatureText(ByVal textToClean As String) As String
        If (textToClean.Contains("-feat")) Then
            textToClean = Split(textToClean, "-feat").GetValue(0)

        End If
        Return textToClean
    End Function
    Public Delegate Sub LemLogDelegate(message As String, title As String)

    Public Shared Async Function CopyFileAsync(source As String, destination As String) As Task
        Dim app As Application = Application.Current
        Dim log As LemLogDelegate = New LemLogDelegate(AddressOf app.LemLog)

        Using SourceStream As FileStream = File.Open(source, FileMode.Open)
            Using DestinationStream As FileStream = File.Create(destination)
                Await app.Dispatcher.BeginInvoke(log, {"Copying " & source & " To " & destination, "Moving Files"})

                Await SourceStream.CopyToAsync(DestinationStream)

                Await app.Dispatcher.BeginInvoke(log, {"Done", "Moving Files"})
            End Using
        End Using

    End Function

    Public Shared Function BurnCD(songs As MusicList) As Boolean

    End Function
    Public Shared Function GetFeaturesStringList(features As List(Of SongData.FeatureInfo)) As List(Of String)
        Dim result As List(Of String) = New List(Of String)
        For Each feat As SongData.FeatureInfo In features
            result.Add(feat.Artist_Name)
        Next
        Return result
    End Function

    Public Shared Function addFeatureText(ByVal songName As String, ByRef features As List(Of SongData.FeatureInfo)) As String
        songName = Utils.removeFeatureText(songName)
        Return UntameVBUtils.NamingConventions.addFeatureText(songName, GetFeaturesStringList(features))



    End Function

    Public Shared Function removeNumbers(ByVal _name As String) As String


        Dim temp As String = _name
        If (temp.Contains(". ")) Then
            temp = Split(temp, ". ").GetValue(1)
        End If
        Return temp


    End Function

    Public Shared Function ExtractClassCodeFromGlobalID(ByRef GlobID As String) As Identity.ClassCode


        Dim arr = Split(GlobID, "-")
        Dim cc = Convert.ToInt32(arr.GetValue(1))
        Dim ccode = Identity.GetClassCodeFromInt(cc)

        Return ccode

    End Function

    Public Shared Function ExtracLocalFromGlobal(ByVal GlobalID As String) As Int32


        If (GlobalID.Contains("UNT-")) Then
            Dim arr = Split(GlobalID, "-")
            Return Convert.ToInt32(arr.GetValue(2))
        End If

        Return -1

    End Function




    Public Shared Function ForceNewIdForSong(ByVal songName As String) As String
        Dim ClassGlobalCount As Int32
        Dim gid As String = ""
        Dim Songdoc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        Dim IDDoc = XDocument.Load(Application.XML_PATH & "Identity.xml")

        Dim songsNode As XElement = Songdoc.Root.Element("Songs")

        Dim selection = From node As XElement In songsNode.Elements
                     Where node.Attribute("name").Value = songName
                     Select node

        Try





            Dim ParentNode = selection.First

            If Not (ParentNode Is Nothing) Then

                Dim IDsongsNode = IDDoc.Root.Element("Songs")

                ClassGlobalCount = Convert.ToInt32(IDsongsNode.Attribute("count").Value)
                ClassGlobalCount = ClassGlobalCount + 1
                IDsongsNode.Attribute("count").Value = ClassGlobalCount

                Dim newidsongnode = New XElement("Song", New XAttribute("id", ClassGlobalCount), New XAttribute("name", songName))

                IDsongsNode.Add(newidsongnode)
                IDDoc.Save(Application.XML_PATH & "Identity.xml")

                gid = Utils.PullGlobalIDOutYaAss(Identity.ClassCode.Song, ClassGlobalCount)
                Dim newSongId As XAttribute = New XAttribute("identity", gid)

                ParentNode.Add(newSongId)

                ParentNode.Parent.Attribute("count").Value = ClassGlobalCount

                Songdoc.Save(Application.XML_PATH & "SongInfo.xml")



            End If



        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
            Throw New Exception("Song not found in SongInfo.xml.  The folder that your retreiving these song names from may be out of sync with the xml file.")

        End Try



        Return gid


    End Function


    Private Shared Function PullGlobalIDOutYaAss(ByVal cc As Identity.ClassCode, ByRef LID As Int32) As String

        Return "UNT-" & cc & LID.ToString()



    End Function

    Public Shared Function UpdateSongInfoXML() As Boolean

        '\\STUDIOB-PC\work\sessions
        Dim songList As List(Of songDate) = New List(Of songDate)




        Dim doc = XDocument.Load(Application.XML_PATH & "Identity.xml")
        Dim loginRoot = Utils.getRootXMLNode("LoginInfo.xml")
        Dim songsDoc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        ' Dim songsRoot = songsDoc.Root.Element("Songs")
        Dim lastmodlist = New Dictionary(Of String, DateTime)

        Dim songCount = 0





        Dim SongInfo = New XElement("SongInfo")
        Dim newSongs = New XElement("Songs", New XAttribute("songCount", 0), New XAttribute("commentCount", 0))
        Dim skip = False
        Dim replace = False

        Dim songobj As SongData
        Dim sd As songDate = New songDate
        Dim loginTime As DateTime

        For Each artistLogin As XElement In loginRoot.Elements

            Try



                If (artistLogin.HasAttributes) Then







                    If (songList.Count > 0) Then
                        skip = False
                        replace = False

                        Dim i = 0
                        For Each pair As songDate In songList
                            Dim xmltime = Convert.ToDateTime(artistLogin.Attribute("loginTime").Value)

                            If (SpaceSafeCompareSongName(SongData.RemoveMp3FromSongName(pair.song.Title), SongData.RemoveMp3FromSongName(artistLogin.Element("song").Attribute("name").Value))) Then
                                If (Convert.ToDateTime(songList.Item(i).logdate) < xmltime) Then
                                    'datelist greater/newer than xml 
                                    skip = True
                                    replace = True

                                Else
                                    ' dateList.RemoveAt(i)
                                    ' stringList.RemoveAt(i)



                                End If


                            End If
                            If Not (skip) Then
                                i = i + 1
                            End If

                        Next


                    End If



                    If Not (skip) Then


                        Dim atts = artistLogin.Attributes()



                        Dim name = atts.First.Value
                        s_artistName = name

                        Dim artist = New Artist(name, False)

                        Dim identity = "UNT-" & 12004 & "-" & songCount

                        m_LocalID = songCount
                        s_id = m_LocalID

                        loginTime = Convert.ToDateTime(atts.ElementAt(1).Value)
                        s_lastModified = loginTime


                        Dim song = Utils.removeNumbers(SongData.RemoveMp3FromSongName(artistLogin.Attribute("song").Value))

                        songobj = New SongData(artist, song)

                        Dim ready = songobj.WaitforInitialization


                        'sd.song = songobj
                        'sd.logdate = loginTime.ToString



                        songList.Add(sd)







                        'dateList.Add(loginTime)
                        'Continue For






                        Dim bounced As Boolean

                        If (Convert.ToBoolean(ready)) Then
                            bounced = Utils.CheckIfSongIsBounced(songobj)
                        End If

                        'Dim features = atts.ElementAt(5).Value
                        s_hasFeatures = False

                        ' Dim songcomments = atts.ElementAt(7).Value
                        s_hasComments = False

                        'replace date 

                        If (replace) Then

                            'find songnode and update date att
                            Dim query = From sng As XElement In newSongs.Elements
                                        Where sng.Attribute("Identity").Value = identity
                                        Select sng


                            Dim existingNode = query.First()

                            existingNode.Attribute("lastModified").Value = loginTime






                        Else



                            'add node


                            Dim songnode = New XElement("song", New XAttribute("Identity", identity), New XAttribute("name", song), New XAttribute("mixReady", False), New XAttribute("features", False), New XAttribute("lastModified", loginTime), New XAttribute("artist", name), New XAttribute("bounced", bounced), New XAttribute("comments", False), New XAttribute("priority", False), New XAttribute("needsFeature", False), New XAttribute("musicSource", SetupSession.SessionMusicSource_Type.SoundClick))

                            newSongs.Add(songnode)



                            'write identity


                            Dim root = doc.Root
                            Dim sngs = root.Element("Songs")
                            sngs.Attribute("count").Value = songCount
                            newSongs.Attribute("songCount").Value = songCount
                            sngs.Add(New XElement("Song", New XAttribute("id", songCount), New XAttribute("name", song)))








                            songCount = songCount + 1





                            'end

                        End If






                        'end skip
                    End If

                End If

            Catch ex As Exception
                Application.LemLog(ex.Message, "ERROR!!!")
                Throw New Exception
            End Try
        Next



        SongInfo.Add(newSongs)
        SongInfo.Save(Application.XML_PATH & "SongInfo.xml")
        doc.Save(Application.XML_PATH & "Identity.xml")


        Return True
    End Function



End Class
'    Public Class XMLUtils

'        Private XML_PATH As String = Application.XML_PATH

'        Public Shared Function updateLoginInfo(ByVal _session As SetupSession.Session, Optional ByRef logIn As Boolean = True) As Int32


'            Dim doc As XmlDocument = New XmlDocument()
'            doc.Load(Application.XML_PATH & "LoginInfo.xml")

'            Dim root As XmlNode = doc.DocumentElement
'            Dim loggedInNode As XmlNode = root.Attributes.ItemOf(3)
'            Dim logCountNode As XmlNode = root.Attributes.ItemOf(4)
'            Dim logCount As Integer = Integer.Parse(logCountNode.Value)

'            root.Attributes.ItemOf(0).Value = _session.Artist_Name
'            root.Attributes.ItemOf(1).Value = _session.SongName
'            root.Attributes.ItemOf(2).Value = Now

'            If (logIn) Then
'                loggedInNode.Value = 1
'            End If
'            Dim thisNode As XmlNode = Nothing




'            If (logIn) Then
'                logCount = logCount + 1
'                root.Attributes.ItemOf(4).Value = logCount

'                Dim newNode As XmlNode = doc.CreateElement("artistLogin")

'                Dim nameAtt As XmlAttribute = doc.CreateAttribute("name")
'                nameAtt.Value = _session.Artist_Name

'                newNode.Attributes.Append(nameAtt)


'                Dim loginTime As XmlAttribute = doc.CreateAttribute("loginTime")
'                loginTime.Value = Now
'                newNode.Attributes.Append(loginTime)

'                Dim duration As XmlAttribute = doc.CreateAttribute("duration")
'                duration.Value = "null"
'                newNode.Attributes.Append(duration)

'                Dim loTime As XmlAttribute = doc.CreateAttribute("logOutTime")
'                loTime.Value = "null"
'                newNode.Attributes.Append(loTime)


'                Dim songAtt As XmlAttribute = doc.CreateAttribute("song")
'                songAtt.Value = _session.SongName
'                newNode.Attributes.Append(songAtt)

'                Dim id As XmlAttribute = doc.CreateAttribute("id")
'                id.Value = logCount
'                newNode.Attributes.Append(id)


'                thisNode = newNode

'                root.AppendChild(thisNode)


'                ' End If

'                thisNode.Attributes.ItemOf(0).Value = _session.Artist_Name
'                thisNode.Attributes.ItemOf(1).Value = Now
'                thisNode.Attributes.ItemOf(2).Value = "null"
'                thisNode.Attributes.ItemOf(3).Value = "null"
'                thisNode.Attributes.ItemOf(4).Value = _session.SongName










'            Else


'                Dim finishNode As XmlNode = doc.SelectSingleNode("/LoginInfo/artistLogin[@id='" & logCount & "']")
'                finishNode.Attributes.ItemOf(3).Value = Now
'                Dim _then As TimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(Date.Parse(finishNode.Attributes.ItemOf(1).Value)))
'                Dim _now As TimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(Now))
'                Dim span As TimeSpan = _now.Subtract(_then)

'                finishNode.Attributes.ItemOf(2).Value = span.Minutes

'                finishNode.Attributes.ItemOf(4).Value = _session.SongName




'                If (_session.Feature_Count > 0) Then
'                    For Each feature As SongData.FeatureInfo In _session.Song_Object.Features_List
'                        Dim fnode As XmlNode = doc.CreateElement("feature")
'                        Dim nameAtt As XmlAttribute = doc.CreateAttribute("name")
'                        nameAtt.Value = feature.Artist_Name

'                        fnode.Attributes.Append(nameAtt)



'                    Next
'                End If

'                root.Attributes.ItemOf(3).Value = 0

'            End If


'            doc.Save(Application.XML_PATH & "LoginInfo.xml")
'            updateSongInfo(_session.Song_Object)
'            Return logCount





'        End Function

'        Public Shared Sub updateSongInfo(ByVal Song_Obj As SongData)



'            Dim doc As XmlDocument = New XmlDocument()
'            doc.Load(Application.XML_PATH & "SongInfo.xml")
'            Dim root As XmlNode = doc.DocumentElement

'            Dim songs As XmlNode = root.FirstChild()

'            Dim songExists As Boolean = False
'            For Each node As XmlNode In songs.ChildNodes
'                If (Convert.ToInt32(node.Attributes.ItemOf(6).Value) = Song_Obj.Identity Or node.Attributes.ItemOf(0).Value = Song_Obj.Title) Then
'                    songExists = True
'                    Exit For
'                End If
'            Next


'            If Not (songExists) Then

'                Dim newSong As XmlNode = doc.CreateElement("song")
'                Dim nameAtt As XmlAttribute = doc.CreateAttribute("name")
'                nameAtt.Value = Song_Obj.Title
'                newSong.Attributes.Append(nameAtt)

'                Dim _mixReady As XmlAttribute = doc.CreateAttribute("mixReady")
'                _mixReady.Value = Song_Obj.MixReady
'                newSong.Attributes.Append(_mixReady)

'                Dim _features As XmlAttribute = doc.CreateAttribute("features")
'                _features.Value = Song_Obj.Has_Features
'                newSong.Attributes.Append(_features)

'                Dim lastMod As XmlAttribute = doc.CreateAttribute("lastModified")
'                lastMod.Value = Now
'                newSong.Attributes.Append(lastMod)

'                Dim artistNameAtt As XmlAttribute = doc.CreateAttribute("artist")
'                artistNameAtt.Value = Song_Obj.Artist
'                newSong.Attributes.Append(artistNameAtt)

'                Dim _bounced As XmlAttribute = doc.CreateAttribute("bounced")
'                _bounced.Value = Song_Obj.IsBounced
'                newSong.Attributes.Append(_bounced)


'                If (Song_Obj.Has_Features) Then

'                    For Each artist As SongData.FeatureInfo In Song_Obj.Features_List
'                        Dim node As XmlNode = doc.CreateElement("feature")
'                        Dim att As XmlAttribute = doc.CreateAttribute("name")
'                        att.Value = artist.Artist_Name
'                        node.Attributes.Append(att)
'                        newSong.AppendChild(node)
'                        doc = updateFeatureInfo(artist.Artist_Name(), Song_Obj.Title, Song_Obj.Artist, doc)

'                    Next

'                End If

'                songs.AppendChild(newSong)

'                doc.Save(Application.XML_PATH & "SongInfo.xml")


'            End If




'        End Sub

'        Public Shared Function updateFeatureInfo(ByVal name As String, ByVal song As String, ByVal hostName As String, ByRef xml As XmlDocument) As XmlDocument

'            Dim root As XmlNode = xml.FirstChild().NextSibling.FirstChild.NextSibling
'            Dim newFeature As XmlNode = xml.CreateElement("feature")
'            Dim count As Integer = 0
'            If (root.ChildNodes.Count > 0) Then
'                For Each nd As XmlNode In root.ChildNodes

'                    If (nd.Attributes.ItemOf(0).Value = name) Then
'                        newFeature = nd
'                        count = Integer.Parse(nd.Attributes.ItemOf(1).Value)

'                        Exit For
'                    Else

'                    End If
'                Next
'                Dim fName As XmlAttribute = xml.CreateAttribute("name")
'                Dim songs As XmlAttribute = xml.CreateAttribute("songs")

'                newFeature.Attributes.Append(fName)
'                newFeature.Attributes.Append(songs)

'                root.AppendChild(newFeature)



'            Else
'                Dim fName As XmlAttribute = xml.CreateAttribute("name")
'                Dim songs As XmlAttribute = xml.CreateAttribute("songs")

'                newFeature.Attributes.Append(fName)
'                newFeature.Attributes.Append(songs)
'                root.AppendChild(newFeature)
'            End If

'            count += 1
'            newFeature.Attributes.ItemOf(0).Value = name
'            newFeature.Attributes.ItemOf(1).Value = count

'            Dim _song As XmlNode = xml.CreateElement("song")
'            Dim songName As XmlAttribute = xml.CreateAttribute("name")
'            Dim artistName As XmlAttribute = xml.CreateAttribute("artist")
'            songName.Value = song
'            artistName.Value = hostName
'            _song.Attributes.Append(songName)
'            _song.Attributes.Append(artistName)
'            newFeature.AppendChild(_song)



'            Return xml

'        End Function


'    End Class



'End Class


Public Class MacroShop




    Public Shared Async Function WaitForMacrosToComplete() As Task(Of Boolean)


        Dim stamp = DateTime.Now
        If (Application.useMacros) Then
            While Process.GetProcessesByName("msched").Count > 0 And DateTime.Now.Subtract(stamp).TotalSeconds < 10
                Await Task.Delay(TimeSpan.FromMilliseconds(500))
            End While
            killAllMacros()
        End If


        Return True

    End Function



    Public Shared Function MaximizeNuendo() As Boolean
        Process.Start(Application.MAXNUENDO)
        Return True
    End Function


    Public Shared Function Clean_Close_Session() As Boolean
        If (Application.useMacros) Then
            Process.Start(Application.CLEAN_CLOSE_SESSION)
        End If
        Return True
    End Function


    Public Shared Async Function closeSessionQuick() As Task(Of Boolean)


        If (Application.useMacros) Then
            Process.Start(Application.CLEAN_CLOSE_SESSION)
        End If

        'Await Task.Delay(100)
        Dim ready As Task(Of Boolean) = WaitForMacrosToComplete()

        Dim go As Boolean = Await ready

        If (go) Then
            Return True
        End If


        Return False

    End Function



    Public Shared Async Function OpenSession(ByVal session As SetupSession.Session) As Task(Of Boolean)

        Dim finalFileName As String = ""
        Dim infoReader As System.IO.FileInfo
        Dim modified As Date = Nothing

        For Each i As String In Directory.GetFiles(session.Final_Recording_Path)
            If (System.IO.Path.GetExtension(i) = ".npr") Then
                infoReader = My.Computer.FileSystem.GetFileInfo(i)
                If (infoReader.LastWriteTime > modified) Then
                    modified = infoReader.LastWriteTime
                    finalFileName = i
                End If


            End If
        Next
        If (finalFileName = "") Then
            MsgBox("Found No Sessions")
            Return False
        End If

        If (Application.useMacros) Then
            If (Process.Start(finalFileName).StartTime < Now) Then

            End If
        End If

        'Await Task.Delay(1000)
        TEST_MESS("try to close any extra windows now!!!!!")

        If (Application.useMacros) Then
            MacroShop.Close_Extra_Nuendo_Child_Windows()
            Dim wait As Task(Of Boolean) = WaitForMacrosToComplete()
            Dim ready As Boolean = Await wait
            TEST_MESS("should be done opening prev sess")
            'If Not (session.Song_Object.Initialized) Then
            '    session.Reinitialize()
            'End If

            Return ready
        Else
            Await Task.Delay(2000)
            TEST_MESS("should be done opening prev sess")
            Return True
        End If






        Return False
    End Function

    Private Shared Sub TEST_MESS(message As String)
        If (Application.UseTEST_MESS) Then
            MsgBox(message)
        End If
    End Sub
    Public Shared Sub closeThis(ByRef session As SetupSession.Session)

        If (Application.useMacros) Then
            Process.Start(Application.CLOSE_THIS)
        End If


    End Sub


    Public Shared Sub killAllMacros()

        Dim processes As Process() = Process.GetProcessesByName("msched")
        For Each process As Process In processes
            process.Kill()

        Next

    End Sub


    Public Shared Sub Close_Extra_Nuendo_Child_Windows()

        If (Application.useMacros) Then
            Process.Start(Application.MACROS_DIRECTORY & "\closeExtraWindows.scp")
        End If



    End Sub
    Public Shared Sub Close_Session(ByRef session As SetupSession.Session)



    End Sub

    Private Shared Async Function Paste_Clipboard_And_TryBounce(ByVal finalPath As String) As Task(Of Boolean)

        'If (finalPath = "") Then
        '    finalPath = Application.TEMP_BOUNCE_PATH

        'Throw New Exception("Final Path is empty")

        ' End If
        ' Process.Start(Application.BOUNCE_SESSION_SCRIPT)
        'Clipboard.Clear()
        Dim count = 0
        ' Clipboard.SetText(finalPath, TextDataFormat.Text)
        While Not (Clipboard.GetText() = finalPath)
            If (count < 50) Then
                'MsgBox("calling bounce session script", MsgBoxStyle.SystemModal)


                Try
                    'MsgBox("try pasting to clipboard now", MsgBoxStyle.SystemModal)
                    Clipboard.Clear()

                    Await Task.Delay(500)
                    Clipboard.SetText(finalPath, TextDataFormat.Text)


                Catch ex As Exception

                    count += 1

                    'Dim processes As Process() = Process.GetProcessesByName("msched")
                    'For Each process As Process In processes
                    '    process.Kill()


                    'Next
                    Thread.Sleep(1000)




                End Try
                'Return True
            Else
                Return False

            End If



        End While
        ' MsgBox("clipboard set", MsgBoxStyle.SystemModal)

        If (Application.useMacros) Then
            Process.Start(Application.BOUNCE_SESSION_SCRIPT)
            ' Await Task.Delay(7000)
        End If

        Return True

    End Function

    Public Shared Function Find_Previous_Version_Of_Same_Song(ByRef bounceFolderName As String, ByRef session As SetupSession.Session) As Boolean




        If Not (session.Song_Object.IsBounced) Then
            Return True
        End If

        For Each songFile As String In My.Computer.FileSystem.GetFiles(bounceFolderName)


            'remove spaces


            If (songFile.Contains(session.SongName) Or session.SongName.Contains(songFile)) Then



                'songs match

                'ask user what to do
                Try
                    My.Computer.FileSystem.MoveFile(songFile, Application.MP3_PATH & "Archives " & Now.Year & "\" & "PreviousVersions\" & session.Artist_Name & "\" & Path.GetFileName(songFile))
                Catch ex As Exception
                    Application.LemLog(ex.Message, String.Format("Overwriting {0}", System.IO.Path.GetFileNameWithoutExtension(songFile), ""))

                    Try
                        File.Delete(songFile)

                    Catch
                        Application.LemLog("Prob moving and deleting " + songFile, "ERROR!!!")
                        'Throw New Exception("Prob moving and deleting " + songFile)

                    End Try

                End Try

                'Select Case MsgBox("Do you want to Replace " & Utils.removeNumbers(Path.GetFileNameWithoutExtension(songFile)) & " ? Which may be and older version of this song or just another song with the same name", MsgBoxStyle.YesNoCancel, songFile & " Already There!")

                '    Case MsgBoxResult.Yes
                '        Try
                '            My.Computer.FileSystem.MoveFile(songFile, Application.MP3_PATH & "Archives " & Now.Year & "\" & "PreviousVersions\" & session.Artist_Name & "\" & Path.GetFileName(songFile))
                '        Catch ex As Exception
                '            Application.LemLog(ex.Message, "ERROR!!!")
                '            Try
                '                File.Delete(songFile)

                '            Catch
                '                Application.LemLog("Prob moving and deleting " + songFile, "ERROR!!!")
                '                Throw New Exception("Prob moving and deleting " + songFile)

                '            End Try

                '        End Try


                '    Case MsgBoxResult.No

                '        Return True

                '    Case MsgBoxResult.Cancel
                '        Return True

                'End Select

            Else
                'currentsong doesnt match the song being evaluated


            End If
        Next
        Return True
    End Function









    Public Shared Async Function BounceSession(ByVal session As SetupSession.Session) As Task(Of Integer)


        'get the name of the artist mp3 location




        
        Process.Start(Application.MACROS_DIRECTORY & "\closeExtraWindows.scp")


        While Process.GetProcessesByName("msched").Count > 0
            Await Task.Delay(TimeSpan.FromSeconds(1))
        End While

        killAllMacros()
        Await Task.Delay(2000)
        If True Then

            Dim finalPath = session.GetArtist().RecordingPath + session.GetMother().New_Song_Name + ".wav"
            If (File.Exists(finalPath)) Then
                File.Delete(finalPath)
            End If
            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.newBounceName, session.GetMother.New_Song_Name)
            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.oldBounceName, session.Song_Object.Title)
            Application.TEMP_BOUNCE_PATH = finalPath

            TEST_MESS(" Paste_Clipboard_And_TryBounce(Application.TEMPORARY_FILENAME_HOLDER)")
            Dim bounced As Task(Of Boolean) = Paste_Clipboard_And_TryBounce(finalPath)
            'Application.useMacros
            If (True) Then


                Dim result = Await bounced

                Dim windowfinder As User32 = New User32()

                'result

                If (result) Then
                    Await Task.Delay(5000)


                    Dim count = 0
                    While ((Not windowfinder.Find(Nothing, "Export Audio Mixdown") = 0) Or (Process.GetProcessesByName("msched").Length > 0) And count < 61)
                        count += 1
                        Await Task.Delay(2000)

                    End While

                    If (count > 60) Then
                        Application.LemLog("Bounce Timed Out...", "ERROR_Bounce_Session")
                        'MacroShop.killAllMacros()
                        Return 2
                    End If

                    Await Task.Delay(2000)
                    TEST_MESS("'should be good_MacroShop.BounceSession")
                    'should be good
                    Application.UpdateSessionFolderName()

                    Return 1
                Else
                    TEST_MESS("probably empty session_MacroShop.BounceSession")
                    'probably empty session
                    Return 2
                End If

            Else

                Await Task.Delay(5000)
                Utils.writeSongFolderName(session.Final_Recording_Path, session.SongName)
                ' Return 1
            End If






        End If

        'unsuccessful
        Return 0

    End Function

    Shared Function NewSession(currentSession As SetupSession.Session, beatPath As String, baseTrackInfo As BaseTrackInfoWrapper) As Boolean

        Dim forreal = Application.useMacros


        My.Computer.FileSystem.CopyDirectory(Application.TEMPLATE_DIRECTORY, currentSession.Final_Recording_Path, True)

        'dA bEAT!!!_01.wav
        Dim trackinfo As FileInfo = baseTrackInfo.m_TrackFileInfo
        Dim finalwav As String = currentSession.Final_Recording_Path & "Audio\dA bEAT!!!.wav"
        If (File.Exists(finalwav)) Then

            File.Delete(finalwav)
        End If





        Using outfile As New StreamWriter(currentSession.Final_Recording_Path & "audio\beatinfo.txt")
            outfile.Write(beatPath)

            baseTrackInfo.AddNewArtistReference(currentSession.Artist_Name, currentSession.Song_Object.GLOBAL_ID)


        End Using
        Using reader As New MediaFoundationReader(beatPath)
            WaveFileWriter.CreateWaveFile(finalwav, reader)

        End Using
        If (forreal) Then
            ' If (Application.FloatingPlayer.writeWave(finalwav)) Then
            Process.Start(Application.OPEN_NEWLY_CREATED_SESSION)

            'Process.Start(currentSession.Final_Recording_Path & "1.npr")


        End If







        ' End If

        Return True


    End Function

    Shared Function StartNewFLStudioSession(ByRef session As SetupSession.Session) As Boolean
        Throw New NotImplementedException
    End Function

    Public Shared Sub MinimizeNuendo()
        ' Process.Start(Application.MINNUENDO)
    End Sub


    ''' <summary>
    ''' returns 1 if window found, 0 for no window by the specified name
    ''' </summary>
    ''' <param name="nameOfWindow"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function WindowFinder(nameOfWindow As String) As Integer
        Dim window_finder As User32 = New User32()

        Return window_finder.Find(Nothing, nameOfWindow)

    End Function

End Class