Imports System.Drawing.Imaging

Public Class PSETerm

    Private CharSetNormal(126) As Bitmap
    Private CharSetInverse(94) As Bitmap
    Private CharSet(126) As Bitmap
    Private CharSize As Size
    Private memBmp As Bitmap  ' Backbuffers bitmap
    Private hMemBmp As IntPtr ' Handle to our memBmp
    Private memDC As Graphics ' We draw on this
    Private hMemdc As IntPtr  ' Handle to our memDC
    Private charQueue As New Queue(1024)
    Private theCaret As Caret
    Private termChar(79, 23) As TerminalCharacter
    Private termSpace As TerminalCharacter
    Private cursorChar As TerminalCharacter
    Private termInChar As TerminalCharacter
    Private theCharMode As CharMode

    Public Enum FontScale
        Small
        Medium
        Large
    End Enum

    Private Structure Caret
        Dim row As Integer
        Dim col As Integer
    End Structure

    Private Enum CharMode
        Normal
        Inverse
        MouseText
    End Enum

    Private Structure TerminalCharacter
        Dim img As Bitmap
        Dim [Character] As Char
        Dim AscVal As Integer
        Dim charMode As CharMode
    End Structure

    Public Sub New(ForegroundColor As Color, BackgroundColor As Color, FontScale As FontScale)
        InitializeComponent()
        Debug.WriteLine("============================== New PSE Term ==============================")
        SetNormalText()
        SetFont(ForegroundColor, BackgroundColor, FontScale)
        memBmp = New Bitmap(CharSize.Width * 80, CharSize.Height * 24)
        Dim g As Graphics = pbTerm.CreateGraphics
        Dim hdc = g.GetHdc
        hMemdc = CreateCompatibleDC(hdc)
        hMemBmp = memBmp.GetHbitmap
        SelectObject(hMemdc, hMemBmp)
        memDC = Graphics.FromHdc(hMemdc)
        memDC.Clear(BackgroundColor)
        g.ReleaseHdc()
        g.Dispose()
        HandleQueue()
        Focus() 'give myself input focus in order to capture keypresses
    End Sub

    'Make these public once we figure out how to change colors without clearing all the text.
    'Private Sub SetFont()
    '    With New dlgFont
    '        If .ShowDialog = DialogResult.OK Then
    '            SetFont(.ForegroundColor, .BackgroundColor, .FontScale)
    '        End If
    '    End With
    'End Sub
    Private Sub SetFont(ForegroundColor As Color, BackGroundColor As Color, FontScale As FontScale)
        Select Case FontScale
            Case FontScale.Small : CharSize.Width = 7 : CharSize.Height = 16
            Case FontScale.Medium : CharSize.Width = 14 : CharSize.Height = 32
            Case FontScale.Large : CharSize.Width = 21 : CharSize.Height = 48
        End Select
        CreateCharacterBitmaps(ForegroundColor, BackGroundColor, FontScale)
        termSpace.AscVal = 32
        termSpace.Character = " "c
        termSpace.img = CharSetNormal(0)
        cursorChar.AscVal = 95
        cursorChar.Character = "_"c
        cursorChar.img = CharSetNormal(95 - 32)
        pbTerm.BackColor = BackGroundColor
        UpdateScreen()
    End Sub

    Public Sub AddText(ByVal txtToAdd As String)
        For Each c As Char In txtToAdd
            charQueue.Enqueue(c)
        Next
    End Sub

    Private Async Sub HandleQueue()
        Await Task.Run(Sub()
                           Dim c As Char
                           Do
                               Do Until charQueue.Count = 0
                                   c = charQueue.Dequeue
                                   Debug.WriteLine("InChar: " & c & ", Asc: " & Asc(c))
                                   Select Case Asc(c)
                                       Case 1 : Debug.WriteLine("theCaret.col = 0") : theCaret.col = 0
                                       Case 2 : Debug.WriteLine("theCaret.col = 79") : theCaret.col = 78
                                       Case 4 : Debug.WriteLine("CursorDelete") : CursorDelete()
                                       Case 6 : Debug.WriteLine("CursorInsert") : CursorInsert()
                                       Case 8 : Debug.WriteLine("CursorLeft") : CursorLeft()
                                       Case 9 : Debug.WriteLine("CursorTab") : CursorTab()
                                       Case 10 : Debug.WriteLine("CursorDown") : CursorDown()
                                       Case 11 : Debug.WriteLine("CursorUp") : CursorUp()
                                       Case 12 : Debug.WriteLine("ClearScreen") : ClearScreen()
                                       Case 13 : Debug.WriteLine("NewLine") : NewLine()
                                       Case 14 : Debug.WriteLine("SetNormalText") : SetNormalText()
                                       Case 15 : Debug.WriteLine("SetInverseText") : SetInverseText()
                                       Case 16 : Debug.WriteLine("SetMouseText") : SetMouseText()
                                       Case 18 : Debug.WriteLine("RepeatChar") : RepeatChar()
                                       Case 21 : Debug.WriteLine("CursorRight") : CursorRight()
                                       Case 22 : Debug.WriteLine("LineInsert") : LineInsert()
                                       Case 23 : Debug.WriteLine("DeleteCursorToEnd") : DeleteCursorToEnd()
                                       Case 24 : Debug.WriteLine("theCaret.col = 0 : theCaret.row = 0") : theCaret.col = 0 : theCaret.row = 0
                                       Case 25 : Debug.WriteLine("DeleteCursorToEOL") : DeleteCursorToEOL()
                                       Case 26 : Debug.WriteLine("LineDelete") : LineDelete()
                                       Case 27 : Debug.WriteLine("Escape Char (nothing to do)")
                                       Case 30 : Debug.WriteLine("PositionCaret") : PositionCaret()
                                       Case Else : Debug.WriteLine("StoreChar(""" & c & """)") : StoreChar(c)
                                   End Select
                                   UpdateScreen()
                               Loop
                               Threading.Thread.Sleep(200)
                               'WriteCursor(theCaret.col, theCaret.row)
                               'UpdateScreen()
                           Loop
                       End Sub)
    End Sub

    Private Sub PositionCaret()
        Do Until charQueue.Count > 0
            Threading.Thread.Sleep(100)
        Loop
        theCaret.col = Asc(charQueue.Dequeue) - 32
        Do Until charQueue.Count > 0
            Threading.Thread.Sleep(100)
        Loop
        theCaret.row = Asc(charQueue.Dequeue) - 32
        If theCaret.row > 23 Then theCaret.row = 23
        If theCaret.row < 0 Then theCaret.row = 0
        If theCaret.col > 79 Then theCaret.col = 79
        If theCaret.col < 0 Then theCaret.col = 0
        Debug.WriteLine("theCaret.col = " & theCaret.col & "; theCaret.row = " & theCaret.row)
    End Sub

    Private Sub LineDelete()
        For j = theCaret.row To 22
            For i = 0 To 79
                termChar(i, j) = termChar(i, j + 1)
                WriteChar(i, j)
            Next
        Next
        For i = 0 To 79
            termChar(i, 23) = termSpace
            WriteChar(i, 23)
        Next
    End Sub

    Private Sub DeleteCursorToEOL()
        For i As Integer = theCaret.col To 79
            termChar(i, theCaret.row) = termSpace
            WriteChar(i, theCaret.row)
        Next
    End Sub

    Private Sub DeleteCursorToEnd()
        DeleteCursorToEOL()
        For i = 0 To 79
            For j = 0 To 23
                termChar(i, j) = termSpace
                WriteChar(i, j)
            Next
        Next
    End Sub

    Private Sub LineInsert()    'Push lines down
        For j = 22 To theCaret.row Step -1
            For i = 0 To 79
                termChar(i, j + 1) = termChar(i, j)
                WriteChar(i, j + 1)
            Next
        Next
        For i = 0 To 79
            termChar(i, theCaret.row) = termSpace
            WriteChar(i, theCaret.row)
        Next
    End Sub

    Private Sub CursorRight()
        If theCaret.col < 79 Then theCaret.col += 1
        Debug.WriteLine("theCaret.col = " & theCaret.col)
    End Sub

    Private Sub RepeatChar()
        Do Until charQueue.Count > 0
            Threading.Thread.Sleep(100)
        Loop
        Dim charToRepeat As Char = charQueue.Dequeue
        Debug.WriteLine("charToRepeat = " & charToRepeat)
        Do Until charQueue.Count > 0
            Threading.Thread.Sleep(100)
        Loop
        Dim numTimes As Integer = Asc(charQueue.Dequeue)
        Debug.WriteLine("numTimes = " & numTimes)
        For i As Integer = 1 To numTimes
            StoreChar(charToRepeat)
        Next
    End Sub

    Public Sub SetMouseText()
        theCharMode = CharMode.MouseText
        CharSet = CharSetNormal
    End Sub

    Public Sub SetInverseText()
        theCharMode = CharMode.Inverse
        CharSet = CharSetInverse
    End Sub

    Public Sub SetNormalText()
        theCharMode = CharMode.Normal
        CharSet = CharSetNormal
    End Sub

    Private Sub NewLine()
        theCaret.col = 0
        If theCaret.row < 23 Then
            theCaret.row += 1
        Else
            DoScroll()
        End If
    End Sub

    Private Sub ClearScreen()
        For i = 0 To 79
            For j = 0 To 23
                termChar(i, j) = termSpace
                WriteChar(i, j)
            Next
        Next
        theCaret.col = 0
        theCaret.row = 0
    End Sub

    Private Sub CursorUp()
        If theCaret.row > 0 Then theCaret.row -= 1
    End Sub

    Private Sub CursorDown()
        If theCaret.row < 23 Then theCaret.row += 1
    End Sub

    Private Sub CursorTab()
        Select Case theCaret.col
            Case Is < 7 : theCaret.col = 7
            Case Is < 15 : theCaret.col = 15
            Case Is < 23 : theCaret.col = 23
            Case Is < 31 : theCaret.col = 31
            Case Is < 39 : theCaret.col = 39
            Case Is < 47 : theCaret.col = 47
            Case Is < 55 : theCaret.col = 55
            Case Is < 63 : theCaret.col = 63
            Case Is < 71 : theCaret.col = 71
            Case Is < 79 : theCaret.col = 79
        End Select
        Debug.WriteLine("theCaret.col = " & theCaret.col)
    End Sub

    Private Sub CursorLeft()
        If theCaret.col > 0 Then theCaret.col -= 1
    End Sub

    Private Sub CursorInsert()
        For i As Integer = 79 To theCaret.col + 1 Step -1
            termChar(i, theCaret.row) = termChar(i - 1, theCaret.row)
            WriteChar(i, theCaret.row)
        Next
        termChar(theCaret.col, theCaret.row) = termSpace
        WriteChar(theCaret.col, theCaret.row)
    End Sub

    Private Sub CursorDelete()
        For i As Integer = theCaret.col + 1 To 79
            termChar(i - 1, theCaret.row) = termChar(i, theCaret.row)
            WriteChar(i - 1, theCaret.row)
        Next
        With termChar(79, theCaret.row)

        End With
        termChar(79, theCaret.row) = termSpace
        WriteChar(79, theCaret.row)
    End Sub

    Private Sub DoScroll()
        For j = 1 To 23
            For i = 0 To 79
                termChar(i, j - 1) = termChar(i, j)
                WriteChar(i, j - 1)
            Next
        Next
        For i = 0 To 79
            termChar(i, 23) = termSpace
            WriteChar(i, 23)
        Next
        UpdateScreen()
    End Sub

    Private Sub WriteCursor(col As Integer, row As Integer)
        Dim rect As New Rectangle(col * CharSize.Width, row * CharSize.Height, CharSize.Width, CharSize.Height)
        memDC.DrawImage(cursorChar.img, rect, 0, 0, CharSize.Width, CharSize.Height, GraphicsUnit.Pixel)
    End Sub

    Private Sub WriteChar(col As Integer, row As Integer)
        'If termChar(col, row).AscVal >= 32 AndAlso termChar(col, row).img IsNot Nothing Then
        Dim rect As New Rectangle(col * CharSize.Width, row * CharSize.Height, CharSize.Width, CharSize.Height)
            memDC.DrawImage(termChar(col, row).img, rect, 0, 0, CharSize.Width, CharSize.Height, GraphicsUnit.Pixel)
        'End If
    End Sub

    Private Sub StoreChar(ByVal c As Char)
        With termInChar
            .AscVal = Asc(c)
            .Character = c
            If theCharMode = CharMode.MouseText Then
                If .AscVal >= 64 AndAlso .AscVal <= 95 Then
                    .img = CharSet(.AscVal - 32 + 63)
                ElseIf .AscVal = 63
                    .img = CharSet(95 - 32)
                ElseIf .AscVal >= 32
                    .img = CharSet(.AscVal - 32)
                End If
            ElseIf .AscVal >= 32
                .img = CharSet(.AscVal - 32)
            End If
        End With
        termChar(theCaret.col, theCaret.row) = termInChar
        WriteChar(theCaret.col, theCaret.row)
        AdvanceCaret()
    End Sub

    Private Sub AdvanceCaret()
        If theCaret.col < 79 Then
            theCaret.col += 1
        Else
            theCaret.col = 0
            If theCaret.row < 23 Then
                theCaret.row += 1
            Else
                DoScroll()
            End If
        End If
    End Sub

    Private Sub pbTerm_Paint(sender As Object, e As PaintEventArgs) Handles pbTerm.Paint
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.SystemDefault
        e.Graphics.TextContrast = 0
        e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half

        Dim hdc As IntPtr = e.Graphics.GetHdc
        BitBlt(hdc, e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height, hMemdc, e.ClipRectangle.X, e.ClipRectangle.Y, &HCC0020)
        e.Graphics.ReleaseHdc(hdc)
    End Sub

    Private Sub UpdateScreen()
        UpdateScreen(0, 0, pbTerm.Width, pbTerm.Height)
    End Sub
    Private Sub UpdateScreen(ByVal col As Integer, ByVal row As Integer, ByVal width As Integer, ByVal height As Integer)
        If pbTerm.IsDisposed Then Exit Sub
        Dim pea As New PaintEventArgs(pbTerm.CreateGraphics, New Rectangle(col, row, width, height))
        pbTerm_Paint(pbTerm, pea)
    End Sub

    Private Sub CreateCharacterBitmaps(ForegroundColor As Color, BackgroundColor As Color, FontScale As FontScale)
        Dim CharMapTable As Bitmap = GetFontTable(ForegroundColor, BackgroundColor, FontScale)

        For i As Integer = 0 To 31  ' !"#$%&`()*+,-./0123456789:;<=>?
            CharSetNormal(i) = New Bitmap(CharSize.Width, CharSize.Height)
            CharSetInverse(i) = New Bitmap(CharSize.Width, CharSize.Height)
            Dim grN As Graphics = Graphics.FromImage(CharSetNormal(i))
            Dim grI As Graphics = Graphics.FromImage(CharSetInverse(i))
            Dim rectN As New Rectangle(i * CharSize.Width, 0, CharSize.Width, CharSize.Height)
            Dim rectI As New Rectangle(i * CharSize.Width, CharSize.Height, CharSize.Width, CharSize.Height)
            grN.DrawImage(CharMapTable, 0, 0, rectN, GraphicsUnit.Pixel)
            grI.DrawImage(CharMapTable, 0, 0, rectI, GraphicsUnit.Pixel)
        Next
        For i As Integer = 32 To 94 '@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_'abcdefghijklmnopqrstuvwxyz{|}~
            CharSetNormal(i) = New Bitmap(CharSize.Width, CharSize.Height)
            CharSetInverse(i) = New Bitmap(CharSize.Width, CharSize.Height)
            Dim grN As Graphics = Graphics.FromImage(CharSetNormal(i))
            Dim grI As Graphics = Graphics.FromImage(CharSetInverse(i))
            Dim rectN As New Rectangle((i - 32) * CharSize.Width, CharSize.Height * 2, CharSize.Width, CharSize.Height)
            Dim rectI As New Rectangle((i - 32) * CharSize.Width, CharSize.Height * 3, CharSize.Width, CharSize.Height)
            grN.DrawImage(CharMapTable, 0, 0, rectN, GraphicsUnit.Pixel)
            grI.DrawImage(CharMapTable, 0, 0, rectI, GraphicsUnit.Pixel)
        Next
        For i As Integer = 95 To 126    'Mousetext characters.
            CharSetNormal(i) = New Bitmap(CharSize.Width, CharSize.Height)
            Dim grN As Graphics = Graphics.FromImage(CharSetNormal(i))
            Dim rectN As New Rectangle((i - 95) * CharSize.Width, CharSize.Height * 4, CharSize.Width, CharSize.Height)
            grN.DrawImage(CharMapTable, 0, 0, rectN, GraphicsUnit.Pixel)
        Next
    End Sub

    Private Function GetFontTable(ForegroundColor As Color, BackgroundColor As Color, FontScale As FontScale) As Bitmap
        Dim cmForeground As New ColorMap With {.OldColor = Color.White, .NewColor = ForegroundColor}
        Dim cmBackground As New ColorMap With {.OldColor = Color.Black, .NewColor = BackgroundColor}
        Dim remap_table As ColorMap() = {cmForeground, cmBackground}
        Dim image_attr As New ImageAttributes
        image_attr.SetRemapTable(remap_table, ColorAdjustType.Bitmap)
        Dim bm_src As Bitmap = My.Resources.A2Small
        Select Case FontScale
            Case FontScale.Medium : bm_src = My.Resources.A2Medium
            Case FontScale.Large : bm_src = My.Resources.A2Large
        End Select
        Dim bm_new As New Bitmap(bm_src.Width, bm_src.Height)
        Dim gr As Graphics = Graphics.FromImage(bm_new)
        Dim rect As Rectangle = Rectangle.Round(bm_src.GetBounds(GraphicsUnit.Pixel))
        gr.DrawImage(bm_src, rect, 0, 0, bm_new.Width, bm_new.Height, GraphicsUnit.Pixel, image_attr)
        GetFontTable = bm_new
        gr.Dispose()
        bm_src.Dispose()
        image_attr.Dispose()
    End Function

    Private Sub PSETerm_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        AddText(e.KeyChar)
    End Sub

    Private Sub PSETerm_Click(sender As Object, e As EventArgs) Handles Me.Click, pbTerm.Click
        Focus()
    End Sub

End Class
