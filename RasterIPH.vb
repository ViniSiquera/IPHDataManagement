''' <summary>
''' Classe de raster que deve ser obrigatoriamente herdada por classes derivadas
''' </summary>
''' <remarks></remarks>
Public MustInherit Class Raster

    Protected _nCols As Integer 'Numero Colunas
    Protected _nRows As Integer 'Numero Linhas
    Protected _xllcorner, _yllcorner As Double 'Limites de coordenadas
    Protected _cellsize As Double 'Resolução do raster
    Protected _NODATAValue As Integer 'Valor do NODATA

    ''' <summary>
    ''' Retorna o número de linhas
    ''' </summary>
    Public ReadOnly Property Linhas() As Integer
        Get
            Return _nRows
        End Get
    End Property
    ''' <summary>
    ''' Retorna o número de colunas
    ''' </summary>
    Public ReadOnly Property Colunas() As Integer
        Get
            Return _nCols
        End Get
    End Property
    ''' <summary>
    ''' Retorna a resolução da grade
    ''' </summary>
    Public ReadOnly Property Cellsize() As Double
        Get
            Return _cellsize
        End Get
    End Property
    ''' <summary>
    ''' Retorna o valor para o qual não se tem dados no raster
    ''' </summary>
    Public ReadOnly Property NoDataValue As Integer
        Get
            Return _NODATAValue
        End Get
    End Property
    ''' <summary>
    ''' Retorna o número total de células
    ''' </summary>
    Public ReadOnly Property TotalDeCelulas() As Long
        Get
            Return _nCols * _nRows
        End Get
    End Property
    ''' <summary>
    ''' Retorna a coordenada X do canto superior da grade
    ''' </summary>
    Public ReadOnly Property XllCorner() As Double
        Get
            Return _xllcorner
        End Get
    End Property
    ''' <summary>
    ''' Retorna a coordenada Y do canto superior da grade
    ''' </summary>
    Public ReadOnly Property YllCorner() As Double
        Get
            Return _yllcorner
        End Get
    End Property

    'Gera uma nova instância da classe
    Public Sub New()

    End Sub

End Class

''' <summary>
''' Classe de Raster para trabalhar com dados inteiros
''' </summary>
Public Class RasterInteger
    Inherits Raster
    Private _Data(,) As Int16
    Private _maxValue As Int16

    ''' <summary>
    ''' Retorna os dados da matriz(y, x) do raster
    ''' </summary>
    Public ReadOnly Property Dados() As Int16(,)
        Get
            Return _Data
        End Get
    End Property
    ''' <summary>
    ''' Retorna o máximo valor dos dados da matriz(y, x) do raster
    ''' </summary>
    Public ReadOnly Property MaxValue As Int16
        Get
            Return _maxValue
        End Get
    End Property

    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Cria um raster a partir de informações indicadas pelo usuário
    ''' </summary>
    Public Sub New(ByVal numeroLinhas As Integer, ByVal numeroColunas As Integer, ByVal xllCorner As Double, ByVal yllcorner As Double, ByVal cellsize As Double, ByVal NoData As Integer)
        MyBase.new()
        ReDim _Data(numeroLinhas - 1, numeroColunas - 1)
        _nCols = numeroColunas
        _nRows = numeroLinhas
        _xllcorner = xllCorner
        _yllcorner = yllcorner
        _cellsize = cellsize
        _NODATAValue = NoData
    End Sub

    ''' <summary>
    ''' Faz uma copia completa dos dados de um raster 
    ''' </summary>
    Public Function CopyData() As RasterInteger

        Dim newRaster As New RasterInteger(Me.Linhas, Me.Colunas, Me.XllCorner, Me.YllCorner, Me.Cellsize, Me.NoDataValue)
        For row = 0 To newRaster.Linhas - 1
            For col = 0 To newRaster.Colunas - 1
                newRaster.Dados(row, col) = Me.Dados(row, col)
            Next
        Next

        Return newRaster
    End Function

    'Lê um raster e retorna dados em inteiro
    Private Sub GetASCData(ByVal ArquivoASC As String, ByVal onlyHeader As Boolean)

        Dim r As New System.Text.RegularExpressions.Regex(" +")
        Dim val() As String

        Using fs As New IO.FileStream(ArquivoASC, IO.FileMode.Open, IO.FileAccess.Read)
            Using str As New IO.StreamReader(fs) 'Abre o arquivo ASC para leitura

                val = r.Split(str.ReadLine) : _nCols = CInt(val(1)) 'Lê a linha do número de colunas
                val = r.Split(str.ReadLine) : _nRows = CInt(val(1)) 'Lê a linha do número de linhas
                val = r.Split(str.ReadLine) : _xllcorner = CDbl(val(1)) 'Lê a linha da coordenada inferior em X
                val = r.Split(str.ReadLine) : _yllcorner = CDbl(val(1)) 'Lê a linha da coordenada inferior em Y
                val = r.Split(str.ReadLine) : _cellsize = CDbl(val(1)) 'Lê a linha do cellsize
                val = r.Split(str.ReadLine) : _NODATAValue = CInt(val(1)) 'Lê a linha do NODATA

                If onlyHeader = True Then GoTo endReading 'Lê apenas as informações do cabeçalho, caso ativado

                ReDim _Data(_nRows - 1, _nCols - 1) 'Redimensiona a matriz de dados

                For i = 0 To _nRows - 1
                    val = r.Split(str.ReadLine)
                    For j = 0 To _nCols - 1
                        _Data(i, j) = CInt(val(j)) 'Lê o arquivo ASC e transforma em uma matriz de inteiros
                        If _Data(i, j) > _maxValue Then _maxValue = _Data(i, j)
                    Next
                Next
