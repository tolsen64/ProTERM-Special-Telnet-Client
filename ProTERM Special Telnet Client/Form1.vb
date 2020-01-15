Imports System.IO

Public Class Form1

    Private pse As PSETerm

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        If My.Settings.UpgradeRequired Then
            My.Settings.Upgrade()
            My.Settings.Save()
        End If

        pse = New PSETerm(My.Settings.ForegroundColor, My.Settings.BackgroundColor, My.Settings.FontScale)
        pse.Dock = DockStyle.Fill
        Controls.Add(pse)
        pse.BringToFront()

        Select Case My.Settings.FontScale
            Case PSETerm.FontScale.Small
                Me.Width = 7 * 80 + 15
                Me.Height = 24 * 16 + 65
            Case PSETerm.FontScale.Medium
                Me.Width = 14 * 80 + 15
                Me.Height = 24 * 32 + 65
            Case PSETerm.FontScale.Large
                Me.Width = 21 * 80 + 15
                Me.Height = 24 * 48 + 65
        End Select
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        With New dlgFont(My.Settings.ForegroundColor, My.Settings.BackgroundColor, My.Settings.FontScale)
            If .ShowDialog(Me) = DialogResult.OK Then
                My.Settings.BackgroundColor = .BackgroundColor
                My.Settings.ForegroundColor = .ForegroundColor
                My.Settings.FontScale = .FontScale
                My.Settings.Save()
                MsgBox("Settings have been saved. You must restart the application.", MsgBoxStyle.Information)
            End If
        End With
    End Sub

    Private Sub btnEmulateFile_Click(sender As Object, e As EventArgs) Handles btnEmulateFile.Click
        With New OpenFileDialog
            .InitialDirectory = "C:\Users\Terry\Desktop\PSE"
            .Multiselect = True
            If .ShowDialog(Me) = DialogResult.OK Then
                For Each s As String In .FileNames
                    pse.AddText(File.ReadAllText(s))
                Next
            End If
        End With
    End Sub
End Class
