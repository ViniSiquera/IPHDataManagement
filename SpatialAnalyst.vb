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
     
     Public Shared Function AreaCell(ByVal linha As Integer, ByVal coluna As Integer, ByVal xll As Double, ByVal yll As Double, ByVal cellsize As Double, ByVal nrows As Integer, ByVal ncols As Integer) As Single        Dim AreaFinal As Double
        Dim xesq, xdir, yinf, ysup, Xmin, Ymin As Double
        Xmin = xll
        Ymin = yll
        xesq = Xmin + (coluna - 1) * cellsize
        xdir = xesq + cellsize
        yinf = Ymin + (nrows - linha) * cellsize
        ysup = yinf + cellsize

        Dim A = 6378.137
        Dim B = 6356.752
        Dim Ylat As Double = (yinf + ysup) / 2
        Dim F As Double = (A - B) / A
        Dim E2 As Double = 2 * F - F ^ 2
        Dim YlatD As Double = Math.PI * Ylat / 180
        Dim RN As Double = A / (1 - E2 * (Math.Sin(YlatD) ^ 2)) ^ 0.5
        Dim Rcirc As Double = RN * Math.Cos(YlatD)
        Dim DGX As Double = xdir - xesq
        Dim DGY As Double = yinf - ysup
        Dim DX As Double = Rcirc * DGX * (Math.PI / 180)
        Dim DY As Double = RN * DGY * (Math.PI / 180)
        AreaFinal = ((Math.PI * RN ^ 2) / 180) * Math.Abs(Math.Sin(yinf * Math.PI / 180) - Math.Sin(ysup * Math.PI / 180))
        AreaFinal = (AreaFinal * Math.Abs(xesq - xdir)) * 1000000
        Return AreaFinal

    End Function

Public Shared Function CellLength(ByVal Ylat As Single, ByVal Xlong As Single, ByVal RelY As Int16, ByVal relX As Int16, ByVal cellsz As Double) As Single

        'CALCULA DISTANCIAS SOBRE A SUPERFICIE CONSIDERANDO O ELIPSOIDE wgs84
        'AS EQUAÇÕES UTILIZADAS AQUI FORAM OBTIDAS EM UMA HOMEPAGE DE PETER DANA
        'DA UNIVERSIDADE DO COLORADO 

        Const A As Single = 6378.137 'COMPRIMENTO DO SEMI EIXO MAIOR DO ELIPSÓIDE (KM)
        Const B As Single = 6356.752 'COMPRIMENTO DO SEMI EIXO MENOR DO ELIPSÓIDE (KM)
        Dim F, E2, RN, RCIRC As Double

        F = (A - B) / A '!ACHATAMENTO DO ELIPSÓIDE
        E2 = (2 * F) - (F ^ 2) '!QUADRADO DA EXCENTRICIDADE
        RN = A / (1 - E2 * (Math.Sin(Ylat * Math.PI / 180) ^ 2)) ^ 0.5 '!RAIO DE CURVATURA DA TERRA NA LATITUDE

        '!CALCULA RAIO DA CIRCUNFERENCIA DE UM CIRCULO DETERMINADO PELO PLANO 
        '!QUE CORTA O ELIPSÓIDE NA LATITUDE YLAT
        RCIRC = RN * Math.Cos(Ylat * Math.PI / 180)

        'Dependendo da direção relativa a partir de qual a célula veio através do Flowdirection (relX, relY), calcula o comprimento
        If relX = 0 AndAlso RelY <> 0 Then Return (RN * cellsz * (Math.PI / 180.0)) * 0.96194 'Vertical
        If relX <> 0 AndAlso RelY = 0 Then Return (RCIRC * cellsz * (Math.PI / 180.0)) * 0.96194 'Horizontal
        If relX <> 0 AndAlso RelY <> 0 Then Return Math.Sqrt(((RN * cellsz * (Math.PI / 180.0)) ^ 2 + (RCIRC * cellsz * (Math.PI / 180.0)) ^ 2)) * (1.36039 / 1.414) 'Diagonal

    End Function

 'Extrair um raster a partir de outro
    Public Shared Function ExtractRasterByRaster(ByVal BigRaster As RasterInteger, ByVal referenceRaster As RasterInteger) As RasterInteger

        'Compatibiliza as informações do raster grande, com o espaço do raster pequeno
        ResampleData.AdjustSpatialData(BigRaster, referenceRaster)

        For linha = 0 To referenceRaster.Linhas - 1
            For coluna = 0 To referenceRaster.Colunas - 1
                'Extrai a informação somente para o intervalo onde houverem valores para o raster de referência
                If referenceRaster.Dados(linha, coluna) <> referenceRaster.NoDataValue Then
                    referenceRaster.Dados(linha, coluna) = BigRaster.Dados(linha, coluna)
                End If
            Next
        Next

        Return referenceRaster

    End Function

End Class