endReading:
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Lê o arquivo no formato Binary Terrain Data
    ''' </summary>
    Private Sub GetBinaryData(ByVal ArquivoBin As String, ByVal onlyHeader As Boolean)
        Using fs As New IO.FileStream(ArquivoBin, IO.FileMode.Open, IO.FileAccess.Read)
            Using br As New IO.BinaryReader(fs)

                'Lê o header
                _nCols = br.ReadInt32
                _nRows = br.ReadInt32
                _xllcorner = br.ReadDouble
                _yllcorner = br.ReadDouble
                _cellsize = br.ReadDouble
                _NODATAValue = br.ReadInt32

                If onlyHeader = True Then GoTo endReading 'Lê apenas as informações do cabeçalho, caso ativado

                'Redimensiona a matriz conforme numero de linhas e colunas
                ReDim _Data(_nRows - 1, _nCols - 1)

                For i = 0 To _nRows - 1
                    For j = 0 To _nCols - 1
                        _Data(i, j) = br.ReadInt16
                        If _Data(i, j) > _maxValue Then _maxValue = _Data(i, j)
                    Next
                Next
endReading:
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Lê os dados a partir de um arquivo extero
    ''' </summary>
    Public Sub ReadData(ByVal Filename As String, Optional ByVal GetOnlyHeader As Boolean = False)
        Dim extension As String = IO.Path.GetExtension(Filename)

        If UCase(extension) = ".ASC" Then 'Extensão .ASC
            GetASCData(Filename, GetOnlyHeader)
        ElseIf UCase(extension) = ".IRST" Then 'Extensão .IRST (IPH Raster)
            GetBinaryData(Filename, GetOnlyHeader)
        Else
            Throw New ArgumentException("O tipo de dados selecionado não é compatível!")
        End If
    End Sub
    ''' <summary>
    ''' Escreve os dados da matriz para um arquivo ASC especificado.
    ''' </summary>
    ''' <param name="ArquivoDestino"></param>
    Private Sub WriteASCData(ByVal ArquivoDestino As String)

        Using fs As New IO.FileStream(ArquivoDestino, IO.FileMode.Create, IO.FileAccess.Write)
            Using str As New IO.StreamWriter(fs) 'Abre o arquivo ASC para escrita

                'Escreve o cabeçalho
                str.WriteLine("ncols".PadRight(14) & _nCols)
                str.WriteLine("nrows".PadRight(14) & _nRows)
                str.WriteLine("xllcorner".PadRight(14) & _xllcorner)
                str.WriteLine("yllcorner".PadRight(14) & _yllcorner)
                str.WriteLine("cellsize".PadRight(14) & _cellsize)
                str.WriteLine("NODATA_value".PadRight(14) & _NODATAValue)

                Dim line As String = ""

                For j = 0 To _nRows - 1
                    For i = 0 To _nCols - 1
                        str.Write(CStr(_Data(j, i)) & " ")
                        'line += CStr(_Data(j, i)) & " " 'Escreve os dados separados por espaços simples
                    Next
                    str.Write(vbCrLf)
                    'str.WriteLine(line)
                    'line = "" 'Reseta a linha
                Next
            End Using
        End Using

    End Sub
    ''' <summary>
    ''' Escreve os dados na forma de arquivo binário
    ''' </summary>
    Private Sub WriteBinaryData(ByVal ArquivoDestino As String)
        Using fs As New IO.FileStream(ArquivoDestino, IO.FileMode.Create, IO.FileAccess.Write)
            Using bw As New IO.BinaryWriter(fs) 'Abre o arquivo binário para escrita

                bw.Write(_nCols)
                bw.Write(_nRows)
                bw.Write(_xllcorner)
                bw.Write(_yllcorner)
                bw.Write(_cellsize)
                bw.Write(_NODATAValue)

                For i = 0 To _nRows - 1
                    For j = 0 To _nCols - 1
                        bw.Write(_Data(i, j))
                    Next
                Next
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' Escreve os dados no disco
    ''' </summary>
    Public Sub WriteData(ByVal filename As String)
        Dim extension As String = IO.Path.GetExtension(filename)

        If UCase(extension) = ".ASC" Then 'Extensão .ASC
            WriteASCData(filename)
        ElseIf UCase(extension) = ".IRST" Then 'Extensão .IRST (IPH Raster)
            WriteBinaryData(filename)
        Else
            Throw New ArgumentException("O tipo de dados selecionado não é compatível!")
        End If
    End Sub
