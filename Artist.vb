Imports System.IO
Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class Artist
    Inherits Admin.Administrator
    Implements IComparer(Of Artist), 
        IComparable(Of Artist)




    Private m_Is_Generic As Boolean


    Private password As Admin.PassWord
    Private myName As String


    Private sessions As SetupSession

    Private m_IsLoggedIn As Boolean = False
    Private m_resourseHandle As XMLResources


    Private m_isintialized As Boolean


    Private m_SongCount As Int32
    Private m_HasComments As Boolean
    Private m_HasFeatureRequests As Boolean
    Private m_isBeatMaker As Boolean = False
    ' Private m_isAGroup As Boolean = False
    Private m_Pinfo As PersonaInfo
    Private m_IsUntameArtist As Boolean = False
    Private m_AsEngineer As Boolean = False
    Private m_BouncePath As String
    Private m_RecordingPath As String




    Private m_History As List(Of LoginData) = New List(Of LoginData)
    Private m_featuredSongs As List(Of SongData) = New List(Of SongData)
    Private m_featureRequests As List(Of FeatureRequest) = New List(Of FeatureRequest)
    Private _closeSessionDialog As CloseSessionDialog




    Public Sub New()

    End Sub

    Public Overrides ReadOnly Property GLOBAL_ID As String
        Get

            If m_name = Application.GLOBAL_ARTIST Then
                Return "UNT-12000-1000"
            End If

            Return MyBase.GLOBAL_ID
        End Get
    End Property


    Public Sub setAsGenericArtist()
        m_IsLoggedIn = True
        m_BouncePath = Application.BASE_PATH & "GLOBAL"
        m_RecordingPath = Application.BASE_PATH & "GLOBAL"
        m_name = "GLOBAL_ARTIST"

        m_Is_Generic = True

    End Sub

    Public Function GetMyPass(groupMaster As Artist) As String
        If (groupMaster.Is_Group) Then
            Dim q = From art As Artist In groupMaster.GroupMembers
                    Where art.GLOBAL_ID = GLOBAL_ID
                    Select art

            If q.FirstOrDefault Is Nothing Then
                Return Nothing
            End If
        End If
        If (password Is Nothing) Then
            password = New Admin.PassWord(My_Name, False)
            SetPasswordOwner(password)
        End If

        Return password.PassWord
    End Function
    ''' <summary>
    ''' Primary Constructor
    ''' </summary>
    ''' <param name="_name"></param>
    ''' <param name="initialize"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal _name As String, ByVal initialize As Boolean)

        MyBase.New(ClassCode.Artist, _name)
        m_isintialized = False
        If Not (Application.propsLoaded) Then
            If Not (Application.loadProperties()) Then
                Throw New Exception("Problem loading App Properties, check xml files")
            End If
        End If



        myName = _name
        GroupMembers = m_GlobalID.GetGroupMembers()
        If (GroupMembers.Count > 0) Then
            Is_Group = True
        End If
        If (myName = "MasterLoginSession") Then
            password = New Admin.PassWord("BackDoor", False)

            If (initialize) Then
                init(sessions)
            End If
            m_IsLoggedIn = True
            Exit Sub
        End If
        'sets untame or outhouse and finds the bounce folder
        ' gatherArtistStatus()

        If Not verifyRecordingPath() Then
            Return
        End If

        If (initialize) Then

            init(sessions)
        Else
            'SETUP RESOURCES

            m_resourseHandle = New XMLResources()

        End If


        If Not (IS_NEW) Then
            Exit Sub
        End If





        'If (pass Is Nothing) Then
        '    myName = m_name
        '    pass = New Admin.PassWord(myName)
        '    password = pass
        '    Return
        'End If

        'password = pass





    End Sub

    Sub New(closeSessionDialog As CloseSessionDialog, name As String)
        ' TODO: Complete member initialization 
        _closeSessionDialog = closeSessionDialog
        m_name = name


    End Sub

    Sub New(name As String, ByVal test As Boolean, ByVal test2 As Boolean)
        ' TODO: Complete member initialization 

        m_name = name


    End Sub

    Public ReadOnly Property Resources As XMLResources
        Get
            Return m_resourseHandle
        End Get
    End Property


    Public Overrides Function ToString() As String
        Return My_Name
    End Function


    Public Delegate Sub LemLogDelegate(ByVal message As String)
    Public Delegate Sub StatWinUpdateDelegate(progress As Integer, barNum As Integer)
    Public Delegate Function StatWinStart()
    Public Delegate Sub MainWinHide()
    Public Delegate Function GetSessionCountDelegate()




    Public Function init(ByRef setup As SetupSession, Optional ByVal backdoor As Boolean = False) As Boolean

        sessions = setup
        Dim worker As BackgroundWorker = Application.getBackGroundWorker()
        worker.WorkerReportsProgress = True

        Dim hide As MainWinHide = New MainWinHide(AddressOf Application.Main_Window.Hide)



        If Not backdoor Then
            ' password = New Admin.PassWord(My_Name)
            ' password.Set_Owner_Reference(Me)

            ' myName = m_name



            Dim app As Application = Application.Current
            'Dim _statText As LemLogDelegate = New LemLogDelegate(AddressOf Application.StatwinText)
            'Dim _statProg As StatWinUpdateDelegate = New StatWinUpdateDelegate(AddressOf Application.StatWinProgress)
            'Dim _open As StatWinStart = New StatWinStart(AddressOf Application.StatWinStart)


            m_IsLoggedIn = True






            m_resourseHandle = sessions.GetResources()

            ' m_HasComments = sessions.Artist_Has_Comments
            ' m_HasFeatureRequests = (m_featureRequests.Count > 0)

            If (m_HasFeatureRequests) Then
                Throw New Exception("Restoring old requests needs to be implemented")

            End If
            m_isintialized = True

        End If

        Return True


    End Function

    Public Function gatherArtistStatus() As Boolean

        Dim foundIt = False
        For Each folder As String In My.Computer.FileSystem.GetDirectories(Application.MP3_PATH, FileIO.SearchOption.SearchAllSubDirectories)
            Dim pathsplit = Split(folder, "\")
            Dim foldername As String = pathsplit.GetValue(pathsplit.Count - 1)
            If (m_name.ToLower = foldername.ToLower) Then
                foundIt = True
                'check if its in general pop or outhouse
                If (Directory.GetParent(folder).Name = "mp3") Then
                    m_BouncePath = folder & "\"
                    m_IsUntameArtist = True

                ElseIf (Directory.GetParent(folder).Name = "Outhouse") Then
                    m_BouncePath = folder & "\"
                    m_IsUntameArtist = False


                End If


            End If

            'set mp3 path
        Next
        'if not found create
        If Not foundIt Then
            Dim newDir As String = Application.MP3_PATH & "Outhouse\" & m_name & "\"
            Directory.CreateDirectory(newDir)
            m_BouncePath = newDir
            m_IsUntameArtist = False
        End If
        Return True
    End Function

    Private Function verifyRecordingPath() As Boolean
        Dim foundIt = False
        If (Not (Directory.Exists(Application.BASE_PATH)) Or (Not Directory.Exists(Application.MP3_PATH))) Then
            ' Throw New Exception("NetworkDown!!!")
            MsgBox("NETWORK DOWN...")
            Application.LemLog("NETWORK DOWN...")


            Return False
        End If
        For Each sessionFolder As String In Directory.GetDirectories(Application.BASE_PATH)
            Dim directoryName As String = Path.GetFileName(sessionFolder)
            If (directoryName = m_name) Then
                m_RecordingPath = sessionFolder & "\"
                foundIt = True
            End If
        Next
        If Not foundIt Then
            m_RecordingPath = Directory.CreateDirectory(Application.BASE_PATH & m_name).FullName & "\"

        End If
        Return True
    End Function
    'Public Sub New(ByVal name As String, ByVal forLogin As Boolean) 'the boolean is primarily put here to change the signature for a quick load for logindata

    '    myName = name



    '    password = New Admin.PassWord(My_Name)












    'End Sub
    Public Function LogIn() As Boolean

        If (password.TryLogin(Me)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub LogOut()

       
        sessions = Nothing
        password.isLoggedIn = False
        m_IsLoggedIn = False
        'Application.Log_Out_Artist()

    End Sub

    Public Sub AddArtistToThisGroup(artistGID As String, artistName As String)
        m_GlobalID.RegisterArtistToGroup(artistGID, artistName)
        Application.IdentityHasChanged = True

        GlobalID.CommitAllData()




    End Sub




    Public Sub SetPasswordOwner(ByRef pass As Admin.PassWord)

        password = pass
        password.Set_Owner_Reference(Me)
        m_IsLoggedIn = password.Is_Logged_In


    End Sub

    Public Function GetSetupSessionObject() As SetupSession
        Return sessions
    End Function

    Public WriteOnly Property ChangePassWord()
        Set(value)
            password.UpdatePassWord()
        End Set
    End Property


    Public ReadOnly Property Is_Initialized
        Get
            Return m_isintialized
        End Get
    End Property

    Public Function StoreAllData() As Boolean



        'save resources

        If (m_featureRequests.Count > 0) Then

            For Each ft As FeatureRequest In m_featureRequests
                Dim args As XMLResources.AddResourceArgs = New XMLResources.AddResourceArgs
                args.type = XMLResources.ResourceType.FeatureRequest
                args.resource = ft
                m_resourseHandle.AddResource(args)

            Next


        End If


        If Not (sessions.Get_Current_Session Is Nothing) Then
            If Not (sessions.Get_Current_Song Is Nothing) Then

           
                sessions.Get_Current_Song.StoreData()


            End If

        End If

        sessions.UpdateAllXML()


      

        Return True
    End Function

    Public Class FeatureRequest

        Private doc As XDocument

        Private Sender As Artist
        Private Recipient As Artist

        Private Song As SongData

        Property One_Sending As String
            Get
                Return Sender.myName
            End Get
            Set(value As String)

            End Set
        End Property

        Property One_Recieving As String
            Get
                Return Recipient.myName
            End Get
            Set(value As String)

            End Set
        End Property

        Property SongToFeatureOn As String
            Get
                Return Song.Title
            End Get
            Set(value As String)

            End Set


        End Property

        Property OtherFeaturedArtists As String
            Get
                If (Song.Has_Features) Then
                    Dim list As List(Of String) = New List(Of String)
                    For Each ft As SongData.FeatureInfo In Song.Get_Features_List
                        list.Add(ft.Artist_Name)
                    Next
                    Return Utils.CombineNames(list)
                End If

                Return "No other features"

            End Get
            Set(value As String)

            End Set
        End Property

        Property SongHasComments As Boolean
            Get
                Return Song.Has_Comments
            End Get
            Set(value As Boolean)

            End Set
        End Property

    End Class



    Private Class PersonaInfo

        Private cellphone As String
        'should be an array of 9 ints
        Private homephone As Integer
        Private address As String
        Private birth As Date







    End Class

    Public Class ArtistRelevence


        ' Private Shared GlobalRelevenceList As List(Of Artist)

        Private thisArtist As Artist
        Private myRelevence As Integer


        Shared Sub New()





        End Sub

        Sub New(ByRef artist As Artist)
            thisArtist = artist


        End Sub












    End Class

    Public Property History As List(Of LoginData)
        Get
            Return m_History
        End Get
        Set(value As List(Of LoginData))
            m_History = value
        End Set
    End Property

    Public Sub SortSessions()
        If (SessionList Is Nothing Or SessionList.Count = 0) Then
            Return
        End If
        sessions.SortSessions()

    End Sub

    Public ReadOnly Property SessionList As ObservableCollection(Of SetupSession.Session)

        Get

            Return sessions.Session_List
        End Get

    End Property

    Public ReadOnly Property Get_Sessions As SetupSession

        Get

            Return sessions
        End Get
    End Property

    Public ReadOnly Property SongList As List(Of SongData)
        Get
            Dim temp As List(Of SongData) = New List(Of SongData)
            For Each ssn As SetupSession.Session In SessionList
                temp.Add(ssn.Song_Object)

            Next

            Return temp

        End Get
    End Property



    Public ReadOnly Property RecordingPath As String
        Get

            Return m_RecordingPath
        End Get

    End Property

    Public ReadOnly Property MP3Path As String
        Get
            If (m_BouncePath Is Nothing) Then
                If (gatherArtistStatus()) Then
                    Return m_BouncePath

                Else
                    Throw New Exception("Problem Gathering Bounce Path...")
                End If
            Else


            End If


            Return m_BouncePath
        End Get
    End Property



    Public ReadOnly Property Has_Comments As Boolean
        Get
            Return m_HasComments
        End Get
    End Property


    Public ReadOnly Property My_Name As String
        Get
            Return m_name
        End Get
    End Property

    Public ReadOnly Property Is_Generic As Boolean
        Get
            Return m_Is_Generic
        End Get
    End Property

    Public ReadOnly Property Is_Untame_Artist As Boolean
        Get
            Return m_IsUntameArtist
        End Get
    End Property

    Public Property Is_Group As Boolean

    Public Property Is_GroupMember As Boolean


    Public Property GroupMembers As List(Of Artist)
       

    Public ReadOnly Property Is_Logged_In As Boolean
        Get
            Return m_IsLoggedIn
        End Get
    End Property

    Public ReadOnly Property Feature_Requests As List(Of FeatureRequest)
        Get
            Return m_featureRequests
        End Get
    End Property

    Public ReadOnly Property Featured_Songs As List(Of SongData)
        Get
            Return m_featuredSongs
        End Get
    End Property

    Public ReadOnly Property Is_BeatMaker As Boolean
        Get
            Return m_isBeatMaker
        End Get
    End Property

    Public Property AS_Engineer As Boolean
        Get
            Return m_AsEngineer
        End Get
        Set(value As Boolean)
            m_AsEngineer = value
        End Set
    End Property

    Public ReadOnly Property Get_Artist_Login_Node
        Get
            Return m_resourseHandle.ArtistLoginNode
        End Get
    End Property










    'Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare

    '    If (x.GetType().Equals(Me)) Then
    '        Return String .Compare (TryCast (x,Artist ).My_Name ,TryCast (y,Artist ).My_Name 
    '        Return String.Compare(x.My_Name, y.My_Name)

    '    Else
    '    If( TryCast(x,ArtistRelevence ).
    '        End If




    'End Function

    Public Function Compare(x As Artist, y As Artist) As Integer Implements IComparer(Of Artist).Compare
        Return String.Compare(x.My_Name, y.My_Name)
    End Function




    Public Function CompareTo1(other As Artist) As Integer Implements IComparable(Of Artist).CompareTo

        Return String.Compare(other.My_Name, My_Name)
    End Function

    Function CheckForUsedSoundClickBeat(NuendoSessionPath As String) As String
        Return Utils.CheckForUsedSoundClickBeat(NuendoSessionPath)

    End Function





End Class