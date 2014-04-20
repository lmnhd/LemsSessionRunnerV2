Imports System.IO


Namespace LemsValueConverters2





    Public Class ScrewedConverter
        Implements IValueConverter
        Public Property screwed As Boolean
        Public Property isScrewed As Brush
        Property isntScrewed As Brush







        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert


            Return isntScrewed
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class

    

    Public Class AllSongsTitleConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Dim val = TryCast(value, String)
            Dim parent = Directory.GetParent(val).Name
            Dim title = Path.GetFileNameWithoutExtension(val)
            Return String.Format("{0} - {1}", parent, title)


        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return ""
        End Function
    End Class

    Public Class ScrewedTextConverter
        Implements IValueConverter


        Property isScrewed As String
        Property notScrewed As String




        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            'If (Application
            '.Screwed) Then
            '    Return isScrewed
            'Else
            '    Return notScrewed
            'End If
            Return ""

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing

        End Function
    End Class



    Public Class UsedBeatConverter
        Implements IValueConverter

        Property UsingArtistName As String

        Property BeatIsUsed As Brush

        Property BeatAintUsed As Brush

        Public Sub New()

        End Sub


        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            If Not (value) Then
                Return BeatAintUsed
            End If
            Return BeatIsUsed
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class


    Public Class TrackWrapperUserTrackBouncedConverter
        Implements IValueConverter

        Property songPath As String

        Property isBounced As Brush

        Property notBounced As Brush


        Public Sub New()

        End Sub


        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert

            If (value.ToString.ToLower.Contains(".mp3")) Then
                If (Path.GetExtension(value.ToString).ToLower = ".mp3") Then
                    Return isBounced
                End If
            End If
            Return notBounced
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function

    End Class

    Public Class IsBouncedBrushConverter
        Implements IValueConverter
        Property IsBounced As Boolean
        Property IsTrue As Brush
        Property IsFalse As Brush

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            If (value) Then
                Return IsTrue
            Else
                Return IsFalse
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class
    Public Class SongPriorityBrushConverter
        Inherits DependencyObject
        Implements IMultiValueConverter




        Property Hi_Priority As Brush

        Property Medium_Priority As Brush

        Property Low_Priority As Brush

        Property No_Priority As Brush

        Property Priority As Int32


        Property PriorityCount As Int32


        Public Sub New()

        End Sub





        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IMultiValueConverter.Convert
            ' If (values.GetValue(1).GetType() = TypeOf () Then

            ' End If

            Try
                Priority = (CType(values.GetValue(1), Int32)) + 1

                PriorityCount = CType(values.GetValue(0), Int32)
            Catch ex As Exception
                ' Return Low_Priority

            End Try




            Dim divisor = PriorityCount / 3

            Dim returnBrush As Brush = No_Priority

            If (Priority = 0) Then
                Return No_Priority
            End If

            If (Priority < divisor) Then
                returnBrush = Hi_Priority
            ElseIf (Priority < divisor * 2) Then
                returnBrush = Medium_Priority
            ElseIf (Priority <= divisor * 3 And PriorityCount > 0) Then
                returnBrush = Low_Priority
            End If


            Return returnBrush


        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As Globalization.CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class

    Public Class SongPriorityTextConverter
        Implements IMultiValueConverter


        Property Priority As Int32

        Property PriorityCount As Int32




        Public Sub New()

        End Sub


        Public Function Convert1(values() As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IMultiValueConverter.Convert
            Try
                Priority = (CType(values.GetValue(1), Int32)) + 1

                PriorityCount = CType(values.GetValue(0), Int32)
            Catch ex As Exception
                Return ""
            End Try




            Dim divisor = PriorityCount / 3

            Dim returnString = "None"

            If (Priority = 0) Then
                Return returnString
            End If

            If (Priority < divisor) Then
                returnString = "High"
            ElseIf (Priority < divisor * 2) Then
                returnString = "Medium"
            ElseIf (Priority <= divisor * 3 And PriorityCount > 0) Then
                returnString = "Low"
            End If


            Return String.Format("{0}:({1})", returnString, Priority)

        End Function

        Public Function ConvertBack1(value As Object, targetTypes() As Type, parameter As Object, culture As Globalization.CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class

End Namespace