End Class

''' <summary>
''' Classe de Raster para trabalhar com dados reais
''' </summary>
Public Class RasterReal
    Inherits Raster
    Private _Data(,) As Single
    Private _maxValue As Single

    'Retorna os dados do raster
    Public ReadOnly Property Dados() As Single(,)
        Get
            Return _Data
        End Get
    End Property
    ''' <summary>
    ''' Retorna o máximo valor dos dados da matriz(y, x) do raster
    ''' </summary>
    Public ReadOnly Property MaxValue As Single
        Get
            Return _maxValue
        End Get
    End Property

    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Cria um raster a partir de informações indicadas pelo usuário
    ''' </summary>
    Public Sub New(ByVal numeroLinhas As Integer, ByVal numeroColunas As Integer, ByVal xllCorner As Double, ByVal yllcorner As Double, ByVal cellsize As Double, ByVal NoData As Integer)
        MyBase.new()
        ReDim _Data(numeroLinhas - 1, numeroColunas - 1)
        _nCols = numeroColunas
        _nRows = numeroLinhas
        _xllcorner = xllCorner
        _yllcorner = yllcorner
        _cellsize = cellsize
        _NODATAValue = NoData
    End Sub

    ''' <summary>
    ''' Faz uma copia completa dos dados de um raster 
    ''' </summary>
    Public Function CopyData() As RasterReal

        Dim newRaster As New RasterReal(Me.Linhas, Me.Colunas, Me.XllCorner, Me.YllCorner, Me.Cellsize, Me.NoDataValue)
        For row = 0 To newRaster.Linhas - 1
            For col = 0 To newRaster.Colunas - 1
                newRaster.Dados(row, col) = Me.Dados(row, col)
            Next
        Next

        Return newRaster
    End Function

    'Lê um raster e retorna dados em real
    Private Sub GetASCData(ByVal ArquivoASC As String, ByVal onlyHeader As Boolean)

        Dim r As New System.Text.RegularExpressions.Regex(" +")
        Dim val() As String

        Using fs As New IO.FileStream(ArquivoASC, IO.FileMode.Open, IO.FileAccess.Read)
            Using str As New IO.StreamReader(fs) 'Abre o arquivo ASC para leitura

                val = r.Split(str.ReadLine) : _nCols = CInt(val(1)) 'Lê a linha do número de colunas
                val = r.Split(str.ReadLine) : _nRows = CInt(val(1)) 'Lê a linha do número de linhas
                val = r.Split(str.ReadLine) : _xllcorner = CDbl(val(1)) 'Lê a linha da coordenada inferior em X
                val = r.Split(str.ReadLine) : _yllcorner = CDbl(val(1)) 'Lê a linha da coordenada inferior em Y
                val = r.Split(str.ReadLine) : _cellsize = CDbl(val(1)) 'Lê a linha do cellsize
                val = r.Split(str.ReadLine) : _NODATAValue = CSng(val(1)) 'Lê a linha do NODATA

                If onlyHeader = True Then GoTo endReading 'Lê apenas as informações do cabeçalho, caso ativado

                ReDim _Data(_nRows - 1, _nCols - 1) 'Redimensiona a matriz de dados

                For i = 0 To _nRows - 1
                    val = r.Split(str.ReadLine)
                    For j = 0 To _nCols - 1
                        _Data(i, j) = CSng(val(j)) 'Lê o arquivo ASC e transforma em uma matriz de inteiros
                        If _Data(i, j) > _maxValue Then _maxValue = _Data(i, j)
                    Next
                Next

