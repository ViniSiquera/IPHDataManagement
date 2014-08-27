''' <summary>
''' Classe que ajusta as matrizes de dados
''' </summary>
Public Class ResampleData

#Region "Resample"
    'Reajusta os atributos de um raster através de um raster de referência
    Public Sub AdjustSpatialData(ByRef rasterToAdjust As RasterInteger, ByVal ReferenceRaster As RasterInteger)

        Dim xllCornerReference, yllCornerReference As Double
        Dim cellSizeReference As Double
        Dim nRowsReference, nColsReference As Integer

        'Pega as informações do Raster de Referencia
        xllCornerReference = ReferenceRaster.XllCorner
        yllCornerReference = ReferenceRaster.YllCorner
        cellSizeReference = ReferenceRaster.Cellsize
        nRowsReference = ReferenceRaster.Linhas
        nColsReference = ReferenceRaster.Colunas

        Dim resampledRaster As New RasterInteger(nRowsReference, nColsReference, xllCornerReference, yllCornerReference, cellSizeReference, rasterToAdjust.NoDataValue)
        Dim row, column As Integer
        Dim posX, posY As Double

        'Coordenada y upper left
        Dim yulCornerReference As Double = yllCornerReference + (cellSizeReference * nRowsReference)

        For i = 0 To nRowsReference - 1
            'Calcula a coordenada de latitude do ponto desejado
            posY = yulCornerReference - (i * cellSizeReference) - (cellSizeReference / 2)

            For j = 0 To nColsReference - 1
                'Calcula a coordenada de longitude do ponto desejado
                posX = xllCornerReference + (j * cellSizeReference) + (cellSizeReference / 2)

                row = IdentifyRow(posY, rasterToAdjust.YllCorner, rasterToAdjust.Cellsize, rasterToAdjust.Linhas)
                column = IdentifyColumn(posX, rasterToAdjust.XllCorner, rasterToAdjust.Cellsize)

                'Se a linha ou coluna ficarem fora dos limites do raster a ser ajustado, escreve NoData
                If row > (rasterToAdjust.Linhas - 1) OrElse row < 0 OrElse column > (rasterToAdjust.Colunas - 1) OrElse column < 0 Then
                    resampledRaster.Dados(i, j) = rasterToAdjust.NoDataValue
                Else
                    resampledRaster.Dados(i, j) = rasterToAdjust.Dados(row, column)
                End If
            Next
        Next

        rasterToAdjust = Nothing
        rasterToAdjust = resampledRaster

    End Sub

    Private Function IdentifyColumn(ByVal x As Double, ByVal xllcorner As Double, ByVal cellsize As Double) As Integer

        'Transformação das coordenadas do ponto em linhas e colunas
        'Lembrando que a posição (1,1) equivale a (0,0)
        Dim xllCenter As Double
        xllCenter = xllcorner + (cellsize / 2) 'Calcula a coordenada X do centro da célula no canto esquerdo

        Return CInt(Math.Round(((x - xllCenter) / cellsize), 0))

    End Function

    Private Function IdentifyRow(ByVal y As Double, ByVal yllcorner As Double, ByVal cellsize As Double, ByVal nRows As Integer) As Integer

        'Transformação das coordenadas do ponto em linhas e colunas
        'Lembrando que a posição (1,1) equivale a (0,0)
        Dim yllCenter As Double
        yllCenter = yllcorner + (cellsize / 2) 'Calcula a coordenada Y do centro da célula no canto esquerdo

        Return CInt(Math.Round((nRows - 1) - (y - yllCenter) / cellsize, 0))

    End Function
#End Region

#Region "ExcludeNoData"
    'Retira valores de noData de um raster, com base em uma área especificada (com valores diferentes de NoData) por um raster de referencia, substituindo por um valor indicado
    Public Shared Sub ExcludeNoDataFromRaster(ByRef RasterToAdjust As RasterInteger, ByVal ReferenceRasterArea As RasterInteger, ByVal SwitchValue As Int16, Optional ByVal XYtolerance As Double = 0)

        If RasterToAdjust.Linhas <> ReferenceRasterArea.Linhas OrElse RasterToAdjust.Colunas <> ReferenceRasterArea.Colunas Then
            Throw New Exception("O número de linhas ou colunas difere de um raster a ser ajustado para o raster de referência!")
        ElseIf Math.Abs(RasterToAdjust.XllCorner - ReferenceRasterArea.XllCorner) > XYtolerance OrElse Math.Abs(RasterToAdjust.YllCorner - ReferenceRasterArea.YllCorner) > XYtolerance Then
            Throw New Exception("Os limites XY dos rasters a ser ajustado e de referência diferem!")
        End If

        For i = 0 To RasterToAdjust.Linhas - 1
            For j = 0 To RasterToAdjust.Colunas - 1
                'Verifica se o raster de referência possui neste ponto um valor diferente de NoDATA
                If ReferenceRasterArea.Dados(i, j) <> ReferenceRasterArea.NoDataValue Then
                    'Verifica se neste ponto o raster a ser ajustado possui NoData
                    'Se tiver, substitui pelo valor indicado
                    If RasterToAdjust.Dados(i, j) = RasterToAdjust.NoDataValue Then RasterToAdjust.Dados(i, j) = SwitchValue
                End If
            Next
        Next

    End Sub
#End Region

End Class

