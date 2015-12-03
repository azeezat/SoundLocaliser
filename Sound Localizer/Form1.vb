
Imports System.Math
Imports System.Numerics
Imports System.Threading
Imports System.IO.Ports
Public Class Form1
    Dim myPort As Array  'COM Ports detected on the system will be stored here
    Delegate Sub SetTextCallback(ByVal [text] As String) 'Added to prevent threading errors during receiveing of data
    Dim str1, STRRECEIVEDDATA As String
    Dim portString As String
    Dim microphone_samples, micA_samples, micB_samples, micC_samples, micD_samples As String
    Dim normalisedA, normalisedB, normalisedC, normalisedD As String
    Dim portStatus As Boolean
    Dim micA() As Decimal
    Dim Count As Integer = 0
    Dim portStringA, portStringB, portStringC, portStringD As String
    Dim maxA, maxB, maxC, maxD, value As Decimal
    Dim DFT() As Double
    Dim DFT_Complex() As Complex
    Dim polar(3) As Double
    Dim micAresults(3) As Double
    Dim micBresults(3) As Double
    Dim micCresults(3) As Double
    Dim micDresults(3) As Double
    Dim y As Double
    Dim z As Complex
    Dim sampleRate As Double
    Dim transform() As Double
    Dim position(2) As Double
    Dim NoSamples, sigpowInit As Integer
    Dim phaseDiffHor, phaseDiffVer, freqHor, freqVer, distDiffHor, distDiffVer As Double
    Dim phaseHorInit, phaseVerInit, freqHorInit, freqVerInit, distVerInit, distHorInit As Double
    Dim horDist_fromSource As Double
    Dim verDist_fromSource As Double


    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        myPort = IO.Ports.SerialPort.GetPortNames() 'Get all com ports available

        For Each sp As String In My.Computer.Ports.SerialPortNames
            cmbPort.Items.Add(sp)
        Next
        cmbPort.Text = ("")    'Set cmbPort text to the first COM port detected
        portStatus = True
        Timer1.Enabled = False
        Dim width As Integer
        Dim height As Integer
        width = dftBox.Width
        height = dftBox.Height
        microphone_samples = ""
        micA_samples = ""
        micB_samples = ""
        micC_samples = ""
        micD_samples = ""
        normalisedA = ""
        normalisedB = ""
        normalisedC = ""
        normalisedD = ""
        phaseDiffHor = 0
        phaseHorInit = 0
        phaseDiffVer = 0
        phaseVerInit = 0
        freqHorInit = 0
        freqVerInit = 0
        distHorInit = 0
        distVerInit = 0
        freqHor = 0
        freqVer = 0
        distDiffHor = 0
        distDiffVer = 0
        sigpowInit = 0
    End Sub

    Private Sub connectButton_Click(sender As System.Object, e As System.EventArgs) Handles connectButton.Click
        If cmbPort.Text = ("") Then
            MessageBox.Show("PLEASE ENTER A VALID PORT NUMBER")
        End If
        If portStatus = True Then
            SerialPort1.PortName = cmbPort.Text         'Set SerialPort1 to the selected COM port at startup
            SerialPort1.BaudRate = "115200"               'Set Baud rate to 115200
            'Other Serial Port Property
            SerialPort1.Parity = IO.Ports.Parity.None
            SerialPort1.StopBits = IO.Ports.StopBits.One
            SerialPort1.DataBits = 8            'Open our serial port
            SerialPort1.Open()
            SerialPort1.DiscardOutBuffer()
            SerialPort1.DiscardInBuffer()
            connectButton.Text = "Dis&connect"
            portStatus = False
            SerialPort1.WriteLine("A")
            Timer1.Enabled = True
            With Me.positionBox.CreateGraphics
                .DrawLine(New Pen(Color.Red, 1), New Point(0, (positionBox.Height) / 2), New Point(positionBox.Width, (positionBox.Height) / 2))

                .DrawLine(New Pen(Color.Red, 1), New Point((positionBox.Width) / 2, 0), New Point((positionBox.Width) / 2, positionBox.Height))

            End With

        Else
            SerialPort1.Close()
            portStatus = True
            connectButton.Text = "Co&nnect"
            Timer1.Enabled = False

        End If
    End Sub
    

    Private Sub SerialPort1_DataReceived(sender As Object, e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived

        SerialPort1.ReadTimeout() = 10


    End Sub

   
    Private Sub Timer1_Tick(sender As Object, e As System.EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        microphone_samples = "" 'clear initial microphone signal samples that have been stored
        micA_samples = ""   'clear mic a samples
        micB_samples = ""
        micC_samples = ""
        micD_samples = ""
        normalisedA = "" 'normalised sound samples
        normalisedB = ""
        normalisedC = ""
        normalisedD = ""
        'resultABox.Text = ""
        'resultBbox.Text = ""
        'resultCbox.Text = ""
        'resultDbox.Text = ""
        Dim positionX As Integer = 0
        Dim positionY As Integer = 0
        Dim power As Double
        portString = (SerialPort1.ReadExisting()) 'command to read signals from computer buffer
        microphone_samples = microphone_samples & portString
       


        Dim words As String() = microphone_samples.Split(New Char() {","c}) 'removes delimiters
        Count = 0
        Do

            micA_samples = micA_samples & words(Count) & ","
            micB_samples = micB_samples & words(Count + 1) & ","
            micC_samples = micC_samples & words(Count + 2) & ","
            micD_samples = micD_samples & words(Count + 3) & ","


            Count = Count + 4
        Loop Until Count > 599
        microphone_samples = ""
        'check maximum values for all the microphones
        maxA = 0
        maxB = 0
        maxC = 0
        maxD = 0
        Dim stringA As String() = micA_samples.Split(New Char() {","c})
        Dim stringB As String() = micB_samples.Split(New Char() {","c})
        Dim stringC As String() = micC_samples.Split(New Char() {","c})
        Dim stringD As String() = micD_samples.Split(New Char() {","c})
        Dim maxstringA As String
        Dim maxs As Decimal
        Dim maxstringB As String
        Dim maxstringC As String
        Dim maxstringD As String
        For Each maxstringA In stringA
            Try
                maxs = Decimal.Parse(maxstringA)
                If (maxs > maxA) Then
                    maxA = maxs
                End If
            Catch ex As Exception

            End Try

        Next
        For Each maxstringA In stringA
            Try
                maxs = Decimal.Parse(maxstringA)
                value = maxs / maxA     'normalise all the values for micA
                normalisedA = normalisedA & value.ToString("F2") & ","
            Catch ex As Exception

            End Try
        Next
        For Each maxstringB In stringB
            Try
                maxs = Decimal.Parse(maxstringB)
                If (maxs > maxB) Then
                    maxB = maxs
                End If
            Catch ex As Exception

            End Try

        Next
        For Each maxstringB In stringB
            Try
                maxs = Decimal.Parse(maxstringB)
                value = maxs / maxB
                
                normalisedB = normalisedB & value.ToString("F2") & ","
            Catch ex As Exception

            End Try
        Next
        For Each maxstringC In stringC
            Try
                maxs = Decimal.Parse(maxstringC)
                If (maxs > maxC) Then
                    maxC = maxs
                End If
            Catch ex As Exception

            End Try

        Next
        For Each maxstringC In stringC
            Try
                maxs = Decimal.Parse(maxstringC)
                value = maxs / maxC
                normalisedC = normalisedC & value.ToString("F2") & ","
            Catch ex As Exception

            End Try
        Next
        For Each maxstringD In stringD
            Try
                maxs = Decimal.Parse(maxstringD)
                If (maxs > maxD) Then
                    maxD = maxs
                End If
            Catch ex As Exception

            End Try

        Next
        For Each maxstringD In stringD
            Try
                maxs = Decimal.Parse(maxstringD)
                value = maxs / maxD
                
                normalisedD = normalisedD & value.ToString("F2") & ","
            Catch ex As Exception

            End Try
        Next
        Count = 0
        Dim sampler As Integer = 150
        sampleRate = 12500
        sampleRate = 1 / sampleRate
        dftBox.Refresh()
        micDFT(normalisedA, sampler) 'DFT functions
        plotDFT(normalisedA, Color.Green)
        micAresults = phaseDFT()
        micDFT(normalisedB, sampler)
        plotDFT(normalisedB, Color.Blue)
        micBresults = phaseDFT()
        micDFT(normalisedC, sampler)
        plotDFT(normalisedC, Color.Orange)
        micCresults = phaseDFT()
        micDFT(normalisedD, sampler)
        plotDFT(normalisedD, Color.Purple)
        micDresults = phaseDFT()
        power = (micAresults(1) + micBresults(1) + micCresults(1) + micDresults(1)) / 4
        Dim sigPower As Integer
        sigPower = (micAresults(1) + micBresults(1) + micCresults(1) + micDresults(1)) / 4
        sigPower = ((sigPower * 223 / 50) + sigpowInit) / 2
        sigpowInit = sigPower
        PictureBox1.Refresh()

        With Me.PictureBox1.CreateGraphics
            .FillRectangle(Brushes.Green, 0, (223 - sigPower), 32, 223)
        End With

        'resultABox.Text = micAresults(0).ToString("F4") & "," & micAresults(1).ToString("F4") & "," & micAresults(2).ToString("F2")
        'resultBbox.Text = micBresults(0).ToString("F4") & "," & micBresults(1).ToString("F4") & "," & micBresults(2).ToString("F2")
        'resultCbox.Text = micCresults(0).ToString("F4") & "," & micCresults(1).ToString("F4") & "," & micCresults(2).ToString("F2")
        'resultDbox.Text = micDresults(0).ToString("F4") & "," & micDresults(1).ToString("F4") & "," & micDresults(2).ToString("F2")

        'Dim positionX, positionY As Integer
        Dim Sx, Sy, Sz As Double
        
        Dim freqA, freqB, freqC, freqD As Double
        freqA = micAresults(2)
        freqB = micBresults(2)
        freqC = micCresults(2)
        freqD = micDresults(2)

        If (freqA > 700) And (freqA < 850) And (freqB > 700) And (freqB < 850) And (freqC > 700) And (freqC < 850) And (freqD > 700) And (freqD < 850) Then
            phaseDiffVer = (micCresults(0) - micDresults(0))
            phaseDiffVer = (phaseVerInit + phaseDiffVer) / 2
            phaseVerInit = phaseDiffVer
            phaseDiffHor = (micAresults(0) - micBresults(0))
            phaseDiffHor = (phaseHorInit + phaseDiffHor) / 2
            phaseHorInit = phaseDiffHor
            freqHor = (micAresults(2) + micBresults(2)) / 2
            freqVer = (micCresults(2) + micDresults(2)) / 2
            'distDiffHor = 0.3 * Sin(phaseDiffHor)
            'distDiffVer = 0.3 * Sin(phaseDiffVer)
            distDiffHor = (phaseDiffHor) * 344 / (2 * PI * freqHor)
            distDiffVer = (phaseDiffVer) * 344 / (2 * PI * freqVer)
            phaseDiffHor = Abs(phaseDiffHor)
            phaseDiffVer = Abs(phaseDiffVer)


            horDist_fromSource = (Pow(0.3, 2) + Pow(distDiffHor, 2) - (2 * 0.3 * distDiffHor * Cos(phaseDiffHor))) / (2 * ((0.3 * Cos(phaseDiffHor)) - distDiffHor))
            verDist_fromSource = (Pow(0.3, 2) + Pow(distDiffVer, 2) - (2 * 0.3 * distDiffVer * Cos(phaseDiffVer))) / (2 * ((0.3 * Cos(phaseDiffVer)) - distDiffVer))
            horDist_fromSource = (distHorInit + horDist_fromSource) / 2
            distHorInit = horDist_fromSource
            verDist_fromSource = (distVerInit + verDist_fromSource) / 2
            distVerInit = verDist_fromSource
            Sx = ((horDist_fromSource + distDiffHor) * Cos(phaseDiffHor)) - 0.15
            Sz = ((verDist_fromSource + distDiffVer) * Cos(phaseDiffVer)) - 0.15
            Sy = Sqrt((Pow(Sx, 2)) + (Pow(Sz, 2)))
            Sy = (Pow((horDist_fromSource + distDiffHor), 2) - (Pow(Sy, 2)))
            If Sy < 0 Then
                Sy = -1 * Sqrt(Abs(Sy))
            End If
            Sx = Sx * 10
            Sz = Sz * 10
            Sy = Sy * 10
            'RichTextBox1.Text = RichTextBox1.Text & phaseDiffHor.ToString("F2") & ", " & phaseDiffVer.ToString("F2") & vbTab '"Hs = " & horDist_fromSource & " Vs = " & verDist_fromSource & " Sx = " & Sx & " Sz = " & Sz & vbCr
            RichTextBox2.Text = ((micAresults(2) + micBresults(2) + micCresults(2) + micDresults(2)) / 4).ToString("F2")
           


            'Sx = Pow(10, (Log(234 / Pow((micAresults(1) + micBresults(1)), 2)) / 1.98))
            'Sy = Pow(10, (Log(64.13 / Pow((micCresults(1) + micDresults(1)), 2)) / 1.65221))

            ' Sx = Sx * Cos(phaseDiffHor)
            ' Sy = Sy * Cos(phaseDiffVer)
            'Sz = Sqrt((Pow(Sx, 2)) + (Pow(Sy, 2)))
            If (Sx < 3) And (Sx > -3) And (Sy < 1.75) And (Sy > -1.75) And (micAresults(1) > 6) And (micBresults(1) > 6) And (micCresults(1) > 6) And (micDresults(1) > 6) Then
                
                verDist.Text = Sx.ToString("F2") & " i " & Sy.ToString("F2") & " j " & Sz.ToString("F2") & " k "

                positionX = 332 + ((Sx * 332) / 3)
                positionY = 191 + ((Sy * 191) / 1.725)
                'If (Sx < 3) And (Sx > -3) And (Sy < 1.75) And (Sy > -1.75) And (micAresults(1) > 6) And (micBresults(1) > 6) And (micCresults(1) > 6) And (micDresults(1) > 6) Then
                positionBox.Refresh()
                'End If
                Dim Count = positionBox.Height / 20
                Dim scale As Integer
                scale = 1
                Do
                    With Me.positionBox.CreateGraphics
                        .DrawLine(New Pen(Color.Red, 1), New Point(0, (scale * Count)), New Point(positionBox.Width, (scale * Count)))

                    End With
                    scale = scale + 1
                Loop While ((scale * Count) < positionBox.Height)
                Count = positionBox.Width / 20
                scale = 1
                Do
                    With Me.positionBox.CreateGraphics
                        .DrawLine(New Pen(Color.Red, 1), New Point((scale * Count), 0), New Point((scale * Count), positionBox.Height))

                    End With
                    scale = scale + 1
                Loop While ((scale * Count) < positionBox.Width)

                With Me.positionBox.CreateGraphics
                    .DrawLine(New Pen(Color.Violet, 2), New Point(0, (positionBox.Height) / 2), New Point(positionBox.Width, (positionBox.Height) / 2))

                    .DrawLine(New Pen(Color.Violet, 2), New Point((positionBox.Width) / 2, 0), New Point((positionBox.Width) / 2, positionBox.Height))

                    .FillEllipse(Brushes.Yellow, positionX, positionY, 10, 10)



                End With
                
            End If
        End If



        SerialPort1.DiscardOutBuffer()
        SerialPort1.DiscardInBuffer()
        SerialPort1.WriteLine("A")
        Timer1.Enabled = True



    End Sub

   
    Private Sub micDFT(ByVal [value] As String, ByVal samples As Integer)
        Dim counter As Integer = 0
        Dim count As Integer = 0
        Dim words As String() = value.Split(New Char() {","c})
        Dim word As String
        Dim mycomplex As Complex
        ReDim DFT(samples)
        ReDim transform(samples)
        ReDim DFT_Complex(samples)
        For Each word In words
            Try
                transform(count) = Double.Parse(word)
            Catch ex As Exception

            End Try

            count = count + 1
        Next
        Do
            count = 0
            z = 0
            Do 'Do Discrete Fourier Transform
                mycomplex = New Complex(Math.Cos(2 * PI * counter * count / (samples)), -1 * Math.Sin(2 * PI * counter * count / (samples)))
                z = (z + (transform(count) * mycomplex))
                count = count + 1

            Loop While (count < samples)
            DFT_Complex(counter) = z
            DFT(counter) = Complex.Abs(z)
            DFT(counter) = DFT(counter) / samples
            counter = counter + 1
        Loop While (counter < samples)
    End Sub
    Private Sub plotDFT(ByVal [values] As String, ByVal colour As System.Drawing.Color)
        Dim words As String() = values.Split(New Char() {","c})
        Dim word As String
        Dim count As Integer = 0
        Dim freqCount As Integer = 0
        Dim maxvalues As Double = 0
        Dim plotY() As Integer
        Dim plotX() As Integer

        For Each word In words
            Try
                transform(count) = Double.Parse(word)
                If transform(count) > maxvalues Then
                    maxvalues = transform(count)
                End If
            Catch ex As Exception

            End Try

            count = count + 1
        Next
        NoSamples = count
        ReDim plotY(NoSamples)
        ReDim plotX(NoSamples)
        count = 0
        Do
            plotY(count) = (dftBox.Height) - (transform(count) * 100 / maxvalues)
            count = count + 1

        Loop Until (count = NoSamples)
        count = 0
        Do
            plotX(count) = (dftBox.Width) * count / NoSamples
            count = count + 1
        Loop Until (count = NoSamples)
        count = 0
        Do
            Try
                With Me.dftBox.CreateGraphics
                    .DrawLine(New Pen(colour, 1), New Point(plotX(count), plotY(count)), New Point(plotX(count + 1), plotY(count + 1)))
                End With
            Catch ex As Exception

            End Try

            count = count + 1
        Loop While (count < (NoSamples / 2))
        With Me.dftBox.CreateGraphics
            .DrawLine(New Pen(Color.Red, 1), New Point((300 * (NoSamples / 2) / dftBox.Width), 0), New Point((300 * (NoSamples / 2) / dftBox.Width), dftBox.Height))
        End With
        With Me.dftBox.CreateGraphics
            .DrawLine(New Pen(Color.Red, 1), New Point((1500 * (NoSamples / 2) / dftBox.Width), 0), New Point((1500 * (NoSamples / 2) / dftBox.Width), dftBox.Height))
        End With

    End Sub
    Private Function phaseDFT()
        Dim x As Integer = 1500
        Dim startAnalysis As Integer = (300 * (NoSamples / 2) / dftBox.Width)
        Dim endAnalysis As Integer = (x * (NoSamples / 2) / dftBox.Width)
        If endAnalysis > 150 Then
            Do
                x = x - 100
                endAnalysis = (x * (NoSamples / 2) / dftBox.Width)
            Loop While (endAnalysis > 150)
        End If
        Label8.Text = x.ToString & "Hz"
        Dim count As Integer
        Dim maxValue As Double = 0
        Dim maxIndex As Integer
        Dim angle As Double
        Dim frequency As Double
        ' ReDim phase(NoSamples)

        count = startAnalysis
        Do
            If DFT(count) > maxValue Then
                maxValue = DFT(count)
                maxIndex = count
            End If
            count = count + 1
        Loop Until (count = endAnalysis)

        Dim Magnitude As Complex
        Magnitude = DFT_Complex(maxIndex)
        angle = Magnitude.Phase


        frequency = ((x - 300) * (maxIndex - startAnalysis) / (endAnalysis - startAnalysis))
        polar = {angle, Magnitude.Magnitude, frequency}


        Return polar
    End Function

   
End Class
