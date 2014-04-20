
Imports BaseTrackInfo
Imports System.IO
Imports System.Threading




Public Class BaseTrackInfoWrapper
    Inherits BaseTrackInfo.BaseTrackInfo

    Private Shared InfoList As List(Of BaseTrackInfoWrapper)
    Private m_LocalID As Int32
    Private m_isNew As Boolean
    Private m_Producer As Artist
    'Private m_Brand As ProducerBrand
    Friend m_TrackFileInfo As FileInfo
    Private m_Resources As XMLResources
    Private SourceType As SetupSession.SessionMusicSource_Type
    Private m_exclusiveOnly As Boolean

    Public Shared MUT As Mutex

    Private DEAD As Boolean


    Public ReadOnly Property TrackSourceType As SetupSession.SessionMusicSource_Type
        Get
            Return Me.SourceType
        End Get
    End Property

    ''' <summary>
    ''' Will reconstruct an existing btw or create a new one
    ''' </summary>
    ''' <param name="trackBouncepath"></param>
    ''' <param name="songUsingThisTrack"></param>
    ''' <param name="type"></param>
    ''' <param name="resources"></param>
    ''' <param name="artistRef"></param>
    ''' <param name="producerInfo"></param>
    ''' <param name="trackFileInfo"></param>
    ''' <remarks></remarks>
    Public Sub New(trackBouncepath As String, ByVal songUsingThisTrack As String, ByVal type As SetupSession.SessionMusicSource_Type, resources As XMLResources, ByVal artistRef As String, Optional producerInfo As ProducerBrand = Nothing, Optional ByRef trackFileInfo As FileInfo = Nothing, Optional forExclusiveOnly As Boolean = True)
        MyBase.New()
        If MUT Is Nothing Then
            MUT = New Mutex
        End If

        If (m_XMLHandle Is Nothing) Then
            m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")
        End If
        If (InfoList Is Nothing) Then
            InfoList = New List(Of BaseTrackInfoWrapper)
        End If

        If Not (songUsingThisTrack = "") Then
            If Not (songUsingThisTrack = "none") Then
                'lets get this song path
                'Dim art3 As Artist = Application.ArtistList.Where(Function(a) a.My_Name = artistRef).FirstOrDefault()
                ' Dim check = Application.test1("Cc", "g")
                Dim try1 As String =
                    Application.Get_Bounced_Song_Path(songUsingThisTrack, artistRef)



                If Not (try1 = "") And Not (try1 = "Not Bounced") Then
                    songUsingThisTrack = try1
                End If


            End If

        End If
        If (forExclusiveOnly) Then
            m_isBidable = True
            m_isExclusive = True
            m_exclusiveOnly = True
            m_isDibable = False
        Else
            m_isBidable = False
            m_isDibable = True
            m_exclusiveOnly = False
            m_isExclusive = False

        End If

        If Not (trackFileInfo Is Nothing) Then
            m_TrackFileInfo = trackFileInfo
        End If

        If (trackBouncepath = "STOPLOOKINGFORTHIS") Then
            storeThisData(True)
        End If

        SourceType = type
        BounceLocation = trackBouncepath
        'If songUsingThisTrack = "none" Then
        '    m_Resources = resources
        '    If (reconstruct()) Then
        '        Exit Sub
        '    End If


        '    ' DEAD = True
        '    m_LocalID = -2

        '    Exit Sub


        'End If



        If (trackBouncepath = "TrackNotFound") Then
            DEAD = True
            m_LocalID = 0
            m_Resources = resources
            Exit Sub
        End If
        DEAD = False
        PaymentMethod = New PaymentMethod("none")
        m_Resources = resources




        If Not (reconstruct()) Then
            m_isNew = True
        Else
            If Not artistRef Is Nothing Then
                AddNewArtistReference(artistRef, songUsingThisTrack)
            End If

            Return
        End If



        'for soundclick only
        If (type = SetupSession.SessionMusicSource_Type.SoundClick) Then
            isForSale = False

            If Not artistRef Is Nothing Then
                AddNewArtistReference(artistRef, songUsingThisTrack)
            End If




        End If
        'for in house producers only
        If Not producerInfo Is Nothing Then

            If (type = SetupSession.SessionMusicSource_Type.SoundClick) Then
                Throw New Exception("Trying create info for a soundclick producer Dummy!!")

            End If
            origination = producerInfo
            m_isBidable = origination.DefaultAllowBidOption
            If Not (m_isBidable) Then
                m_isDibable = True
            End If
            'm_isDibable = origination .DefaultDibOption
        Else
            If (type = SetupSession.SessionMusicSource_Type.FlBounced) Then
                If (m_isNew) Then
                    Throw New Exception("need to send in a producerBrand object if this is a new nonSoundclick trackInfo")
                End If
            Else
                Dim ProducersNode As XElement = XMLResources.GetProducersResourceNode

                origination = New ProducerBrand(type.ToString(), ProducersNode)

            End If
        End If
        'storeThisData()
        MUT.WaitOne()
        InfoList.Add(Me)

        MUT.ReleaseMutex()



    End Sub

    Public Overrides Function GetProducerInfo() As ProducerBrand

        Return origination



    End Function

    Public Function getAAList(Optional ByVal featurelist As Boolean = False) As Dictionary(Of String, List(Of String))

        If Not (featurelist) Then
            Return m_UsingArtists
        Else
            Return m_FeaturedArtists
        End If

    End Function

    Public Sub New()
        m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")
        PaymentMethod = New PaymentMethod("none")
    End Sub

    Private Shared Function GetExistingTrackInfo(ByRef trackPath As String, ByRef info As BaseTrackInfoWrapper) As BaseTrackInfoWrapper
        MUT.WaitOne()
        If (InfoList.Count > 0) Then
            For Each bi As BaseTrackInfoWrapper In InfoList
                If (bi.BounceLocation = trackPath) Then

                    info = bi

                    Exit For
                End If
            Next
        End If

        Return info
        MUT.ReleaseMutex()
    End Function
    ''' <summary>
    ''' info based on using song and artist
    ''' </summary>
    ''' <param name="songName"></param>
    ''' <param name="artistRef"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    
    Public Shared Function GetTrackInfoBasedOnUser(songName As String, artistRef As Artist) As BaseTrackInfoWrapper

        If MUT Is Nothing Then
            MUT = New Mutex
        End If

        'check list first
        ' Dim nfo As BaseTrackInfoWrapper = New BaseTrackInfoWrapper()
        'nfo.reconstruct()
        If Not (InfoList Is Nothing) Then
            MUT.WaitOne()
            For Each item As BaseTrackInfoWrapper In InfoList
                For Each kv As KeyValuePair(Of String, List(Of String)) In item.getAAList()

                    For Each song As String In kv.Value
                        If (song = songName) Then
                            MUT.ReleaseMutex()
                            Return item

                        End If
                    Next
                Next
            Next
            MUT.ReleaseMutex()
        End If


        'if not in list
        Dim result As BaseTrackInfoWrapper = New BaseTrackInfoWrapper()







        'create temporary new one to search BaseTrackInfo resource node

        BaseTrackInfoWrapper.m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")

        'searching for node
        If (BaseTrackInfoWrapper.m_XMLHandle.HasElements) Then
            For Each trkNode As XElement In BaseTrackInfoWrapper.m_XMLHandle.Elements

                If (trkNode.HasElements) Then

                    For Each child As XElement In trkNode.Elements

                        If (child.Name = "MainAssociatedArtists") Then

                            For Each art As XElement In child.Elements

                                If art.Attribute("artist").Value = artistRef.My_Name Then

                                    For Each song As XElement In art.Elements
                                        If song.Attribute("title").Value = songName Then
                                            result = New BaseTrackInfoWrapper(trkNode.Attribute("bouncepath").Value,
                                                                          song.Attribute("title").Value,
                                                                          SetupSession.GetSessionMusicSourceType(trkNode.Attribute("musicSourceType").Value),
                                                                          artistRef.Resources, artistRef.My_Name)

                                            MUT.WaitOne()

                                            InfoList.Add(result)

                                            MUT.ReleaseMutex()
                                            Return result
                                        End If
                                    Next


                                End If

                            Next



                        End If

                    Next


                End If

            Next

        End If










        'if still here this must be a new found track so check for soundclickbeat
        'and create a whole new track info
        'each real song should go thru this process once
        Dim check As String = Utils.CheckForUsedSoundClickBeat(artistRef.RecordingPath & songName)

        If Not check = "" Then
            result = New BaseTrackInfoWrapper(check, songName, SetupSession.SessionMusicSource_Type.SoundClick, artistRef.Resources, artistRef.My_Name)


        Else
            result = New BaseTrackInfoWrapper("TrackNotFound", songName, SetupSession.SessionMusicSource_Type.none, artistRef.Resources, artistRef.My_Name)
        End If






        Return result
    End Function

    Public ReadOnly Property AssociatedArtistList As Dictionary(Of String, List(Of String))
        Get
            Return m_UsingArtists


        End Get

    End Property

    Public Overrides Property origination As ProducerBrand
        Get
            Return MyBase.origination
        End Get
        Set(value As ProducerBrand)
            MyBase.origination = value
        End Set
    End Property

    Public ReadOnly Property ID As Int32
        Get
            Return m_LocalID
        End Get
    End Property

    Public ReadOnly Property ProducerNamedTrackTitle As String
        Get
            Return Path.GetFileNameWithoutExtension(BounceLocation)

        End Get
    End Property

    Public Function AddNewArtistReference(ByRef artist As String, ByRef songUsing As String, Optional ByVal feature As Boolean = False) As Boolean
        If (isForSale) Then
            Return False
        End If
        If (songUsing = "Not Bounced") Then
            Return True
        End If


        If (feature) Then
            Dim songThere = False
            Dim artistThere = False
            Dim foundlistHandle As List(Of String) = Nothing

            If (m_FeaturedArtists.Count > 0) Then

                For Each ft As KeyValuePair(Of String, List(Of String)) In m_FeaturedArtists
                    If (ft.Key = artist) Then

                        artistThere = True
                        foundlistHandle = ft.Value
                        For Each song As String In ft.Value
                            If (song = songUsing) Then
                                songThere = True

                                Exit For
                            End If

                        Next

                        Exit For
                    End If
                Next
            End If
            If Not (artistThere) Then
                Dim temp As List(Of String) = New List(Of String)
                temp.Add(songUsing)
                m_FeaturedArtists.Add(artist, temp)
            ElseIf Not (songThere) Then
                foundlistHandle.Add(songUsing)


            End If

        Else
            Dim mainThere = False
            Dim songThere = False
            Dim foundArtistHandle As KeyValuePair(Of String, List(Of String)) = Nothing

            If (m_UsingArtists.Count > 0) Then

                For Each main As KeyValuePair(Of String, List(Of String)) In m_UsingArtists
                    If (main.Key = artist) Then
                        foundArtistHandle = main
                        mainThere = True
                        If (main.Value.Count > 0) Then
                            For Each song As String In main.Value
                                If song = songUsing Then
                                    songThere = True
                                End If
                            Next
                        End If

                        Exit For

                    End If
                Next
            End If
            If Not mainThere Then
                Dim temp As List(Of String) = New List(Of String)
                temp.Add(songUsing)
                songThere = True
                If Not artist Is Nothing Then
                    m_UsingArtists.Add(artist, temp)
                End If

            ElseIf Not songThere Then
                foundArtistHandle.Value.Add(songUsing)

            End If
            If (storeThisData()) Then
                Application.ResourcesHaveChanged = True
                XMLResources.SubmitResources()
            End If

        End If

        Return True

    End Function
    Public Shared Function FromXMLData(songUsing As String) As BaseTrackInfoWrapper
        If (m_XMLHandle Is Nothing) Then
            m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")
        End If

    End Function
    Public Shared Function GetExistingBeatInfo(beatPath As String, artistname As String, type As SetupSession.SessionMusicSource_Type, Optional songUsingThisBeat As String = "none") As BaseTrackInfoWrapper
        If (m_XMLHandle Is Nothing) Then
            m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")
        End If
        For Each node As XElement In m_XMLHandle.Elements
            If (node.Attribute("bouncepath").Value = beatPath) Then
                Dim bti = New BaseTrackInfoWrapper(beatPath, songUsingThisBeat, type, Application.XMLRESOURCES, artistname)
                Return bti
            End If
        Next

        Return Nothing
    End Function
    Private Function reconstruct(Optional ByRef track_path As String = "") As Boolean
        'If Not (Me.SourceType = SetupSession.SessionMusicSource_Type.SoundClick) Then
        If (Me.BounceLocation = "STOPLOOKINGFORTHIS") Then
            Return True
        End If
        If Not track_path = "" Then
            If (GetExistingTrackInfo(track_path, Me).ID > 0) Then
                Return True
            End If
        End If


        For Each node As XElement In m_XMLHandle.Elements

            If (node.Attribute("bouncepath").Value = Me.BounceLocation) Then


                m_LocalID = Convert.ToInt32(node.Attribute("id").Value)
                ' m_Producer = New Artist(node.Attribute("beatmakerartistGID").Value, False)

                Dim hasmainaanode = False
                Dim hasfeataanode = False

                For Each chld As XElement In node.Elements
                    If chld.Name = "MainAssociatedArtists" Then
                        hasmainaanode = True
                    ElseIf chld.Name = "FeaturedAssociatedArtists" Then
                        hasfeataanode = True
                    End If
                Next

                If hasmainaanode Then

                    For Each aaNode As XElement In node.Element("MainAssociatedArtists").Elements

                        Dim myList As List(Of String) = New List(Of String)
                        For Each song As XElement In aaNode.Elements
                            Dim sngname = song.Attribute("title").Value
                            If (sngname.Contains("UNT-12004")) Then
                                sngname = Identity.GlobalID.FindNameByGlobalID(sngname)
                                If (sngname = "") Then
                                    node.Remove()
                                    Application.ResourcesHaveChanged = True
                                    Continue For
                                Else
                                    song.Attribute("title").Value = sngname
                                    Application.ResourcesHaveChanged = True
                                End If
                            End If
                            myList.Add(sngname)

                        Next

                        m_UsingArtists.Add(aaNode.Attribute("artist").Value, myList)
                    Next

                End If
                If hasfeataanode Then

                    For Each ftNode As XElement In node.Element("FeaturedAssociatedArtists").Elements

                        Dim myList As List(Of String) = New List(Of String)
                        For Each song As XElement In ftNode.Elements
                            myList.Add(song)

                        Next
                        m_FeaturedArtists.Add(ftNode.Attribute("artist").Value, myList)

                    Next

                End If



                If Not Me.SourceType = SetupSession.SessionMusicSource_Type.SoundClick Then
                    origination = New ProducerBrand()

                End If

                isForSale = node.Attribute("forsale").Value
                PaymentMethod = New PaymentMethod(node.Attribute("paymethod").Value)
                isAuctionable = node.Attribute("auctionable").Value
                AcceptsOffers = node.Attribute("acceptsoffers").Value
                SourceType = SetupSession.GetSessionMusicSourceType(node.Attribute("musicSourceType").Value)
                If Not (node.Attribute("producerName").Value = "SoundClick Producer") Then
                    origination = New ProducerBrand(node.Attribute("producerName").Value, XMLResources.GetProducersResourceNode())

                End If
                m_isExclusive = Convert.ToBoolean(node.Attribute("isExclusive").Value)




                If Not (SourceType = SetupSession.SessionMusicSource_Type.SoundClick) Then


                    If (isForSale) Then

                        BeatBID = New BeatBID(Me)
                    Else

                        BeatDIB = New BeatDib(Me)


                    End If
                End If
                MUT.WaitOne()
                InfoList.Add(Me)
                MUT.ReleaseMutex()
                Return True
            End If
        Next
        ' End If



        Return False
    End Function
    Public ReadOnly Property Is_Exclusive_Only As Boolean
        Get
            Dim result = False
            If Not origination Is Nothing Then
                result = Not origination.DefaultAllowNonExclusiveUseForTracks
                
            End If
            If (isBidable Or isForSale) Then
                result = True
            End If

            Return result
        End Get
    End Property
    Public Function storeThisData(Optional ByVal killThis As Boolean = False) As Boolean
        If (DEAD) Then
            Return True
        End If
        If (TrackSourceType = SetupSession.SessionMusicSource_Type.SoundClick) Then
            If (AssociatedArtistList.Count = 0) Then
                Return True
            End If
        End If
        If (killThis) Then
            'need to store this in a way to notify whoever needs to stop looking to stop looking for this trkinfo
            m_LocalID = -1
            DEAD = True
            Return True
        End If
        If (m_XMLHandle Is Nothing) Then
            m_XMLHandle = XMLResources.GetResource(XMLResources.ResourceType.NewType, NewType:="BaseTrackInfo")
        End If
        Dim myNode As XElement = Nothing

        'For Each trkinfo As XElement In m_XMLHandle.Elements
        '    If (trkinfo.Attribute("bouncepath").Value = BounceLocation) Then
        '        trkinfo.Remove()
        '        Exit For
        '    End If
        'Next

        If (m_LocalID = Nothing) Then
            'new node
            m_LocalID = m_Resources.updateResourceCount(m_XMLHandle, True)
        Else
            If (m_XMLHandle.HasElements) Then

                For Each node As XElement In m_XMLHandle.Elements
                    If (Convert.ToInt32(node.Attribute("id").Value = m_LocalID)) Then
                        node.Remove()
                        Exit For
                    End If
                Next
            End If


        End If
        Dim pdname = "SoundClick Producer"

        If Not (origination Is Nothing) Then
            pdname = origination.ArtistName
        End If
        Dim beatmakerid = 0
        If Not origination Is Nothing Then
            beatmakerid = origination.ID
        End If
        myNode = New XElement("TrackInfo", New XAttribute("nameofbeat", ProducerNamedTrackTitle),
                                                New XAttribute("bouncepath", BounceLocation),
                                                New XAttribute("id", m_LocalID),
                                                New XAttribute("mainAssociatedaArtistsCount",
                                                               associatedArtists(True).Count),
                                                New XAttribute("featuredAssociatedArtistsCount",
                                                               associatedArtists(False).Count),
                                                New XAttribute("beatmakerartistGID", beatmakerid),
                                                New XAttribute("forsale", isForSale),
                                                New XAttribute("paymethod", PaymentMethod),
                                                New XAttribute("auctionable", isAuctionable),
                                                New XAttribute("acceptsoffers", AcceptsOffers),
                                                New XAttribute("musicSourceType", SourceType),
                                                New XAttribute("producerName", pdname),
                                                New XAttribute("isExclusive", m_isExclusive))

        If Not (BeatBID Is Nothing) Then
            BeatBID.StoreBaseData(myNode)

        End If
        If Not (BeatDIB Is Nothing) Then
            BeatDIB.StoreBaseData(myNode)
        End If

        If (m_UsingArtists.Count > 0) Then




            Dim mainaanode As XElement = New XElement("MainAssociatedArtists", New XAttribute("count", m_UsingArtists.Count))


            For Each aa As KeyValuePair(Of String, List(Of String)) In m_UsingArtists

                Dim art As String = aa.Key
                Dim mysonglist As List(Of String) = aa.Value
                ' Dim cleanlist As List(Of String) = New List(Of String)
                ' Dim doubleCheck As String = mysonglist(0)
                ' cleanlist.Add(doubleCheck)
                ' Dim checkCount = 0
                'For Each song As String In mysonglist
                '    If (song = doubleCheck) Then

                '    Else
                '        cleanlist.Add(song)

                '    End If

                'Next


                Dim aNode As XElement = New XElement("MainAssociatedArtist",
                                                      New XAttribute("artist", art), New XAttribute("count", mysonglist.Count))
                For Each song As String In mysonglist
                    Dim songnode As XElement = New XElement("associatedSong", New XAttribute("title", song))
                    aNode.Add(songnode)




                Next

                mainaanode.Add(aNode)
            Next
            myNode.Add(mainaanode)

            If (m_FeaturedArtists.Count > 0) Then

                Dim feataanode As XElement = New XElement("FeaturedAssociatedArtists", New XAttribute("count", m_FeaturedArtists.Count))




                For Each fa As KeyValuePair(Of String, List(Of String)) In m_FeaturedArtists

                    Dim art As String = fa.Key
                    Dim mysonglist As List(Of String) = fa.Value

                    Dim faNode As XElement = New XElement("FeaturedAssociatedArtist", New XAttribute("artist", art), New XAttribute("count", mysonglist.Count))
                    For Each song As String In mysonglist
                        Dim songnode As XElement = New XElement("associatedFeatureSong", New XAttribute("title", song))
                        faNode.Add(songnode)




                    Next


                    feataanode.Add(faNode)

                Next
                myNode.Add(feataanode)


            End If
        End If


        m_XMLHandle.Add(myNode)
        'Application.ResourcesHaveChanged = True
        'XMLResources.SubmitResources()

        Return True
    End Function

