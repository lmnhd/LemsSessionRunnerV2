Public Class PassWordWindow




    Private result As String

    Public Shared Function ShowDialoque(ByVal message As String) As String

        Dim screen As PassWordWindow = New PassWordWindow()
        screen.caption.Text = message

        screen.ShowDialog()

        Return screen.result

    End Function

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        passBox.Focus()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub CancelClick(sender As Button, e As RoutedEventArgs) Handles CancelBTN.Click

        result = "cancelthisshit"
        Me.Close()

    End Sub


    Private Sub GoClick(sender As Button, e As RoutedEventArgs)
        If (passBox.Password = "1234") Or passBox.Password = "" Then

            passBox.Password = ""
            passBox.Focus()
            Return
        Else

            result = passBox.Password
            Me.Close()


            Return
        End If
    End Sub


    Private Sub enterKeyPressed(sender As Key, e As RoutedEventArgs)
        If (sender = Key.Enter) Then
            MsgBox("You Got It")

        End If
    End Sub

    Private Sub passBox_GotFocus_1(sender As Object, e As RoutedEventArgs)
        passBox.Password = ""
        passBox.Focus()

    End Sub
End Class
