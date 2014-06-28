Imports System.Xml
Imports System.Text.RegularExpressions
Imports System.IO
Imports Appbar










Class MainWindow



   

    




    Private ctlName As Control
    Private addNameBox As TextBox = New TextBox()

    Private loginButton As Button = New Button()
    Private openSessionButton As Button = New Button()
    Private selectedSong As String = ""
    Private artistName As String
    Private beatPath As String = ""
    Private myPass1 As String = ""
    Private mypass2 As String = ""
    Private artistsList As List(Of Artist)
    Private Shared aTimer As System.Timers.Timer

    Private passList As New ArrayList

    Private passwords As System.Xml.XmlDocument

    Private gridIndex As Int32 = -1

    Private WithEvents app As Application = Application.Current

    Private Const namePrompt As String = "please enter your Stage or Group Name"
    Private Const passPrompt As String = "type a new password"
    Private Const passPrompt2 As String = "please retype the password"
    Private Const LISTMODE_ARTIST As Integer = 0
    Private Const LISTMODE_SONG As Integer = 1
    Private Const NOT_REGISTERED As String = " NOT IN DATABASE"

    Private MIX_READY As Boolean = False

    Private featuredArtists As List(Of Artist)


    Private songList As List(Of DirectoryInfo)

    Public nuendoPath As String = ""
    Public MP3_PATH As String = ""
    Private sessionButtonHandler As EventHandler = AddressOf SessionButtonHandlerDelegater
    Private sessionStartedHandler As System.Timers.ElapsedEventHandler = AddressOf nowRunning
    Private sessionEndedHandler As System.Timers.ElapsedEventHandler = AddressOf nowClosed

    'Public Const mp3Path As String = "\\BRICKLYFEA-PC\mp3"

    Private currentState As Integer = Application.sessionState.closed




    'Private sessionRunning As Boolean = False

    Dim CodeModule As Object



    'Public Sub OpenSessionsWindow() Handles LoginButton1.Click
    '    Dim sessionW As SessionWindow = New SessionWindow
    '    Application.BasePath = "C:\Users\BrickLyfeA\Desktop\NA\Spody"

    '    Dim data As List(Of SetupSession.Session) = data.("Spody")
    '    sessionW.DataContext = data

    '    sessionW.Show()
    '    Me.Close()

    'End Sub

    'Public Function getArtists()
    '    Return artistsList
    'End Function



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.ContentRendered


        ' CloseSessionDialog.ShowDialog("Close Session", "What's the hook gon be?", "TYPICAL NIGGAZ ENT PRESENTZ ANOTHER TURN THAT SHIT DOWN PRODUCTION", "", "", "", False, "", New List(Of Artist))

        If (Application.CLEANUPXMLFORDEBUGENABLED) Then
            Return
        End If
        'Me.Close()
        checkLoginStatus()
        killAllMacros()


        closeSessionQuick()

        If (updateList()) Then
            Me.DataContext = Application.Login_Data

        End If
        ' initDynamicButtons()
        Application.MWHANDLE = Me

        Dim dd As DeviceDetecter = New DeviceDetecter


    End Sub


    Protected Overrides Sub OnGotFocus(e As RoutedEventArgs)
        MyBase.OnGotFocus(e)

        If (updateLogins()) Then
            e.Handled = True
        End If
    End Sub

    Public Delegate Function UpdateLoginsDelegate() As Boolean

    Public Function updateLogins() As Boolean
        Try
            If Application.LEMTESTING Or Application.data Is Nothing Then
                Return True
            End If

            If (Application.data.Count > 0) Then

                LoginInfo.DataContext = Nothing
                Application.data.Sort()




                LoginInfo.DataContext = Application.data
                LoginInfo.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget()

            End If




            Return True
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
            Return False
        End Try
       
    End Function


    Private Sub initDynamicButtons()

        ' StartSessionButton.Hide()

        ' Dim lgbX = PasswordTB.Location.X
        'Dim lgbY = PasswordTB.Location.Y + PasswordTB.Height + 20
        ' Dim lgbWidth = PasswordTB.Width
        ' Dim lgbHeight = 100

        ' loginButton.SetBounds(lgbX, lgbY, lgbWidth, lgbHeight)
        'loginButton.Text = "Login"
        ' Me.Controls.Add(loginButton)
        ' loginButton.BackColor = Color.Cornsilk



        ' AddHandler loginButton.Click, AddressOf loginButtonHandler


        ' addToDBButton.Hide()

        ' loginButton.Hide()





    End Sub

    Private Sub checkLoginStatus()

        Dim path As String = Application.XML_PATH & "LoginInfo.xml"
        Dim doc As XmlDocument = New XmlDocument()
        doc.Load(path)

        Dim root As XmlNode = doc.DocumentElement()
        Dim loginTime As Date = Date.Parse(root.Attributes.ItemOf(2).Value)



        Dim diff As Integer = Now.Subtract(loginTime).Minutes
        Dim lastLoginMinutes As Integer = Now.Subtract(root.Attributes.ItemOf(2).Value).Minutes
        If (root.Attributes.ItemOf(3).Value = "1" Or Now.Subtract(root.Attributes.ItemOf(2).Value).Minutes < 1) Then
            artistName = root.Attributes.ItemOf(0).Value
            selectedSong = cleanSongName(root.Attributes.ItemOf(1).Value)


        End If





    End Sub

    Property Update_List As Int32
        Get
            updateList()
            Return 0

        End Get
        Set(value As Int32)

        End Set
    End Property

    Private Function updateList() As Boolean
        Try
            ' addToDBButton.Hide()
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR!!!")
        End Try

        If (Application._List_Mode = LISTMODE_ARTIST) Then
            ListBox1.Items.Clear()









            SortListBox()

            If Not (artistName = "") Then
                Application.Current_Index = getArtistListIndex(artistName) + 2
                ListBox1.Items.Add(Application.Current_Index)
                'PasswordTB.Select()

                'passTB_gotFocus()


            End If
        Else

            artistsList = Application.ArtistList
        End If

        Return True

    End Function



    

    Private Function getArtistListIndex(ByVal nameOfArtist As String)
        Dim id As Integer = -1


        For Each artist As Artist In artistsList
            id += 1
            If (artist.My_Name = nameOfArtist) Then

                Exit For
            End If
        Next
        Return id
    End Function

    Sub SortListBox()

        artistsList = Application.ArtistList
        artistsList.Sort(Function(p1, p2) p1.Compare(p1, p2))







        ListBox1.Items.Clear()
        'ListBox1.Items.Add(NOT_REGISTERED)
        'ListBox1.Items.Add(" COLLAB")

        For Each item As Artist In artistsList
            ListBox1.Items.Add(item.My_Name)


        Next
        ' ListBox1.Items.AddRange(displayList.ToArray())

        ListBox1.Items.Refresh()










    End Sub

    Private Function sortSongList(ByRef list As List(Of String)) As List(Of String)
        Dim infoList As List(Of System.IO.DirectoryInfo) = New List(Of System.IO.DirectoryInfo)
        Dim result As List(Of String) = New List(Of String)
        For Each dir As String In list
            Dim info As DirectoryInfo = New DirectoryInfo(dir)
            infoList.Add(info)



        Next

        Dim sortedSongList = From DirectoryInfos In infoList
                                Order By DirectoryInfos.CreationTime Descending
