Public Class XMLResources


    Private artistOBJ As Artist
    Private sessionSetup As SetupSession
    Private LoginID As Identity.GlobalID
    Private liveSession As SetupSession.Session

    Private Shared resourcesPath As String
    Private Shared handle As XDocument
    Private Shared root As XElement




    Private Shared loginHandle As XDocument
    Private Shared infoRoot As XElement

    Private Shared loggedInAtt As XAttribute

    Private Shared currentUserAtt As XAttribute
    Private Shared currentSongAtt As XAttribute

    Private Shared loginCountAtt As XAttribute
    Private Shared currentLogin As XElement
    Private Shared loginTimeAtt As XAttribute

    'local attributes

    'times
    Private localloginTimeAtt As XAttribute
    Private locallogoutTimeAtt As XAttribute


    'MAY REMOVE THIS TO BE DERIVED
    Private featureRequestsNode As XElement
    Private featureRequestCountAtt As XAttribute

    Private Shared CURRENT_INSTANCE As XMLResources
    Private Shared CURRENT_INSTANCE_WITH_SETUPSESSION As XMLResources

    Private thisLoginTime As DateTime
    Private thisLogoutTime As DateTime
    Private Shared loginCount As Int32
    Dim localResourceRoot As XElement




    Public Function SongClosing() As Boolean

        AddSongNode()

        Return True

    End Function



    Enum ResourceType

        NewType = 780
        FeatureRequest = 781
        'FeatureRequest = 782
        'FeatureRequest = 783
        'FeatureRequest = 784

    End Enum
    Shared LOGIN_HANDLE As String = Application.LOGIN_HANDLE

    Private Shared Sub initAllNodes()

        If loginHandle Is Nothing Or infoRoot Is Nothing Or handle Is Nothing Or root Is Nothing Then
            resourcesPath = Application.RESOURCES_PATH
            handle = XDocument.Load(resourcesPath)
            root = handle.Root



            loginHandle = XDocument.Load(LOGIN_HANDLE)
            infoRoot = loginHandle.Root


        End If




    End Sub

    ''' <summary>
    ''' Every artist will have one of these, Designed to pretty much handle All xml Login, and extraneus needs
    ''' If needed will create a new XML Document for each new resource needed by the artist or setupsession
    ''' </summary>
    ''' <param name="motherSession"></param>
    ''' <remarks>The current setup session</remarks>
    Public Sub New(ByRef motherSession As SetupSession)

        initAllNodes()
        If (motherSession Is Nothing) Then





            motherSession = New SetupSession(New Artist("MasterLoginSession", False))



        End If


        If (motherSession.Get_Current_Artist.Is_Logged_In Or motherSession.Get_Current_Artist.Is_Group) Then

            thisLoginTime = Now.ToString()


            artistOBJ = motherSession.Get_Current_Artist
            sessionSetup = motherSession


            If Not sessionSetup.Get_Current_Artist.My_Name = "MasterLoginSession" Then
                LoginID = Identity.CreateLoginID(artistOBJ.My_Name)









                If (loginHandle Is Nothing) Or (infoRoot Is Nothing) Then

                    loginHandle = XDocument.Load(LOGIN_HANDLE)
                    infoRoot = loginHandle.Root
                End If

                'get all LoginInfo attributes
                currentUserAtt = infoRoot.Attribute("currentUser")
                currentSongAtt = infoRoot.Attribute("currentSong")

                loginTimeAtt = infoRoot.Attribute("LoginTime")
                If (loginTimeAtt Is Nothing) Then
                    loginTimeAtt = infoRoot.Attribute("loginTime")

                End If
                loggedInAtt = infoRoot.Attribute("loggedIn")
                loginCountAtt = infoRoot.Attribute("count")

                'get current login count
                loginCount = Convert.ToInt32(loginCountAtt.Value)


                featureRequestsNode = root.Element("FeatureRequests")
                featureRequestCountAtt = featureRequestsNode.Attribute("count")




                Init()

            Else
                If (loginHandle Is Nothing) Or (infoRoot Is Nothing) Then

                    loginHandle = XDocument.Load(LOGIN_HANDLE)
                    infoRoot = loginHandle.Root


                End If
            End If



        Else
            'Throw New Exception("ARTIST NOT LOGGED IN")

        End If


    End Sub

    Public Sub New()
        If Application.LEMTESTING Then

            resourcesPath = "C:\Users\BricklyfeA\Documents\Visual Studio 2012\Projects\LemsSessionRunnerV2\LemsSessionRunnerV2\bin\Debug\xml\" & "resources.xml"
            handle = XDocument.Load(resourcesPath)
            root = handle.Root



            loginHandle = XDocument.Load(LOGIN_HANDLE)
            infoRoot = loginHandle.Root

        Else
            initAllNodes()

        End If
        

    End Sub

    Public Shared Function GET_XMLResources(Optional motherSessions As SetupSession = Nothing) As XMLResources
        If (motherSessions Is Nothing) Then
            If (CURRENT_INSTANCE Is Nothing) Then
                CURRENT_INSTANCE = New XMLResources()

            End If

            Return CURRENT_INSTANCE
        Else
            If (CURRENT_INSTANCE_WITH_SETUPSESSION Is Nothing) Then
                CURRENT_INSTANCE_WITH_SETUPSESSION = New XMLResources(motherSessions)
            End If
            Return CURRENT_INSTANCE_WITH_SETUPSESSION
        End If
    End Function

    ''' <summary>
    ''' Initialize current LoginInfo and artistLogin nodes
    ''' 
    ''' </summary>
    ''' <remarks></remarks>

    Private Sub Init()
        currentUserAtt.Value = artistOBJ.My_Name

        '


        ' currentSongAtt.Value = session.Get_Current_Song.Title
        loginTimeAtt.Value = thisLoginTime
        loggedInAtt.Value = 1
        createLoginNode()
    End Sub

    Public Shared Function GetLoginRoot() As XElement

        Return infoRoot
    End Function


    ''' <summary>
    ''' Returns A Brand New ArtistLogin element to be used and updated througout the current session and stored for
    ''' History
    ''' </summary>
    ''' 
    ''' <remarks></remarks>
    Private Sub createLoginNode()

        currentLogin = New XElement("artistLogin", New XAttribute("name", artistOBJ.My_Name), New XAttribute("Identity", LoginID.GLOBAL_ID), New XAttribute("loginTime", thisLoginTime.ToString
), New XAttribute("logOutTime", ""), New XAttribute("duration", "0"))


        infoRoot.Add(currentLogin)
        localloginTimeAtt = currentLogin.Attribute("loginTime")
        locallogoutTimeAtt = currentLogin.Attribute("logOutTime")
        updateLoginCount(True)
        SubmitLogin()





    End Sub

    Private Sub AddSongNode()

        Dim liSong As XElement = New XElement("song", New XAttribute("Identity", sessionSetup.Get_Current_Song.GLOBAL_ID), New XAttribute("opened", sessionSetup.Get_Current_Session.Start_Time), New XAttribute("closed", Now))

        currentLogin.Add(liSong)
        SubmitLogin()


    End Sub

    Public Function LoggingOut() As Boolean

        Try
            locallogoutTimeAtt.Value = DateTime.Now
            thisLogoutTime = Now
        Catch ex As Exception
            MsgBox(ex.Message + " " + ex.StackTrace)
            Return False

        End Try

       

        ' Dim length As TimeSpan = Convert.ToDateTime(thisLoginTime).TimeOfDay - Convert.ToDateTime(locallogoutTimeAtt.Value).TimeOfDay
        Dim _to As DateTime = Convert.ToDateTime(locallogoutTimeAtt.Value)

        Dim _from As DateTime = Convert.ToDateTime(thisLoginTime)



        Dim length As String = Utils.Get_Time_Duration(_from.ToShortTimeString, _to.ToShortTimeString)


        ' Dim lnt As String = length.Hours & "hr:" & length.Minutes & "min:" & length.Seconds & "sec"

        ' Dim length2 As TimeSpan = Now - thisLoginTime
        ' Dim finLength As String = length2.TotalHours.ToString & "s and " & length2.TotalMinutes.ToString






        currentLogin.Attribute("duration").Value = length.ToString

        SubmitLogin()
        SubmitIdentity()


        Return True
    End Function

    Private Shared Sub updateLoggedIn(ByVal user As String, ByRef song As String, ByVal logged_In As Boolean)
        currentUserAtt.Value = user
        currentSongAtt.Value = song

        loggedInAtt.Value = logged_In

        SubmitLogin()

    End Sub


    Private Shared Function updateLoginCount(Optional ByVal up As Boolean = True)
        If (up) Then
            loginCount = loginCount + 1
            loginCountAtt.Value = loginCount
        Else
            loginCount = loginCount - 1
            loginCountAtt.Value = loginCount
        End If
        Return loginCount
    End Function

    'Private Function SongClosing() As Boolean



    'End Function





    Public Function ArtistLoginNode() As XElement

        Return currentLogin

    End Function

    'Public Shared Function PreviousLogins(ByVal songName As String) As List(Of XElement)


    '    Dim list As List(Of XElement) = New List(Of XElement)

    '    For Each node As XElement In infoRoot.Elements
    '        For Each sng As XElement In node.Elements
    '            If (Utils.SpaceSafeCompareSongName(sng.Attribute("name").Value, songName)) Then
    '                list.Add(node)

    '            End If

    '        Next
    '    Next

    '    Return list


    'End Function

    Private Sub SubmitIdentity()
        If Application.LEMTESTING Then
            Return
        End If
        If (Application.IdentityHasChanged) Then
            handle.Save(resourcesPath)
            Application.IdentityHasChanged = False
        End If
        ' handle.Save(resourcesPath)

    End Sub

    Private Shared Sub SubmitLogin()
        If Application.LEMTESTING Then
            Return
        End If
        ' Utils.UpdateLogininfoNode()
        Try
            loginHandle.Save(LOGIN_HANDLE)
        Catch ex As Exception
            Application.LemLog(ex.Message, "ERROR SAVING LOGIN DATA...")
        End Try


    End Sub
    Public Shared Sub QuickUpdateLoginNode()
        SubmitLogin()
    End Sub
    Public Shared Sub SubmitResources()

        If Application.LEMTESTING Then
            Return
        End If

        If (Application.ResourcesHaveChanged) Then
            Application.ResourcesHaveChanged = False
            Try
                handle.Save(resourcesPath)
            Catch ex As Exception
                Application.ResourcesHaveChanged = True
            End Try


        End If



    End Sub

    Public Function updateResourceCount(ByRef resourceNode As XElement, Optional ByVal up As Boolean = True) As Int32

        Dim count = Convert.ToInt32(resourceNode.Attribute("count").Value)
        If (up) Then
            count = count + 1
        Else
            count = count - 1
        End If

        resourceNode.Attribute("count").Value = count
        Return count
    End Function

    Private Shared Function ConvertResourceToTypeName(ByRef res As ResourceType) As String
        Select Case res
            Case ResourceType.FeatureRequest
                Return "FeatureRequest"
        End Select
        Return res.ToString
    End Function

    Public Structure AddResourceArgs
        Property type As ResourceType
        Property resource As Object
        Property newTypeName As String
        Property attParams As Dictionary(Of String, String)
        Property NestedParams As Dictionary(Of String, Dictionary(Of String, String))
        Property ParentSubNode As String
        Property TextContent As String
    End Structure


    ''' <summary>
    ''' Adds A Brand New Artist Resource To The Resources DataBase
    ''' To Add Attributes, Create a Dictionary(String,String) full of *Key=>NameOfAtt *Value=>ValueAsString
    ''' To add Nested Elements, Create a Dictionary(String=>ElName,Dictionary(String=>NameOfAtt,String=>ValueAsString)
    ''' To Enumerate items more specifically include The ParentSubNode param with the SubValues name
    ''' See Notification for example..
    ''' Nested Elements Can Not Contain Nested Elements right now..
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddResource(args As AddResourceArgs) As Integer

        Select Case args.type

            Case ResourceType.NewType
                Dim BrandNew = False
                If (args.newTypeName Is Nothing) Or (args.newTypeName = "") Then
                    Throw New Exception("The Generic typeName parameter requires a name")
                End If



                'check if exists

                Dim mainNode As XElement = Nothing
                Dim myNode As XElement = Nothing

                Try
                    mainNode = root.Element(args.newTypeName & "s")

                Catch ex As Exception

                End Try

                If mainNode Is Nothing Then
                    mainNode = New XElement(args.newTypeName & "s", New XAttribute("count", "1"))
                    BrandNew = True
                End If


                'If Not ParentSubNode = "" Then
                '    Dim subNode As XElement = New XElement(ParentSubNode, New XAttribute("count", 1))

                'End If
                Dim sub1 As XElement = Nothing
                Dim subCount As Integer = -1
                Dim count = Convert.ToInt32(mainNode.Attribute("count").Value)
                If Not args.ParentSubNode Is Nothing And Not args.ParentSubNode = "" Then
                    Dim foundSub As Boolean = False
                    For Each _subNode As XElement In mainNode.Elements
                        If (_subNode.Name = args.ParentSubNode) Then
                            myNode = _subNode
                            'subCount = updateCount(myNode)


                            foundSub = True
                            Exit For
                        End If

                    Next
                    If Not foundSub Then
                        myNode = New XElement(args.ParentSubNode, New XAttribute("id", "1"))
                        mainNode.Add(myNode)
                        myNode.Attribute("id").Value = mainNode.Elements.Count


                    End If
                End If




                If Not (args.attParams Is Nothing) Then

                    For Each kv As KeyValuePair(Of String, String) In args.attParams
                        Dim att As XAttribute = New XAttribute(kv.Key, kv.Value)
                        myNode.Add(att)


                    Next
                End If
                If Not (args.NestedParams Is Nothing) Then

                    For Each nestedEl As KeyValuePair(Of String, Dictionary(Of String, String)) In args.NestedParams
                        Dim newNode As XElement = New XElement(nestedEl.Key)
                        Dim idAtt As XAttribute = New XAttribute("id", "")
                        newNode.Add(idAtt)
                        For Each nestedAtt As KeyValuePair(Of String, String) In nestedEl.Value

                            Dim newAtt As XAttribute = New XAttribute(nestedAtt.Key, nestedAtt.Value)

                            If Not (args.TextContent Is Nothing) Then
                                newNode.Value = args.TextContent
                            End If

                            newNode.Add(newAtt)

                        Next
                        newNode.Attribute("id").Value = (myNode.Elements.Count).ToString
                        myNode.Add(newNode)
                        newNode.Attribute("id").Value = myNode.Elements.Count


                    Next

                End If
                If (BrandNew) Then
                    root.Add(mainNode)
                End If
                mainNode.Attribute("count").Value = mainNode.Elements.Count
                Application.ResourcesHaveChanged = True
                Return myNode.Elements.Count
                'Submit()





            Case ResourceType.FeatureRequest

                If args.resource Is Nothing Then
                    Return False
                End If
                Dim fr As Artist.FeatureRequest = TryCast(args.resource, Artist.FeatureRequest)
                If Not (fr Is Nothing) Then
                    featureRequestsNode.Add(New XElement("FeatureRequest", New XAttribute("id", updateCount(featureRequestsNode, True)), New XAttribute("sender", fr.One_Sending), New XAttribute("recipient", fr.One_Recieving), New XAttribute("song", fr.SongToFeatureOn), New XAttribute("otherFeatures", fr.OtherFeaturedArtists), New XAttribute("comments", fr.SongHasComments)))
                    'Submit()
                End If

                Return 1
        End Select


        Return 0
    End Function





    Public Function DeleteResource(ByRef type As ResourceType, ByRef id As Int32, Optional GenericName As String = "") As Boolean



        If (type = ResourceType.NewType And Not GenericName = "") Then
            Dim parentNode1 = root.Element(GenericName & "s")
            For Each child As XElement In parentNode1.Elements
                If (Convert.ToInt32(child.Attribute("id").Value) = id) Then
                    child.Remove()
                    ' updateCount(parentNode1, False)
                    ' Submit()
                    Return True
                End If
            Next


        Else


            Dim parentNode = root.Element(ConvertResourceToTypeName(type) & "s")
            For Each child As XElement In parentNode.Elements
                If (Convert.ToInt32(child.Attribute("id").Value) = id) Then
                    child.Remove()
                    ' updateCount(parentNode, False)
                    ' Submit()
                    Return True
                End If
            Next
        End If
        Return False

    End Function

    Public Class SimpleResourceArgs

        Private _categoryName As String
        Private _nodeName As String
        Private _id As Integer
        Private _parent As SimpleResourceArgs

        Sub New(nameOfResourceCategory As String, nameOfResourceChild As String, parent As SimpleResourceArgs)
            _nodeName = nameOfResourceChild
            _categoryName = nameOfResourceCategory
            _parent = parent
            children = New List(Of SimpleResourceArgs)
            atts = New List(Of ResourceAttribute)

            If Not parent Is Nothing Then
                parent.children.Add(Me)

                _id = parent.children.Count
            Else
                _id = 1
            End If
        End Sub





        ReadOnly Property categoryName As String
            Get
                Return _categoryName
            End Get
        End Property

        ReadOnly Property nodeName As String
            Get
                Return _nodeName
            End Get
        End Property

        ReadOnly Property id As Integer
            Get
                Return _id
            End Get

        End Property
        Property atts As List(Of ResourceAttribute)
        Property children As List(Of SimpleResourceArgs)



    End Class

    Class ResourceAttribute

        Private _name As String
        Private _val As String


        Sub New(attributeName As String, attributeValue As String)
            _name = attributeName
            _val = attributeValue


        End Sub
        ReadOnly Property name As String
            Get
                Return _name
            End Get
        End Property
        ReadOnly Property value As String
            Get
                Return _val
            End Get
        End Property
    End Class

    Public Structure RestoredResource
        Property category As String
        Property name As String
        Property id As Integer
        Private _atts As List(Of ResourceAttribute)
        Private _children As List(Of RestoredResource)

        ReadOnly Property atts As List(Of ResourceAttribute)
            Get
                Return _atts
            End Get
           
        End Property

        Public Sub addAttribute(att As ResourceAttribute)
            If _atts Is Nothing Then
                _atts = New List(Of ResourceAttribute)

            End If
            _atts.Add(att)
        End Sub
        ReadOnly Property children As List(Of RestoredResource)
            Get
                Return _children
            End Get
        End Property

        Public Sub addChild(child As RestoredResource)
            If _children Is Nothing Then
                _children = New List(Of RestoredResource)
            End If
            _children.Add(child)
        End Sub


    End Structure
    ''' <summary>
    ''' Get a Resources Hierarchy starting from the given resource name
    ''' </summary>
    ''' <param name="nameOfResource"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function RestoreResource(nameOfCategory As String, nameOfResource As String, Optional parent As RestoredResource = Nothing, Optional resNode As XElement = Nothing) As RestoredResource
        Dim result As RestoredResource = Nothing
        Dim resources As XMLResources = New XMLResources()
        Dim myCategory As XElement = resources.helperCheckIfNodeExists(nameOfCategory & "s", root, False, True)
        Dim myNode As XElement = Nothing

        If Not parent.name Is Nothing Then
            result = New RestoredResource()
            myNode = resources.helperCheckIfNodeExists(nameOfResource, resNode, False, True)
            If Not myNode Is Nothing Then
                result.name = nameOfResource
                result.id = myNode.Attribute("id").Value
                result.category = nameOfCategory.Substring(0, nameOfCategory.Length)

                For Each att As XAttribute In myNode.Attributes
                    Dim tempatt As ResourceAttribute = New ResourceAttribute(att.Name.LocalName, att.Value)
                    result.addAttribute(tempatt)

                Next

                If myNode.HasElements Then

                    For Each child As XElement In myNode.Elements
                        result.addChild(RestoreResource(nameOfCategory, child.Name.LocalName, result, myNode))

                    Next
                Else
                    Return result
                End If



            End If
            Return result

        End If



       








        If Not myCategory Is Nothing Then
            myNode = resources.helperCheckIfNodeExists(nameOfResource, myCategory, False, True)

            If Not myNode Is Nothing Then


                result = New RestoredResource()

                result.name = myNode.Name.LocalName
                result.id = myNode.Attribute("id").Value
                result.category = nameOfCategory.Substring(0, nameOfCategory.Length)



                For Each att As XAttribute In myNode.Attributes
                    Dim tempatt As ResourceAttribute = New ResourceAttribute(att.Name.LocalName, att.Value)
                    result.addAttribute(tempatt)

                Next

                If myNode.HasElements Then


                    For Each child As XElement In myNode.Elements
                        result.addChild(RestoreResource(nameOfCategory, child.Name.LocalName, result, myNode))


                    Next

                Else
                    Return result
                End If

               
            End If

            'For Each att As XAttribute In node.Attributes
            '    result.atts.Add(New ResourceAttribute(att.Name.LocalName, att.Value))

            'Next
            'For Each child As XElement In node.Elements
            '    Dim newEl As RestoredResource = New RestoredResource()
            '        newEl .

            ' Next
        End If

        Return result
    End Function


    Private Function getCategoryNode(categoryName As String) As XElement

        For Each name As XElement In root.Elements
            If name.Name.LocalName = categoryName Then
                Return name
            End If
        Next
        Dim result As XElement = New XElement(categoryName)
        root.Add(result)
        Return result

    End Function

    Private Function helperCheckIfNodeExists(nodeName As String, parent As XElement, removeInsted As Boolean, Optional forRestoration As Boolean = False) As XElement
        Dim result As XElement = Nothing
        For Each name As XElement In parent.Elements
            If name.Name.LocalName = nodeName Then


                If forRestoration Then
                    Return name
                End If

                name.Remove()
                name.RemoveAll()
                result = name
                If removeInsted Then
                    Return Nothing
                End If

            End If

        Next
        Return result
    End Function

    Public Function RemoveResourceQuickly(resourceCategory As String, resourceName As String) As Boolean
        Dim args As SimpleResourceArgs = New SimpleResourceArgs(resourceCategory, resourceName, Nothing)

        AddResourceRecursive(args, True, Nothing, True)
        Return True

    End Function
    ''' <summary>
    ''' Create a nested SimpleResourceArgs object and let me know if I should overwrite anything by that name
    ''' </summary>
    ''' <param name="args"></param>
    ''' <param name="overWriteIfThere"></param>
    ''' <param name="parent"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddResourceRecursive(args As SimpleResourceArgs, overWriteIfThere As Boolean, parent As XElement, Optional removeInsted As Boolean = False) As XElement


        'FIND TOP resource NODE for this category
        Dim categoryNode As XElement = getCategoryNode(args.categoryName & "s")
        Dim subCategorynode As XElement = Nothing
        Dim myNode As XElement = Nothing
        Dim myParent As XElement = root


        'IF THERE IS A PARENT THIS FUNCTION IS OPERATING ON A SUBCAT
        If Not parent Is Nothing Or removeInsted Then
            'this node is a subcategory
            myParent = parent
            Try

            
                If removeInsted Then
                    subCategorynode = helperCheckIfNodeExists(args.nodeName, categoryNode, True)
                    If subCategorynode Is Nothing Then
                        categoryNode.SetAttributeValue("count", categoryNode.Elements.Count)


                        Application.ResourcesHaveChanged = True
                        XMLResources.SubmitResources()
                        Return Nothing
                    End If


                ElseIf overWriteIfThere Then
                    If Not myParent Is Nothing Then
                        subCategorynode = helperCheckIfNodeExists(args.nodeName, myParent, removeInsted)
                    Else

                    End If

                End If

            Catch ex As Exception
                Console.WriteLine(ex.StackTrace.ToString)
                Return Nothing
            End Try



            If subCategorynode Is Nothing Then
                'we need it but it doesnt exist or
                'we just needed to remove it
                If removeInsted Then
                    Application.ResourcesHaveChanged = True
                    XMLResources.SubmitResources()


                    Return categoryNode
                End If

                If overWriteIfThere Then
                    subCategorynode = Nothing
                    subCategorynode = helperCheckIfNodeExists(args.nodeName, categoryNode, removeInsted)
                End If

                If subCategorynode Is Nothing Then
                    subCategorynode = New XElement(args.nodeName)
                    myParent.Add(subCategorynode)
                End If


            End If
        Else
            'parent is root
            If overWriteIfThere Then
                subCategorynode = Nothing
                subCategorynode = helperCheckIfNodeExists(args.nodeName, categoryNode, removeInsted)
                subCategorynode = Nothing
                'If subCategorynode.Elements.Count = 0 Then

                'End If
        End If

        If subCategorynode Is Nothing Then
            subCategorynode = New XElement(args.nodeName)
            categoryNode.Add(subCategorynode)
        End If

        End If



        'got category node
        'create atts

        If Not myParent.Name = root.Name Then

            subCategorynode.SetAttributeValue("id", myParent.Elements.Count)


        End If



        subCategorynode.SetAttributeValue("count", args.children.Count)
        subCategorynode.SetAttributeValue("id", subCategorynode.Parent.Elements.Count)

        For Each att As ResourceAttribute In args.atts
            Dim myatt As XAttribute = New XAttribute(att.name, att.value)
            subCategorynode.SetAttributeValue(att.name, att.value)

        Next






        If args.children.Count > 0 Then
            For Each child As SimpleResourceArgs In args.children

                AddResourceRecursive(child, overWriteIfThere, subCategorynode)


            Next
        End If






        categoryNode.SetAttributeValue("count", categoryNode.Elements.Count)
        'myParent.Add(categoryNode)


        Application.ResourcesHaveChanged = True
        XMLResources.SubmitResources()
        Return categoryNode

    End Function

    Private Function updateCount(ByRef parent As XElement, Optional ByVal up As Boolean = True) As Int32

        Dim countNode As XAttribute = parent.Attribute("count")
        Dim count = Convert.ToInt32(countNode.Value)
        If (up) Then
            count = count + 1
        Else
            count = count - 1
        End If

        countNode.Value = count
        Return count
    End Function

    ''' <summary>
    ''' Returns the base node of a generic resource
    ''' you can optionally get a child node if you include the id
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="id"></param>
    ''' <param name="NewType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetResource(ByRef type As ResourceType, Optional ByRef id As Int32 = 0, Optional ByVal NewType As String = "") As XElement


        If root Is Nothing Then
            initAllNodes()
        End If

        If (type = ResourceType.NewType And Not NewType = "") Then

            For Each child As XElement In root.Elements
                If (child.Name = NewType & "s") Then
                    If Not id = 0 Then
                        For Each node As XElement In child.Elements
                            If (Convert.ToInt32(node.Attribute("id").Value = id)) Then
                                Return node
                            Else
                                ' Application.LEM_ERR("Cant find any " & NewType & " with ID " &
                                '  id.ToString)
                                Return Nothing
                            End If
                        Next
                    End If
                    Return child
                End If
            Next
            'Not there so Create it
            Dim baseNode = New XElement(NewType & "s", New XAttribute("count", 0))
            root.Add(baseNode)

            Return baseNode



        Else



            If Not (id = 0) Then



                Dim parentNode = root.Element(ConvertResourceToTypeName(type) & "s")
                For Each child As XElement In parentNode.Elements
                    If (Convert.ToInt32(child.Attribute("id").Value) = id) Then

                        Return child
                    End If
                Next
            End If
        End If
        Return Nothing

    End Function

    Public Shared Function GetLoginXMLResourceRoot() As XElement
        If (root Is Nothing) Then
            handle = XDocument.Load(resourcesPath)
            root = handle.Root
        End If
        Return root
    End Function

    Public ReadOnly Property Current_Login_Time As String
        Get
            Return loginTimeAtt.Value

        End Get
    End Property

    Public ReadOnly Property Logged_In As Boolean
        Get
            Return Convert.ToBoolean(loggedInAtt.Value)
        End Get
    End Property

    Public ReadOnly Property Log_In_Count As Int32
        Get
            Return loginCount
        End Get
    End Property

    Public ReadOnly Property Login_ID As Identity.GlobalID
        Get
            Return LoginID

        End Get
    End Property

    Shared Function GetProducersResourceNode() As XElement
        Return GetResource(XMLResources.ResourceType.NewType, NewType:="ProducerBrand")


    End Function



End Class