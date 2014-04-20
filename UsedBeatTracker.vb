Imports System.IO
Public Class UsedBeatTracker
    Public Shared Sub UpdateTracksNow()

        For Each art As Artist In Application.ArtistList
            For Each path As String In Directory.GetDirectories(art.RecordingPath)
                Dim title = New DirectoryInfo(path).Name
                Application.LemLog("Getting info for " & title, "Scanning for used tracks...")
                Dim info = BaseTrackInfoWrapper.GetExistingBeatInfo(path, art.My_Name, SetupSession.SessionMusicSource_Type.SoundClick)
                If (info Is Nothing) Then
                    Application.LemLog(title & " info not found, Scanning beats folder...")
                    Dim soundclickBeat As String = art.CheckForUsedSoundClickBeat(path)



                    If Not (soundclickBeat = "") Then


                        info = New BaseTrackInfoWrapper(soundclickBeat, title,
                                                             SetupSession.SessionMusicSource_Type.SoundClick, Application.XMLRESOURCES, art.My_Name)


                        'If (info.storeThisData()) Then
                        '    Application.ResourcesHaveChanged = True

                        'End If
                    End If
                End If


               
            Next
        Next


        XMLResources.SubmitResources()

        
    End Sub

End Class
