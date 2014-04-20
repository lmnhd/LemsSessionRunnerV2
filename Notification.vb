Imports System.Collections.ObjectModel

Public Class Notification
    Implements IComparable(Of Notification)



    Private Shared resources As XMLResources
    Private Shared staticArgs As NewNotificationArgs
    'Private creatingArtist As Artist
    Public NotificationID As Integer
    Private MyArgs As NewNotificationArgs
    Private n_from As String
    Private n_to As String
    Private messageTitle As String
    Private message As String
    Private atLogin As Boolean = False

    Private Shared CurrentMessages As List(Of Notification)



    Private Shared WithEvents window As NotificationWindow

    Private callBack As Func(Of Boolean)
    Dim SendingArtist As Artist

    Property CreationDate As DateTime

    Property HasBeenRead As Boolean
    Property ResponseRequired As Boolean

    ' Property ContainsUnreadResponse As Boolean
    Property ResponseFromSender As Boolean
    Property ResponseFromSendee As Boolean

    Shared Property MySessionList As ObservableCollection(Of SetupSession.Session)
    ReadOnly Property Subject As String
        Get
            Return messageTitle
        End Get

    End Property

    Private MyType As NotificationType
    Private SendStat As SenderStatus

    Private Shared ResourcesNode As XElement
    Private Shared NotificationNode As XElement


    Private MyNode As XElement


    Public Enum NotificationType

        Notification
        Verification

    End Enum

    ReadOnly Property Get_Message As String
        Get
            Return message
        End Get
    End Property

    ReadOnly Property ToArtist As String
        Get
            Return n_to
        End Get

    End Property

    ReadOnly Property To_Artist_Name As String
        Get
            Return Utils.GetNameByGlobalID(n_to)

        End Get
    End Property

    ReadOnly Property FromArtist As String
        Get
            Return Identity.GlobalID.FindNameByGlobalID(n_from)

        End Get
    End Property



    Public Enum SenderStatus

        CEO
        Lem
        OtherArtist
        NonUntame



    End Enum

    Public Structure NewNotificationArgs
        Property atLogin As Boolean
        Property callBack As [Delegate]
        Property IsResponseFromSender As Boolean
        Property IsResponseFromSendee As Boolean
        Property ResponseRequired As Boolean
        Property message As String
        Property header As String
        Property fromEntity As Artist
        Property toArtist As Artist
        Property notificationType As NotificationType
        Property sendStat As Notification.SenderStatus
        Property creationdate As DateTime
        Property attachment As Object
        Property sessArgs As sessionArgs



        Structure sessionArgs
            Property sessionListID As Integer
            Property session As SetupSession.Session
            Property requestedBars As String
            Property whichVerse As String
        End Structure
    End Structure

    Public Structure SentMessageArgs
        Property toArtist As Artist
        Property fromArtist As Artist
        Property messageID As Integer
    End Structure

    Public Sub New()
        staticArgs = New NewNotificationArgs
    End Sub



    Public Shared Function NotificationTypeFromString(type As String) As NotificationType

        Select Case type
            Case "Notification"
                Return NotificationType.Notification

            Case "Verification"
                Return NotificationType.Verification


        End Select

        Return Nothing

    End Function

    Public Shared Function SenderStatFromString(type As String) As SenderStatus

        Select Case type
            Case "CEO"
                Return SenderStatus.CEO
            Case "Lem"
                Return SenderStatus.Lem
            Case "OtherArtist"
                Return SenderStatus.OtherArtist
            Case "NonUntame"
                Return SenderStatus.NonUntame
            Case Else
                Return SenderStatus.OtherArtist

        End Select
    End Function


    Public Sub Display()

        'staticArgs = New NewNotificationArgs

        window = New NotificationWindow()
        staticArgs.message = message
        staticArgs.fromEntity = New Artist(Identity.GlobalID.FindNameByGlobalID(n_from), False)


        'staticArgs.message = message
        'staticArgs.fromEntity = New Artist(n_from, False)
        staticArgs.toArtist = New Artist(To_Artist_Name, False)

        window.ui_MessageSent_BackButton.Visibility = Visibility.Hidden
        window.ui_BackButton.Visibility = Visibility.Hidden

        Dim result As NotificationWindow.MessageArgs = window.DisplayNotification(staticArgs)

        If result.IsReply Then
            staticArgs.IsResponseFromSendee = True
            staticArgs.message = result.Message
            staticArgs.sendStat = SenderStatus.OtherArtist
            staticArgs.toArtist = staticArgs.fromEntity
            staticArgs.fromEntity = result.to_Artist
            staticArgs.header = "RESPONSE TO " & staticArgs.header
            staticArgs.creationdate = Now


            If CreateNotification(staticArgs) Then
                MsgBox("Your Reply has been sent")
            End If

        End If
        UpdateHasBeenRead(n_to, NotificationID)

    End Sub



    Public Shared Sub DisplayAllNotifications(artist As Artist, notifications As Integer(), sentMessages As List(Of Notification))
        If (CurrentMessages Is Nothing) Then
            CurrentMessages = New List(Of Notification)
        End If
        CurrentMessages.Clear()
        For Each node As XElement In GetNotificationNode.Elements
            If node.Name = artist.GLOBAL_ID Then
                For Each message As XElement In node.Elements
                    For Each i As Integer In notifications
                        If Integer.Parse(message.Attribute("id").Value) = i Then
                            CurrentMessages.Add(Notification.GetNotification(artist, i))

                        End If
                    Next
                Next
            End If
        Next

        CurrentMessages.Sort()


        window = New NotificationWindow

        'window.DataContext = CurrentMessages
        Dim result As NotificationWindow.MessageArgs = window.ShowMessageList(CurrentMessages, sentMessages)
        If (result.IsReply) Then

            Dim replyArgs As NewNotificationArgs = New NewNotificationArgs


            replyArgs.IsResponseFromSendee = result.isResponseFromSendee
            replyArgs.IsResponseFromSender = result.isResponseFromSender


            replyArgs.message = result.Message
            replyArgs.sendStat = SenderStatus.OtherArtist
            replyArgs.toArtist = result.from_Artist
            replyArgs.fromEntity = result.to_Artist
            replyArgs.header = "RESPONSE TO : " & result.Subject
            replyArgs.creationdate = Now
            replyArgs.notificationType = NotificationType.Verification
            replyArgs.sessArgs = result.sessionArgs
            replyArgs.ResponseRequired = result.RepsonseRequired

            replyArgs.atLogin = True


            If CreateNotification(replyArgs) Then
                MsgBox("Your Reply has been sent")
            Else
                MsgBox("Problem sending message")
            End If
        End If

    End Sub


    Public Shared Function GetNotificationNode(Optional sendeeGID As String = "", Optional messageID As Integer = 0) As XElement

        If NotificationNode Is Nothing Then
            NotificationNode = XMLResources.GetResource(XMLResources.ResourceType.NewType, 0, "Notification")
        End If
        If Not sendeeGID = "" And messageID > 0 Then
            For Each myNode As XElement In NotificationNode.Elements
                If myNode.Name = sendeeGID Then
                    For Each messageNode As XElement In myNode.Elements
                        If messageNode.Attribute("id").Value = messageID Then

                            Return messageNode
                        End If
                    Next
                End If
            Next
        End If


        Return NotificationNode
    End Function
    Public Shared Function WriteNewNotification(letterfrom As Artist, Optional defaultMessage As String = "", Optional Subj As String = "", Optional sessionArgs As NewNotificationArgs.sessionArgs = Nothing, Optional status As SenderStatus = SenderStatus.OtherArtist, Optional attachment As Object = Nothing, Optional artistListID As Integer = -1, Optional artistList As List(Of Artist) = Nothing) As Notification


        Dim isFeatureRequest = False

        window = Nothing
        window = New NotificationWindow



        window.ui_CreateNotificationScreen.Visibility = Visibility.Visible
        window.ui_MessageScreen.Visibility = Visibility.Collapsed
        window.ui_AllNotifiacationsScreen.Visibility = Visibility.Collapsed
        MySessionList = letterfrom.Get_Sessions.GetCurrentSessions

        If (artistList Is Nothing) Then
            window.ui_CreateNotificationScreen_ComboBox_To.ItemsSource = Application.ArtistList
        Else
            window.ui_CreateNotificationScreen_ComboBox_To.ItemsSource = artistList
        End If

        If (artistListID > -1) Then
            window.ui_CreateNotificationScreen_ComboBox_To.SelectedIndex = artistListID
        End If

        window.ui_CreateNotificationScreen_ComboBox_LinkSession.ItemsSource = MySessionList

        If Not defaultMessage = "" Then
            window.ui_CreateNotificationScreen_Message.Text = defaultMessage
        Else
            window.ui_CreateNotificationScreen_Message.Text = ("Write your message here")

        End If

        If Not (Subj = "") Then
            window.ui_CreateNotificationScreen_Subject.Text = Subj

        End If

        If sessionArgs.sessionListID > -1 Then

            isFeatureRequest = True
            window.ui_CreateNotificationScreen_ComboBox_LinkSession.SelectedIndex = sessionArgs.sessionListID
            window.ui_FeatureRequestSection.Visibility = Visibility.Visible
            window.ui_FeatureRequestSection_Bars.SelectedIndex = 3
            window.ui_FeatureRequestSection_Verse.SelectedIndex = 2




        End If



        Dim ntf As Notification = New Notification(NotificationType.Notification, letterfrom.My_Name, "")

        Dim mesArgs = window.ShowNotificationDialog(NotificationWindow.MessageMode.write)
        Dim temp As Notification.NewNotificationArgs.sessionArgs = mesArgs.sessionArgs

        If (temp.session Is Nothing) Then
            temp.session = sessionArgs.session
            If (temp.session Is Nothing) Then
                Throw New Exception("GET IT RIGHT!!")

            End If
        End If





        If (mesArgs.ToArtist < 0) Then
            Return Nothing
        End If
        Dim recipientArtist As Artist = artistList.Item(mesArgs.ToArtist)
        If Not (mesArgs.Message = "") And Not (mesArgs.Subject = "") Then


            staticArgs.toArtist = recipientArtist
            staticArgs.atLogin = True
            staticArgs.IsResponseFromSendee = False
            staticArgs.IsResponseFromSender = False
            staticArgs.ResponseRequired = False
            staticArgs.creationdate = Now
            staticArgs.sendStat = status
            staticArgs.notificationType = NotificationType.Notification
            staticArgs.fromEntity = letterfrom
            staticArgs.header = mesArgs.Subject
            staticArgs.attachment = attachment
            staticArgs.message = mesArgs.Message


            staticArgs.sessArgs = temp








            If (CreateNotification(staticArgs)) Then
                'XMLResources.SubmitResources()
                ntf.MyArgs = staticArgs
                window = New NotificationWindow
                window.ShowSentWindow(ntf)
                Return ntf
            End If
        End If
        Return Nothing
    End Function

    Private Shared Sub SelectSendee(index As Integer) Handles window.FireSelectSendee
        ' n_to = Application.ArtistList.Item(index).LOCAL_ID



    End Sub

    Public Shared Function CreateNotification(args As NewNotificationArgs) As Boolean
        'Check if resources is null
        If Not (args.fromEntity.Resources Is Nothing) Then
            resources = args.fromEntity.Resources
        Else
            Application.LEM_ERR("No Resource Object")
            Return False
        End If
        Dim message As Dictionary(Of String, Dictionary(Of String, String)) = New Dictionary(Of String, Dictionary(Of String, String))

        Dim messageAtts As Dictionary(Of String, String) = New Dictionary(Of String, String)
        Dim notificationAtts As Dictionary(Of String, String) = New Dictionary(Of String, String)
        ' notificationAtts.Add("id", "")
        messageAtts.Add("to", args.toArtist.GLOBAL_ID)
        messageAtts.Add("from", args.fromEntity.GLOBAL_ID)
        messageAtts.Add("header", args.header)
        messageAtts.Add("type", args.notificationType.ToString)
        messageAtts.Add("senderStatus", args.sendStat.ToString)
        messageAtts.Add("atLogin", args.atLogin.ToString)
        messageAtts.Add("IsResponseFromSender", args.IsResponseFromSender.ToString)
        messageAtts.Add("IsResponseFromSendee", args.IsResponseFromSendee.ToString)
        messageAtts.Add("HasBeenRead", Boolean.FalseString)
        messageAtts.Add("originalDate", Now)
        messageAtts.Add("responseRequired", args.ResponseRequired)
        If Not (args.sessArgs.session Is Nothing) Then
            messageAtts.Add("FeatureForSession", args.sessArgs.session.Song_Object.LOCAL_ID)
            messageAtts.Add("FeatureBarsRequested", args.sessArgs.requestedBars)
            messageAtts.Add("FeatureVerse", args.sessArgs.whichVerse)
        End If




        'messageAtts.Add("messageNum", 1)
        messageAtts.Add("responseFromSender", args.IsResponseFromSender.ToString)
        messageAtts.Add("responseFromSendee", args.IsResponseFromSendee.ToString)

        message.Add("message", messageAtts)
        Dim resArgs As XMLResources.AddResourceArgs = New XMLResources.AddResourceArgs
        resArgs.type = XMLResources.ResourceType.NewType
        resArgs.newTypeName = "Notification"
        resArgs.attParams = notificationAtts
        resArgs.NestedParams = message
        resArgs.TextContent = args.message
        resArgs.ParentSubNode = args.toArtist.GLOBAL_ID
        'resArgs .


        Dim mesID As Integer = resources.AddResource(resArgs)


        If (mesID > 0) Then
            Dim sma As SentMessageArgs = New SentMessageArgs
            sma.fromArtist = args.fromEntity
            sma.messageID = mesID
            sma.toArtist = args.toArtist
            If (StoreSentMessageNode(sma)) Then
                XMLResources.SubmitResources()
                Return True
            End If


        End If



        Return False





    End Function

    Private Shared Function StoreSentMessageNode(sentMessageArgs As SentMessageArgs) As Boolean

        Dim resArgs As XMLResources.AddResourceArgs = New XMLResources.AddResourceArgs
        resArgs.newTypeName = "NotificationHistory"
        resArgs.ParentSubNode = sentMessageArgs.fromArtist.GLOBAL_ID
        resArgs.type = XMLResources.ResourceType.NewType
        Dim sentMessageData As Dictionary(Of String, Dictionary(Of String, String)) = New Dictionary(Of String, Dictionary(Of String, String))




        Dim atts As Dictionary(Of String, String) = New Dictionary(Of String, String)
        atts.Add("ArtistID", sentMessageArgs.toArtist.GLOBAL_ID)
        atts.Add("MessageID", sentMessageArgs.messageID)

        sentMessageData.Add("sent", atts)
        resArgs.NestedParams = sentMessageData
        If resources.AddResource(resArgs) Then

        End If

        Return True
    End Function
    Delegate Sub LemLogDelegate(message As String, overallStatus As String)

    Public Shared Function GetNotification(artist As Artist, id As Integer) As Notification

        Dim app As Application = Application.Current
        Dim log As LemLogDelegate = New LemLogDelegate(AddressOf app.LemLog)

        Dim result As Notification = Nothing

        For Each subNode As XElement In GetNotificationNode.Elements
            If (subNode.Name = artist.GLOBAL_ID) Then
                For Each messageNode As XElement In subNode.Elements
                    If (Integer.Parse(messageNode.Attribute("id").Value) = id) Then

                        app.Dispatcher.Invoke(log, {"Found message from " & messageNode.Attribute("from").Value, "Get Messages"})


                        Dim type As NotificationType = Notification.NotificationTypeFromString(messageNode.Attribute("type").Value)


                        result = New Notification(type, messageNode.Attribute("from").Value, messageNode.Attribute("to").Value)
                        result.atLogin = Boolean.Parse(messageNode.Attribute("atLogin").Value)
                        'result.ContainsUnreadResponse = Boolean.Parse(messageNode.Attribute("HasBeenRead").Value)
                        result.HasBeenRead = Boolean.Parse(messageNode.Attribute("HasBeenRead").Value)
                        result.message = messageNode.Value
                        result.messageTitle = messageNode.Attribute("header").Value
                        result.ResponseFromSendee = Boolean.Parse(messageNode.Attribute("responseFromSendee").Value)
                        result.ResponseFromSender = Boolean.Parse(messageNode.Attribute("responseFromSender").Value)
                        result.MyType = Notification.NotificationTypeFromString(messageNode.Attribute("type").Value)
                        result.SendStat = Notification.SenderStatFromString(messageNode.Attribute("senderStatus").Value)
                        result.NotificationID = Integer.Parse(messageNode.Attribute("id").Value)
                        result.CreationDate = Date.Parse(messageNode.Attribute("originalDate").Value)
                        Dim sessionArgs As New NewNotificationArgs.sessionArgs
                        For Each att As XAttribute In messageNode.Attributes
                            Select Case att.Name
                                Case "FeatureForSession"

                                    Dim tempSongID As String = "UNT-12004-" & messageNode.Attribute("FeatureForSession").Value

                                    Dim tempSongName As String = Utils.GetNameByGlobalID(tempSongID)

                                    Dim songArtist As String = "UNT-12000-" & SongData.GetSongArtistName(tempSongID, True)

                                    If Not (songArtist.Length < 11) Then
                                        sessionArgs.session = New SetupSession.Session(tempSongName, tempSongID, songArtist)


                                        sessionArgs.requestedBars = messageNode.Attribute("FeatureBarsRequested").Value

                                        sessionArgs.whichVerse = messageNode.Attribute("FeatureVerse").Value
                                    End If





                            End Select
                        Next

                        staticArgs.sessArgs = sessionArgs
                        result.MyArgs = staticArgs


                    End If

                Next


            End If
        Next
        app.Dispatcher.Invoke(log, {"message loaded", "Get Messages"})

        Return result
    End Function







    Public Sub New(ByVal type As NotificationType, _from As String, _to As String)
        n_from = _from
        n_to = _to
        MyType = type







    End Sub

    Public Shared Function GetSentMessages(art As Artist) As List(Of Tuple(Of String, Integer))

        Dim result As List(Of Tuple(Of String, Integer)) = New List(Of Tuple(Of String, Integer))


        For Each node As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, 0, "NotificationHistory").Elements
            If art.GLOBAL_ID = node.Name Then

                For Each sent As XElement In node.Elements


                    result.Add(New Tuple(Of String, Integer)(sent.Attribute("ArtistID").Value, Integer.Parse(sent.Attribute("MessageID").Value)))





                Next

            End If

        Next
        Return result
    End Function

    Public Shared Function CheckForWaitingNotifications(art As Artist, newOnly As Boolean) As Integer()

        For Each node As XElement In XMLResources.GetResource(XMLResources.ResourceType.NewType, 0, "Notification").Elements
            If (Utils.ExtracLocalFromGlobal(node.Name.LocalName) = art.LOCAL_ID.ToString) Then
                Dim arr As List(Of Integer) = New List(Of Integer)
                For Each child As XElement In node.Elements
                    If Not (Boolean.Parse(child.Attribute("HasBeenRead").Value)) Then
                        arr.Add(Integer.Parse(child.Attribute("id").Value))
                    Else
                        If Not newOnly Then
                            arr.Add(Integer.Parse(child.Attribute("id").Value))

                        End If

                    End If
                Next
                Return arr.ToArray

            End If

        Next

        Return Nothing
    End Function


    Public ReadOnly Property My_Notification_Args As NewNotificationArgs
        Get
            Return MyArgs
        End Get
    End Property


    Public Shared Sub UpdateHasBeenRead(sendeeGID As String, messageID As Integer) Handles window.MessageRead
        Dim myNode As XElement = GetNotificationNode(sendeeGID, messageID)
        myNode.Attribute("HasBeenRead").Value = True
        Application.ResourcesHaveChanged = True

    End Sub


    Public Function CompareTo(other As Notification) As Integer Implements IComparable(Of Notification).CompareTo
        Return -CreationDate.CompareTo(other.CreationDate)

    End Function
End Class