endreading:
            End Using
        End Using

    End Sub
    ''' <summary>
    ''' Lê o arquivo no formato Binário
    ''' </summary>
    Public Sub GetBinaryData(ByVal ArquivoBin As String, ByVal onlyHeader As Boolean)
        Using fs As New IO.FileStream(ArquivoBin, IO.FileMode.Open, IO.FileAccess.Read)
            Using br As New IO.BinaryReader(fs)

                'Lê o header
                _nCols = br.ReadInt32
                _nRows = br.ReadInt32
                _xllcorner = br.ReadDouble
                _yllcorner = br.ReadDouble
                _cellsize = br.ReadDouble
                _NODATAValue = br.ReadInt32

                If onlyHeader = True Then GoTo endReading 'Lê apenas as informações do cabeçalho, caso ativado

                'Redimensiona a matriz conforme numero de linhas e colunas
                ReDim _Data(_nRows - 1, _nCols - 1)

                For i = 0 To _nRows - 1
                    For j = 0 To _nCols - 1
                        _Data(i, j) = br.ReadSingle
                        If _Data(i, j) > _maxValue Then _maxValue = _Data(i, j)
                    Next
                Next
endreading:
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' Lê os dados a partir de um arquivo extero
    ''' </summary>
    Public Sub ReadData(ByVal Filename As String, Optional ByVal getOnlyHeader As Boolean = False)
        Dim extension As String = IO.Path.GetExtension(Filename)

        If UCase(extension) = ".ASC" Then 'Extensão .ASC
            GetASCData(Filename, getOnlyHeader)
        ElseIf UCase(extension) = ".IRST" Then 'Extensão .IRST (IPH Raster)
            GetBinaryData(Filename, getOnlyHeader)
        Else
            Throw New ArgumentException("O tipo de dados selecionado não é compatível!")
        End If
    End Sub
    ''' <summary>
    ''' Escreve os dados da matriz para um arquivo ASC especificado.
    ''' </summary>
    ''' <param name="ArquivoDestino"></param>
    Private Sub WriteASCData(ByVal ArquivoDestino As String)

        Using fs As New IO.FileStream(ArquivoDestino, IO.FileMode.Create, IO.FileAccess.Write)
        Using str As New IO.StreamWriter(fs) 'Abre o arquivo ASC para escrita

            'Escreve o cabeçalho
            str.WriteLine("ncols".PadRight(14) & _nCols)
            str.WriteLine("nrows".PadRight(14) & _nRows)
            str.WriteLine("xllcorner".PadRight(14) & _xllcorner)
            str.WriteLine("yllcorner".PadRight(14) & _yllcorner)
            str.WriteLine("cellsize".PadRight(14) & _cellsize)
            str.WriteLine("NODATA_value".PadRight(14) & _NODATAValue)

            Dim line As String = ""

            For j = 0 To _nRows - 1
                For i = 0 To _nCols - 1
                    'line += CStr(_Data(j, i)) & " " 'Escreve os dados separados por espaços simples
                    str.Write(CStr(_Data(j, i)) & " ")
                Next
                'str.writeLine(line)
                str.Write(vbCrLf)
                'line = "" 'Reseta a linha
            Next
        End Using
        End Using
    End Sub

    'Escreve dados em formato binário
    Private Sub WriteBinaryData(ByVal ArquivoDestino As String)
        Using fs As New IO.FileStream(ArquivoDestino, IO.FileMode.Create, IO.FileAccess.Write)
            Using bw As New IO.BinaryWriter(fs) 'Abre o arquivo binário para escrita

                bw.Write(_nCols)
                bw.Write(_nRows)
                bw.Write(_xllcorner)
                bw.Write(_yllcorner)
                bw.Write(_cellsize)
                bw.Write(_NODATAValue)

                For i = 0 To _nRows - 1
                    For j = 0 To _nCols - 1
                        bw.Write(_Data(i, j))
                    Next
                Next
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' Escreve os dados no disco
    ''' </summary>
    Public Sub WriteData(ByVal filename As String)
        Dim extension As String = IO.Path.GetExtension(filename)

        If UCase(extension) = ".ASC" Then 'Extensão .ASC
            WriteASCData(filename)
        ElseIf UCase(extension) = ".IRST" Then 'Extensão .IRST (IPH Raster)
            WriteBinaryData(filename)
        Else
            Throw New ArgumentException("O tipo de dados selecionado não é compatível!")
        End If
    End Sub
End Class


