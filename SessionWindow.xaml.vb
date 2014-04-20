Imports System.Net.WebClient
Imports System.IO
Imports System.Timers
Imports System.Threading.Tasks
Imports System.Threading



Public Class SessionWindow

    Private WithEvents app As Application
    Private session As SetupSession.Session
    Private setupSess As SetupSession
    Private selected As Boolean
    Private selectedWrapper As TrackWrapper
    Private newmessages As Boolean
    'Private Shared player As LemPlayer
    Private notificationsAlreadyDisplayed As Boolean
    'Private selectedTrackInfo As BaseTrackInfoWrapper
    'Private selectedSoundclickInfo As FileInfo
    Private screwed As Boolean
    Private m_playerIsPlaying As Boolean
    Private m_currentPlayingsong As String
    'Private sliderTimer As Timer
    Public MUT As Mutex
    Private m_addGroupMemberEnabled As Boolean = False
    Private m_CancelationTokenSource As CancellationTokenSource = New CancellationTokenSource()
    Private m_CancelationToken As CancellationToken
    Private m_CurrentBeatsList = 0

    Private SoundClickList As List(Of TrackWrapper) = New List(Of TrackWrapper)

    Public Sub New(ByRef mother As SetupSession)

        ' This call is required by the designer.

        InitializeComponent()
        m_CancelationToken = m_CancelationTokenSource.Token
        notificationsAlreadyDisplayed = False
        setupSess = mother
        CURRENTINSTANCE = Me
        Application.Main_Window.Hide()


       


        MUT = New Mutex

        FeatureSelection.Visibility = Windows.Visibility.Collapsed

        SoundClickList = Application.SoundClickList

        ui_SoundClickSongList.DataContext = SoundClickList
        Dim notifications As Integer() = {}



        notifications = Notification.CheckForWaitingNotifications(setupSess.Get_Current_Artist, True)


        If Not notifications Is Nothing Then
            If notifications.Count > 0 Then
                'Unread Messages
                notifications = Notification.CheckForWaitingNotifications(setupSess.Get_Current_Artist, False)


                UI_MessagesGrid.Visibility = Windows.Visibility.Visible

            End If
        End If







        ' scThread.Start()

        app = Application.Current
        ' sliderTimer = LemPlayer.GetTimer()
        'AddHandler sliderTimer.Elapsed, AddressOf updateTrackSlider



        ' player = New LemPlayer

        ' ui_LemPlayer.Child = LemPlayer.GetPlayerHost()

        'ui_BottomLemPlayerSection.Children.Add(LemPlayer.GetPlayerHost())





        UI_editPriorityButtonsGrid.Visibility = Windows.Visibility.Collapsed


        selected = False
        screwed = False

        ui_sessionsList.SelectedItem = 0
        If (mother.Get_Session_Count > 0) Then
            session = mother.GetCurrentSessions.Item(0)
            If (session.Song_Object.IsBounced) Then
                TrackPlayButton.Visibility = Windows.Visibility.Visible
            Else
                TrackPlayButton.Visibility = Windows.Visibility.Hidden
            End If
        Else
            '  session = New SetupSession.Session(mother.Get_Current_Session.Title, mother.Get_Current_Session.Song_Object.GLOBAL_ID, mother.Get_Current_Artist.LOCAL_ID.ToString())







        End If
       


        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Delegate Sub InitsessionListDelegate(setupsess As SetupSession)
    Private Sub InitializeSessionList(setupsess As SetupSession)


        Dim SongList = Utils.GetSongSessionList(setupsess.Get_Current_Artist)



        If (setupsess.GetSessions(SongList)) Then

            Dim app2 As Application = Application.Current
            Dim sw As SessionWindow = app2.SessionWindow

            Dim finInitSessDel As FinalizeInitSessionsListDelegate = New FinalizeInitSessionsListDelegate(AddressOf FinalizeInitSessionList)
            sw.Dispatcher.BeginInvoke(finInitSessDel, setupsess)

        End If
    End Sub
    Public Delegate Sub FinalizeInitSessionsListDelegate(setup As SetupSession)
    Public Sub FinalizeInitSessionList(setup As SetupSession)
        ui_sessionsList.DataContext = setup.Session_List
        ui_sessionsList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()
        Application.LemLog(setupSess.Session_List.Count.ToString() & " sessions found...", "Loading Sessions Complete.")
        'BeginSessionListInit()
    End Sub
    Private Sub BeginSessionListInit()
        For Each sess As SetupSession.Session In setupSess.Session_List
            If Not (sess.Song_Object.Initialized) Then
                InitializeSession(sess)
            End If


        Next
    End Sub
    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)

         Task.Factory.StartNew(Sub() InitializeSessionList(setupSess), m_CancelationToken)
        Application.LemLog("Getting all sessions for " + setupSess.Get_Current_Artist.My_Name)


        If (Application.USE_SCHEDULE) Then
            SessionTimer.Start(setupSess)
        End If


    End Sub


    Private Sub getMessages(newOnly As Boolean)

        Dim logdel As lemLog = New lemLog(AddressOf Application.LemLog)
        Dim sentList As List(Of Notification) = New List(Of Notification)
        app.Dispatcher.Invoke(logdel, {"Looking for messages..", "Getting Messages"})
        For Each kv As Tuple(Of String, Integer) In Notification.GetSentMessages(setupSess.Get_Current_Artist)
            Dim noti As Notification = Notification.GetNotification(New Artist(Utils.GetNameByGlobalID(kv.Item1), False), kv.Item2)
            sentList.Add(noti)

        Next
        Dim notifications As Integer() = {}



        notifications = Notification.CheckForWaitingNotifications(setupSess.Get_Current_Artist, newOnly)

        Dim name As String = setupSess.Get_Current_Artist.My_Name


        If Not notifications Is Nothing Then
            app.Dispatcher.Invoke(logdel, {"found " & notifications.Count & " messages for " & name, "Messages Retrieved"})

            If (notifications.Count() > 0) Then
                If (notifications.Count = 1 And sentList.Count = 0) Then
                    Dim noti As Notification = Notification.GetNotification(setupSess.Get_Current_Artist, notifications.GetValue(0))
                    notificationsAlreadyDisplayed = True
                    app.Dispatcher.Invoke(logdel, {"Only one message", "Messages Retrieved"})
                    noti.Display()
                Else
                    sentList.Sort()
                    notificationsAlreadyDisplayed = True
                    app.Dispatcher.Invoke(logdel, {notifications.Count & " messages found.", "Messages Retrieved"})
                    Notification.DisplayAllNotifications(setupSess.Get_Current_Artist, notifications, sentList)

                End If
                notificationsAlreadyDisplayed = True
            End If
        Else


            app.Dispatcher.Invoke(logdel, {"Messages are empty", "Welcome to Untame " & setupSess.Get_Current_Artist.My_Name})

        End If
    End Sub

    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)

        If Not notificationsAlreadyDisplayed Then

            Dim getMessageTask As Thread = New Thread(AddressOf getMessages)
            getMessageTask.SetApartmentState(ApartmentState.STA)

            getMessageTask.IsBackground = True
            getMessageTask.Start()


            notificationsAlreadyDisplayed = True
        End If
    End Sub

    Public Overrides Sub EndInit()
        MyBase.EndInit()

    End Sub

    Private Sub updateTrackSlider()

        ' TrackSlider.Value = LemPlayer.getPlayPosition()


    End Sub

    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        MyBase.OnClosing(e)
        SessionTimer.StopNow()
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.startUp.ToString())
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookingDuration, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.currentSong, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.currentUser, "")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.loggedIn, "0")
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.overTime, "false")
        Application.LemLog("Logging Out " & setupSess.Get_Current_Artist.My_Name & " ...")
        If (Application.LEMTESTING) Then
            Application.Main_Window.Show()

            app.Log_Out_Artist()
            

            Exit Sub
        End If
        m_CancelationTokenSource.Cancel()
        app.BeginLogOutArtist(setupSess)





    End Sub


    Private Sub handleLemLog(message As String, overallStat As String) Handles app.LemLogEvent

        If ui_StatWindow.Text.Count > 100 Then
            ui_StatWindow.Text = "..."
        End If
        If Not ui_StatWindow Is Nothing Then
            ui_StatWindow.Text = message & vbNewLine & ui_StatWindow.Text & "..."
            If overallStat = "" Then
                If session Is Nothing Then
                    ui_OverallStatusText.Text = "Untame Studios"
                Else
                    ui_OverallStatusText.Text = "Welcome " & session.GetArtist.My_Name
                End If
            Else
                ui_OverallStatusText.Text = overallStat
            End If

        End If


    End Sub
    Private Shared CURRENTINSTANCE As SessionWindow

    Public Shared Function CURRENT_INSTANCE() As SessionWindow
        Return CURRENTINSTANCE
    End Function
    Delegate Sub UpdateAfterSongDelegate(updatedSession As SetupSession.Session)
    Public Sub UpdateAfterSongInitialization(updatedSession As SetupSession.Session)
        If session Is Nothing Then
            Return
        End If
        
        If (session.Song_Object.LOCAL_ID = updatedSession.Song_Object.LOCAL_ID) Then
            If (updatedSession.Song_Object.IsBounced) Then
                TrackPlayButton.Visibility = Windows.Visibility.Visible
            Else
                TrackPlayButton.Visibility = Windows.Visibility.Hidden
            End If
            If (Not (session.Song_Object.Initialized) And (updatedSession.Song_Object.Initialized)) Then
                SessionProperties.DataContext = updatedSession

            End If


        End If
        Try
            If ((Not updatedSession Is Nothing) And IsActive And (Not setupSess Is Nothing) And (Not setupSess.Get_Current_Artist Is Nothing) And (setupSess.Get_Current_Artist.My_Name = updatedSession.Artist_Name)) Then

                Dim udsld As SetupSession.UpdateSessionListDelegate = New SetupSession.UpdateSessionListDelegate(AddressOf setupSess.UpdateSessionList)

                setupSess.Dispatcher.Invoke(udsld, updatedSession)

                ' setupSess.UpdateSessionList(updatedSession)

                'QuickUpdateBindings(updatedSession)
            End If

        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try






    End Sub

    Public Function GetSetupSession() As SetupSession
        Return setupSess
    End Function

    Private Sub QuickUpdateBindings(newsession As SetupSession.Session)
        SessionProperties.DataContext = newsession
        songNameBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()
        ui_sessionsList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()
        FeaturingTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()
        If (File.Exists(newsession.Song_Object.MP3_File_Path)) Then

            TrackPlayButton.Visibility = Windows.Visibility.Visible
        Else
            TrackPlayButton.Visibility = Windows.Visibility.Hidden
        End If
    End Sub
    Private Sub InitializeSession(sess As SetupSession.Session)
        If Not (sess.Song_Object.Initialized) Then
           
            songNameBox.SetValue(TextBox.TextProperty, "loading...")
            Dim init As Task(Of SetupSession.Session) = New Task(Of SetupSession.Session)(Function()
                                                                                              sess.Song_Object.ReInitialize()


                                                                                              Return sess



                                                                                          End Function, Nothing, m_CancelationToken
)

            Dim scheduler = TaskScheduler.Current
            Dim finish = init.ContinueWith(Sub(s)
                                               Try
                                                   If (s.Result Is Nothing) Then
                                                       Return
                                                   Else
                                                       Dim win As SessionWindow = SessionWindow.CURRENT_INSTANCE()
                                                       Dim updateDel As UpdateAfterSongDelegate = New UpdateAfterSongDelegate(AddressOf win.UpdateAfterSongInitialization)
                                                       Dispatcher.BeginInvoke(updateDel, s.Result)
                                                   End If
                                               Catch ex As Exception
                                                   Dim logdel As Application.LemLogDel = New Application.LemLogDel(AddressOf Application.LemLog)
                                                   app.Dispatcher.BeginInvoke(logdel, {ex.Message, "ERROR!!!"})

                                               End Try



                                           End Sub, m_CancelationToken, TaskContinuationOptions.None, scheduler)


            init.Start()
        Else

        End If
        'Dim info = BaseTrackInfoWrapper.GetTrackInfoBasedOnUser(sess.Song_Object.Title, sess.GetArtist())
        'If sess.Song_Object.UpdateBaseBeatInfo(info) Then

        'End If
    End Sub
    Private Sub SessionSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If (TryCast(sender, ListBox) Is Nothing) Then
            Return
        End If
        If (ui_sessionsList.SelectedItem Is Nothing) Then
            Return
        End If
        session = ui_sessionsList.SelectedItem
        InitializeSession(session)
        FeatureSelection.Visibility = Windows.Visibility.Collapsed
        selected = True
        QuickUpdateBindings(session)

    End Sub


    Private Async Sub BounceSession(sender As Object, e As RoutedEventArgs)

        If Not selected Then
            MsgBox("Make sure to select the song first")
            Return
        End If
        Dim mother As SetupSession = session.GetMother()
        Application.TEST_MESS("Attempting to Bounce")
        Try
            Dim result As Task(Of String) = mother.BounceClosedSession(session)

            Dim fnResult As String
            fnResult = Await result
        Catch ex As Exception
            Dim logDel = New Application.LemLogDel(AddressOf Application.LemLog)
            app.Dispatcher.BeginInvoke(logdel, {ex.Message, "ERROR!!!"})
            Throw New Exception("There was a problem bouncing track")
        End Try


        Post_Close_Session(session)






        'If fnResult Then

        '    MsgBox("Bounced!!!")

        'Else
        '    MsgBox("Something Failed")
        'End If
    End Sub
    Public Sub RefreshSessions(Optional sess As SetupSession.Session = Nothing)
        If (sess Is Nothing) Then
            sess = Me.session
        End If
        RefreshSessionWindow(sess, False, False)
    End Sub


    Private Sub RefreshSessionWindow(newsession As SetupSession.Session, brandNewSession As Boolean, Optional autoPlayPlayer As Boolean = True)
        Dim overT = False


        Application.UpdateSessionFolderName()
        session = newsession
        setupSess.GetCurrentSessions().FirstOrDefault()
        If (setupSess.GetCurrentSessions().FirstOrDefault(Function(s) s.Title.Equals(session.Title)) Is Nothing) Then
            setupSess.GetCurrentSessions().Add(newsession)
        Else
            setupSess.GetCurrentSessions().Remove(newsession)
            setupSess.GetCurrentSessions().Add(newsession)

        End If





        If (autoPlayPlayer) Then
            If Not (Application.Floating_Player() Is Nothing) Then

                If (session.Song_Object.IsBounced) Then
                    Try
                        If Not Application.Current_Playing_Song = session.Song_Object.MP3_File_Path Then

                            Application.PlayFloater(newsession.Song_Object.MP3_File_Path, newsession.Title)
                        End If

                    Catch ex As Exception
                        Dim message = "Attempting to play " + newsession.Song_Object.MP3_File_Path + " failed...   " + ex.Message + "..........  " + ex.StackTrace
                        Application.TEST_MESS(message)

                        Application.LEM_ERR(message)

                        Application.LemLog(message)

                    End Try

                End If
            End If
        End If


        If (newsession.Title = session.Title) Then
            SessionProperties.DataContext = session
            songNameBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()
            FeaturingTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()
        End If

        ui_sessionsList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()


        If Not (IsVisible) Then
            Me.Show()
        End If
    End Sub
    Public Delegate Sub StartNewSessionRemotelyDelegate(trackPath As String)
    Private is_remote_call As Boolean = False
    Public Sub StartNewSessionRemotely(trackPath As String)
        Dim wrapper As TrackWrapper = New TrackWrapper(trackPath, New BaseTrackInfoWrapper(trackPath, "", SetupSession.SessionMusicSource_Type.DeviceBeat, Application.XMLRESOURCES, setupSess.Get_Current_Artist.My_Name, trackFileInfo:=New System.IO.FileInfo(trackPath)))
        selectedWrapper = wrapper
        is_remote_call = True
        StartNewSession()
    End Sub
    Private Async Function StartNewSession() As Task
        If (selectedWrapper Is Nothing) Then
            Application.LemLog("No beat selected!!!", "START NEW SESSION...")
            Return
        End If
        Application.PlayFloater(selectedWrapper.BeatPath, selectedWrapper.BeatName)
        If Not (is_remote_call) Then
            If MsgBox("Start new session with this track?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then

            Else
                Return
            End If

            If (selectedWrapper.BaseTrackInfo.Is_Exclusive_Only) Then
                Dim mess = "The beat selected is not available without Exclusive permission"
                If Not (selectedWrapper.BaseTrackInfo.origination Is Nothing) Then
                    mess &= " from " & selectedWrapper.BaseTrackInfo.GetProducerInfo.ArtistName

                End If
                MsgBox(mess)
                Application.LemLog(mess, "Exclusive use only...")
                Return

            End If

        End If
        is_remote_call = False
        
        Dim logdel As lemLog = New lemLog(AddressOf Application.LemLog)

        Dim currenttrack As String = selectedWrapper.File_Info.FullName
        Application.ResetFloater()

        Me.Hide()
        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.liveSession.ToString())
        Application.LemLog("Creating new session from " & currenttrack)

        Dim newsession As SetupSession.Session = Await setupSess.NewSession(selectedWrapper.BaseTrackInfo)
        Me.Show()
        Post_Close_Session(newsession)



    End Function
    Public Delegate Sub PlayFloaterDelegate(songPath As String, songName As String)

    Private Sub Post_Close_Session(newsession As SetupSession.Session)


        If Not Application.NEW_BOUNCE Then

            newsession.Song_Object.FLAG_POSTBOUNCE_UPDATE = True
            Dim finalName = newsession.UpdateName(newsession.GetMother().New_Song_Name, False)
            Return
        End If
        Application.NEW_BOUNCE = False
        Dim logdel As lemLog = New lemLog(AddressOf Application.LemLog)
        logdel("Updating Final Bounce", newsession.Song_Object.Title + " Finalizing")
        Dim trimTask = Task(Of SetupSession.Session).Factory.StartNew(Function()
                                                                          Return Application.TrimAndConvertFinalBounceAndRenameSessionFolder(newsession)
                                                                      End Function).ContinueWith(Sub(s)
                                                                                                     Try
                                                                                                         Dim sess As SetupSession.Session = s.Result
                                                                                                         Dim bouncePath = sess.Song_Object.MP3_Location + Utils.addFeatureText(sess.GetMother().New_Song_Name, sess.Song_Object.Get_Features_List) + ".mp3"

                                                                                                         Dim app As Application = Application.Current



                                                                                                         Dim playDel = New PlayFloaterDelegate(AddressOf Application.PlayFloater)
                                                                                                         app.Dispatcher.BeginInvoke(playDel, bouncePath, newsession.GetMother().New_Song_Name)





                                                                                                         Dim logdel2 As lemLog = New lemLog(AddressOf Application.LemLog)
                                                                                                         ' logdel2("Publishing song to server...", "Please wait...")
                                                                                                         If (sess.Reinitialize()) Then

                                                                                                             Application.SendSongToDataBase(sess.Song_Object)

                                                                                                             ' logdel("Serever Upload Complete.", "Please wait...")
                                                                                                             Dim win As SessionWindow = SessionWindow.CURRENT_INSTANCE()
                                                                                                             Dim updateDel As UpdateAfterBounceDel = New UpdateAfterBounceDel(AddressOf win.UpdateAfterBounce)
                                                                                                             Dispatcher.BeginInvoke(updateDel, sess)
                                                                                                         End If

                                                                                                     Catch ex As Exception

                                                                                                     End Try


                                                                                                 End Sub)
    End Sub
    Delegate Sub UpdateAfterBounceDel(sess As SetupSession.Session)
    Public Sub UpdateAfterBounce(sess As SetupSession.Session)
        Dim logdel As lemLog = New lemLog(AddressOf Application.LemLog)
        ' logdel("Sending song to UntameMusic Server...", "Please Wait")

        ' logdel("Sending song to UntameMusic Server...", "Please Wait")



        'check for open nuendo sessions and close
        'If (sess.Title = session.Title) Then
        RefreshSessionWindow(sess, True)
        'End If
        Application.LemLog(session.Title & "has been closed...", "SESSION COMPLETE!!!")

        LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.loggedIn.ToString())

        logdel(sess.Title + " Bounced and ready!!", "")





    End Sub
    Private Sub changeNameClicked(sender As Object, e As RoutedEventArgs)
        If (session Is Nothing) Then
            MsgBox("Make sure to select a song first")
            Return
        Else



            Dim newName As String = Utils.converToTitleCase(InputBox("What do you want to call this?", "Rename Track", session.Song_Object.Title))
            If (newName = "") Or (newName = " ") Or (newName = "  ") Then
                Return
            ElseIf newName = session.Song_Object.Title Then
                Return
            End If

            If (MsgBox("Rename to " & newName & " ?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok) Then
                newName = Utils.converToTitleCase(newName)
                If Not (Utils.converToTitleCase(session.SongName) = newName) Then

                    If (session.Song_Object.MP3_File_Path = "Not Bounced" Or session.Song_Object.MP3_File_Path = "") Then

                        Application.TEMP_BOUNCE_PATH = ""
                    Else
                        Application.TEMP_BOUNCE_PATH = session.Song_Object.MP3_File_Path
                    End If

                    Dim updatedName = session.UpdateName(newName, False)
                    If (session.Reinitialize()) Then
                        RefreshSessionWindow(session, False)

                        'Application.UpdateSessionFolderName()
                        'SessionProperties.DataContext = session
                        'songNameBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()


                        ' InvalidateVisual()
                    End If
                End If
            End If





        End If
    End Sub
    Public m_currentArtistList As List(Of Artist) = New List(Of Artist)
    Private Sub changeFeatures(sender As Object, e As RoutedEventArgs) Handles changeFeaturesButton.Click
        e.Handled = True
        If session Is Nothing Then
            Return
        End If
        m_currentArtistList.Clear()
        ArtistList.UnselectAll()

        If Not (ArtistList.Items.Count > 1) Then
            ArtistList.Items.Clear()
            For Each art As Artist In Application.ArtistList
                If Not (art.My_Name = session.Artist_Name) Then
                    ArtistList.Items.Add(art.My_Name)
                    m_currentArtistList.Add(art)

                End If



            Next
        End If
        FeatureSelection.Visibility = Windows.Visibility.Visible
    End Sub




    Private Function addOrRemoveFeature() As Boolean

        Dim log As lemLog = New lemLog(AddressOf Application.LemLog)
        Dim removeMe As Boolean = False
        Dim selectedArtist As Artist = New Artist(ArtistList.SelectedItem, False)
        If (session.Song_Object.Has_Features) Then
            For Each ft As SongData.FeatureInfo In session.Song_Object.Get_Features_List
                If (ft.Artist_Name = selectedArtist.My_Name) Then
                    removeMe = True
                End If
            Next
        End If
        Dim action As String = ""
        If removeMe Then
            action = "removing"
        Else
            action = "adding"
        End If
        app.Dispatcher.Invoke(log, {selectedArtist.My_Name & " selected..", action & " " & selectedArtist.My_Name})
        If (selectedArtist.My_Name = session.Artist_Name) Then
            MsgBox("You can't be a feature on your own song Chump!!!,this may be an error on my part However.")
            FeatureSelection.Visibility = Windows.Visibility.Collapsed
            app.Dispatcher.Invoke(log, {action & " " & selectedArtist.My_Name & " canceled." & vbNewLine, ""})

            Return True
        ElseIf (session.Featuring.Contains(selectedArtist.My_Name)) Then
            If (MsgBox("Do you want to remove " & selectedArtist.My_Name & " from " & session.Song_Object.Title, MsgBoxStyle.YesNo) = MsgBoxResult.No) Then
                MsgBox("NeverMind", MsgBoxStyle.SystemModal)
                FeatureSelection.Visibility = Windows.Visibility.Collapsed
                app.Dispatcher.Invoke(log, {action & " " & selectedArtist.My_Name & " canceled." & vbNewLine, ""})
                Return True
            End If
        ElseIf (MsgBox("Are you sure you want to add " & selectedArtist.My_Name & "'s name to this track?", MsgBoxStyle.YesNo) = MsgBoxResult.No) Then
            MsgBox("Canceled", MsgBoxStyle.SystemModal)
            FeatureSelection.Visibility = Windows.Visibility.Collapsed
            app.Dispatcher.Invoke(log, {action & " " & selectedArtist.My_Name & " canceled." & vbNewLine, ""})
            Return True



        End If

        Dim currentSessionNowPlaying = False

        If Not removeMe Then


            Select Case MsgBox("Is " & ArtistList.SelectedItem & " Already On " & session.Title & " " & vbNewLine & "and you just want to update the name?" & vbNewLine & "Pressing no will send this artist a request..", MsgBoxStyle.YesNo)
                Case MsgBoxResult.Yes

                    'addOrRemoveFeature()


                Case MsgBoxResult.No

                    SubmitFeatureRequest()
                    Return True

            End Select

            If (m_currentPlayingsong = session.Song_Object.MP3_File_Path) Then
                If (m_playerIsPlaying) Then
                    If (Application.ResetFloater()) Then
                        currentSessionNowPlaying = True
                    End If

                End If


            End If

            session.Song_Object.AddFeature(selectedArtist, False)

            session.CreateNotification(Notification.NotificationType.Verification, selectedArtist, "Are you featured on this song?", True, New Func(Of Boolean)(AddressOf FeatureVerificationCallBack))





        Else
            If (m_currentPlayingsong = session.Song_Object.MP3_File_Path) Then
                If (m_playerIsPlaying) Then
                    If (Application.ResetFloater()) Then
                        currentSessionNowPlaying = True
                    End If
                End If


            End If
            session.Song_Object.RemoveFeature(selectedArtist)


        End If

        'find the current file

        If (currentSessionNowPlaying) Then

            Application.PlayFloater(session.Song_Object.MP3_File_Path, session.Song_Object.Title)
            currentSessionNowPlaying = False
            m_playerIsPlaying = True

        End If


        SessionProperties.DataContext = session
        FeaturingTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget()
        Return True
    End Function
    Private Sub FeatureListItemChanged(sender As Object, e As SelectionChangedEventArgs) Handles ArtistList.SelectionChanged
        e.Handled = True
        If (m_addGroupMemberEnabled) Then
            m_addGroupMemberEnabled = False
            If Not (ArtistList.SelectedItem Is Nothing) Then
                Dim art As Artist = New Artist(TryCast(ArtistList.SelectedValue, String), False)
                setupSess.Get_Current_Artist().AddArtistToThisGroup(art.GLOBAL_ID, art.My_Name)
                FeatureSelection.Visibility = Windows.Visibility.Collapsed
                Return

            End If

        ElseIf Not (ArtistList.SelectedItem Is Nothing) Then

            addOrRemoveFeature()


        End If

        FeatureSelection.Visibility = Windows.Visibility.Collapsed

        ' ArtistList.SelectedIndex = Nothing

    End Sub
    Private Function FeatureVerificationCallBack() As Boolean
        'create a window to pop up asking if they are on this song

        ' session.Song_Object .RemoveFeature (
        Return False
    End Function
    Public Class TrackWrapper
        Implements IComparable(Of TrackWrapper)




        Private m_UsingArtistName As String
        Private m_TrackPath As String
        Private m_Base As BaseTrackInfoWrapper
        Private m_users As List(Of userartist)



        Private m_Songs As List(Of BaseTrackInfoWrapper)


        ReadOnly Property BeatNameCleanedUp As String
            Get
                Dim result As String = Path.GetFileNameWithoutExtension(m_TrackPath).Replace("MP3_Player_", "").Replace("SCLK2HW", "...")
                Return result

            End Get
        End Property


        ReadOnly Property BeatName As String
            Get
                Return Path.GetFileNameWithoutExtension(m_TrackPath)

            End Get
        End Property

        ReadOnly Property BeatPath As String
            Get
                Return m_TrackPath
            End Get
        End Property

        ReadOnly Property Has_Users As Boolean
            Get
                Return m_users.Count > 0

            End Get
        End Property



        ReadOnly Property Users As List(Of userartist)
            Get
                Return m_users
            End Get
        End Property

        ReadOnly Property File_Info As FileInfo
            Get
                Return m_Base.m_TrackFileInfo
            End Get
        End Property

        ReadOnly Property BaseTrackInfo As BaseTrackInfoWrapper
            Get
                Return m_Base
            End Get
        End Property

        Structure userartist
            Public Property name As String

            Public Property songs As List(Of usertrack)
        End Structure

        Structure usertrack
            Public Property name As String
            Public Property path As String
        End Structure



        Public Sub New(ByRef beatpath As String, Optional ByRef base As BaseTrackInfoWrapper = Nothing)

            m_TrackPath = beatpath
            m_users = New List(Of userartist)
            m_Songs = New List(Of BaseTrackInfoWrapper)
            If Not (base Is Nothing) Then
                m_Base = base
                m_TrackPath = base.m_TrackFileInfo.FullName
                For Each user As KeyValuePair(Of String, List(Of String)) In base.associatedArtists(True)
                    Dim ua As userartist = New userartist
                    ua.name = user.Key
                    Dim lst As List(Of usertrack) = New List(Of usertrack)
                    For Each sng As String In user.Value


                        'THIS IS JUST FOR TESTING
                        'If sng.ToLower.Contains(".mp3") Then
                        '    MsgBox(sng)
                        'End If


                        Dim ut As usertrack = New usertrack
                        ut.name = Path.GetFileNameWithoutExtension(sng)

                        ut.path = sng
                        lst.Add(ut)



                    Next
                    ua.songs = lst
                    m_users.Add(ua)


                Next
            End If
            

        End Sub


        Public Function CompareTo(other As TrackWrapper) As Integer Implements IComparable(Of TrackWrapper).CompareTo
            If (m_Base.m_TrackFileInfo.LastWriteTime > other.m_Base.m_TrackFileInfo.LastWriteTime) Then
                Return -1
            ElseIf (m_Base.m_TrackFileInfo.LastWriteTime < other.m_Base.m_TrackFileInfo.LastWriteTime) Then
                Return 1
            End If
            Return 0
        End Function
    End Class

    Public Delegate Sub lemLog(message As String, overallStat As String)








    Private Sub SoundClickSongChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_SoundClickSongList.SelectionChanged

        e.Handled = True

        selectedWrapper = TryCast(ui_SoundClickSongList.SelectedItem, TrackWrapper)



        ui_UsedSongList.DataContext = selectedWrapper

        Try
            Application.PlayFloater(selectedWrapper.File_Info.FullName, selectedWrapper.BeatName)

            m_playerIsPlaying = True
            m_currentPlayingsong = selectedWrapper.File_Info.FullName
            Application.Current_Playing_Song = m_currentPlayingsong
            Application.LemLog("Now Playing " & selectedWrapper.BeatName)

        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try




    End Sub

    Public Sub releasePlayer()

        RemoveLogicalChild(ui_Player_Host)

        ui_Player_Host.Dispose()
    End Sub

    Private Sub stopLemPlayer(sender As Object, e As RoutedEventArgs) Handles StopButton.Click
        ' ui_LemPlayer.stop()
        m_playerIsPlaying = False

    End Sub

    Private Sub ToggleScrewed(sender As Object, e As RoutedEventArgs) Handles PitchButton.Click


        ' LemPlayer.FLA_Style()


        e.Handled = True
        Dim btn As Button = TryCast(sender, Button)
        btn.GetBindingExpression(Button.BackgroundProperty).UpdateTarget()
        btn.GetBindingExpression(Button.ContentProperty).UpdateTarget()



        'If (screwed) Then

        '    If (LemPlayer.FLA_Style()) Then
        '        screwed = True
        '    End If
        'Else
        '    If (LemPlayer.resetPitch()) Then
        '        screwed = False
        '    End If
        'End If


    End Sub

    Private Async Function StartNow() As Task(Of Boolean)
        If (session Is Nothing) Then
            MsgBox("No song selected...")
            Application.LemLog("No session selected...")
            Return False

        End If


        'Dim selectedItem = ui_sessionsList.SelectedItem

        If Not (Application.Floating_Player Is Nothing) Then

            Application.StopFloater(True)



        End If



        Application.LemLog("Opening " & session.Title, "Please Wait...")
        Me.Hide()
        Dim result = Await setupSess.Open_Previous_Session(session)


       
        If (result) Then
            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.mode, LoginData.AppMode.liveSession.ToString())
            Me.Show()

            Post_Close_Session(session)
        Else
            Me.Show()
            Application.LemLog("", "")
        End If

        Application.BlockerWindow(False)

        Return True
    End Function

    Public Sub TakeoverMusicPlayer(player As FloatingLemPlayer)


        ' ui_LemPlayer = player.ui_LemPlayer


    End Sub

    Public Function GetLemPlayer() As AxCCLEMMUSICPLAYERACTIVEXV11Lib.AxCCLEMMUSICPLAYERACTIVEXV11
        ' ui_Player_Host.Child = Nothing
        'ui_LemPlayer.Dispose()
        ' ui_LemPlayer = Nothing
        Return Nothing

    End Function

    Public Function StopLemPlayer() As Boolean
        If m_playerIsPlaying Then
            ' ui_LemPlayer.stop()
            m_playerIsPlaying = False
        End If
        Return True
    End Function
    Private Sub ToggleTemporaryLemPlayerStop()
        If (m_playerIsPlaying) Then
            m_playerIsPlaying = False
            stopLemPlayer()
        Else

        End If
    End Sub
    Private Sub PlayCurrentSessionBounce(sender As Object, e As RoutedEventArgs) Handles TrackPlayButton.Click
        Dim path = session.Song_Object.GetBouncedSongPath(True)
        If Not (path = "") Then
            '  ui_LemPlayer.playTrack(session.Song_Object.MP3_File_Path)
            Application.PlayFloater(path, session.Song_Object.Title)

            m_playerIsPlaying = True
            m_currentPlayingsong = path
            Application.Current_Playing_Song = m_currentPlayingsong
            Application.LemLog("Now Playing Bounced Song " & session.Song_Object.Title)


        End If
    End Sub

    Private Async Sub OpenSession(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If (session Is Nothing) Then
            Return

        End If

        'Application.LemLog("Initializing " + session.Title, "Please wait...")
        If (Await StartNow()) Then

        End If


    End Sub

    Private Async Sub ui_sessionsList_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles ui_sessionsList.MouseDoubleClick
        e.Handled = True
        If (MsgBox("Open This Session ?", vbOKCancel) = MsgBoxResult.Ok
            ) Then
            If (Await StartNow()) Then


            End If

        End If

    End Sub

    Private Sub UsingArtistSongClicked(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True

        Dim box As ListBox = TryCast(sender, ListBox)
        Dim tw As TrackWrapper.usertrack = box.SelectedItem
        Dim selectedPath As String = tw.path
        If (selectedPath.ToLower.Contains(".mp3")) Then
            If (Path.GetExtension(selectedPath).ToLower = ".mp3") Then

                Application.PlayFloater(selectedPath, tw.name)


            End If
        End If

    End Sub

    Private Sub DeleteSong(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim index = ui_sessionsList.SelectedIndex
        If ui_sessionsList.SelectedItem Is Nothing Then
            MsgBox("No Session selected")
            Return

        End If
        Dim item As SetupSession.Session = TryCast(ui_sessionsList.SelectedItem, SetupSession.Session)

        If (MsgBox(String.Format("Are you sure you want to delete {0}?", item.Title), MsgBoxStyle.OkCancel, "Delete Session") = MsgBoxResult.Ok) Then
            ' ItemsControl.ItemsSource

            'ui_sessionsList.Items.Remove(item)
            ui_sessionsList.SelectedIndex = index + 1

            SessionProperties.DataContext = ui_sessionsList.SelectedItem

            If (setupSess.Delete_Session(item)) Then
                ui_sessionsList.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget()


                MsgBox("Its Gone")
            Else
                MsgBox("Problem")

            End If

        End If

    End Sub

    Private Sub SubmitFeatureRequest()
        Dim log As lemLog = New lemLog(AddressOf Application.LemLog)
        app.Dispatcher.Invoke(log, {"", "Submit Feature Request!!"})
        Dim sessArgs As Notification.NewNotificationArgs.sessionArgs = New Notification.NewNotificationArgs.sessionArgs
        If Not (ui_sessionsList.SelectedIndex < 0) Then
            sessArgs.sessionListID = ui_sessionsList.SelectedIndex
            sessArgs.session = ui_sessionsList.SelectedItem


        Else
            sessArgs.sessionListID = 0
            sessArgs.session = setupSess.GetCurrentSessions.Item(0)
            app.Dispatcher.Invoke(log, {"Nobody" & " selected..", ""})
        End If
        Dim artIndex As Integer = 0
        If Not (ArtistList.SelectedIndex = Nothing) Then
            artIndex = ArtistList.SelectedIndex
        End If

        m_currentArtistList.Clear()
        For Each art As Artist In Application.ArtistList
            If Not (art.My_Name = session.Artist_Name) Then

                m_currentArtistList.Add(art)

            End If



        Next


        Dim newNote = Notification.WriteNewNotification(setupSess.Get_Current_Artist, "Will you please Bless this track with your lyrical talents?", "Feature Request From " & setupSess.Get_Current_Artist.My_Name, sessArgs, Notification.SenderStatus.OtherArtist, Nothing, artIndex, m_currentArtistList)
        app.Dispatcher.Invoke(log, {"Request Sent", ""})
        ' MsgBox("Request Sent")
    End Sub
    Private Sub ui_RequestFeatureButton_Click(sender As Object, e As RoutedEventArgs) Handles ui_RequestFeatureButton.Click

        SubmitFeatureRequest()
        e.Handled = True


    End Sub

    Private Sub ReadMessagesClicked(sender As Object, e As RoutedEventArgs)
        Dim log As lemLog = New lemLog(AddressOf Application.LemLog)
        app.Dispatcher.Invoke(log, {"Checking for messages", "Get Messages"})
        getMessages(False)
    End Sub


    Public ReadOnly Property MusicPlayerIsPlaying As Boolean
        Get
            Return m_playerIsPlaying
        End Get
    End Property

    Public ReadOnly Property GetPlayerCurrentLoadedSong As String
        Get
            Return m_currentPlayingsong
        End Get
    End Property

    Private Sub updatePriority(sender As Object, e As RoutedEventArgs)
        Dim btn As Button = TryCast(sender, Button)


        Dim priorities As SongData.SongPriority = setupSess.GetSongPrioritys()

        Dim btnName As String = TryCast(btn.Content, String)


        If (btnName.Equals("Up")) Then
            priorities.Up(session.Song_Object.GLOBAL_ID)

        ElseIf (btnName.Equals("Down")) Then

            priorities.Down(session.Song_Object.GLOBAL_ID)

        ElseIf (btnName.Equals("First")) Then

            priorities.Top(session.Song_Object.GLOBAL_ID)

        ElseIf (btnName.Equals("Last")) Then
            priorities.Bottom(session.Song_Object.GLOBAL_ID)

        ElseIf (btnName.Equals("Add")) Then
            priorities.AddPriority(session.Song_Object.GLOBAL_ID)
        ElseIf (btnName.Equals("Remove")) Then
            priorities.RemovePriority(session.Song_Object.GLOBAL_ID)

        End If




        BindingOperations.GetMultiBindingExpression(UI_LocalPriorityValue, TextBox.TextProperty).UpdateTarget()
        ' BindingOperations.GetMultiBindingExpression(UI_PriorityGrid, Grid.BackgroundProperty).UpdateTarget()







    End Sub

    Private Sub UI_updatePriorityButton_Click_1(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If UI_updatePriorityButton.Content = "Change" Then

            UI_editPriorityButtonsGrid.Visibility = Windows.Visibility.Visible

            UI_updatePriorityButton.Content = "Commit"

        Else
            UI_editPriorityButtonsGrid.Visibility = Windows.Visibility.Collapsed

            UI_updatePriorityButton.Content = "Change"

            setupSess.GetSongPrioritys().StoreData()


            'XMLResources.SubmitResources()




        End If






    End Sub

    Private Sub UpdateReadyForMixStatus(sender As Object, e As RoutedEventArgs)
        e.Handled = True

        If (session.Song_Object.MixReady = True) Then
            session.Song_Object.MixReady = False
        Else
            session.Song_Object.MixReady = True


        End If

        UI_MixReadyUpdate.GetBindingExpression(TextBox.TextProperty).UpdateTarget()



    End Sub

    Private Sub Logout(sender As Object, e As RoutedEventArgs)
        Me.Close()

    End Sub

    Private Sub UI_GroupMember_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True

        m_currentArtistList.Clear()
        ArtistList.UnselectAll()

        If Not (ArtistList.Items.Count > 1) Then
            ArtistList.Items.Clear()
            For Each art As Artist In Application.ArtistList
                If Not (art.My_Name = session.Artist_Name) Then
                    ArtistList.Items.Add(art.My_Name)
                    m_currentArtistList.Add(art)

                End If



            Next
        End If
        FeatureSelection.Visibility = Windows.Visibility.Visible
        m_addGroupMemberEnabled = True

    End Sub


    Private Sub ui_BeatSelectBtn_Click(sender As Object, e As RoutedEventArgs)
        Dim count = Application.AllProducersAndBeats.Count
        ui_SoundClickSongList.DataContext = Nothing

        m_CurrentBeatsList += 1
        If (m_CurrentBeatsList > count) Then
            m_CurrentBeatsList = 0
        End If

        If (m_CurrentBeatsList = 0) Then
            ui_SoundClickSongList.DataContext = Application.SoundClickList
            ui_BeatSourceName.Content = "SOUNDCLICK"
        Else
            ui_SoundClickSongList.DataContext = Application.AllProducersAndBeats.Item(m_CurrentBeatsList - 1).Item2
            ui_BeatSourceName.Content = Application.AllProducersAndBeats.Item(m_CurrentBeatsList - 1).Item1

        End If
        ui_SoundClickSongList.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget()



    End Sub
End Class
