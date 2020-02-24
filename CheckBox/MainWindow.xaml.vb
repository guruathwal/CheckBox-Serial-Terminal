Imports System.IO
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.Drawing
Imports System.Text
Imports System.Net

Class MainWindow

    Dim finishloading = False
    Dim Serial_Port1 As New System.IO.Ports.SerialPort

    Dim last_text_from_port As Boolean = False
    Dim resetting As Boolean = False

    Dim DataBits As Array = {5, 6, 7, 8}
    Dim maxLines = 100

    'for font styles
    Dim installedFonts As New Text.InstalledFontCollection
    Dim fontFamilies() As FontFamily = installedFonts.Families()

    Delegate Sub myMethodDelegate(ByVal [text] As String)
    Dim myD1 As New myMethodDelegate(AddressOf myShowStringMethod1)


    'Strings
    Dim string_not_connected = "Serial Port is Not connected."
    Dim string_data_notsent = "Unable to send data to Serial Port."

    Dim AplicationFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CheckBox\"
    Public applink As String = "https://github.com/guruathwal/CheckBox-Serial-Terminal"
    Public versionlink As String = "https://raw.githubusercontent.com/guruathwal/CheckBox-Serial-Terminal/master/CheckBox/Release-version/version.txt"

    Private Sub setToolTips()
        combo_ports.ToolTip = "Select a Serial Port"
        Combospeed1.ToolTip = "Set Baudrate"
        btn_refreshport.ToolTip = "Rescan Serial Ports"
        btn_connect.ToolTip = "Connect"
        btn_reset.ToolTip = "Reset setting"
        btn_send.ToolTip = "Send text to serial port"
    End Sub

    ''' <summary>
    '''build text String For sending To serial port
    ''' </summary>
    Private Function buildString() As String
        Dim tempstr As New StringBuilder

        If My.Settings.escapeSequence Then
            tempstr.Append(Regex.Unescape(txt_send.Text))
        End If

        'avoid duplicate line ending an carriage return charaters
        If check_cr.IsChecked And check_lf.IsChecked And tempstr.ToString.EndsWith(vbCrLf) Then
            Return tempstr.ToString
        Else
            If My.Settings.sendCR And tempstr.Chars(tempstr.Length - 1) <> vbCr Then
                tempstr.Append(vbCr)
            End If
            If My.Settings.sendLF And tempstr.Chars(tempstr.Length - 1) <> vbLf Then
                tempstr.Append(vbLf)
            End If
        End If

        Return tempstr.ToString

    End Function

    ''' <summary>
    ''' Refresh available serial ports
    ''' </summary>
    Private Sub RefreshPorts()
        If Serial_Port1.IsOpen Then Return

        combo_ports.Items.Clear()

        For Each sp As String In My.Computer.Ports.SerialPortNames
            combo_ports.Items.Add(sp)
        Next

        If combo_ports.Items.Count > 0 Then
            combo_ports.SelectedIndex = 0
            Serial_Port1.PortName = combo_ports.SelectedItem
        End If


    End Sub

    ''' <summary>
    ''' update buttons and status based on serial port status
    ''' </summary>
    Private Sub UpdateConnectionStatus()
        If Serial_Port1.IsOpen Then
            btn_connect.Content = "" 'disconnect icon
            btn_send.IsEnabled = True
            combo_ports.IsEnabled = False
        Else
            btn_connect.Content = "" 'conncect icon
            btn_send.IsEnabled = False
            combo_ports.IsEnabled = True
        End If

    End Sub

    ''' <summary>
    ''' close serial port when closing
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Serial_Port1.IsOpen Then
            Serial_Port1.Close()
        End If
    End Sub


#Region "settings"
    ''' <summary>
    ''' reset settings to default
    ''' </summary>
    Private Sub Reset_settings()
        With My.Settings
            .firsttime = False
            .databit = 3               'index number
            .flowControl = 0           'index number
            .stopbit = 1               'index number
            .parity = 0                'index number
            .handshake = 0             'index number
            .s1speed = 4               'index number
            '.escapeSequence = True
            .localEcho = True
            .wordWrap = False
            .fontName = "Courier New"
            .fontSize = 5              'index number
            .sendLF = True
            .sendCR = False
            .autoScroll = True
            .maxLines = maxLines
        End With
        My.Settings.Save() 'save settings
    End Sub

    ''' <summary>
    ''' Save setting
    ''' </summary>
    Private Sub Save_settings()
        With My.Settings
            .firsttime = False
            .databit = combo_databit.SelectedIndex                'index number
            .flowControl = combo_flowcontrol.SelectedIndex        'index number
            .stopbit = combo_stopbit.SelectedIndex + 1            'index number
            .parity = combo_parity.SelectedIndex                  'index number
            .handshake = combo_handshake.SelectedIndex            'index number
            .s1speed = Combospeed1.SelectedIndex                  'index number
            '.escapeSequence = check_escape.IsChecked
            .localEcho = check_localEcho.IsChecked
            .wordWrap = check_wordWrap.IsChecked
            .fontName = combo_fontFamily.SelectedItem
            .fontSize = combo_fontSize.SelectedIndex              'index number
            .sendLF = check_lf.IsChecked
            .sendCR = check_cr.IsChecked
            .autoScroll = check_autoScroll.IsChecked
            .maxLines = txt_maxlines.Text
        End With

        update_serialSettings()

        My.Settings.Save() 'save settings
    End Sub

    ''' <summary>
    ''' load setting values in controls and serial port
    ''' </summary>
    Private Sub Load_settings()

        'show setting

        combo_databit.SelectedIndex = My.Settings.databit
        combo_flowcontrol.SelectedIndex = My.Settings.flowControl
        combo_stopbit.SelectedIndex = My.Settings.stopbit - 1
        combo_parity.SelectedIndex = My.Settings.parity
        combo_handshake.SelectedIndex = My.Settings.handshake
        Combospeed1.SelectedIndex = My.Settings.s1speed
        check_localEcho.IsChecked = My.Settings.localEcho
        check_wordWrap.IsChecked = My.Settings.wordWrap
        'check_escape.IsChecked = My.Settings.escapeSequence
        combo_fontSize.SelectedIndex = My.Settings.fontSize
        check_lf.IsChecked = My.Settings.sendLF
        check_cr.IsChecked = My.Settings.sendCR
        check_autoScroll.IsChecked = My.Settings.autoScroll
        txt_maxlines.Text = My.Settings.maxLines
        'check if font name exists
        If combo_fontFamily.Items.Contains(My.Settings.fontName) = False Then
            My.Settings.fontName = "Courier New"
            My.Settings.Save()
        End If
        combo_fontFamily.SelectedItem = My.Settings.fontName

        ' load settings
        update_textSetting()
        update_serialSettings()

    End Sub

    ''' <summary>
    ''' update text setting for terminal
    ''' </summary>
    Private Sub update_textSetting()
        Dim fnt As Windows.Media.FontFamily = New Windows.Media.FontFamily(My.Settings.fontName)
        txt_terminal.FontFamily = fnt
        txt_terminal.FontSize = combo_fontSize.Items(My.Settings.fontSize)
        txt_send.FontFamily = fnt
        txt_send.FontSize = combo_fontSize.Items(My.Settings.fontSize)
    End Sub

    ''' <summary>
    ''' load serial port setting
    ''' </summary>
    Private Sub update_serialSettings()
        With Serial_Port1
            .DataBits = DataBits(My.Settings.databit)
            .StopBits = My.Settings.stopbit             'index based
            .Parity = My.Settings.parity                'index based
            .Handshake = My.Settings.handshake          'index based
            .BaudRate = Combospeed1.Items.Item(My.Settings.s1speed)
            'set flow control
            Select Case My.Settings.flowControl
                Case 0
                    .DtrEnable = False
                    .RtsEnable = False
                Case 1
                    .DtrEnable = True
                    .RtsEnable = False
                Case 2
                    .DtrEnable = False
                    .RtsEnable = True
            End Select
        End With


    End Sub

    ''' <summary>
    ''' prepare controls and load settings
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        setToolTips()
        RefreshPorts()
        With Combospeed1.Items
            .Add(300)
            .Add(1200)
            .Add(2400)
            .Add(4800)
            .Add(9600)
            .Add(19200)
            .Add(38400)
            .Add(57600)
            .Add(74880)
            .Add(115200)
            .Add(230400)
            .Add(250000)
            .Add(500000)
            .Add(1000000)
            .Add(2000000)
        End With

        With combo_fontSize.Items
            '.Add(5)
            '.Add(6)
            '.Add(7)
            .Add(8)
            .Add(9)
            .Add(10)
            .Add(11)
            .Add(12)
            .Add(14)
            .Add(16)
            .Add(18)
            '.Add(20)
            '.Add(22)
            '.Add(24)
            '.Add(26)
            '.Add(28)
            '.Add(30)
            '.Add(32)
            '.Add(34)
        End With

        With combo_flowcontrol.Items 'Do not change the order
            .Add("NONE")
            .Add("DTR/DSR")
            .Add("RTS/CSR")
        End With

        With combo_databit.Items 'Do not change the order
            .Add("5")
            .Add("6")
            .Add("7")
            .Add("8")
        End With

        With combo_stopbit.Items 'Do not change the order
            .Add("1")
            .Add("2")
            .Add("1.5")
        End With

        With combo_parity.Items 'Do not change the order
            .Add("NONE")
            .Add("ODD")
            .Add("EVEN")
            .Add("MARK")
            .Add("SPACE")
        End With

        With combo_handshake.Items 'Do not change the order
            .Add("NONE")
            .Add("XONXOFF")
            .Add("RTS/CTS")
            .Add("RTS/XONXOFF")
        End With

        'load installed fonts
        For Each font_family As FontFamily In fontFamilies
            combo_fontFamily.Items.Add(font_family.Name)
        Next


        'reset settings on first run
        If My.Settings.firsttime Then
            Dim dr As Window = New aboutWindow
            dr.ShowDialog()
            Reset_settings()
        End If

        'load setting
        Load_settings()

        AddHandler Serial_Port1.DataReceived, AddressOf SerialPort1_DataReceived
        AddHandler combo_flowcontrol.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_databit.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_fontFamily.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_fontSize.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_handshake.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_parity.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_stopbit.SelectionChanged, AddressOf combo_settings_SelectionChanged
        AddHandler combo_stopbit.SelectionChanged, AddressOf combo_settings_SelectionChanged


        AddHandler check_wordWrap.Checked, AddressOf check_settings_Checked
        AddHandler check_cr.Checked, AddressOf check_settings_Checked
        'AddHandler check_escape.Checked, AddressOf check_settings_Checked
        AddHandler check_lf.Checked, AddressOf check_settings_Checked
        AddHandler check_localEcho.Checked, AddressOf check_settings_Checked
        AddHandler check_autoScroll.Checked, AddressOf check_settings_Checked

        finishloading = True

        UpdateMe()

    End Sub

    'update serial port settings
    Private Sub combo_ports_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles combo_ports.SelectionChanged,
                                                                                                        Combospeed1.SelectionChanged
        If finishloading = False Or resetting Then Return
        If combo_ports.Items.Count = 0 Then Return

        My.Settings.s1speed = Combospeed1.SelectedIndex
        Serial_Port1.BaudRate = My.Settings.s1speed
        If Not Serial_Port1.IsOpen Then
            Serial_Port1.PortName = combo_ports.SelectedValue
        End If
        My.Settings.Save()

        update_textSetting()
        update_serialSettings()

    End Sub

    Private Sub btn_refreshport_Click(sender As Object, e As RoutedEventArgs) Handles btn_refreshport.Click
        RefreshPorts()
    End Sub

    'save setting on combobox option change
    Private Sub combo_settings_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        If finishloading = False Or resetting Then Return
        If sender.selectedindex < 0 Then Return
        Save_settings()

        update_textSetting()
        update_serialSettings()
    End Sub

    'save setting on checkbox option toggle
    Private Sub check_settings_Checked(sender As Object, e As RoutedEventArgs)

        If finishloading = False Or resetting Then Return
        Save_settings()

    End Sub

    Private Sub btn_reset_Click(sender As Object, e As RoutedEventArgs) Handles btn_reset.Click
        resetting = True
        Reset_settings()
        Load_settings()
        resetting = False
    End Sub

    Private Sub UpdateMe()

        If My.Computer.Network.Ping("8.8.8.8") Then
            If DateDiff(DateInterval.Day, My.Settings.updateDATE, Date.Now) > 1 Then

                If My.Settings.updateCheck = False Then
                    check_update()
                    My.Settings.updateDATE = Date.Now
                    My.Settings.updateCheck = True
                    My.Settings.Save()
                End If

            End If

        End If

    End Sub

    Public Sub check_update(Optional showprompt As Boolean = False)
        Try
            If My.Computer.Network.Ping("8.8.8.8") Then

                Dim instance As New WebClient

                'download update file
                Dim fname As String = "checkbox_version.txt"
                instance.DownloadFile(versionlink, Path.GetTempPath & fname)

                'get update version
                Dim ver As String = ""
                Dim fs As New FileStream(Path.GetTempPath & fname, FileMode.Open, FileAccess.Read)
                Dim d As New StreamReader(fs)
                d.BaseStream.Seek(0, SeekOrigin.Begin)

                While d.Peek() > -1
                    ver &= d.ReadLine()
                End While

                Dim i As Integer = ver.IndexOf("|", 0)
                Dim vno As String = ver.Substring(0, i)
                Dim vurl As String = ver.Substring(i + 1, ver.Length - i - 1)
                'check version
                Dim vno_num = Replace(vno, ".", "")  'New Version

                Dim currentVersion = My.Application.Info.Version.Major & My.Application.Info.Version.Minor & My.Application.Info.Version.Build & My.Application.Info.Version.MinorRevision  'Build current Version to variable
                'MsgBox(currentVersion)
                If Val(currentVersion) >= Val(vno_num) Then
                    If showprompt Then
                        MsgBox("You already have the latest version.")
                    End If
                Else
                    If MsgBox("A New Version (" & vno & ") is Available. Do you want to Download it?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        Process.Start(vurl)
                    End If
                End If
                My.Settings.updateDATE = Date.Now
                My.Settings.updateCheck = True
                My.Settings.Save()
                fs.Close()
                instance.Dispose()
            Else
                MsgBox("No internet connection detected. make sure you are connected to internet.", MsgBoxStyle.Exclamation, "Unable to Check for Updates")
            End If
        Catch ex As Exception
            If showprompt Then
                MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Unable to Check for Updates")
            End If
        End Try
    End Sub
#End Region


#Region "Serial comm"
    ''' <summary>
    ''' send text string to serial port
    ''' </summary>
    Private Sub send_text()
        If txt_send.Text.Length = 0 Then Return

        If Serial_Port1.IsOpen Then
            Serial_Port1.Write(buildString)
            If last_text_from_port Then
                If Not txt_terminal.Text.EndsWith(vbLf) Then
                    txt_terminal.AppendText(vbLf)
                End If
                last_text_from_port = False

            End If
            txt_terminal.AppendText("▲ " & buildString())
                clear_lines()
                txt_send.Clear()
                txt_send.Focus()
            Else
                MsgBox(string_data_notsent, MsgBoxStyle.Exclamation, string_not_connected)
            UpdateConnectionStatus()
        End If
    End Sub

    Private Sub clear_lines()
        'remove old lines if lines counts exceed maximum
        With txt_terminal

            If .LineCount > My.Settings.maxLines + 1 Then

                'Dim tempsrt As String = txt_terminal.Text

                Do While .LineCount > My.Settings.maxLines + 1
                    Dim n As Integer = 0
                    If .Text.StartsWith(vbLf) Then
                        n = 1
                    End If
                    .Text = .Text.Substring(.Text.IndexOf(vbLf, n) + 1)
                Loop

            End If
        End With

        If My.Settings.autoScroll Then
            txt_terminal.ScrollToEnd()
        End If

    End Sub

    ''' <summary>
    ''' add received string to terminal
    ''' </summary>
    ''' <param name="myString"></param>
    Sub myShowStringMethod1(ByVal myString As String)

        If last_text_from_port = False Then
            If Not txt_terminal.Text.EndsWith(vbLf) Then
                txt_terminal.AppendText(vbLf)
            End If
            last_text_from_port = True
        End If
        txt_terminal.AppendText("▼ ")
        txt_terminal.AppendText(myString)
        clear_lines()

    End Sub

    ''' <summary>
    ''' perform action on receiving data
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SerialPort1_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs)
        Dim str As String = Serial_Port1.ReadExisting
        'Invoke(myD1, str)
        Dispatcher.Invoke(myD1, str)
    End Sub


    ''' <summary>
    ''' setup and start serial port connection
    ''' </summary>
    ''' <returns></returns>
    Public Function SerialSetup1() As Boolean
        Try

            If Serial_Port1.IsOpen Then
                Serial_Port1.Close()
            End If

            Serial_Port1.PortName = combo_ports.SelectedItem
            Serial_Port1.BaudRate = Combospeed1.SelectedItem
            Serial_Port1.Encoding = System.Text.Encoding.GetEncoding(1252) 'change the Encoding to enable 8-bit communications
            Serial_Port1.Open()
            Return True

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error - " & Serial_Port1.PortName & " not connected")
            Return False
        End Try
    End Function

    Public Sub ClosePort1()
        Serial_Port1.Close()
    End Sub

    'Public Sub SerialClose1()
    '    Dim t As New Thread(AddressOf ClosePort1)
    '    t.Start()
    'End Sub

    Private Sub btn_send_Click(sender As Object, e As RoutedEventArgs) Handles btn_send.Click
        send_text()
    End Sub

    Private Sub Btn_connect_Click(sender As Object, e As RoutedEventArgs) Handles btn_connect.Click

        If Serial_Port1.IsOpen = False Then
            SerialSetup1()
        Else

            Try
                Serial_Port1.Close()

            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error - " & Serial_Port1.PortName)
            End Try

        End If

        UpdateConnectionStatus()
    End Sub

#End Region

#Region "text copy paste"

    ''' <summary>
    ''' convert newline and carriage return and tab caracters to Escape sequence
    ''' </summary>
    Private Sub paste_clean()
        Dim tempsrt As New StringBuilder
        tempsrt.Append(Clipboard.GetText)
        tempsrt.Replace(vbTab, "\n")     'tab
        tempsrt.Replace(vbCr, "\r")      'Carriage return
        tempsrt.Replace(vbLf, "\n")      'New line
        txt_send.SelectedText = tempsrt.ToString
    End Sub

    ''' <summary>
    ''' detect paste command and perform clean paste
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub txt_send_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles txt_send.PreviewKeyDown

        'detect ctrl + v
        If My.Computer.Keyboard.CtrlKeyDown And e.Key = Key.V Then
            If Clipboard.ContainsText Then
                paste_clean()
                e.Handled = True
            End If
        ElseIf e.Key = Key.Return Then
            send_text()
        End If

    End Sub

    ''' <summary>
    ''' detect paste command through context menu and perform clean paste
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub txt_send_paste_Click(sender As Object, e As RoutedEventArgs) Handles txt_send_paste.Click

        If Clipboard.ContainsText Then
            paste_clean()
        End If

    End Sub

    Private Sub txt_send_cut_Click(sender As Object, e As RoutedEventArgs) Handles txt_send_cut.Click
        txt_send.Cut()
    End Sub

    Private Sub txt_send_copy_Click(sender As Object, e As RoutedEventArgs) Handles txt_send_copy.Click
        txt_send.Copy()
    End Sub

    ''' <summary>
    ''' allow only number
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub txt_maxlines_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles txt_maxlines.PreviewTextInput
        If Not Char.IsDigit(CChar(e.Text)) Then e.Handled = True
    End Sub

    Private Sub txt_maxlines_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txt_maxlines.TextChanged
        If IsNumeric(txt_maxlines.Text) Then
            My.Settings.maxLines = txt_maxlines.Text
        End If
    End Sub

    Private Sub txt_maxlines_LostFocus(sender As Object, e As RoutedEventArgs) Handles txt_maxlines.LostFocus
        If IsNumeric(txt_maxlines.Text) = False Then
            txt_maxlines.Text = maxLines
        End If
    End Sub

#End Region

    Private Sub btn_About_Click(sender As Object, e As RoutedEventArgs) Handles btn_About.Click
        Dim dr As Window = New aboutWindow
        dr.ShowDialog()

    End Sub

End Class
