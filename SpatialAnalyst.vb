Public Class SpatialAnalyst

    Public Class ReclassParameters
        Public LowerBound, UpperBound As Single
        Public NewValue As Single
    End Class

    'Realiza a operação de Reclass, com base nos parâmetros específicados
    Public Shared Sub ReclassRasterInteger(ByRef rasterToReclass As RasterInteger, ByVal parameters() As ReclassParameters)

        Dim parametersCount As Integer = parameters.Count

        For i = 0 To rasterToReclass.Linhas - 1
            For j = 0 To rasterToReclass.Colunas - 1
                For k = 0 To parametersCount - 1
                    If rasterToReclass.Dados(i, j) >= parameters(k).LowerBound AndAlso rasterToReclass.Dados(i, j) <= parameters(k).UpperBound Then
                        rasterToReclass.Dados(i, j) = parameters(k).NewValue : Exit For
                    End If
                Next
            Next
        Next
    End Sub



End Class
