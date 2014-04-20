
Public Class LoginWindow


    Private result As LoginWindowResult

    Private addMeBoxOpen As Boolean = False

    Private tryCount As Int32 = 0

    


    Public Overloads Function ShowDialog(ByVal message As String) As Nullable(Of LoginWindowResult)

        'Dim lw As LoginWindow = New LoginWindow()
        Me.ShowDialog()

        'MyBase.ShowDialog()


        Return result

    End Function

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        result = New LoginWindowResult()
        Me.DataContext = Application.ArtistList
        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Private Sub getNewArtistInfo()
        If (yourname.Text = "" Or yourname.Text = "Type Your Name Here") Then
            MsgBox("No Name")
            Return
        End If

        If (yourpass2.Password = yourretypedpass2.Password) Then
            result.newPass = yourpass2.Password
            result.newName = Utils.converToTitleCase(yourname.Text)
            result.result_Type = LoginWindowResult.ResultType.addNew

        Else

            If (tryCount < 3) Then


                MsgBox("passwords no match")
                tryCount += 1
                getNewArtistInfo()
            Else

                result.result_Type = LoginWindowResult.ResultType.cancel

            End If

           
        End If

        Me.Close()
    End Sub



    Private Sub ListBox_SelectionChanged_1(sender As Object, e As SelectionChangedEventArgs) Handles LB.SelectionChanged

        If (LB.SelectedValue Is Nothing) Then
            Return
        Else
            result.artist = LB.SelectedValue
            result.result_Type = LoginWindowResult.ResultType.gotArtist
            Me.Close()

        End If


    End Sub

    Public Structure LoginWindowResult
        Public artist As Artist
        Public result_Type As ResultType
        Public newPass As String
        Public newName As String


        Enum ResultType
            addNew = 1
            cancel = 2
            gotArtist = 3
        End Enum
    End Structure




    Private Sub textBoxGotFocus(sender As Object, e As RoutedEventArgs)

        If Not (addMeBoxOpen) Then
            Return
        End If
        Dim box

        Dim trybox As TextBox = TryCast(sender, TextBox)

        If (trybox Is Nothing) Then
            box = TryCast(sender, PasswordBox)
        Else
            box = trybox
        End If

        If (box Is Nothing) Then
            Return
        End If



        If (box.Name = yourname.Name) Then

            yourname.Text = ""
            yourretypedpass2.Focusable = False
            yourretypedpass.Focusable = False
            yourpass2.Focusable = False
            yourpass.Focusable = True

            AddHandler yourpass.GotFocus, AddressOf textBoxGotFocus
            RemoveHandler yourname.GotFocus, AddressOf textBoxGotFocus
            

        End If

        If (box.Name = yourpass.Name) Then
            If Not (yourname.Text = "") Then
                yourpass2.Focusable = True
                yourpass.Visibility = Windows.Visibility.Collapsed
                yourpass2.Visibility = Windows.Visibility.Visible
                yourretypedpass.Focusable = True
                yourpass2.Focus()



                AddHandler yourretypedpass.GotFocus, AddressOf textBoxGotFocus
                RemoveHandler yourpass.GotFocus, AddressOf textBoxGotFocus
            End If

        End If

        If (box.Name = yourretypedpass.Name) Then

            yourretypedpass.Visibility = Windows.Visibility.Collapsed
            yourretypedpass2.Visibility = Windows.Visibility.Visible
            yourretypedpass2.Focusable = True

            yourpass2.Focusable = False

            yourretypedpass2.Focus()

            RemoveHandler yourretypedpass.GotFocus, AddressOf textBoxGotFocus

        End If


    End Sub

    Private Sub AddMeClicked(sender As Object, e As RoutedEventArgs) Handles addme.Click



        If (yourname.Text = " Type Your Name Here") Then



            LB.Visibility = Windows.Visibility.Collapsed
            AddNameAndPassBlock.Visibility = Windows.Visibility.Visible

            'yourname.Focus()
            selectLabel.Text = "Add New Artist"


            ' yourpass.Focusable = False
            yourpass.Cursor = Cursors.None


            yourretypedpass.Focusable = False
            yourretypedpass.Cursor = Cursors.None

            addme.Content = "ADD ME"

            addMeBoxOpen = True

            AddHandler yourname.GotFocus, AddressOf textBoxGotFocus



        Else

            If (yourpass2.Password = yourretypedpass2.Password) And Not (yourpass2.Password = "") Then

                result.newName = yourname.Text
                result.newPass = yourpass2.Password
                result.result_Type = LoginWindowResult.ResultType.addNew

                Me.Close()


                ' getNewArtistInfo()
            Else

                MsgBox("Passwords don't match")
                result.result_Type = LoginWindowResult.ResultType.cancel
                Me.Close()

            End If

        End If

    End Sub

    Private Sub cancel() Handles cancelButton.Click

        result.result_Type = LoginWindowResult.ResultType.cancel
        result.newName = "Hahaha"
        Me.Close()
    End Sub
End Class
