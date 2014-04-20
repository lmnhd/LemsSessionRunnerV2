Public Class Testwindow



    Private artist As Artist = New Artist("Cc", True)
    Private artistList As List(Of Artist) = New List(Of Artist)
    Private app As LemsSessionRunnerV2.Application = New LemsSessionRunnerV2.Application()

    Public Sub getlistofartist()
        app.getArtistList()
    End Sub

    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)
        getlistofartist()




        Dim sess As SetupSession.Session = artist.Get_Sessions.GetCurrentSessions.Item(0)
        sess.intitalize()

        Select Case LemsSessionRunnerV2.CloseSessionDialog.showCloseSessionDialogue(sess.SongName, sess).m_Choice
            Case CloseSessionOptionsChoice.BounceSession
                MsgBox("Bonunce")

            Case CloseSessionOptionsChoice.CloseSessionNoBounce
                MsgBox("Close")

            Case CloseSessionOptionsChoice.Cancel
                MsgBox("cancel")

        End Select

    End Sub



End Class
