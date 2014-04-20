Public Class NotificationWindow
    Private session As SetupSession.Session
    Private songPathHolder As String
    Private currentNotification As Notification
    Private currentFromArtist As Artist
    Private currentToArtist As Artist
    Dim MultiMessageMode As Boolean

    Public Event MessageRead(sendeeGID As String, messageID As Integer)

    Public Structure MessageArgs
        Property Subject As String
        Property Message As String

        Property ToArtist As Integer
        Property to_Artist As Artist
        Property from_Artist As Artist
        Property IsReply As Boolean
        Property RepsonseRequired As Boolean

        Property isResponseFromSender As Boolean
        Property isResponseFromSendee As Boolean

        Property LinkSession As Integer
        Property sessionArgs As Notification.NewNotificationArgs.sessionArgs

        
    End Structure

    Public Enum MessageMode
        read
        write
        view_all
        show_sent
    End Enum
    Private currentmode As MessageMode
    Private fromArtist As Artist
    Private Myargs As MessageArgs = New MessageArgs

    Public Function ShowMessageList(list As List(Of Notification), sentList As List(Of Notification)) As MessageArgs
        CollapseAll()

        MultiMessageMode = True

        Dim oldMessages As List(Of Notification) = New List(Of Notification)
        Dim newMessages As List(Of Notification) = New List(Of Notification)

        For Each noti As Notification In list
            If noti.HasBeenRead Then
                oldMessages.Add(noti)
            Else
                newMessages.Add(noti)

            End If
        Next


        ' ui_AllNotifications_RecievedMessagesTab.DataContext = list

        ui_MainScreen_AlreadyReadMessages_DataGrid.DataContext = oldMessages
        ui_NotificationsGrid.DataContext = newMessages
        ui_AllNotifications_SentMessagesTab.DataContext = sentList

        ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Visible
        Me.ShowDialog()
        Return Myargs

    End Function

    Public Function ShowNotificationDialog(mode As MessageMode, Optional starterMessage As String = "") As MessageArgs
        CollapseAll()
        ' Myargs = New MessageArgs
        If mode = MessageMode.write And Not starterMessage = "" Then
            ui_CreateNotificationScreen_Message.Text = starterMessage
        End If
        currentmode = mode

        Select Case currentmode
            Case MessageMode.read
                ui_MessageScreen.Visibility = Windows.Visibility.Visible
                ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Collapsed
                ui_CreateNotificationScreen.Visibility = Windows.Visibility.Collapsed

            Case MessageMode.view_all
                ui_MessageScreen.Visibility = Windows.Visibility.Collapsed
                ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Visible
                ui_CreateNotificationScreen.Visibility = Windows.Visibility.Collapsed

            Case MessageMode.write
                ui_MessageScreen.Visibility = Windows.Visibility.Collapsed
                ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Collapsed
                ui_CreateNotificationScreen.Visibility = Windows.Visibility.Visible

            Case Else

        End Select


        Me.ShowDialog()
        Return Myargs



    End Function

    Public Sub EnableSessionButtons(bars As String, verse As String, isBounced As Boolean)
        ui_FTRQ_SessionButtons.Visibility = Windows.Visibility.Visible
        If (isBounced) Then
            ui_FTRQ_PlaySongButton.Visibility = Windows.Visibility.Visible
        Else
            ui_FTRQ_PlaySongButton.Visibility = Windows.Visibility.Hidden
        End If

        ui_FTRQ_BarsText.Text = bars
        ui_FTRQ_VerseText.Text = verse

    End Sub
    Public Sub ShowSentWindow(notif As Notification)
        CollapseAll()
        ui_MessageSentScreen.Visibility = Windows.Visibility.Visible
        ui_MessageSentScreen_Recipient.Text = notif.My_Notification_Args.toArtist.My_Name
        ui_MessageSEntScreen_Attachment.Text = notif.My_Notification_Args.sessArgs.session.Title
        ui_MessageSEntScreen_Date.Text = notif.My_Notification_Args.creationdate
        ui_MessageSentScreen_Body.Text = notif.My_Notification_Args.message
        Me.ShowDialog()

    End Sub

    Private Sub CollapseAll()
        ui_CreateNotificationScreen.Visibility = Windows.Visibility.Collapsed
        ui_MessageSentScreen.Visibility = Windows.Visibility.Collapsed
        ui_MessageScreen.Visibility = Windows.Visibility.Collapsed
        ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Collapsed
    End Sub


    Private Sub ReadMessage(sender As Object, e As SelectionChangedEventArgs) Handles ui_NotificationsGrid.SelectionChanged, ui_SentNotificationsGrid.SelectionChanged, ui_MainScreen_AlreadyReadMessages_DataGrid.SelectionChanged
        Dim dt As DataGrid = CType(sender, DataGrid)

        ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Collapsed


        ' ui_MessageScreen.DataContext = dt.SelectedItem
        Dim noti As Notification = TryCast(dt.SelectedItem, Notification)
        If (noti Is Nothing) Then
            ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Visible
            Return
        End If
        currentNotification = noti
        currentFromArtist = New Artist(noti.FromArtist, False)
        currentToArtist = New Artist(noti.To_Artist_Name, False)


        If (dt.Name = ui_MainScreen_AlreadyReadMessages_DataGrid.Name) Then

        End If

        If (dt.Name = ui_SentNotificationsGrid.Name) Then
            ui_MessageSentScreen.Visibility = Windows.Visibility.Visible

            ui_MessageSentScreen.Visibility = Windows.Visibility.Visible
            ui_MessageSentScreen_Recipient.Text = Utils.GetNameByGlobalID(noti.ToArtist)

            If Not (noti.My_Notification_Args.sessArgs.session Is Nothing) Then
                ui_MessageSEntScreen_Attachment.Text = noti.My_Notification_Args.sessArgs.session.Title
            End If

            ui_MessageSEntScreen_Date.Text = noti.CreationDate
            ui_MessageSentScreen_Body.Text = noti.Get_Message
            ui_MessageSent_BackButton.Visibility = Windows.Visibility.Visible
            Return
        End If
        ui_MainMessageScreen_Body.Text = noti.Get_Message
        ui_From.Text = noti.FromArtist

        If Not (noti.My_Notification_Args.sessArgs.session Is Nothing) Then
            ui_MessageSEntScreen_Attachment.Text = noti.My_Notification_Args.sessArgs.session.Title
            ui_FTRQ_BarsText.Text = noti.My_Notification_Args.sessArgs.requestedBars & " needed"
            ui_FTRQ_VerseText.Text = "On the " & noti.My_Notification_Args.sessArgs.whichVerse & " verse"

            If Not noti.Subject.Contains("RESPONSE TO") Then
                ui_MainMessageScreen_Body.Text += vbNewLine &
                    "Please provide " & noti.My_Notification_Args.sessArgs.requestedBars & " on the " & noti.My_Notification_Args.sessArgs.whichVerse & " verse."

                ui_MainMessageScreen_Body.Text += vbNewLine & "You can choose to listen to the song first by pressing the play button instead of the 'record now' button.."



            End If


            ui_MainMessageScreen_SubjectText.Text = noti.Subject
            If (noti.My_Notification_Args.sessArgs.session.GetSong().IsBounced) Then

                songPathHolder = noti.My_Notification_Args.sessArgs.session.Song_Object.MP3_File_Path


                ui_FTRQ_PlaySongButton.Visibility = Windows.Visibility.Visible
            Else
                ui_FTRQ_PlaySongButton.Visibility = Windows.Visibility.Hidden
            End If
            ui_FTRQ_SessionButtons.Visibility = Windows.Visibility.Visible

        End If

        ui_MessageScreen.Visibility = Windows.Visibility.Visible


        'Select Case dt.Name
        '    Case ui_NotificationsGrid.Name

        '        ui_MessageScreen.DataContext = dt.SelectedItem
        '        ui_MessageScreen.Visibility = Windows.Visibility.Visible


        '    Case ui_SentNotificationsGrid.Name
        '        ui_MessageSentScreen.DataContext = dt.SelectedItem
        '        ui_MessageSentScreen.Visibility = Windows.Visibility.Visible

        'End Select

        RaiseEvent MessageRead(noti.ToArtist, noti.NotificationID)

        e.Handled = True
    End Sub

    Public Event FireSelectSendee(sendeeIndex As Integer)
    Public Event FireSessionSelected(index As Integer)
    Public Event FireSendButtonClicked(args As MessageArgs)

    Public Function DisplayNotification(args As Notification.NewNotificationArgs) As MessageArgs

        CollapseAll()
        fromArtist = args.fromEntity
        currentFromArtist = args.fromEntity
        currentToArtist = args.toArtist

        ui_From.Text = args.fromEntity.My_Name
        ui_MainMessageScreen_Body.Text = args.message
        If Not (args.sessArgs.session Is Nothing) Then
            ui_FTRQ_SessionButtons.Visibility = Windows.Visibility.Visible
            ui_FTRQ_BarsText.Text = args.sessArgs.requestedBars & " needed"
            ui_FTRQ_VerseText.Text = "on the " & args.sessArgs.whichVerse & " verse"
            If args.sessArgs.session.Song_Object.IsBounced Then
                ui_FTRQ_PlaySongButton.Visibility = Windows.Visibility.Visible
            End If
            session = args.sessArgs.session
        End If
        ui_MessageScreen.Visibility = Windows.Visibility.Visible
        If (Me.ShowDialog()) Then

            Return Myargs
        End If

        Return Myargs
    End Function


    Private Sub ui_CreateNotificationScreen_ComboBox_To_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_CreateNotificationScreen_ComboBox_To.SelectionChanged

        'Dim result = ui_CreateNotificationScreen_ComboBox_To.SelectedIndex



    End Sub

    Private Sub ui_CreateNotificationScreen_ComboBox_LinkSession_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_CreateNotificationScreen_ComboBox_LinkSession.SelectionChanged

        ' Dim result = ui_CreateNotificationScreen_ComboBox_LinkSession.SelectedIndex
        ' RaiseEvent FireSessionSelected(result)

    End Sub

    Private Sub ui_SendButton_Click(sender As Object, e As RoutedEventArgs) Handles ui_SendButton.Click

        Select Case currentmode

            Case MessageMode.write
                If (ui_CreateNotificationScreen_Message.Text = "") Then
                    If (MsgBox("Message Empty, Nothing Sent", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok) Then
                        Me.Close()
                    Else
                        Return
                    End If
                ElseIf ui_CreateNotificationScreen_ComboBox_To.SelectedIndex < 0 Then
                    MsgBox("Please Select an Artist to send this to.")

                    Return
                End If

                If (ui_FeatureRequestSection.Visibility = Windows.Visibility.Visible) Then

                    If (ui_CreateNotificationScreen_ComboBox_LinkSession.SelectedIndex < 0) Then
                        MsgBox("Session needs to be linked, Choose the song from 'Link Session'")
                        Return
                    ElseIf ui_FeatureRequestSection_Bars.SelectedIndex = -1 Then
                        MsgBox("How Many Bars?")
                        Return
                    ElseIf ui_FeatureRequestSection_Verse.SelectedIndex = -1 Then
                        MsgBox("Which Verse?")
                        Return


                    End If

                End If

            Case Else
                Return

        End Select
        


        Dim args As MessageArgs = New MessageArgs
        Dim sessArgs As Notification.NewNotificationArgs.sessionArgs = New Notification.NewNotificationArgs.sessionArgs
        sessArgs.requestedBars = ui_FeatureRequestSection_Bars.Text
        sessArgs.sessionListID = ui_CreateNotificationScreen_ComboBox_LinkSession.SelectedIndex
        sessArgs.session = Application.CurrentArtist.GetSetupSessionObject.Get_Current_Session
        sessArgs.whichVerse = ui_FeatureRequestSection_Verse.Text
        Myargs.Message = ui_CreateNotificationScreen_Message.Text

        Myargs.Subject = ui_CreateNotificationScreen_Subject.Text

        Myargs.ToArtist = ui_CreateNotificationScreen_ComboBox_To.SelectedIndex

        Myargs.LinkSession = ui_CreateNotificationScreen_ComboBox_LinkSession.SelectedIndex

        Myargs.sessionArgs = sessArgs
        Me.Close()


    End Sub

    Private Sub ui_CreateNotificationScreen_Message_GotFocus(sender As Object, e As RoutedEventArgs) Handles ui_CreateNotificationScreen_Message.GotFocus

        If ui_CreateNotificationScreen_Message.Text = "Write your message here" Then
            ui_CreateNotificationScreen_Message.Text = ""
        End If
    End Sub

    Private Sub ui_FeatureRequestSection_Bars_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_FeatureRequestSection_Bars.SelectionChanged
        If (ui_FeatureRequestSection_Bars.SelectedIndex = 4) Then
            ui_FeatureRequestSection_Bars.IsEditable = True
            ui_FeatureRequestSection_Bars.Text = "Enter Total Bars"


        End If
    End Sub

    Private Sub ui_FeatureRequestSection_Bars_TextInput(sender As Object, e As TextCompositionEventArgs) Handles ui_FeatureRequestSection_Bars.TextInput
        If (ui_FeatureRequestSection_Bars.Text = "Enter Total Bars") Then
            ui_FeatureRequestSection_Bars.Text = ""
        End If
    End Sub

    Private Sub ui_FeatureRequestSection_Verse_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ui_FeatureRequestSection_Verse.SelectionChanged
        If (ui_FeatureRequestSection_Verse.SelectedIndex = 4) Then
            ui_FeatureRequestSection_Verse.IsEditable = True
            ui_FeatureRequestSection_Verse.Text = "Any Verse"

        End If
    End Sub

    Private Sub ui_FeatureRequestSection_Verse_TextInput(sender As Object, e As TextCompositionEventArgs) Handles ui_FeatureRequestSection_Verse.TextInput
        If (ui_FeatureRequestSection_Verse.Text = "Any Verse") Then
            ui_FeatureRequestSection_Verse.Text = ""
        End If
    End Sub
    Public Delegate Sub playfloaterDel(path As String, name As String)
    Private Sub ui_FTRQ_PlaySongButton_Click(sender As Object, e As RoutedEventArgs) Handles ui_FTRQ_PlaySongButton.Click
        If (songPathHolder Is Nothing) Then
            Return
        End If
        Dim play As playfloaterDel = New playfloaterDel(AddressOf Application.PlayFloater)
        Dim app As Application = Application.Current

        app.Dispatcher.Invoke(play, {songPathHolder, songPathHolder})



        e.Handled = True
    End Sub

    Private Sub Reply(sender As Object, e As RoutedEventArgs) Handles ui_ReplyButton.Click
        e.Handled = True

        If (MultiMessageMode) Then

        End If

        If (ui_MessageScreen_ReplyText.Text = "") Then
            MsgBox("Reply Box is empty.")
            Return
        End If


        If Not currentNotification Is Nothing Then
            If (currentNotification.ResponseFromSendee) Then
                Myargs.isResponseFromSendee = False
                Myargs.isResponseFromSender = True
            Else
                Myargs.isResponseFromSendee = True
                Myargs.isResponseFromSender = False
            End If
        Else
            Myargs.isResponseFromSender = False
            Myargs.isResponseFromSendee = True
        End If
        

        Myargs.Message = ui_MessageScreen_ReplyText.Text
        Myargs.IsReply = True
        Myargs.RepsonseRequired = False
        Myargs.Subject = ui_MainMessageScreen_SubjectText.Text

        If Not currentToArtist Is Nothing Then
            Myargs.to_Artist = currentToArtist


        End If
        If Not currentFromArtist Is Nothing Then
            Myargs.from_Artist = currentFromArtist
        End If




        Dim sessArgs As Notification.NewNotificationArgs.sessionArgs = New Notification.NewNotificationArgs.sessionArgs
        If Not currentNotification Is Nothing Then
            If Not currentNotification.My_Notification_Args.sessArgs.session Is Nothing Then
                sessArgs.session = currentNotification.My_Notification_Args.sessArgs.session
                sessArgs.requestedBars = currentNotification.My_Notification_Args.sessArgs.requestedBars
                sessArgs.whichVerse = currentNotification.My_Notification_Args.sessArgs.whichVerse

            End If
        End If

        Myargs.sessionArgs = sessArgs

        Me.Close()




    End Sub

    Private Sub ui_BackButton_Click(sender As Object, e As RoutedEventArgs) Handles ui_BackButton.Click
        CollapseAll()
        ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub ui_MessageSent_BackButton_Click(sender As Object, e As RoutedEventArgs) Handles ui_MessageSent_BackButton.Click
        ui_MessageSent_BackButton.Visibility = Windows.Visibility.Hidden
        ui_MessageSentScreen.Visibility = Windows.Visibility.Collapsed
        ui_AllNotifiacationsScreen.Visibility = Windows.Visibility.Visible
        e.Handled = True
    End Sub
End Class
