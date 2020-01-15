Public Class dlgFont

    Public Property FontScale As PSETerm.FontScale
    Public Property ForegroundColor As Color
    Public Property BackgroundColor As Color

    Public Sub New(ForegroundColor As Color, BackgroundColor As Color, FontScale As PSETerm.FontScale)
        InitializeComponent()
        cboFontSize.SelectedItem = FontScale.ToString
        cboFontSize.BackColor = BackgroundColor
        cboFontSize.ForeColor = ForegroundColor
        For Each col As KnownColor In [Enum].GetValues(GetType(KnownColor))
            If col.ToString <> "Transparent" AndAlso Not Color.FromKnownColor(col).IsSystemColor Then
                cboBackgroundColor.Items.Add(col.ToString)
                cboForegroundColor.Items.Add(col.ToString)
            End If
        Next
        cboForegroundColor.SelectedItem = ForegroundColor.Name
        cboBackgroundColor.SelectedItem = BackgroundColor.Name
        cboForegroundColor.BackColor = BackgroundColor
        cboForegroundColor.ForeColor = ForegroundColor
        cboBackgroundColor.BackColor = BackgroundColor
        cboBackgroundColor.ForeColor = ForegroundColor
        ForeColor = ForegroundColor
        BackColor = BackgroundColor
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub cboForegroundColor_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboForegroundColor.SelectedIndexChanged
        Dim c As Color = Color.FromName(cboForegroundColor.SelectedItem.ToString)
        cboFontSize.ForeColor = c
        cboForegroundColor.ForeColor = c
        cboBackgroundColor.ForeColor = c
        ForeColor = c
        ForegroundColor = c
    End Sub

    Private Sub cboBackgroundColor_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboBackgroundColor.SelectedIndexChanged
        Dim c As Color = Color.FromName(cboBackgroundColor.SelectedItem.ToString)
        cboFontSize.BackColor = c
        cboForegroundColor.BackColor = c
        cboBackgroundColor.BackColor = c
        BackColor = c
        BackgroundColor = c
    End Sub

    Private Sub cboFontSize_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFontSize.SelectedIndexChanged
        FontScale = DirectCast([Enum].Parse(GetType(PSETerm.FontScale), cboFontSize.SelectedItem.ToString), PSETerm.FontScale)
    End Sub
End Class