End Class


Class BeatDib
    Inherits BaseTrackInfo.BeatDIB


    Public Sub New(ByRef base As BaseTrackInfoWrapper)
        MyBase.New(base)

        TrackID = base.ID
        TrackName = base.BounceLocation


    End Sub

    Public Overrides Function StoreBaseData(ByRef parent As XElement) As Boolean

        'remove old node
        For Each child As XElement In parent.Elements
            If (child.Attribute("id").Value = TrackID) Then
                child.Remove()
                Exit For
            End If
        Next
        Dim newData As XElement = New XElement("DIBS",
                                               New XAttribute("id", TrackID),
                                               New XAttribute("songname", TrackName),
                                               New XAttribute("dibcount", ListOfDibs.Count),
                                               New XAttribute("firstdibs", GetFirstPlace().Key),
                                               New XAttribute("firstdibsexpiration",
                                                              getNextExpiration().ToString))

        For Each Dib As KeyValuePair(Of String, DateTime) In ListOfDibs
            Dim newdib As XElement = New XElement("Dib",
                                                  New XAttribute("artist", Dib.Key),
                                                  New XAttribute("dateadded",
                                                                 Convert.ToDateTime(Dib.Value)),
                                                  New XAttribute("expired", False))

        Next
        For Each Dib2 As KeyValuePair(Of String, DateTime) In ExpiredListOfDibs
            Dim newdib As XElement = New XElement("Expired",
                                                   New XAttribute("artist", Dib2.Key),
                                                   New XAttribute("dateadded",
                                                                   Convert.ToDateTime(Dib2.Value)),
                                                               New XAttribute("expired", True))
        Next
        Application.ResourcesHaveChanged = True
        Return True

    End Function


End Class