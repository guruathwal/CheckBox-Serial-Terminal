
Public Class aboutWindow

    Private Sub aboutWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        label_app_name.Content = My.Application.Info.AssemblyName
        label_app_version.Content = "Current Version: " & My.Application.Info.Version.ToString & " (Beta)"
        label_app_link.Content = My.Windows.MainWindow.applink

        label_desc.Inlines.Add(New Run("A simple Serial/UART terminal Monitor with support for escape character.") With {.FontWeight = FontWeights.Bold})
        label_desc.Inlines.Add(New Run(vbCrLf))
        label_desc.Inlines.Add(New Run(My.Application.Info.ProductName & " (c) by " & My.Application.Info.CompanyName) With {.FontStyle = FontStyles.Italic})
        label_desc.Inlines.Add(New Run(vbCrLf))
        label_desc.Inlines.Add(New Run(My.Application.Info.ProductName & " is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.") With {.FontStyle = FontStyles.Italic})
        label_desc.Inlines.Add(New Run("You should have received a copy of the license along with this work. If Not, see :"))
        label_desc.Inlines.Add(New Run(vbCrLf))
        label_desc.Inlines.Add(New Run("http://creativecommons.org/licenses/by-sa/4.0/") With {.Foreground = Brushes.Blue})
        label_github.Text = "If you enjoy using " & My.Application.Info.AssemblyName & ", please star our project on GitHub!"

    End Sub

    Private Sub label_app_link_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles label_app_link.MouseUp
        Process.Start(label_app_link.Content)
        Me.Close()
    End Sub

    Private Sub check_update_Click(sender As Object, e As RoutedEventArgs) Handles check_update.Click
        My.Windows.MainWindow.check_update(True)
    End Sub

    Private Sub label_desc_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles label_desc.MouseUp
        Process.Start("http://creativecommons.org/licenses/by-sa/4.0/")
    End Sub
End Class
