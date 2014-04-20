Imports System.Data
Imports System.IO
Imports System.Text.RegularExpressions
Imports PriorityLib
Imports LemsUTools
Imports LemsUTools.WaveAndMp3Manipulation
Imports LemsUTools.Mp3Utils
Imports LemsDotNetHelpers
Imports UntameVBUtils



Public Class SongData
    Inherits Identity
    Implements IComparable(Of SongData)







    Private s_artistName As String
    Private s_TitleName As String
    Private s_lastModified As DateTime
    Private s_mixReady As Boolean = False
    Private s_bounced As Boolean
    Private s_mp3FilePath As String = "Not Bounced"
    Private s_RecordingPath As String
    Private s_TrackInfo As BaseTrackInfoWrapper
    Private s_hasFeatures As Boolean = False
    Private s_hasComments As Boolean = False
    Private s_SongComments As List(Of Comment) = New List(Of Comment)
    Private s_priorityLocal As String
    Private s_priorityGlobal As String
    Private s_id As Int32
    Private s_artistOBJ As Artist
    Private Shared s_songsXML As SongInfoXML = Application.SONG_INFO_XML
    Private s_needsFeature As Int32 = 0
    Private s_musicSource As SetupSession.SessionMusicSource_Type
    Private rebuilt As Boolean
    Private s_isNew As Boolean
    Private s_features As List(Of FeatureInfo) = New List(Of FeatureInfo)
    Private IsInitialized As Boolean = False
    Private Shared s_tempNameHolder As String

    Protected Feautres_Node As XElement
    Protected featureInfoNode As XElement
    Protected commentsNode As XElement
    Protected songNode As XElement


    Public Class SongPriority
        Inherits PriorityOrganizerBase

        Private m_artist As Artist



        Public Sub New(name As String, artist As Artist)
            MyBase.New(name)
            m_artist = artist
        End Sub

        Public Shared Function Retrieve(setName As String, artist As Artist) As SongPriority
            Dim result As SongPriority = New SongPriority(setName, Artist)

            result.LoadPrioritySet(setName)

            Return result
        End Function

        Public Function StoreData() As Boolean
            StorePrioritySet()
            Return True
        End Function

        Protected Overrides Function AddCompetitor(descriptor As String) As Integer
            Return MyBase.AddCompetitor(descriptor)
        End Function

        Public Function AddPriority(desc As String) As Int32
            Return AddCompetitor(desc)
        End Function

        Public Function RemovePriority(desc As String) As Int32

            Return RemoveCompetitor(desc)

        End Function

        Public Function GetPriorityName(index As Int32) As String
            Return (GetPriorityObject(index).Descriptor)
        End Function

        Public Function Get_Priority(desc As String) As Int32
            Return Me.GetPriority(desc)


        End Function

        Public Function Up(desc As String) As Int32
            Return MoveUp(desc)


        End Function

        Public Function Down(desc As String) As Int32
            Return MoveDown(desc)
        End Function

        Public Function Top(desc As String) As Int32
            Return MoveToTop(desc)
        End Function

        Public Function Bottom(desc As String) As Int32
            Return MoveToBottom(desc)
        End Function

        Public Shared Property Get_Priority_Count As Int32
            Get
                Return 0

            End Get
            Set(value As Int32)

            End Set
        End Property

        Private Sub StorePrioritySet()

            Dim args As XMLResources.SimpleResourceArgs = New XMLResources.SimpleResourceArgs("SongPriority", _name, Nothing)



            For Each p As PriorityObject In Me
                Dim pnode As XMLResources.SimpleResourceArgs = New XMLResources.SimpleResourceArgs("SongPriority", p.Descriptor, args)
                pnode.atts.Add(New XMLResources.ResourceAttribute("position", p.Priority))



                'args.children.Add(pnode)



            Next


            m_artist.Resources.AddResourceRecursive(args, True, Nothing)


        End Sub

        Private Sub LoadPrioritySet(setName As String)

            setName = setName.Replace(" ", "-")
            Dim motherNode As XElement = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="SongPriority")

            If motherNode Is Nothing Then
                Return
            End If

            Dim myNode As XElement = motherNode.Element(_name)

            If myNode Is Nothing Then
                Return
            End If


            For Each e As XElement In myNode.Elements
                Dim po As PriorityObject = New PriorityObject()
                po.Descriptor = e.Name.LocalName
                po.Priority = e.Attribute("position").Value
                Add(po)

            Next



        End Sub




    End Class
    Public Class Comment
        Inherits Identity



        ' Private Const baseId As String = "UNT-12001-000"
        Private id As Int32
        Private Shared GlobalCommentCount As Int32

        'Private commentList As Dictionary(Of Int32, KeyValuePair(Of String, String))
        Private m_Song As String = ""
        Private songID As String = ""
        Private songOwner As String = ""
        Private songOBJ As SongData
        Private temporarySongHolder As SongData
        Private status As String = ""
        Private author As String = ""
        Private text As String = ""
        Private replyText As String
        Private c_HasBeenRead = False
        Private c_RepliedTo = False
        Private c_Replies = New ReplyList()



        'Private resolvedComments As Dictionary(Of Int32, Boolean) = New Dictionary(Of Int32, Boolean)

        'Private flags As List(Of String)
        Public Class ReplyList
            Inherits List(Of String)



        End Class

        



        Public Overrides Sub OnRebuild(ByRef GlobalID As GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")

            If (songOBJ Is Nothing And Not temporarySongHolder Is Nothing) Then
                Create(temporarySongHolder)
            End If
            Dim gid = GlobalID.GLOBAL_ID

            Dim cmtNode = temporarySongHolder.Get_Song_Node_Child("Comments")
            Dim myNodeQuery = From mynode As XElement In cmtNode.Elements
                              Where mynode.Attribute("id").Value = gid
                              Select mynode


            status = myNodeQuery.First.Attribute("status").Value
            author = myNodeQuery.First.Attribute("author").Value
            text = myNodeQuery.First.Attribute("text").Value
        End Sub

        Public Sub New(ByRef Song As SongData, Optional ByRef GlbID As String = "", Optional ByVal Status As Admin.Administrator.AdministratorType = Admin.Administrator.AdministratorType.ARTIST, Optional ByVal CommentAuthor As String = "Anonymous", Optional ByVal CommentText As String = "Nothing To Say I Guess")

            MyBase.New(ClassCode.Comment)

            If Not (IS_NEW) Then

                Exit Sub
            End If

            If (GlbID.Contains("UNT-")) Then
                temporarySongHolder = Song
                Exit Sub
            End If


            Create(Song)

            CreateComment(Status, CommentAuthor, CommentText)

        End Sub



        Public Sub Create(ByRef song As SongData)

            songOBJ = song
            m_Song = song.Title
            songID = song.LOCAL_ID
            songOwner = song.Artist_Name


        End Sub

        Public Sub CreateComment(ByVal StatusArg As Admin.Administrator.AdministratorType, ByVal CommentAuthor As String, ByVal _text As String)
            status = StatusArg
            author = CommentAuthor
            text = _text
        End Sub





        Public Function GetCommentText() As String
            Return text
        End Function

        Public Function GetCommentAuthor() As String
            Return author
        End Function

        Public Function GetCommentStatus() As String
            Return status
        End Function

        Public Function GetSongOwner() As String
            Return songOwner
        End Function

        Public Function GetCommentIdAsString() As String
            Return m_GlobalID.GLOBAL_ID
        End Function

        Public Function GetCommentIdAsInt() As Int32
            Return m_LocalID
        End Function





        Public Function StoreCommentNode(ByRef CommentsNode As XElement) As XElement

            Dim gid = GLOBAL_ID
            Dim query = From node As XElement In CommentsNode.Elements
                        Where node.Attribute("id") = gid
                        Select node

            If (query.Single Is GetType(XElement)) Then
                query.Single.Attribute("author").Value = author
                query.Single.Attribute("status").Value = status
                query.Single.Attribute("text").Value = text

            End If


            Dim commentNode As XElement = New XElement("Comment", New XAttribute("author", author), New XAttribute("id", m_LocalID), New XAttribute("status", status), New XAttribute("text", text))


            CommentsNode.Add(commentNode)


            Return CommentsNode

        End Function










    End Class


    'Public Sub New(ByRef Artist As Artist)

    '    MyBase.New(ClassCode.Song, Artist.Song.Title)
    '    LocalID = m_LocalID
    '    lastModified = Now
    '    id = GlobalID
    '    artistOBJ = Artist

    '    Artist.Song = Me




    '    If (LocalID = 0) Then
    '        Return
    '    End If
    '    ' Create(LocalID)

    'End Sub

    Public Shared Function Get_Song_Info_Root() As XElement
        If (s_songsXML Is Nothing) Then
            s_songsXML = Application.SONG_INFO_XML
        End If
        Return SongInfoXML.Get_Root
    End Function

    Public Shared Function CreateAndReturn(theArtist As Artist, songName As String) As SongData


        Return New SongData(theArtist, songName)

    End Function

    Public Shared Function Get_Global_Features_Node() As XElement

        If (s_songsXML Is Nothing) Then
            s_songsXML = Application.SONG_INFO_XML

        End If

        Dim featureInfo As XElement = s_songsXML.Get_Feature_Info_Node



        Return featureInfo


    End Function

    Public Function DeleteAllTracesOfSong() As Boolean


        If (IsBounced) Then
            Try
                My.Computer.FileSystem.DeleteFile(MP3_File_Path)
            Catch ex As Exception
                Return False
            End Try

        End If

        Return True


    End Function


    Public Shared Sub Update_Songs_XML()

        s_songsXML.UpdateData()


    End Sub

    Public Sub SubmitChanges()


        s_songsXML.UpdateData()





    End Sub


    Public Async Function WaitforInitialization() As Task(Of Boolean)

        While Not (IsInitialized)
            Await Task.Delay(50)
            Await WaitforInitialization()
        End While

        Return True

    End Function

    Private Function LocalWaitForInitialization() As Boolean

        While Not (IsInitialized)
            LocalWaitForInitialization()
        End While
        Return True
    End Function

    Public Overrides Sub OnRebuild(ByRef GlobalID As Identity.GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")
        If (GlobalID.GLOBAL_ID = m_GlobalID.GLOBAL_ID) Then
            rebuilt = True
            s_isNew = False
            s_TitleName = m_name
            s_id = m_LocalID
            s_artistOBJ = returnOBJ
            s_artistName = s_artistOBJ.My_Name
            s_mp3FilePath = GetBouncedSongPath(False)
            s_TrackInfo = BaseTrackInfoWrapper.GetTrackInfoBasedOnUser(Title, s_artistOBJ)







            If (Utils.CheckIfSongIsBounced(Me)) Then
                Dim p = GetBouncedSongPath(True)
                If Not (p = "") Then
                    s_bounced = True
                    s_mp3FilePath = GetBouncedSongPath(True)
                    Dim info As FileInfo = My.Computer.FileSystem.GetFileInfo(s_mp3FilePath)
                    s_lastModified = info.LastWriteTime
                End If
                
            End If

            Init()

        End If
    End Sub

    Public Shared Function GetSongArtistName(songNameOrLID As String, Optional getGIDinstead As Boolean = False) As String
        Dim regx = New Regex("[0-9]")
        Dim result As String = ""
        If (regx.IsMatch(songNameOrLID)) Then
            For Each song As XElement In SongInfoXML.Get_Root.Element("Songs").Elements
                If (song.Attribute("Identity").Value) = songNameOrLID Then

                    result = song.Attribute("artist").Value
                    Exit For
                End If

            Next

        Else
            'If (Not songNameOrLID = "") Then
            '    For Each song As XElement In s_songsXML.Get_Root.Elements
            '        If (Utils.ExtracLocalFromGlobal(song.Attribute("Identity").Value) = Integer.Parse(songNameOrLID)) Then
            '            result = song.Attribute("artist").Value
            '        End If

            '    Next
            'End If

        End If

        If (getGIDinstead) Then
            Return Identity.GlobalID.FindGlobalIDByName(result)
        Else
            Return result
        End If
    End Function
    Private Function GetFeatureStrings() As List(Of String)
        Dim result As List(Of String) = New List(Of String)
        For Each feat As FeatureInfo In Get_Features_List
            result.Add(feat.Artist_Name)
        Next
        Return result
    End Function
    Private Shared Sub TEST_MESS(message As String)
        If (Application.UseTEST_MESS) Then
            MsgBox(message)
        End If
    End Sub
    Public Overrides Sub OnNameChanged(ByRef newName As String, ByRef globalId As Identity.GlobalID, Optional ByRef data As Object = "")
        If Not (globalId.CLASS_CODE = ClassCode.Song) Then
            Return
        End If
        MyBase.OnNameChanged(newName, globalId)
        Dim nuname As String = NamingConventions.addFeatureText(Title, GetFeatureStrings())
        Dim nufilename As String = nuname & ".mp3"
        Dim fullpath = MP3_Location + nufilename
        s_mp3FilePath = fullpath
        Dim notnewsong As Boolean = Utils.CheckIfSongIsBounced(Me)
        Dim nameChange As Boolean = (Not s_tempNameHolder = "") And (Not s_tempNameHolder = newName)



        s_TitleName = m_name
        Dim app As Application = Application.Current
        Dim tempDelegate As Application.SetTempFileLocDelegate = New Application.SetTempFileLocDelegate(AddressOf Application.SetTempFileLoc)

        If Not s_tempNameHolder = "" Then





            app.Dispatcher.Invoke(tempDelegate, {True, fullpath})
            Dim bouncepath As String = GetBouncedSongPath(True, False)


            If (notnewsong Or nameChange) Then
                Try

                    If (System.IO.File.Exists(fullpath) And newName = s_tempNameHolder) Then
                        If Not FLAG_POSTBOUNCE_UPDATE Then
                            ' System.IO.File.Delete(fullpath)

                            s_tempNameHolder = ""
                        Else

                        End If



                        Return
                    ElseIf (nameChange) Then
                        Dim oldPath = Application.TEMP_BOUNCE_PATH
                        bouncepath = MP3_Location + Utils.addFeatureText(Old_Name, Get_Features_List)
                        If (ChangeNameForBounce) Then
                            If (System.IO.File.Exists(oldPath)) Then
                                System.IO.File.Delete(oldPath)

                            End If

                        ElseIf Not (oldPath = "") Then
                            nuname = NamingConventions.addFeatureText(Title, GetFeatureStrings())
                            nufilename = nuname & ".mp3"
                            fullpath = MP3_Location + nufilename
                            s_mp3FilePath = fullpath

                            If (System.IO.File.Exists(oldPath)) Then
                                My.Computer.FileSystem.RenameFile(oldPath, nufilename)
                                Return
                            End If


                        End If


                    End If


                    If ((Not bouncepath Is Nothing) And (Not bouncepath = "") And (System.IO.File.Exists(bouncepath))) Then
                        If (Not Path.GetFileName(bouncepath) = nufilename) Then
                            My.Computer.FileSystem.RenameFile(bouncepath, nufilename)
                        End If

                    End If




                Catch ex As Exception
                    TEST_MESS("RenameFile error")
                    Throw New Exception("RENAME FILE ERROR -- " + ex.Message)


                    Return
                End Try


            End If
            If Not (m_name = "") Then

            End If
            Utils.writeSongFolderName(s_artistOBJ.RecordingPath & s_tempNameHolder, m_name, True)
            s_tempNameHolder = ""



            ' s_artistOBJ.StoreAllData()
            If (songNode Is Nothing) Then
                songNode = s_songsXML.Get_Song_Node(s_id, GLOBAL_ID, Title, MixReady, Has_Features, s_lastModified, Artist_Name, IsBounced, Has_Comments, LocalPriority, GetBouncedSongPath(False), Has_Features, Needs_Feature, Music_Source, s_TrackInfo.ID, False, False)
            Else
                If (Title.Contains("Unfinished_")) Then
                    songNode.Remove()
                Else
                    songNode.Attribute("name").Value = m_name


                    songNode.Attribute("mp3").Value = s_mp3FilePath
                    If (Utils.CheckIfSongIsBounced(Me)) Then

                        songNode.Attribute("mp3").Value = GetBouncedSongPath(True)
                    End If
                    ' s_mp3FilePath = songNode.Attribute("mp3").Value

                End If
            End If
            

        If (Has_Features) Then
            Dim ft As FeatureInfo = s_features.Item(0)
            ft.UpdateFeatureSongName()
        End If
        SubmitChanges()
        Identity.GlobalID.CommitAllData()
        Init()
        'MsgBox("name should be changed in all places")
        End If
        FLAG_POSTBOUNCE_UPDATE = False


    End Sub



    Public Function UpdatePriority(priority As Int32) As Boolean


        Return False

    End Function




    Public Sub New()

    End Sub
    ''' <summary>
    ''' Use this to make sure features are only added once
    ''' </summary>
    ''' <param name="feature"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function updateFeatureList(ByRef feature As Artist, Optional ByVal ftID As String = "") As List(Of FeatureInfo)

        Dim isThere As Boolean = False
        Dim i As Int16
        Dim index As Int16 = 0
        Dim remove As SongData.FeatureInfo = Nothing
        For Each ft As FeatureInfo In s_features
            If (ft.Artist_Name = feature.My_Name) Then
                isThere = True
                remove = ft
                index = i
            End If
            i += 1
        Next
        If (isThere) Then
            s_features.RemoveAt(index)
            StoreData()
        Else
            s_features.Add(New FeatureInfo(feature, Me))
            StoreData()
        End If


        Return s_features
    End Function



    Public Sub New(ByRef artist As Artist, ByVal songName As String, Optional ByRef GlobalID As String = "", Optional ByRef returnObject As Object = Nothing, Optional noInit As Boolean = False)
        MyBase.New(ClassCode.Song, songName, GlobalID, returnObject)

        If (GlobalID.Contains("UNT-")) Then
            s_isNew = False
        End If
        ' s_priority = New Priority(artist)

        s_id = m_LocalID
        s_TitleName = m_name
        s_RecordingPath = artist.RecordingPath & s_TitleName
        s_artistOBJ = artist

        s_TrackInfo = New BaseTrackInfoWrapper()





        ' s_songsXML = New SongInfoXML()

        s_artistName = artist.My_Name

        's_musicSource = artist.Get_Current_Session.Get_Music_Source

       
        ' s_lastModified = Now
        's_priority = New Priority(s_artistOBJ)









        'not sure if i need this yet
        If (noInit) Then
            ' s_mp3FilePath = artist.MP3Path & m_name & ".mp3"
            ' Init()
            If Not (Title Is Nothing) Then
                songNode = s_songsXML.Get_Song_Node(s_id, GLOBAL_ID, Title, MixReady, Has_Features, s_lastModified, Artist_Name, IsBounced, Has_Comments, LocalPriority, GetBouncedSongPath(False), Has_Features, Needs_Feature, Music_Source, s_TrackInfo.ID, False, False)
                featureInfoNode = s_songsXML.Get_Feature_Info_Node
            End If
            
        Else
            Init()
        End If












    End Sub
    Private ChangeNameForBounce As Boolean = False
    Public Sub UpdateName(ByVal newName As String, forBounce As Boolean)
        ChangeNameForBounce = forBounce
        s_features = Utils.Get_Features_Fresh(Me)
        If (Not newName = "") Or (Not newName = m_name) Or (Not newName.Contains("Unfinishedsong_")) Then

            s_tempNameHolder = m_name
            Old_Name = m_name
            m_GlobalID.UpdateName(newName)
        Else
            MsgBox("Name Invalid or the same")
        End If





    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        Return MyBase.Equals(obj)

    End Function

    Public Function GetArtist() As Artist
        Return s_artistOBJ
    End Function

    Public Function AddFeature(ByRef artist As Artist, forBounce As Boolean) As Integer
        Dim result = 0
        If (s_features.Count = 5) Then
            MsgBox("That's too many people for 1 Song")
            s_features.RemoveRange(2, 3)
            Return s_features.Count
        End If
        'For Each ft1 As FeatureInfo In s_features
        '    If (ft1.Artist_Name = artist.My_Name) Then

        '    End If
        'Next
        'Dim ft As FeatureInfo = New FeatureInfo(artist, Me)
        s_features = updateFeatureList(artist)
        s_hasFeatures = s_features.Count > 0
        Dim newName As String = Utils.addFeatureText(Title, s_features) & ".mp3"
        If (IsBounced) Then
            Dim file As String = GetBouncedSongPath(True, False)
            If (file = "") Then


            ElseIf Not (file = My.Computer.FileSystem.CombinePath(MP3_Location, newName)) Then


                My.Computer.FileSystem.RenameFile(file, newName)
                'Try
                '    My.Computer.FileSystem.RenameFile(file, newName)
                'Catch ex As Exception
                '    TEST_MESS(ex.Message)
                '    Return s_features.Count
                'End Try


                's_mp3FilePath = GetBouncedSongPath(m_name)
            Else
                'Application.LEM_ERR("Names the same")
                ' Return
            End If
            Dim newMp3Path = GetBouncedSongPath(True)
            songNode.Attribute("mp3").Value = newMp3Path
        End If
        If Not (forBounce) Then
            StoreData()

            'If (Title.Contains("Unfinished_")) Then
            '    songNode.Remove()
            'End If

            SubmitChanges()
            GlobalID.CommitAllData()
            Init(True)
        End If
       

        Return s_features.Count()
    End Function
    Public Sub RemoveFeature(ByRef artist As Artist)

        Dim removedArtist As SongData.FeatureInfo = Nothing
        For Each ft2 As FeatureInfo In s_features
            If (ft2.Artist_Name = artist.My_Name) Then
                removedArtist = ft2
                updateFeatureList(ft2.Artist_Object)
                Exit For
            End If
        Next
        If (s_features.Count = 0) Then
            s_hasFeatures = False
        End If
        Dim newName As String = Utils.addFeatureText(Title, Get_Features_List) & ".mp3"
        If (IsBounced) Then
            Dim file As String = GetBouncedSongPath(True, False)
            Dim newPath = My.Computer.FileSystem.CombinePath(MP3_Location, newName)
            If Not (file = newPath) Then
                If (Directory.Exists(newPath)) Then
                    Directory.Delete(newPath)
                End If
                My.Computer.FileSystem.RenameFile(file, newName)

                's_mp3FilePath = GetBouncedSongPath(m_name)
            Else


                ' s_mp3FilePath = GetBouncedSongPath(m_name)

            End If
        End If

        StoreData(removeFeature:=True, removedFeature:=removedArtist, storeOnlyDontUpdate:=True)
        If (Title.Contains("Unfinished_")) Then
            songNode.Remove()
        End If
        songNode.Attribute("mp3").Value = GetBouncedSongPath(True)
        SubmitChanges()
        Init(True)
    End Sub
    Public Function ReInitialize()
        Return Init()
    End Function
    Private Function Init(Optional ByRef updateNow As Boolean = False, Optional ByRef detachFromSongInfoXML As Boolean = False) As Boolean
        If (Title Is Nothing) Then
            Return True
        End If
        '

        'If update this node will be filled with the updated properties, else it will look like the xml files version if it exists
        ' featuresNode = songNode.Attribute("features")

        'get the modified date from original session
        's_TrackInfo = BaseTrackInfoWrapper.GetTrackInfoBasedOnUser(s_TitleName, s_artistOBJ)
        For Each di As String In Directory.GetDirectories(s_artistOBJ.RecordingPath)
            If (Path.GetFileName(di) = m_name) Then
                Dim songinfo As DirectoryInfo = My.Computer.FileSystem.GetDirectoryInfo(di)
                s_lastModified = songinfo.LastWriteTime
            End If
        Next
        If (s_TrackInfo.storeThisData()) Then

        End If
        featureInfoNode = s_songsXML.Get_Feature_Info_Node

        songNode = s_songsXML.Get_Song_Node(s_id, GLOBAL_ID, Title, MixReady, Has_Features, s_lastModified, Artist_Name, IsBounced, Has_Comments, LocalPriority, GetBouncedSongPath(False), Has_Features, Needs_Feature, Music_Source, s_TrackInfo.ID, updateNow, detachFromSongInfoXML)


        If (songNode.HasElements) Then



            s_features = Utils.Get_Features_Fresh(Me)

        End If
        s_mp3FilePath = MP3_Location + Utils.addFeatureText(m_name, s_features) + ".mp3"


        If (Convert.ToInt32(songNode.Attribute("musicSource").Value) = 9) Then
            If (Application.useMacros) Then
                s_musicSource = SetupSession.SessionMusicSource_Type.SoundClick
            Else
                s_musicSource = Convert.ToInt32(songNode.Attribute("musicSource").Value)
            End If

        Else
            s_musicSource = Convert.ToInt32(songNode.Attribute("musicSource").Value)
        End If


        Dim hasTrackInfoNode As Boolean = False
        For Each att As XAttribute In songNode.Attributes
            If (att.Name = "trackInfoId") Then
                hasTrackInfoNode = True
                Exit For
            End If
        Next

        If (s_TrackInfo.ID = -1) Then
            'only if soundclick beat and trackInfo is 0
            If (s_musicSource = SetupSession.SessionMusicSource_Type.SoundClick) Then

                If (hasTrackInfoNode) Then




                    Dim soundclickBeat As String = s_artistOBJ.CheckForUsedSoundClickBeat(s_RecordingPath)


                    If Not (soundclickBeat = "") Then
                        s_TrackInfo = New BaseTrackInfoWrapper(soundclickBeat, Title,
                                                             SetupSession.SessionMusicSource_Type.SoundClick, s_artistOBJ.Resources, s_artistOBJ.My_Name)

                    Else
                        s_TrackInfo = New BaseTrackInfoWrapper("not found", Title, SetupSession.SessionMusicSource_Type.SoundClick, s_artistOBJ.Resources, s_artistOBJ.My_Name)

                    End If


                Else
                    'no trackinfo node
                    Dim soundclickBeat As String = s_artistOBJ.CheckForUsedSoundClickBeat(s_RecordingPath)

                    'if we found a beat
                    If Not (soundclickBeat = "") Then
                        s_TrackInfo = New BaseTrackInfoWrapper(soundclickBeat, Title,
                                                             SetupSession.SessionMusicSource_Type.SoundClick, s_artistOBJ.Resources, s_artistOBJ.My_Name)

                        s_TrackInfo.storeThisData()
                        Application.ResourcesHaveChanged = True


                    End If


                End If




            End If

        End If

        s_mixReady = songNode.Attribute("mixReady").Value

        s_needsFeature = Integer.Parse(songNode.Attribute("needsFeature").Value)

        s_hasComments = songNode.Attribute("comments").Value
        s_hasFeatures = Has_Features
        ' s_needsFeature = songNode.Attribute("needsFeatures").Value


        If (Utils.CheckIfSongIsBounced(Me)) Then
            s_bounced = True
            ' s_mp3FilePath = GetBouncedSongPath(Title)
            Dim info As FileInfo = My.Computer.FileSystem.GetFileInfo(s_mp3FilePath)
            s_lastModified = info.LastWriteTime
        Else
            Dim path = GetBouncedSongPath(False, False)
            Dim isthere = File.Exists(path)
            If (isthere) Then
                s_bounced = True
                s_mp3FilePath = path
            Else
                s_bounced = False
                s_mp3FilePath = "Not Bounced"
            End If


        End If

        s_RecordingPath = s_artistOBJ.RecordingPath & s_TitleName

        IsInitialized = True
        s_isNew = False
        Return IsInitialized

    End Function

    Public Function UpdateBaseBeatInfo(Optional info As BaseTrackInfoWrapper = Nothing) As Boolean
        If (info Is Nothing) Then
            s_TrackInfo = BaseTrackInfoWrapper.GetTrackInfoBasedOnUser(s_TitleName, s_artistOBJ)
        Else
            s_TrackInfo = info
        End If

        Return True
    End Function

    Public Function Get_Feature_Info_Node() As XElement
        Return s_songsXML.Get_Feature_Info_Node
    End Function

    Public Function StoreData(Optional ByRef loginNode As XElement = Nothing, Optional ByVal removeFeature As Boolean = False, Optional ByVal removedFeature As SongData.FeatureInfo = Nothing, Optional storeOnlyDontUpdate As Boolean = False) As Boolean


       

            If Not (loginNode Is Nothing) Then

                loginNode = StoreSongNode(loginNode)
                Return True



        End If

        songNode = StoreSongNode(songNode, removeFeature, removedFeature, storeOnlyDontUpdate)


        Return True
    End Function

    Class FeatureInfo
        Inherits Identity


        Private artist As Artist
        Private f_song As SongData
        Private Count As Int32
        Private songs As List(Of SongData) = New List(Of SongData)
        Private songOwner As String
        Private _globID As String


        Public Sub New(ByRef FeaturedArtist As Artist, ByRef SongOBJ As SongData, Optional ByVal gid As String = "")
            MyBase.New(ClassCode.FeatureInfo, GlbID:=gid)
            artist = FeaturedArtist
            f_song = SongOBJ
            ' artist.init(SongOBJ .,True)
            songOwner = SongOBJ.Artist_Name
            If Not (IS_NEW) Then
                Exit Sub
            End If

            ' songs = FeaturedArtist.SongList
            'Count = songs.Count

        End Sub

        Sub New(GlobID As String, ByRef theSong As SongData, ByRef rto As Object)
            ' TODO: Complete member initialization 
            MyBase.New(ClassCode.FeatureInfo, GlbID:=GlobID, returnOBJ:=rto)
            'f_song = mySong
        End Sub


        Public Function UpdateFeatureSongName() As Boolean
            Dim root As XElement = Song_Object.Get_Feature_Info_Node

            For Each feature As XElement In root.Elements

                For Each song As XElement In feature.Elements
                    If (song.Attribute("songID").Value = Song_Object.GLOBAL_ID) Then
                        song.Attribute("name").Value = Song_Object.Title
                    End If
                Next

            Next


            Return False
        End Function




        'Private Function getSongList(ByRef artist As Artist)

        '    Dim templist As List(Of SongData) = New List(Of SongData)
        '    Dim doc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        '    Dim root = doc.Root
        '    Dim songsNode = root.Element("Songs")

        '    For Each node As XElement In songsNode.Elements


        '        If (node.Attribute("artist").Value = artist) Then


        '            Dim temp As SongData = New SongData(node.Attribute("name").Value)
        '            templist.Add(NewSong)



        '        End If
        '        If Not (NewSong Is Nothing) Then
        '            templist.Add(NewSong)
        '        End If
        '        templist.Sort()

        '    Next
        '    Return templist


        'End Function

        Public Function StoreFeatureNode(ByRef featuresNode As XElement, Optional FeatureInfoSection As Boolean = False, Optional _Remove As Boolean = False, Optional ByRef removedFeature As FeatureInfo = Nothing, Optional ByRef featuredArtistToAdd As String = "") As XElement

            If (_Remove And removedFeature Is Nothing) Then
                Application.LEM_ERR("You need to send featureInfo ID")
                Throw New Exception
                Return Nothing
            End If



            If Not (FeatureInfoSection) Then

                If Not _Remove Then
                    Dim featureNode = New XElement("feature", New XAttribute("name", artist.My_Name), New XAttribute("ftGID", GLOBAL_ID))
                    featuresNode.Add(featureNode)

                Else
                    'Remove empty nodes if there
                    If (featuresNode.HasElements) Then

                        For Each ftNode As XElement In featuresNode.Elements
                            If (ftNode.Attribute("name").Value = Artist_Name) Then
                                ftNode.Remove()
                                If (featuresNode.HasElements) Then
                                    featuresNode.Attribute("count").Value = featuresNode.Elements.Count

                                Else
                                    featuresNode.Remove()

                                End If



                                Exit For
                            End If
                        Next

                    Else
                        'featuresNode.Remove()


                    End If

                End If





            Else


                'featureinfo node only
                Dim myNodeWasAlreadyThere As Boolean = False

                If (featuresNode Is Nothing) Then
                    MsgBox("right here")
                End If
                If Not (featuresNode.Elements.Count = 0) Then
                    Dim myNode As XElement = Nothing
                    Dim featuresCount As Int32 = featuresNode.Attribute("count").Value


                    'check for existing featured artist node
                    For Each featNode As XElement In featuresNode.Elements
                        'found the right featured artist node

                        If (_Remove) Then
                            If (featNode.Attribute("name").Value = removedFeature.Artist_Name) Then



                                For Each song As XElement In featNode.Elements
                                    If (song.Attribute("ftID").Value = GLOBAL_ID) Then
                                        song.Remove()
                                        If (featNode.Elements.Count > 0) Then
                                            featNode.Attribute("songs").Value = featNode.Elements.Count

                                        Else
                                            featNode.Remove()

                                        End If
                                        featuresNode.Attribute("count").Value = featuresNode.Elements.Count
                                        Return featuresNode
                                    End If
                                Next





                            End If
                        End If



                        If (featNode.Attribute("name").Value = Artist_Name) Then
                            myNode = featNode
                            myNodeWasAlreadyThere = True



                            Exit For
                        End If




                    Next

                    If Not (myNode Is Nothing) Then

                        'found it
                        'check for existing song node


                        ' Dim i As Int32 = 0
                        For Each songNode As XElement In myNode.Elements
                            If songNode.Attribute("songID").Value = Song_Object.GLOBAL_ID Then
                                'song node exists allready
                                If (_Remove And songNode.Attribute("ftID").Value = GLOBAL_ID) Then
                                    songNode.Remove()
                                    If (myNode.Elements.Count > 0) Then
                                        myNode.Attribute("songs").Value = myNode.Elements.Count
                                    Else
                                        myNode.Remove()

                                        featuresNode.Attribute("count").Value = featuresNode.Elements.Count


                                    End If
                                End If
                                'Song already there so return unchanged node

                                Return featuresNode
                            End If
                            ' i += 1
                        Next
                    Else
                        'mynode = newnode
                        myNode = New XElement("feature", New XAttribute("name", Artist_Name), New XAttribute("songs", "1"))

                    End If



                    'my node found but song not there so
                    'create song node only if this is the right myNode


                    If (featuredArtistToAdd = Artist_Name) Then

                        Dim count = Convert.ToInt32(myNode.Attribute("songs").Value)
                        Dim newsong = New XElement("song", New XAttribute("name", Song_Object.Title), New XAttribute("songOwner", songOwner), New XAttribute("songID", Song_Object.GLOBAL_ID), New XAttribute("artistID", Artist_Object.GLOBAL_ID), New XAttribute("ftID", GLOBAL_ID))
                        count = count + 1

                        myNode.Add(newsong)
                        myNode.Attribute("songs").Value = myNode.Elements.Count

                        'if not existing node
                        If Not (myNodeWasAlreadyThere) Then
                            featuresNode.Add(myNode)

                        End If

                        featuresNode.Attribute("count").Value = featuresNode.Elements.Count
                    End If

                Else 'Feature node doesn't exist so add both
                    Dim featurNode = New XElement("feature", New XAttribute("name", Artist_Name), New XAttribute("songs", 1), New XElement("song", New XAttribute("name", Song_Object.Title), New XAttribute("songOwner", songOwner), New XAttribute("songID", Song_Object.GLOBAL_ID), New XAttribute("artistID", Artist_Object.GLOBAL_ID), New XAttribute("ftID", GLOBAL_ID)))

                    featuresNode.Add(featurNode)
                    featuresNode.Attribute("count").Value = featuresNode.Elements.Count







                End If

            End If

            Return featuresNode
        End Function

        ReadOnly Property Artist_Name As String
            Get
                Return artist.My_Name

            End Get

        End Property

        ReadOnly Property Song_Object As SongData
            Get
                Return f_song
            End Get
        End Property

        ReadOnly Property Artist_Object As Artist
            Get
                Return artist
            End Get
        End Property

        Property Song_List As List(Of SongData)
            Get

                Return songs


            End Get
            Set(value As List(Of SongData))
                If (songs.Count > 0) Then
                    'Throw New Exception()

                End If
            End Set
        End Property

        Property Song_Owner As String
            Get
                Return songOwner
            End Get
            Set(value As String)
                songOwner = value
            End Set
        End Property

        Private Sub initializeRebuild()

            ' Dim songx As SongInfoXML = New SongInfoXML()
            Dim ftInfo As XElement = f_song.Get_Feature_Info_Node()
            Dim songNode As XElement = Nothing
            For Each ft As XElement In ftInfo.Elements
                For Each song As XElement In ft.Elements
                    If (song.Attribute("ftID").Value = GLOBAL_ID) Then
                        songNode = song
                        Exit For
                    End If
                Next
            Next
            If Not (songNode Is Nothing) Then


                songs.Add(f_song)
                songOwner = f_song.Artist_Name
            Else
                ' Application.LEM_ERR("Something wrong with feature globalID, check xmls")
            End If
        End Sub

        Public Overrides Sub OnRebuild(ByRef GlobalID As GlobalID, ByRef parent As Identity, Optional ByRef returnOBJ As Object = "")
            If (GLOBAL_ID = GlobalID.GLOBAL_ID) Then
                'get feature info from xml
                Dim rto = TryCast(returnOBJ, Array)
                f_song = rto.GetValue(1)
                artist = rto.GetValue(0)
                songs = New List(Of SongData)
                initializeRebuild()




            End If
        End Sub
    End Class

    Private Function GetNewSongId() As Int32

        Dim doc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        Dim root = doc.Root
        Dim result As Int32 = root.Attribute("songCount").Value
        result = result + 1
        root.Attribute("songCount").Value = result
        doc.Save(Application.XML_PATH & "SongInfo.xml")

        Return result
    End Function

  
    Public Shared Function RemoveMp3FromSongName(ByRef songname As String) As String

        If (songname.Contains(".Mp3")) Then
            Dim temp = Split(songname, ".Mp3")
            songname = temp.GetValue(0)
        ElseIf (songname.Contains(".mp3")) Then
            Dim temp = Split(songname, ".mp3")
            songname = temp.GetValue(0)
        Else
            Dim upper = songname.ToUpper
            If (upper = "MP3") Then
                Dim temp = Split(songname, ".")
                songname = temp.GetValue(0)
            End If

        End If

        Return songname
    End Function

  

    Public Function GetBouncedSongPath(explicitPath As Boolean, Optional refreshFeatures As Boolean = True) As String
        If Not (LAST_FOUND_MP3_PATH = "") Then
            If (File.Exists(LAST_FOUND_MP3_PATH)) Then
                Return LAST_FOUND_MP3_PATH
            End If

        End If
        Dim result As String = s_artistOBJ.MP3Path + Utils.addFeatureText(Title, Get_Features_List) + ".mp3"
        Dim songName = Title
        Dim finalSong = ""
        If Not refreshFeatures Then
            songName = Path.GetFileNameWithoutExtension(s_mp3FilePath)
        Else
            songName = Utils.addFeatureText(Title, Get_Features_List)
        End If
        If (Not (explicitPath) And songName.Contains("-ft ")) Then
            songName = Utils.removeFeatureText(songName)
        End If



        Dim count = 0
        Dim songs() = IO.Directory.GetFiles(s_artistOBJ.MP3Path, "*.mp3")


        Dim temp As String = ""
        For Each song As String In songs
            temp = song

            temp = System.IO.Path.GetFileNameWithoutExtension(temp)


            If (explicitPath) Then
                If (songName = temp) Then

                    finalSong = song
                    count += 1

                End If
            Else
                temp = NamingConventions.RemoveFeatureText(temp)
                temp = NamingConventions.removeSpaces(temp)
                songName = NamingConventions.removeSpaces(songName)
                If ((songName.ToLower() = temp.ToLower) Or temp.ToLower().Contains(songName.ToLower())) Then
                    finalSong = song
                    count += 1
                End If
            End If


        Next
        If (count > 1) Then
            Dim copies = songs.Where(Function(c) c.ToLower().Contains(Title.ToLower())).ToList()







            copies.Sort(Function(a As String, b As String)
                            Return New FileInfo(a).LastWriteTime.CompareTo(New FileInfo(b).LastWriteTime)
                        End Function)
            result = copies.Item(0)

        Else
            If Not finalSong = "" Then
                result = finalSong
            End If

        End If
        LAST_FOUND_MP3_PATH = result
        Return result
    End Function
    ''' <summary>
    ''' DEPRECATED
    ''' </summary>
    ''' <param name="local"></param>
    ''' <remarks></remarks>
    Private Sub Create(ByRef local As Int32)

        'Dim newSong As SongData = New SongData(SongName)
        Dim doc = XDocument.Load(Application.XML_PATH & "SongInfo.xml")
        Dim root = doc.Root
        Dim songs = root.Element("Songs")
        Dim song As XElement
        For Each _song As XElement In songs.Elements
            If (Convert.ToInt32(_song.Attribute("id").Value) = local) Then


                song = _song
                s_TitleName = song.Attribute("name").Value

                s_artistName = song.Attribute("artist").Value
                ' ModifiedDate = Convert.ToDateTime(song.Attribute("lastModified").Value)








                If (song.Attribute("features").Value = "True") Then
                    s_hasFeatures = True
                Else
                    s_hasFeatures = False
                End If
                If (song.Attribute("mixReady").Value = "True") Then
                    MixReady = True
                Else
                    MixReady = False
                End If
                If (song.Attribute("bounced").Value = "True") Then
                    s_bounced = True
                Else
                    s_bounced = False


                End If

                Try
                    If (song.Attribute("comments")) Then
                        If (song.Attribute("comments").Value = "True") Then


                            For Each childNode As XElement In song.Element("Comments").Elements



                                Dim temp As SongData.Comment = New SongData.Comment(Me, childNode.Attribute("status"), childNode.Attribute("author").Value)




                                'temp.From_CEO.Add(childNode.Attribute("author"), childNode.Value())
                                'SongComments.Add(temp)
                                'ElseIf (childNode.Attribute("status").Value = "ARTIST") Then
                                'temp.From_Artist.Add(childNode.Attribute("author"), childNode.Value())
                                'SongComments.Add(temp)
                                'ElseIf (childNode.Attribute("status").Value = "LEM") Then
                                'temp.From_Lem = childNode.Value

                            Next
                        End If


                    End If



                Catch ex As Exception

                End Try

                Exit Sub
            End If
        Next







    End Sub

    Class SongPriorityContext



        Private dSet As DataSet
        ' Private Shared readMode = dSet.ReadXml(Application.XML_PATH & globalPriorityXMLPath





        Public Shared Function Get_Priority_Count() As Int32

            Return 0

        End Function



        Public Shared Function Get_Priority_List() As List(Of String)

            Dim list As List(Of String) = New List(Of String)

            'For Each Priority As DataRow In globalPriorities.Rows
            '    list.Add(Priority.Item(2))

            'Next
            Return list
        End Function


        Public Shared Sub Set_Priority(ByVal songid As Int32, ByRef priority As Int32, ByVal songName As String)

            'If (priority = 0) Then

            '    For Each row As DataRow In globalPriorities.Rows
            '        If (row.Item(0) = songid) Then
            '            row.Delete()
            '            Exit Sub
            '        End If
            '    Next

            'End If

            ''add to row(value)
            'Dim dr = globalPriorities.NewRow()
            'dr.Item(0) = songid
            'dr.Item(1) = priority
            'dr.Item(2) = songName


            'globalPriorities.Rows.InsertAt(dr, priority - 1)

            '' Dim pCol As DataColumn
            ''Dim iCol As DataColumn
            ''Dim nCol As DataColumn

            ''Dim myFKC As ForeignKeyConstraint = New ForeignKeyConstraint ("PriorityConstraint",













            'globalPriorities.Columns.Add("priority")
            'globalPriorities.Columns.Add("id")
            'globalPriorities.Columns.Add("nameOfSong")


            'globalPriorities.Rows.Add(songid, priority)



        End Sub

        Public Shared Function Get_Global_Priority(ByRef songID As Int32) As Int32

            'For Each p As DataRow In globalPriorities.Rows
            '    If (p.Item(1) = songID) Then
            '        Return p.Item(0)

            '    End If
            'Next
            Return 0
        End Function





        '---------------LOCAL OBJECT IMPLEMENTATION




        Private artistXMLPath = "ArtistInfo.xml"


        Public myPriorities As DataTable = New DataTable()
        Public CurrentGlobal = 0
        Public CurrentLocal = 0

        Private reader As IDataReader


        Public localCompetitors As List(Of SongData)



        Public Sub New()





            myPriorities.ReadXml(Application.XML_PATH & artistXMLPath)


            reader = myPriorities.CreateDataReader()










        End Sub




    End Class


    Public Sub SetNameFirstTime(ByRef options As CloseSessionOptions)


        m_name = m_GlobalID.UpdateName(options.SongTitle, True)
        Application.IdentityHasChanged = True
        GlobalID.CommitAllData()




    End Sub




    Private Function StoreSongNode(ByRef SongNode As XElement, Optional ByVal removeFeature As Boolean = False, Optional ByVal removedFeature As FeatureInfo = Nothing, Optional storeOnlyDontUpdate As Boolean = False) As XElement
        'Dim featuresNode As XElement
        'Dim commentsNode As XElement
        If (Not storeOnlyDontUpdate) Then
            SongNode = s_songsXML.Get_Song_Node(m_LocalID, m_GlobalID.GLOBAL_ID, Title, MixReady, Has_Features, Now, Artist_Name, s_bounced, Has_Comments, Is_Priority, s_mp3FilePath, False, s_needsFeature, Music_Source, s_TrackInfo.ID, True)
        End If



        If (Has_Comments) Then

            If (SongNode.Elements.Count > 0) Then
                For Each node As XElement In SongNode.Elements
                    If (node.Name = "Comments") Then
                        node.Remove()
                    End If
                Next
            End If


            commentsNode = New XElement("Comments", New XAttribute("count", GetComments.Count))

            For Each cmt As Comment In GetComments()
                commentsNode = cmt.StoreCommentNode(commentsNode)

            Next

            SongNode.Add(commentsNode)
        End If


        ' if songnode has child elements
        If (SongNode.HasElements) Then

            For Each feat1 As XElement In SongNode.Elements


                If (feat1.Name = "Features") Then

                    feat1.Remove()




                    Exit For


                End If
            Next

        End If



        If (Has_Features) Then
            'in features array



            'FIND THE FEATURES NODE IF IT EXISTS AND REMOVE IT


            SongNode.Attribute("features").Value = True
            'if there is a comments node and a FeaturesNode



            Feautres_Node = New XElement("Features", New XAttribute("count", s_features.Count))


            For Each ft As FeatureInfo In s_features
                Feautres_Node = ft.StoreFeatureNode(Feautres_Node)

                If Not removeFeature Then
                    featureInfoNode = ft.StoreFeatureNode(featureInfoNode, True, False, featuredArtistToAdd:=s_features.Item(s_features.Count - 1).Artist_Name)
                End If





            Next

            ' remove features
            If (removeFeature) Then

                featureInfoNode = removedFeature.StoreFeatureNode(featureInfoNode, True, True, removedFeature)




            End If





            If (SongNode.HasElements) Then
                Feautres_Node.Attribute("count").Value = Feautres_Node.Elements.Count





            End If
            SongNode.Add(Feautres_Node)
            SongNode.Attribute("mp3").Value = GetBouncedSongPath(True)
        Else
            SongNode.Attribute("features").Value = False
        End If




        Return SongNode
    End Function

    Private Function GetStoredId() As Int32
        Dim root = Utils.getRootXMLNode("SongInfo.xml", "Songs")
        Dim q = From s In root.Elements
                Where s.Attribute("name") = Title
                Select s.Attribute("id").Value

        If (q.First Is Nothing) Then
            Return -1
        End If

        Return Convert.ToInt32(q.First)


    End Function



    Public Function Get_Song_Node_Child(ByVal nodeName As String) As XElement
        Return songNode.Element(nodeName)

    End Function

    Public ReadOnly Property Artist_Name As String
        Get
            Return s_artistName
        End Get
    End Property

    Public Property ParentSongContainer As LoginData.SongContainer

    Public Property SessionDuration As String
        Get
            Return ParentSongContainer.DurationString

        End Get
        Set(value As String)

        End Set
    End Property
    Public ReadOnly Property Title As String
        Get
            Dim newstring As String = m_name
            Return m_name
        End Get

    End Property
    Public ReadOnly Property Initialized As Boolean
        Get
            Return IsInitialized
        End Get
    End Property
    Public ReadOnly Property ModifiedDate As DateTime
        Get
            Return s_lastModified
        End Get

    End Property
    Public Property LengthInMillis As Integer


    Public ReadOnly Property MP3_Location As String
        Get

            Return s_artistOBJ.MP3Path
        End Get

    End Property

    Public Property MixReady As Boolean
        Get
            Return s_mixReady
        End Get
        Set(value As Boolean)
            s_mixReady = value
        End Set
    End Property

    Public ReadOnly Property IsBounced As Boolean
        Get
            Return s_bounced
        End Get

    End Property

    Public ReadOnly Property Has_Features As Boolean
        Get
            If (s_features Is Nothing) Then
                s_features = New List(Of FeatureInfo)
            End If
            Return s_features.Count > 0
        End Get

    End Property
    Public Property FLAG_POSTBOUNCE_UPDATE As Boolean
    Public Property Needs_Feature As Int32
        Get
            Return s_needsFeature
        End Get
        Set(value As Int32)
            If (s_needsFeature < 3) Then
                If (value = 0) Then
                    s_needsFeature = 0

                Else
                    s_needsFeature = s_needsFeature + 1
                End If
            Else : Return
            End If
        End Set
    End Property


    Public ReadOnly Property Has_Comments As Boolean
        Get

            Return s_hasComments


        End Get

    End Property

    Public Function GetComments() As List(Of SongData.Comment)
        Return s_SongComments
    End Function

    Public ReadOnly Property MP3_File_Path As String
        Get
            Return s_mp3FilePath
        End Get
    End Property

    Private LAST_FOUND_MP3_PATH = ""

    Public ReadOnly Property Get_Features_List As List(Of FeatureInfo)
        Get
            If (s_features Is Nothing) Then
                s_features = New List(Of FeatureInfo)
            End If
            Dim count = Utils.Get_Features_Count_Fresh(LOCAL_ID)
            If Not (s_features.Count = count) And (count > 0) Then
                s_features = Utils.Get_Features_Fresh(Me)
            End If


            Return s_features


        End Get

    End Property

    Public ReadOnly Property Featuring As String
        Get
            Dim result As List(Of String) = New List(Of String)
            If (s_features.Count > 0) Then





                For Each fi As FeatureInfo In s_features
                    result.Add(fi.Artist_Name)

                Next
                Return Utils.CombineNames(result)
            End If
            Return "Solo"

        End Get
    End Property



    Public ReadOnly Property Global_Priority_Order As Int32
        Get
            Return 0
        End Get
    End Property

    Public ReadOnly Property LocalPriority As Int32
        Get

            Return 0





        End Get

    End Property

    Public ReadOnly Property Is_Priority As Boolean
        Get
            ' Return Priority.Get_Global_Priority(LOCAL_ID) > 0
            Return 0

        End Get
    End Property


    Public ReadOnly Property Music_Source
        Get
            Return s_musicSource
        End Get
    End Property


    Public Function CompareTo(other As SongData) As Integer Implements IComparable(Of SongData).CompareTo


        If (other.ModifiedDate > ModifiedDate) Then
            Return 1
        ElseIf (other.ModifiedDate < ModifiedDate) Then
            Return -1
        Else
            Return 0
        End If

    End Function


    Public Overrides Function ToString() As String
        Return Title
    End Function

    ''' <summary>
    ''' overloaded  GLOBAL id
    ''' </summary>
    ''' <param name="GlobID"></param>
    ''' <remarks></remarks>
    Private Function updateFeatureList(GlobID As String, ByRef featuredArtist As Artist) As Integer
        Dim ft As SongData.FeatureInfo = Nothing
        Dim rto(2) As Object

        rto(0) = featuredArtist
        rto(1) = Me
        Dim isThere = False
        s_features = Get_Features_List()
        For Each feat As SongData.FeatureInfo In s_features
            If (feat.Artist_Name = featuredArtist.My_Name) Then
                isThere = True
                ft = feat
                Exit For
            End If
        Next

        If (isThere) Then
            s_features.Remove(ft)
        Else
            s_features.Add(New SongData.FeatureInfo(featuredArtist, Me))
        End If

        Return s_features.Count()

        s_artistOBJ.StoreAllData()


    End Function


End Class