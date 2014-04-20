Imports System.Windows.Interop
Imports System.Runtime.InteropServices
Imports Appbar

Public Class LiveSessionDockingWindow

    Const ABM_NEW As Integer = 0
    Const ABM_REMOVE As Integer = 1
    Const ABM_QUERYPOS As Integer = 2
    Const ABM_SETPOS As Integer = 3
    Const ABM_GETSTATE As Integer = 4
    Const ABM_GETTASKBARPOS As Integer = 5
    Const ABM_ACTIVATE As Integer = 6
    Const ABM_GETAUTOHIDEBAR As Integer = 7
    Const ABM_SETAUTOHIDEBAR As Integer = 8
    Const ABM_WINDOWPOSCHANGED As Integer = 9
    Const ABM_SETSTATE As Integer = 10
    Const ABN_STATECHANGE As Integer = 0
    Const ABN_POSCHANGED As Integer = 1
    Const ABN_FULLSCREENAPP As Integer = 2

    Const ABN_WINDOWARRANGE As Integer = 3
    Const ABE_LEFT As Integer = 0
    Const ABE_TOP As Integer = 1
    Const ABE_RIGHT As Integer = 2
    Const ABE_BOTTOM As Integer = 3

    Private fBarRegistered As Boolean = False
    Private uCallBack As Integer






    Private Sub RegisterBar()
        Dim helper As WindowInteropHelper = New WindowInteropHelper(Me)
        Dim mainwindowSrc As HwndSource = TryCast(HwndSource.FromHwnd(helper.Handle), HwndSource)

        Dim abd As APPBARDATA = New APPBARDATA
        abd.cbSize = Marshal.SizeOf(abd)
        abd.hWnd = mainwindowSrc.Handle

        If Not (fBarRegistered) Then
            uCallBack = RegisterWindowMessageW("AppBarMessage")
            abd.uCallbackMessage = uCallBack

            Dim ret As UInteger = SHAppBarMessage(ABM_NEW, abd)

            fBarRegistered = True

            ABSetPos()

        End If
    End Sub





    Public Sub UnregisterBar()
        Dim helper As WindowInteropHelper = New WindowInteropHelper(Me)
        Dim mainwindowSrc As HwndSource = TryCast(HwndSource.FromHwnd(helper.Handle), HwndSource)

        Dim abd As APPBARDATA = New APPBARDATA
        abd.cbSize = Marshal.SizeOf(abd)
        abd.hWnd = mainwindowSrc.Handle


        If (fBarRegistered) Then
            SHAppBarMessage(ABM_REMOVE, abd)
            fBarRegistered = False
        End If
    End Sub


    Public Sub ABSetPos()

        If (fBarRegistered) Then
            Dim helper As WindowInteropHelper = New WindowInteropHelper(Me)
            Dim mainWindowSrc As HwndSource = TryCast(HwndSource.FromHwnd(helper.Handle), HwndSource)

            Dim abd As APPBARDATA = New APPBARDATA
            abd.cbSize = Marshal.SizeOf(abd)
            abd.hWnd = mainWindowSrc.Handle
            'abd.uEdge = MySettings.Default.uEdge

            If (abd.uEdge = ABE_TOP) Or (abd.uEdge = ABE_BOTTOM) Then
                abd.rc.left = 0
                abd.rc.right = Convert.ToInt32(SystemParameters.PrimaryScreenWidth)

                If (abd.uEdge = ABE_TOP) Then
                    abd.rc.top = 0
                    abd.rc.bottom = Convert.ToInt32(Me.ActualHeight)
                Else
                    abd.rc.bottom = SystemParameters.PrimaryScreenHeight
                    abd.rc.top = abd.rc.bottom - Convert.ToInt32(Me.ActualHeight)

                End If
            Else
                abd.rc.top = 0
                abd.rc.bottom = Convert.ToInt32(SystemParameters.PrimaryScreenHeight)
                If (abd.uEdge = ABE_LEFT) Then
                    abd.rc.left = 0
                    abd.rc.right = Convert.ToInt32(Me.ActualWidth)
                Else
                    abd.rc.right = Convert.ToInt32(SystemParameters.PrimaryScreenWidth)
                    abd.rc.left = abd.rc.right - Convert.ToInt32(Me.ActualWidth)

                End If

            End If

            SHAppBarMessage(ABM_QUERYPOS, abd)

            SHAppBarMessage(ABM_SETPOS, abd)

            MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, True)

        End If
    End Sub

    Public Structure APPBARDATA
        Public cbSize As Integer
        Public hWnd As IntPtr
        Public uCallbackMessage As Integer
        Public uEdge As Integer
        Public rc As Rect
        Public lParam As IntPtr
    End Structure

    Public Structure RECT
        Public left As Integer
        Public right As Integer
        Public top As Integer
        Public bottom As Integer


    End Structure

    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        MyBase.OnClosing(e)



    End Sub


    Public Function WndProc(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr


        ABSetPos()
        handled = True

        Return IntPtr.Zero



    End Function

    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)

        Dim hwnd As IntPtr = New WindowInteropHelper(Me).Handle


        Dim source As HwndSource = HwndSource.FromHwnd(hwnd)
        source.AddHook(New HwndSourceHook(AddressOf WndProc))

        RegisterBar()


    End Sub


    Declare Function SHAppBarMessage Lib "SHELL32" (dwMessage As Integer, ByRef PData As APPBARDATA) As UInteger


    Declare Function MoveWindow Lib "User32.dll" (hwnd As IntPtr, x As Integer, y As Integer, cx As Integer, cy As Integer, repaint As Boolean) As Boolean

    Declare Function RegisterWindowMessageW Lib "User32.dll" (message As String) As Integer







    Private Sub Window_Closing_1(sender As Object, e As ComponentModel.CancelEventArgs)
        UnregisterBar()
    End Sub
End Class