Select DirectoryInfos

        For Each song As DirectoryInfo In sortedSongList
            result.Add(song.Name)

        Next

        Return result


    End Function
    'Public Sub Login(ByVal ArtistName As String, ByVal pass As String)

    '    Dim artist As Artist = New Artist(ArtistName)
    '    If (artist.myPass = pass) Then
    '        Dim sessions As SetupSession = New SetupSession(artist)
    '        Dim wnd = New SessionWindow()
    '        wnd.DataContext = sessions.GetCurrentSessions()

    '        Me.Close()
    '    Else
    '        MsgBox("Password WRONG FUCKA!!!")
    '    End If


    '    Return





    'End Sub




    Private Sub listBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.SelectionChanged
        ' Get the currently selected item in the ListBox. 

        NameTB.Text = TryCast(sender, ListBox).SelectedItem

        artistName = NameTB.Text



        'Select Case Application._List_Mode
        '    Case LISTMODE_ARTIST
        '        loginButton.Text = "LOGIN"
        '        addToDBButton.Hide()
        '        loginButton.Hide()
        '        If (Application.Current_Index = 1) Then

        '            artistName = "Collab"
        '            NameTB.Text = artistName

        '            Dim pass = InputBox("PASSWORD?")
        '            login(artistName)



        '        ElseIf (Application.Current_Index > 1) Then
        '            artistName = ListBox1.SelectedItem.ToString()







        '        End If





        '        MsgBox("you selected " & curItem & " ##" & index)
        '    Case LISTMODE_SONG
        '        If (ListBox1.Items.Count > 1) Then

        '            selectedSong = ListBox1.SelectedItem.ToString
        '            StartSessionButton.Text = "Open " & selectedSong

        '        End If
        'End Select




    End Sub


    

    Private Function removeSpaces(ByRef line As String)
        Dim tempArray = line.Split(" ")


        line = Join(tempArray, "")


        Return line
    End Function

    Private Function nameComparator(ByRef name1 As String, ByRef name2 As String)
        name1 = removeSpaces(name1.ToLower)
        name2 = removeSpaces(name2.ToLower)
        Dim isTheSame As Boolean = name1 = name2

        Return isTheSame


    End Function

    Private Function findNameInArraylist(ByVal name As String)

        For Each _artist As Artist In artistsList
            If (nameComparator(_artist.My_Name, name)) Then
                Return True
            End If

        Next
        Return False
    End Function



    Private Sub validateNewEntry()

        If (NameTB.Text = "") Then
            MsgBox(namePrompt)
            Return
            ' ElseIf (PasswordTB.Text = "") Then
            '    MsgBox("password empty " & passPrompt)

            '  Return
        ElseIf (findNameInArraylist(NameTB.Text)) Then

            MsgBox("name already in database, Choose another")
            Return
        End If
        artistName = converToTitleCase(NameTB.Text.ToLower)


        ' myPass1 = PasswordTB.Text

        mypass2 = InputBox(passPrompt2)


        If Not (myPass1 = mypass2) Then
            MsgBox("Password MisMatch!!!")
            Return
        End If


        'capitalize name
        addArtistToXML(artistName, myPass1)

    End Sub

    Private Sub createNewPassword()


        If Regex.IsMatch(myPass1, "[A-Z]") Then
            Dim msg As Integer = MsgBox("I'd prefer it if you didn't use Capitals", vbOKCancel, "CAPS DETECTED!")
            If (msg = 2) Then
                Return
            End If
        End If




        If (myPass1 = "") Then
            myPass1 = InputBox("type password", "PASSWORD SETUP")
            If (myPass1 = "") Then
                MsgBox("password not set")

                updateList()
                'refreshUIText()
                Return


            End If
        End If



        mypass2 = InputBox("one mo gen!")
        If (myPass1 = mypass2) Then
            editXMLPass(False)

            MsgBox("ALL SET")
            updateList()
            'refreshUIText()

            ' Return


        Else


            MsgBox("NAW MY BOY, PASSWORDS NO MATCH!!!")
            ListBox1.SelectedIndex = Application.Current_Index

            updateList()

            'refreshUIText()
        End If















    End Sub

    Public Sub editXMLPass(ByRef reset As Boolean)
        Dim xmlDoc As XmlDocument = New XmlDocument()
        xmlDoc.Load(Application.PASSWORDS_PATH)
        Dim node As XmlNode
        node = xmlDoc.DocumentElement
        Dim node2 As XmlNode = xmlDoc.SelectSingleNode("/Artists/artist[@name='" & artistName & "']")
        'Dim node3 As XmlNode
        ' Dim node4 As XmlNode

        If (reset) Then
            node2.Attributes().ItemOf(1).Value = "temp"
        Else

            node2.Attributes().ItemOf(1).Value = myPass1


        End If


        'node4 = node2.FirstChild

        'node3 = node2.FirstChild.NextSibling
        '  node3.InnerText = myPass1
        '  node3.InnerText = artistName

        xmlDoc.Save(Application.PASSWORDS_PATH)



    End Sub

    Public Sub addArtistToXML(ByRef name As String, ByRef password As String)

        MsgBox("Only Lem can add new artists now!")
        Return



        If (name = "") Then
            Return
        End If

        Dim doc As New XmlDocument()
        doc.Load(Application.PASSWORDS_PATH)
        Dim root As XmlElement = doc.Item("Artists")
        For Each art As XmlNode In root.ChildNodes
            If (art.Attributes.ItemOf(0).Value = name) Then
                Return
            End If
        Next
        Dim artistNode As XmlElement = doc.CreateElement("artist")
        ' Dim nameNode As XmlElement = doc.CreateElement("name")
        ' Dim passNode As XmlElement = doc.CreateElement("pass")






        'nameNode.InnerText = artistName
        'passNode.InnerText = myPass1

        root.AppendChild(artistNode)
        artistNode.SetAttribute("name", converToTitleCase(name, True))
        artistNode.SetAttribute("pass", password)
        'artistNode.AppendChild(nameNode)
        'artistNode.AppendChild(passNode)

        doc.Save(Application.PASSWORDS_PATH)

        updateList()
        'refreshUIText()
        'PasswordTB.Text = "WELCOME"
        MsgBox(name & " is up in dis bitch!!!")


    End Sub




    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)
        'Process.Start("macros\cleanclosesession.scp")

        ui_LEMLOG_BOTTOM.Text = ""

        ui_LEMLOG_TOP.Text = Application.INTRO_MESSAGE
        If (Application.INTRO_MESSAGE2 = "") Then
            ui_LEMLOG_MIDDLE.Text = DateTime.Now.TimeOfDay.ToString().Substring(0, 8)
        Else
            ui_LEMLOG_MIDDLE.Text = Application.INTRO_MESSAGE2
        End If

        'Application.FloatingPlayer = New FloatingLemPlayer()
        updateLogins()
        app.StartLoadProducerBeats()
        If Not (WindowUtils.User32.FindWindowsWithText("Nuendo").Count > 0) Then
            Process.Start("C:\Program Files (x86)\Steinberg\Nuendo 4\Nuendo4.exe")
            System.Threading.Thread.Sleep(10000)
            MacroShop.MinimizeNuendo()
        Else

            MacroShop.Clean_Close_Session()


        End If

    End Sub




    Private Sub handleLemLog(message As String, overallStat As String) Handles app.LemLogEvent
        Dim bottomMsg = ui_LEMLOG_BOTTOM.Text
        Dim topMsg = ui_LEMLOG_TOP.Text
        Dim maingMsg = message & vbNewLine & "..."

        If Not (overallStat = "") Then
            bottomMsg = ui_LEMLOG_MIDDLE.Text & vbNewLine & bottomMsg
            'maingMsg = "...." & vbNewLine & vbNewLine
            topMsg = overallStat
        End If
        ' maingMsg &= message & vbNewLine
        ui_LEMLOG_MIDDLE.Text = maingMsg
        ui_LEMLOG_TOP.Text = topMsg
        ui_LEMLOG_BOTTOM.Text = bottomMsg
    End Sub



    Private Sub addNameBox_GotFocus()
        NameTB.Text = ""
        ' PasswordTB.Text = ""
    End Sub

    Private Sub nameTB_gotFocus() Handles NameTB.GotFocus


        If (Application.Current_Index = 0) Then

            NameTB.Text = ""
            ' If (PasswordTB.Text = "!SET!UP!PASS") Then
            'PasswordTB.Text = ""
        End If

        ' Else

        ' End If


    End Sub

    Private Sub nameTB_lostFocus() Handles NameTB.LostFocus

        ' Me.Controls.Remove(addButton)

    End Sub


    Private Sub passTB_gotFocus()

        If (Application.Current_Index < 2) Then
            ' fill in name and password and push add to db button

            ' addToDBButton.Show()
            'loginButton.Hide()

            'PasswordTB.Text = "TYPE NEW PASSWORD"
            ' PasswordTB.PasswordChar = "*"
            If (NameTB.Text = "") Then
                NameTB.Text = "enter stagename"
            End If










        Else
            'accept password for login


            'PasswordTB.Text = ""
            ' PasswordTB.PasswordChar = "*"
            ' If (artistsList.Item(index - 2).myPass = "temp") Then
            'loginButton.Text = "set pass"
            '   loginButton.Show()
            '   Else
            '  loginButton.Text = "login"
            '  loginButton.Show()
        End If



        ' addToDBButton.Hide()


        ' End If

    End Sub

    'Private Sub passTB_lostFocus() Handles PasswordTB.LostFocus

    '    ' addToDBButton.Hide()

    '    'loginButton.Hide()
    'End Sub

    'Private Sub login(ByRef wait As Boolean)
    '    If Not (wait) Then
    '        If (currentState = Application.sessionState.closing) Then

    '            currentState = Application.sessionState.closed
    '            System.Threading.Thread.Sleep(3000)
    '            killAllMacros()
    '            ListBox1.Items.Clear()
    '            ' Application.getSongSessionList()
    '            Application._List_Mode = LISTMODE_SONG

    '        ElseIf (currentState = Application.sessionState.running) Then
    '            Return

    '        End If



    '        If (Application._Session_Count > 0 And currentState = Application.sessionState.closed) Then
    '            closeSessionQuick()
    '        End If


    '        ' currentState = sessionState.closed

    '        'Me.Controls.pWlabel.Hide()

    '        'Me.Controls.Remove(PasswordTB)
    '        Application.BASE_PATH = nuendoPath & artistName
    '        createNewDirectory(Application.BASE_PATH)

    '        If (Application._Session_Count > 0) Then
    '            System.Threading.Thread.Sleep(3000)
    '        End If
    '        ' getSongSessionList()
    '        ' Application._List_Mode = LISTMODE_SONG
    '        ' Application.createStartSessionOptions()

    '    Else
    '        System.Threading.Thread.Sleep(3000)
    '        killAllMacros()
    '        ListBox1.Items.Clear()
    '        ' getSongSessionList()
    '        Application._List_Mode = LISTMODE_SONG
    '        System.Threading.Thread.Sleep(3000)
    '        currentState = Application.sessionState.closed
    '        'createStartSessionOptions()
    '    End If


    '    'bringMeToFront()

    '    ' bringMeToFront()









    'End Sub

    'Private Sub logOut()
    '    ' Form1_Load(Me, New System.EventArgs())
    '    updateList()
    '    initDynamicButtons()
    '    bringMeToFront()


    'End Sub



    Public Sub closeSessionQuick()
        ' Process.Start(Application.CLEAN_CLOSE_SESSION)


    End Sub


    Private Sub closeThis()
        Dim doc As XmlDocument = New XmlDocument()
        doc.Load(Application.PASSWORDS_PATH)
        Dim currentLoginInfo As XmlNode = doc.CreateElement("LastCurrentUser")
        Dim userName As XmlAttribute = doc.CreateAttribute("name")
        userName.Value = artistName
        currentLoginInfo.Attributes.Append(userName)

        Dim currentPass As XmlAttribute = doc.CreateAttribute("pass")
        currentPass.Value = myPass1
        currentLoginInfo.Attributes.Append(currentPass)

        Dim time As Date = System.DateTime.Now()
        Dim timeAtt As XmlAttribute = doc.CreateAttribute("when")
        timeAtt.Value = time.ToShortTimeString
        currentLoginInfo.Attributes.Append(timeAtt)

        doc.Save(Application.PASSWORDS_PATH)
        '  Process.Start(Application.CLOSE_THIS)



    End Sub

    Private Sub restart()




        closeThis()












    End Sub










    Private Sub SessionButtonHandlerDelegater()
        If (currentState = Application.sessionState.closed) Then
            PrepareSessionSetup()
        ElseIf (currentState = Application.sessionState.running) Then

            'closeSession()
        Else
            Return
        End If
    End Sub

    Private Sub PrepareSessionSetup()

        'RemoveHandler StartSessionButton.Click, sessionButtonHandler


        'StartSessionButton.Text = "Close Current Session"
        'ListBox1.Items.Clear()



        'NEW SESSION






        If (Application.Current_Index = 0) Then


            Dim newSessionDir As String = Application._SongPath
            Dim newSessionFile As String = "\1.npr"
            Dim macroscript As String = ""
            Dim clipText As String = ""
            'If File.Exists(newSessionfile) Then
            'My.Computer.FileSystem.DeleteFile(newSessionfile)


            '  End If
            Dim songNamePathSplit = Split(newSessionDir, "\")
            selectedSong = songNamePathSplit.GetValue(songNamePathSplit.Count - 1)

            Dim dirCount As Integer = 0
            If (Directory.Exists(newSessionDir)) Then

                For Each Dir As String In Directory.GetDirectories(Directory.GetParent(newSessionDir).FullName)
                    If (Dir.Contains(newSessionDir)) Then
                        dirCount += 1
                    End If

                Next
                newSessionDir += "_" & dirCount + 1
            End If
            My.Computer.FileSystem.CreateDirectory(newSessionDir)
            My.Computer.FileSystem.CopyDirectory(Application.TEMPLATE_DIRECTORY, newSessionDir)

            If Not beatPath = "" Then
                My.Computer.FileSystem.CopyFile(beatPath, Application._SongPath & "\audio\" & My.Computer.FileSystem.GetName(beatPath))
                Clipboard.SetText(beatPath, TextDataFormat.Text)
                macroscript = Application.START_SESSION_WITH_BEAT
            Else
                clipText = Application.INTERNET_BEATS_DIRECTORY



                macroscript = Application.START_SESSION_AUDITION_BEATS
            End If





            ' Process.Start(newSessionDir & newSessionFile)
            '  Process.Start(macroscript)
            'Utils.XMLUtils.updateLoginInfo(artistName, selectedSong, True)


            If (addToClipBoard(clipText) = 1) Then

                currentState = Application.sessionState.started
                'System.Threading.Thread.Sleep(12000)
                aTimer = New System.Timers.Timer(12000)
                AddHandler aTimer.Elapsed, sessionStartedHandler
                aTimer.Start()
            Else

                closeSessionQuick()
                MsgBox("Problem loading name to clipboard")
                Me.Close()
            End If


        Else

            openPreviousSession(selectedSong)
            currentState = Application.sessionState.started
            aTimer = New System.Timers.Timer(2000)
            AddHandler aTimer.Elapsed, sessionStartedHandler
            aTimer.Start()
        End If


        Application._Session_Count += 1






    End Sub

    Private Function addToClipBoard(ByRef textToAdd As String)
        Dim count = 0
        Dim clipText = ""
        While Not (clipText = textToAdd)
            Try

                clipText = Clipboard.GetText()
            Catch ex As Exception
                count += 1
                Application.LemLog("add to clipboard failed " & count.ToString() & vbNewLine & ex.Message, "ERROR!!!")
            End Try




            Try
                ' MsgBox("attempting to add data to clipboard")
                count += 1
                Clipboard.SetText(textToAdd, TextDataFormat.Text)




            Catch ex As Exception
                ' MsgBox("Technical issue, try again please", MsgBoxStyle.Exclamation, vbOKOnly)
                'aSystem.Threading.Thread.Sleep(1000)


                Application.LemLog(ex.Message, "ERROR!!!")
                If (count = 10) Then
                    Application.LemLog("Error pasting to clipboard...", "ERROR!!!")
                    Return 0
                Else
                    System.Threading.Thread.Sleep(1000)

                End If
            End Try


        End While
        Return 1
    End Function



    Private Sub nowRunning()
        aTimer.Stop()

        RemoveHandler aTimer.Elapsed, sessionStartedHandler

        ' currentState = sessionState.running
        'StartSessionButton.BackColor = Color.Black
        'StartSessionButton.ForeColor = Color.Gold




    End Sub



    Private Sub nowClosed()
        closeThis()

        'aTimer.Stop()
        'If (currentState = sessionState.closing) Then
        '    currentState = sessionState.closed
        '    RemoveHandler aTimer.Elapsed, sessionEndedHandler

        ' Me.Close()



        ' End If



    End Sub

    Private Function cleanSongName(ByVal name As String) As String
        Dim newString As String = name
        If (newString.Contains(". ")) Then
            newString = Split(newString, ". ").GetValue(1)


        End If
        Return newString

    End Function


    Private Sub openPreviousSession(ByRef name As String)
        Dim finalFileName As String = ""
        Dim infoReader As System.IO.FileInfo
        Dim modified As Date = Nothing
        Application.BASE_PATH = Application.BASE_PATH & "\" & Split(name, ". ").GetValue(1)
        For Each i As String In Directory.GetFiles(Application.BASE_PATH)
            If (System.IO.Path.GetExtension(i) = ".npr") Then
                infoReader = My.Computer.FileSystem.GetFileInfo(i)
                If (infoReader.LastWriteTime > modified) Then
                    modified = infoReader.LastWriteTime
                    finalFileName = i
                End If


            End If
        Next
        If (finalFileName = "") Then
            Return
        End If
        'currentState = sessionState.started
        ' Process.Start(finalFileName)
        'Process.Start(CLOSE_SELECTED_DIRECTORY_WINDOW)


        'openSessionButton.BackColor = Color.Red
        ' openSessionButton.Text = "Stop and Close Session"
        Application._Session_Count += 1

        ' updateLoginInfo(artistName, cleanSongName(selectedSong), True)



    End Sub
















    Private Sub createNewDirectory(ByVal folderName As String, Optional ByRef artistFolder As String = "")
        If artistFolder = "" Then
            Dim artistDir As String = nuendoPath & folderName
            My.Computer.FileSystem.CreateDirectory(folderName)
        Else
            My.Computer.FileSystem.CreateDirectory(nuendoPath & artistFolder & "\" & folderName)
        End If
    End Sub



    Private Sub openPreviousSession()




    End Sub


    Public Function converToTitleCase(ByRef word As String, Optional ByRef keepSpaces As Boolean = False)
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


    Private Sub titleCaseFolders()
        Dim i As Integer = 0
        For Each songFolder In Directory.GetDirectories("K:\wamp\www\mp3")
            ' MsgBox("")
            Dim pathArray() As String = Split(songFolder, "\")
            Dim nameToChange As String = pathArray.GetValue(pathArray.Length - 1)
            Dim regx As New Regex("[a-zA-Z]")
            Dim matches As MatchCollection

            matches = regx.Matches(nameToChange)


            'If (nameToChange.StartsWith("C")) Then



            Dim songFolder2 As String = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pathArray.GetValue(pathArray.Length - 1))
            'Dim songFolder2 As String = "yo"


            pathArray(pathArray.Length - 1) = songFolder2
            Dim newFolderPath = Join(pathArray, "\")


            ' My.Computer.FileSystem.RenameDirectory("K:\wamp\www\mp3\Kountry Kool", "songFolder2")
            My.Computer.FileSystem.MoveDirectory(songFolder, newFolderPath)
            MsgBox("")
            ' End If



        Next




    End Sub

    Private Sub bringMeToFront()


        ' Process.Start(Application.BRING_TO_FRONT)
    End Sub


    Private Sub killAllMacros()
        Dim processes As Process() = Process.GetProcessesByName("msched")
        For Each process As Process In processes
            process.Kill()

        Next
        Dim processes2 As Process() = Process.GetProcessesByName("startsessionauditionbeats")
        For Each process As Process In processes2
            process.Kill()

        Next
        Dim processes3 As Process() = Process.GetProcessesByName("closesessionstarter")
        For Each process As Process In processes3
            process.Kill()

            'Next
            'Dim processes4 As Process() = Process.GetProcessesByName("bouncesession")
            'For Each process As Process In processes4
            '    process.Kill()

            'Next
            'Dim processes5 As Process() = Process.GetProcessesByName("bouncesession")
            'For Each process As Process In processes5
            '    process.Kill()

            'Next
            'Dim processes6 As Process() = Process.GetProcessesByName("bouncesession")
            'For Each process As Process In processes6
            '    process.Kill()

        Next
    End Sub











    Private Sub infobox1_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub LoginInfo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LoginInfo.SelectionChanged
        Tabs.Visibility = Windows.Visibility.Visible

        gridIndex = LoginInfo.SelectedIndex
        If (gridIndex < 0) Or (LoginInfo.SelectedIndex = Nothing) Then
            Return
        End If
        optionsWindowSongName.Text = Application.data.Item(gridIndex).Artist & " By " & Application.data.Item(gridIndex).Artist

        'playSongTab.Header = "Play " & Application.data.Item(gridIndex).Name
        SessionOptionsButton.Content = "Close Options Window"

    End Sub


    Private Sub CloseSessionOptions(sender As Object, e As RoutedEventArgs)
        Tabs.Visibility = Windows.Visibility.Collapsed
        SessionOptionsButton.Content = "Options"
    End Sub

    Private Async Sub LoginButton1_Click(sender As Object, e As RoutedEventArgs) Handles LoginButton1.Click
        ' Dim sw = New SessionWindow()
        'sw.Show()
        'Me.Close()
        Dim artist As Artist = Nothing
        If (ListBox1.SelectedValue Is Nothing) Then

            Dim lw As LoginWindow = New LoginWindow()
            Dim result? As LoginWindow.LoginWindowResult = lw.ShowDialog("jjj")

            Select Case result.GetValueOrDefault().result_Type

                Case LoginWindow.LoginWindowResult.ResultType.cancel
                    Return

                Case LoginWindow.LoginWindowResult.ResultType.addNew

                    Dim arr As Array = Admin.CreateNewUntameRecordingArtist(result.GetValueOrDefault.newName, result.GetValueOrDefault.newPass, "artist")

                    If (arr.GetValue(0) = 1) Then
                        artist = arr.GetValue(1)


                        Exit Select
                    Else

                        Return
                    End If




                Case LoginWindow.LoginWindowResult.ResultType.gotArtist
                    artist = result.GetValueOrDefault.artist
                    Exit Select

                Case Else
                    Exit Sub



            End Select

        Else
            For Each ar As Artist In Application.ArtistList
                If (ar.My_Name = ListBox1.SelectedValue) Then
                    artist = ar
                End If
            Next

        End If




        '




        If (artist Is Nothing) Then
            Return
        End If
        'Application.statWin = New ProgressModalWindow
        Try
            Dim loggedIn As Boolean = Application.Login_As_Artist(artist)


        Catch ex As Exception
            'Dim logdel = New Application.LemLogDel(AddressOf Application.LemLog)
            'Dim app As Application = Application.Current

            Application.LemLog("Problem Logging in " & artist.My_Name & vbNewLine & ex.Message & "Probably error with server database or Artist is not registered with Untame.", "ERROR!!!")

            'app.Dispatcher.Invoke(logdel, {"Problem Logging in " & artist.My_Name & vbNewLine & ex.Message & "Probably error with server database or Artist is not registered with Untame.", "ERROR!!!"})


        End Try
        Return
        ' artist.SetPasswordOwner()









    End Sub

    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        Application.LemLog("SHUTTING DOWN!!!!")
        MyBase.OnClosing(e)

        'If (LemPlayer.destroy()) Then
        '    Return
        'End If

    End Sub

    Private Sub paidSessionClicked(sender As Object, e As RoutedEventArgs)
        ' Me.Close()
        'Application.Current.Shutdown()
        'Application.FloatingPlayer.Close()
        'Application.FloatingPlayer = Nothing




        Application.Current.Shutdown()



        ' Me.Hide()

        'Dim dock As LiveSessionDockingWindow = New LiveSessionDockingWindow
        'dock.Show()

        ' Dim windo As Appbar.MainWindow = New Appbar.MainWindow

        'Me.Hide()
        'If (Appbar.MainWindow.showNow()) Then
        '    Me.Show()
        'End If
        'Dim blocker As BlockingWindow = New BlockingWindow
        'blocker.Show()

        'Dim cc As Artist = New Artist("Cc", False)
        'Dim song = New SongData(cc, "Server Test 22F")
        'Application.SendSongToDataBase(song)







    End Sub

    Private Sub NewArtistClicked(sender As Object, e As RoutedEventArgs)
        'If (Application.RegisterLemPlayerNow()) Then
        '    MsgBox("Registered Successfully")
        'Else
        '    MsgBox("Problem registering player...")
        'End If
        Application.ResetFloater(True)
        'Application.Floating_Player().Show()
    End Sub

    Private Sub LaunchSoundclickBrowser(sender As Object, e As RoutedEventArgs) Handles SoundClickButton.Click
        If (System.IO.File.Exists("C:\Program Files (x86)\Mozilla Firefox\firefox.exe")) Then
            Process.Start("C:\Program Files (x86)\Mozilla Firefox\firefox.exe")
        End If



    End Sub

    'Private Sub RefreshSoundclickList(sender As Object, e As RoutedEventArgs) Handles RefreshSoundclickList.Click
    '    Dim app As Application = Application.Current
    '    ' app.StartLoadSCList()


    'End Sub

    Private Sub RefreshSoundClickList_Click(sender As Object, e As RoutedEventArgs)
        Dim app As Application = Application.Current
        app.StartLoadSCList()

    End Sub
End Class
