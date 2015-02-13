Public Class SpatialAnalyst

    Public Class ReclassParameters
        Private _LowerBound, _UpperBound As Double
        Private _NewValue As Double

        ''' <summary>
        ''' Creates an instance for this class
        ''' </summary>
        ''' <param name="LB">The Lower bound (greater or equal) </param>
        ''' <param name="UB">The Upper bound (less than)</param>
        ''' <param name="NewVal">The new value to be assigned.</param>
        Public Sub New(ByVal LB As Double, ByVal UB As Double, ByVal NewVal As Double)
            _LowerBound = LB
            _UpperBound = UB + 0.001
            _NewValue = NewVal
        End Sub
        ''' <summary>
        ''' Retorna o valor Limite inferior
        ''' </summary>
        Public ReadOnly Property lowerBound As Double
            Get
                Return _LowerBound
            End Get
        End Property
        ''' <summary>
        ''' Retorna o valor Limite superior 
        ''' </summary>
        Public ReadOnly Property upperBound As Double
            Get
                Return _UpperBound
            End Get
        End Property
        ''' <summary>
        ''' Retorna o novo valor
        ''' </summary>
        Public ReadOnly Property newValue As Double
            Get
                Return _NewValue
            End Get
        End Property

    End Class

    'Realiza a operação de Reclass, com base nos parâmetros específicados
    Public Shared Sub ReclassRasterInteger(ByRef rasterToReclass As RasterInteger, ByVal parameters As List(Of ReclassParameters))

        Dim parametersCount As Integer = parameters.Count

        For i = 0 To rasterToReclass.Linhas - 1
            For j = 0 To rasterToReclass.Colunas - 1
                For k = 0 To parametersCount - 1
                    If rasterToReclass.Dados(i, j) >= parameters.Item(k).lowerBound AndAlso _
                        rasterToReclass.Dados(i, j) < parameters.Item(k).upperBound Then

                        rasterToReclass.Dados(i, j) = CInt(parameters.Item(k).newValue) : Exit For
                    End If
                Next
            Next
        Next
    End Sub

End Class
