Imports System.IO

Public Class MusicSource

    Public Shared Function GetList(ParamArray directoryStrings()) As List(Of FileInfo)


        Dim result As List(Of FileInfo) = New List(Of FileInfo)
        Try
            For Each location As String In directoryStrings
                For Each tempPath As String In Directory.GetFiles(location, "*", SearchOption.AllDirectories)
                    If tempPath.ToLower.EndsWith(".mp3") Then
                        result.Add(New FileInfo(tempPath))
                    End If






                Next
            Next
        Catch ex As Exception
            Dim app As Application = Application.Current
            Dim log As Application.LemLogDel = New Application.LemLogDel(AddressOf Application.LemLog)
            app.Dispatcher.Invoke(log, {"Drive not ready " + vbNewLine + ex.Message, ""})
        End Try

       

        Return result

    End Function

    Public Delegate Sub lemLog(message As String, overallStat As String)

    Public Shared Function GetSoundClickList() As List(Of SessionWindow.TrackWrapper)

        Dim app As Application = Application.Current
        Dim log As lemLog = New lemLog(AddressOf Application.LemLog)
        Dim resources As XMLResources = New XMLResources(Nothing)
        Dim result As List(Of SessionWindow.TrackWrapper) = New List(Of SessionWindow.TrackWrapper)

        app.Dispatcher.BeginInvoke(log, {"", "Loading SoundClickList"})
        'Application.LemLog("Loading SoundClickList")

        For Each filePath As String In Directory.GetFiles(Application.INTERNET_BEATS_DIRECTORY)

            If (Path.GetExtension(filePath).ToLower = ".mp3") Then
                'create new trackinfo and trackwrapper
                Dim btiw As BaseTrackInfoWrapper = New BaseTrackInfoWrapper(filePath, "none", SetupSession.SessionMusicSource_Type.SoundClick, resources, Nothing, Nothing, (New FileInfo(filePath)))

                'If (btiw.ID > 0) Then



                Dim wrapper As SessionWindow.TrackWrapper = New SessionWindow.TrackWrapper(btiw.BounceLocation, btiw)



                result.Add(wrapper)
                app.Dispatcher.BeginInvoke(log, {wrapper.BeatName, "Loading SoundClickList"})



            End If
            ' End If

        Next
        result.Sort()


        Return result
        'UsedSongList.DataContext = SoundClickList



    End Function




End Class
