
Public Class aboutWindow

    Private Sub aboutWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        label_app_name.Content = My.Application.Info.AssemblyName
        label_app_version.Content = "Current Version: " & My.Application.Info.Version.ToString & " (Beta)"
        label_app_link.Content = My.Windows.MainWindow.applink

        label_desc.Text = "A simple Serial/UART terminal Monitor with support for escape character (like '\n',\'r')."
        label_github.Text = "If you enjoy using " & My.Application.Info.AssemblyName & ", please star our project on GitHub!"
        My.Windows.MainWindow.check_update()
    End Sub

    Private Sub label_app_link_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles label_app_link.MouseUp
        Process.Start(label_app_link.Content)
        Me.Close()
    End Sub
End Class
