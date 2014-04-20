Imports System.Xml





Public Class CloseSessionDialog
    Implements CloseSessionUtils.CloseSessionOptionsWindow





    Private m_title As String
    Private Shared m_ArtistList As List(Of LemsSessionRunnerV2.Artist)
    Private m_defaultName As String
    Private m_options As CloseSessionOptions
    Private m_features As List(Of SongData.FeatureInfo)
    Private m_ThisSession As SetupSession.Session
    Private m_Artist As LemsSessionRunnerV2.Artist

    Public isSolo As Boolean

    Private m_SelectedFeaturedArtist As LemsSessionRunnerV2.Artist



    Public Shared Function showCloseSessionDialogue(ByVal DefaultSongTitle As String, ByRef session As SetupSession.Session) As CloseSessionOptions

        Dim window As CloseSessionDialog = New CloseSessionDialog



        m_ArtistList = Application.ArtistList






        window.m_title = DefaultSongTitle

        window.m_defaultName = DefaultSongTitle

        window.m_features = session.Song_Object.Get_Features_List

        window.m_options.m_Choice = CloseSessionOptionsChoice.Cancel

        window.m_options.FeaturesList = New List(Of Object)

        window.m_Artist = session.GetArtist


        If (Not session.Song_Object.IS_NEW) And (session.Song_Object.Get_Features_List.Count = 0) Then
            window.isSolo = True
        End If

        window.TopLabel.Content = window.m_Artist.My_Name

        window.m_ThisSession = session

        window.m_Artist = session.GetArtist


        If Not (window.m_ThisSession.IS_NEW) And Not (window.m_ThisSession.Song_Object.Has_Features) Then
            window.isSolo = True
        End If

        If (window.cleanArtistList()) Then
            window.CheckBoxMixReady.IsChecked = window.m_ThisSession.Song_Object.MixReady

            If (window.m_ThisSession.Song_Object.Needs_Feature > 0) Then
                window.CheckBoxNeedsFeature.IsChecked = True
            End If

        End If




        window.TypeNameBox.Text = window.m_title

        window.StatusWindow.Text = "Bounce or Close?"

        window.updateFinalText()

        window.TypeNameBox.Focus()

        window.TypeNameBox.SelectAll()




        window.ShowDialog()

        Return window.m_options




    End Function

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        m_options = New CloseSessionOptions
        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Function cleanArtistList() As Boolean

        Dim list1 As List(Of Artist) = New List(Of Artist)
        Dim list2 As List(Of Artist) = New List(Of Artist)

        Dim dummyArtist As LemsSessionRunnerV2.Artist = New LemsSessionRunnerV2.Artist("SOLO", True, True)


        ArtistListBox.Items.Add(dummyArtist)



        ' Dim sortList As List(Of LemsSessionRunnerV2.Artist) = New List(Of Artist)


        For Each art As Artist In m_ArtistList
            If Not (art.My_Name = m_Artist.My_Name) Then


                If Not (Application.ARTIST_RELEVANCE_OBJECT.getRelevance(art) = 0) Then
                    list1.Add(art)
                Else
                    list2.Add(art)
                End If
            End If
        Next



        list1.Sort(New ArtistRelevanceComparer)
        list2.Sort()



        For Each ar As LemsSessionRunnerV2.Artist In list1

            ArtistListBox.Items.Add(ar)

        Next

        For Each ar As Artist In list2

            ArtistListBox.Items.Add(ar)


        Next
        Return True
    End Function

    Private Function updateFinalText() As Boolean

        

        If (m_title.Contains("Unfinishedsong_")) Then
            FinalNameTextBlock.Text = "Song Not Named"

            If Not (StatusWindow.Text.Contains(" --You need to name this song before it can be bounced to MP3-- ")) Then
                StatusWindow.Text += " --You need to name this song before it can be bounced to MP3-- "
            End If


        End If
        Dim fnl As String = m_title

        Dim list As List(Of String) = New List(Of String)
        For Each ft As SongData.FeatureInfo In m_features
            list.Add(ft.Artist_Name)

        Next
        If (m_ThisSession.Song_Object.Has_Features) Then
            fnl += " feat - " & Utils.CombineNames(list)
        End If
        fnl = fnl.Replace("'", "").Replace("-", "").Replace(".", "")


        FinalNameTextBlock.Text = fnl

        Return True

    End Function


    Private Sub ArtistListBox_SelectionChanged(sender As Object, e As MouseButtonEventArgs) Handles ArtistListBox.MouseLeftButtonUp
        e.Handled = True

        m_SelectedFeaturedArtist = ArtistListBox.SelectedItem

        If (m_SelectedFeaturedArtist.My_Name = "SOLO") Then
            StatusWindow.Text = " --No Features On this Track!!-- "
            m_ThisSession.Song_Object.Get_Features_List.Clear()
            isSolo = True

        Else

            AddFeature(m_SelectedFeaturedArtist)

        End If


        updateFinalText()











    End Sub


    Public Function AddFeature(artist As Object) As Boolean
        Dim isThere As Boolean = False


        If (m_SelectedFeaturedArtist.My_Name = "SOLO") Then
            StatusWindow.Text = " --No Features On this Track!!-- "
            m_ThisSession.Song_Object.Get_Features_List.Clear()
            isSolo = True

            m_features = m_ThisSession.Song_Object.Get_Features_List
            ArtistListBox.UnselectAll()
            updateFinalText()
            Return True


        ElseIf (m_SelectedFeaturedArtist.My_Name = m_Artist.My_Name) Then

            m_features = m_ThisSession.Song_Object.Get_Features_List
            ArtistListBox.UnselectAll()
            updateFinalText()
            Return True

        End If

        Dim newFT As LemsSessionRunnerV2.Artist = TryCast(artist, LemsSessionRunnerV2.Artist)
        For Each ft As SongData.FeatureInfo In m_ThisSession.Song_Object.Get_Features_List()
            If (ft.Artist_Name = newFT.My_Name) Then
                isThere = True
                Exit For
            End If
        Next

        If (isThere) Then
            m_ThisSession.Song_Object.AddFeature(newFT, True)
            StatusWindow.Text += " --REMOVED " & newFT.My_Name & " from Features-- "

            If (m_ThisSession.Song_Object.Get_Features_List.Count = 0) Then
                isSolo = True
            End If

        Else
            m_ThisSession.Song_Object.AddFeature(newFT, True)
            StatusWindow.Text += " --ADDED " & newFT.My_Name & " to Features-- "
        End If
        m_features = m_ThisSession.Song_Object.Get_Features_List
        ArtistListBox.UnselectAll()
        updateFinalText()
        Return True

    End Function

    Public Property ArtistList As List(Of Object) Implements CloseSessionUtils.CloseSessionOptionsWindow.ArtistList
        Get
            Return Nothing
        End Get
        Set(value As List(Of Object))

        End Set
    End Property


    Public Property DefaultTitle As String Implements CloseSessionUtils.CloseSessionOptionsWindow.DefaultTitle

    Public Property Options As CloseSessionOptions Implements CloseSessionUtils.CloseSessionOptionsWindow.Options
        Get
            Return m_options
        End Get
        Set(value As CloseSessionOptions)
            m_options = value
        End Set
    End Property

    Public Property SessionToClose As Object Implements CloseSessionUtils.CloseSessionOptionsWindow.SessionToClose

        Get
            Return TryCast(m_ThisSession, SetupSession.Session)

        End Get
        Set(value As Object)
            m_ThisSession = TryCast(value, SetupSession.Session)

        End Set
    End Property



    Public Sub SetTitle(title As String) Implements CloseSessionUtils.CloseSessionOptionsWindow.SetTitle
        m_title = m_title.Remove(".").Remove("'")
        m_title = Utils.converToTitleCase(title)

    End Sub

    Public Function ShowWindow() As CloseSessionOptions Implements CloseSessionUtils.CloseSessionOptionsWindow.ShowWindow
        Return Nothing
    End Function

    Public Property SongTitle As String Implements CloseSessionUtils.CloseSessionOptionsWindow.SongTitle

        Get
            Return m_title
        End Get
        Set(value As String)
            SetTitle(value)
        End Set
    End Property
    Public Property ThisArtist As Object Implements CloseSessionUtils.CloseSessionOptionsWindow.ThisArtist
        Get
            Return m_Artist
        End Get
        Set(value As Object)
            If (TypeOf (value) Is Artist) Then
                m_Artist = value
            End If

        End Set
    End Property

    Private Sub TextBox_GotFocus_1(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If (m_title.Contains("Unfinishedsong_")) Then
            TypeNameBox.Text = ""
            If (TypeNameBox.Text.Contains("this will be used to display the current properties for this song and to warn of changes")) Then
                TypeNameBox.Clear()
                TypeNameBox.Text = ""
            Else
                ' TypeNameBox.ScrollToEnd()
                'TypeNameBox.Select(TypeNameBox.Text.Length - 1, 1)
                TypeNameBox.SelectAll()
                'TypeNameBox.SelectAll()


            End If

            If Not (StatusWindow.Text.Contains("Enter New Name and Select a Feature")) Then
                StatusWindow.Text += "--Enter New Name and Select a Feature or two from the list below or choose none if this song is solo--"
            End If


        End If

    End Sub





    Private Sub CheckBoxMixReady_Checked(sender As Object, e As RoutedEventArgs) Handles CheckBoxMixReady.Checked
        e.Handled = True
        m_ThisSession.Song_Object.MixReady = TryCast(sender, CheckBox).IsChecked


    End Sub

    Private Sub CheckBoxNeedsFeature_Checked(sender As Object, e As RoutedEventArgs) Handles CheckBoxNeedsFeature.Click
        e.Handled = True
        If (CheckBoxNeedsFeature.IsChecked) Then
            m_ThisSession.Song_Object.Needs_Feature += 1
        Else
            m_ThisSession.Song_Object.Needs_Feature = 0
        End If

        'm_ThisSession.Song_Object.Needs_Feature = TryCast(sender, CheckBox).IsChecked



    End Sub


    Private Function validateFinalName() As Boolean

        If (TypeNameBox.Text.StartsWith("Unfinished")) Then
            Return False
        End If




        If Not (TypeNameBox.Text.Length > 1) Then
            StatusWindow.Text = "  NAME TOO SHORT!!!, I WILL NOT BOUNCE UNDER THESE CIRCUMSTANCES  "

            Return False
        ElseIf (m_title = m_defaultName) Then
            Return True

        ElseIf (m_title.Contains("  ")) Then
            StatusWindow.Text += " --Don't use Double Spaces-- "
            m_title = m_defaultName
            updateFinalText()
            Return False
        ElseIf (m_title = "") Or (m_title = " ") Then
            StatusWindow.Text = " --Song Title Empty-- "
            m_title = m_defaultName
            updateFinalText()
            Return False
        End If


        updateFinalText()
        Return True
    End Function

    Private Sub BounceButton_Click(sender As Object, e As RoutedEventArgs) Handles BounceButton.Click
        e.Handled = True
        If Not (validateFinalName()) Then
            StatusWindow.Text += "  Try Again  "
            Return
        End If



        If (m_title.Contains("Unfinishedsong_")) Then
            StatusWindow.Text = "  NAME NOT SET..CAN NOT BOUNCE UNAMED SONG"
        ElseIf (m_features.Count = 0) Then



            If (ArtistListBox.SelectedItem Is Nothing) Then

                If Not (isSolo) Then
                    StatusWindow.Text = "  PLEASE SELECT SOMETHING FROM THE FEATURES LIST FIRST,  EVEN IF YOU ONLY SELECT SOLO  "
                    Return
                Else



                    m_options.m_Choice = CloseSessionOptionsChoice.BounceSession
                    m_options.SongTitle = m_title
                    ' m_ThisSession.Song_Object.SetNameFirstTime(m_options)
                    m_ThisSession.Song_Object.MixReady = CheckBoxMixReady.IsChecked

                    Me.Close()

                End If

        Else
            m_options.m_Choice = CloseSessionOptionsChoice.BounceSession
            m_options.SongTitle = m_title
            ' m_ThisSession.Song_Object.SetNameFirstTime(m_options)
            m_ThisSession.Song_Object.MixReady = CheckBoxMixReady.IsChecked

            'If (CheckBoxNeedsFeature.IsChecked) Then
            '    m_ThisSession.Song_Object.Needs_Feature += 1
            'Else
            '    m_ThisSession.Song_Object.Needs_Feature = 0
            'End If

            Me.Close()


            End If




        Else


            m_options.m_Choice = CloseSessionOptionsChoice.BounceSession
            m_options.SongTitle = m_title
            ' m_ThisSession.Song_Object.SetNameFirstTime(m_options)
            m_ThisSession.Song_Object.MixReady = CheckBoxMixReady.IsChecked

            Me.Close()


        End If





    End Sub

    Private Sub CloseSessionButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseSessionButton.Click

        If Not (validateFinalName()) Then
            Return
        End If


        m_options.m_Choice = CloseSessionOptionsChoice.CloseSessionNoBounce
        m_options.SongTitle = m_title
        'For Each ft As SongData.FeatureInfo In m_features
        '    m_options.FeaturesList.Add(ft)

        'Next

        

        'If Not (m_title = m_defaultName) Then
        '    m_ThisSession.UpdateName(m_title)
        'End If


        Me.Close()


    End Sub


    Private Sub OnTextBoxKeyEntered(sender As Object, e As KeyEventArgs)
        If (TypeNameBox.IsFocused) Then
            If (TypeNameBox.Text.Length > 1) Then
                m_title = Utils.converToTitleCase(TypeNameBox.Text)

                updateFinalText()
            End If



        End If
        ' e.Handled = True
    End Sub


End Class