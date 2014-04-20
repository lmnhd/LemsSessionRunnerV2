Imports System.Timers

Public Class SessionTimer

    Private Shared endTime As DateTime
    Private Shared timer As System.Timers.Timer
    Private Shared setupSession As SetupSession
    Private Shared session As SetupSession.Session
    Private Shared window As SessionWindow
    Private Shared overTime As Boolean = False
    Private Shared firstWarning As Boolean = False

    Public Shared Sub Start(ByRef setup As SetupSession)

        endTime = DateTime.Now.Add(TimeSpan.FromHours(4))
        If Not (Application.SERVER_ERROR) Then
            Dim bookedTill = LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil)
            If Not (bookedTill = "") Then
                Try
                    endTime = DateTime.Parse(bookedTill)
                Catch ex As Exception
                    Application.LemLog(ex.Message, "ERROR...Session Timer")
                End Try


            End If
        End If
        




        setupSession = setup

        Dim app As Application = Application.Current
        window = app.SessionWindow
        timer = New Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
        AddHandler timer.Elapsed, AddressOf HandleTimerEvent

        timer.Start()


    End Sub
    Shared Sub CloseRunningSession()
        Dim mode = LoginData.GetAppModeFromString(LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.mode.ToString()))
        Application.LemLog("You are about to be logged out, Please save your session now!!!")
        If (mode = LoginData.AppMode.liveSession) Then

            Appbar.MainWindow.kill()
        ElseIf (mode = LoginData.AppMode.loggedIn) Then
            window.Close()
        End If
        StopNow()
    End Sub

    Public Shared Sub StopNow()
        firstWarning = False
        If Not (timer Is Nothing) Then
            timer.Stop()
        End If

    End Sub
    Shared Sub AdministerRemainingTime()
        Dim sessionEnd = endTime
        If (DateTime.Now.TimeOfDay.Hours >= 3 And DateTime.Now.TimeOfDay.Hours < 9) Then
            If (DateTime.Now.Add(TimeSpan.FromMinutes(10)).TimeOfDay.Hours = 4) Then
                If Not firstWarning Then
                    MsgBox("Your session will be closed in 10 minutes so the studio PC can begin its nightly routines...")
                    firstWarning = True
                End If
                

            ElseIf (DateTime.Now.TimeOfDay.Hours = 4) Then
                CloseRunningSession()

            End If
        End If
        If (sessionEnd < DateTime.Now) Then
            If (LoginData.GetCurrentLoginInfoValue(LoginData.LoginInfoType.overTime) = "true") Then
                overTime = True
                If (sessionEnd < DateTime.Now.Add(TimeSpan.FromHours(2))) Then
                    Application.LemLog("Your Session is bieng terminated...", "Session Time Expired!")
                    CloseRunningSession()
                Else
                    Return
                End If
            End If
            If Not (overTime) Then
                Application.Current_Booking = Application.UNTAMEHTTP.CancelBooking(Application.Current_Booking, Application.LOCAL_HOST)
                If (Application.Current_Booking Is Nothing) Then
                    Application.LemLog("Server Down...You are still logged in...")
                Else

                    If (Application.Current_Booking.TotalMinutesUsedToday > TimeSpan.FromHours(4).TotalMinutes) Then

                        Dim timeLeft = Application.UNTAMEHTTP.QuickCheckSchedule(Application.LOCAL_HOST).when.Subtract(DateTime.Now).TotalMinutes
                        If (timeLeft > 5) Then
                            If (timeLeft > TimeSpan.FromHours(2).TotalMinutes) Then
                                timeLeft = TimeSpan.FromHours(2).TotalMinutes
                            End If
                            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.bookedUntil, DateTime.Now.Add(TimeSpan.FromMinutes(timeLeft)))
                            LoginData.UpdateCurrentLoginInfoValue(LoginData.LoginInfoType.overTime, "true")
                            Dim message = "Your time has been extended to " & DateTime.Now.Add(TimeSpan.FromMinutes(timeLeft)).TimeOfDay.ToString()

                            MsgBox(message)
                            Application.LemLog(message)

                        End If
                    End If


                    ' lemLog()
                    'Close()
                End If
            End If

        End If
    End Sub

    Public Shared Sub HandleTimerEvent(source As Object, e As ElapsedEventArgs)
        AdministerRemainingTime()
    End Sub

End Class
