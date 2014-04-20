Public Class ProgressModalWindow
    Private bars As List(Of ProgressBar)

    Private que As LinkedList(Of String)
    Private Shared m_IsLive As Boolean

    Private LOCKED As Boolean
    Public Shared Function ShowNow(message As String) As Boolean
        Dim window As ProgressModalWindow = New ProgressModalWindow
        window.ui_StatusText.Text = message
        Return window.ShowDialog()
    End Function


    Public Async Sub setProgress(ByVal progress As Integer, Optional ByVal barNum As Integer = 0)

        While (LOCKED)
            If (Await wait()) Then
                Exit While
            End If
        End While

        LOCKED = True
        If (barNum = 0) Then
            ui_ProgressBar.Value = progress
            Await Task.Delay(10)

        Else
            If (Application.statWinBars.Count > 0) Then


                Dim bar = Application.statWinBars.Item(barNum - 1)

                bar.Value = progress
            End If

        End If
        LOCKED = False

    End Sub

    Private Async Function wait() As Task(Of Boolean)
        Await Task.Delay(500)
        If (LOCKED) Then
            Await wait()

        End If
        Return True
    End Function

    Public Async Sub setText(ByVal text As String)

        While (LOCKED)
            If (Await wait()) Then
                Exit While
            End If
        End While


        LOCKED = True
        If (ui_StatusText.Text = "Status Text") Then
            ui_StatusText.Text = ""
        End If

        ' ui_StatusText.Text &= Environment.NewLine & text
        ui_StatusText.Text = text
        Await Task.Delay(20)
        LOCKED = False

    End Sub

    Public Function ShowNow() As Boolean
        If (m_IsLive) Then
            Return True
        End If

        m_IsLive = True

        bars = New List(Of ProgressBar)
        que = New LinkedList(Of String)
        Me.Show()
        ' m_IsLive = False
        Return True
    End Function

    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        MyBase.OnClosing(e)
        ui_StatusText.Text = "Done"
        m_IsLive = False
        ' bars.Clear()
    End Sub



    Public ReadOnly Property IsLive As Boolean
        Get
            Return m_IsLive
        End Get
    End Property

    Function addNewProgressBar() As Integer
        'Return 0

        If (m_IsLive) Then
            Dim bar As ProgressBar = New ProgressBar()
            ui_ProgressBars.Children.Add(bar)
            bar.Visibility = Windows.Visibility.Visible
            bar.Height = 50
            bars.Add(bar)
            Application.statWinBars.Add(bar)


        End If

        Return Application.statWinBars.Count


    End Function

    Function closeBar(barNum As Integer) As Boolean
        Return True
        ui_ProgressBars.Children.Remove(Application.statWinBars.Item(barNum - 1))



        Return True
    End Function

End Class
