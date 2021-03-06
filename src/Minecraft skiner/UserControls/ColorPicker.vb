﻿Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports OpenTK.Graphics.OpenGL
Imports OpenTK
Imports Minecraft_skiner.Common

Public Class ColorPicker
    <Description("Render it or not (only used when design because when it became false in design mode it crashs VS and can be removed in the release)"), Category("Behavior")>
    Property InDesignMode As Boolean = True

    Dim TheColor As Color = Color.Red
    <Description("The current color"), Category("Appearance")>
    Property Color As Color
        Set(value As Color)
            If value.A <> 0 AndAlso value.A <> 255 Then
                value = Color.FromArgb(255, value.R, value.G, value.B)
            ElseIf value.A = 0 Then
                value = Color.Transparent
            End If
            RGBHex.Text = RGB(value.R, value.G, value.B).ToString("X6")

            TheColor = value
        End Set
        Get
            Return TheColor
        End Get
    End Property

    Dim _IsPicking As Boolean
    <Description("Is Pick color active or not"), Category("Appearance")>
    Property IsPicking As Boolean
        Set(value As Boolean)
            If value Then
                ColorPick.BorderStyle = BorderStyle.Fixed3D
            Else
                ColorPick.BorderStyle = BorderStyle.None
            End If
            If IsFilling AndAlso value Then IsFilling = False
            _IsPicking = value
        End Set
        Get
            Return _IsPicking
        End Get
    End Property

    Dim _IsFilling As Boolean
    <Description("Is flood fill active or not"), Category("Appearance")>
    Property IsFilling As Boolean
        Set(value As Boolean)
            If value Then
                Fill.BorderStyle = BorderStyle.Fixed3D
            Else
                Fill.BorderStyle = BorderStyle.None
            End If
            If IsPicking Then IsPicking = False
            If IsMirroring Then IsMirroring = False
            _IsFilling = value
        End Set
        Get
            Return _IsFilling
        End Get
    End Property

    Dim _IsMirroring As Boolean
    <Description("Is Mirroring effect active or not"), Category("Appearance")>
    Property IsMirroring As Boolean
        Set(value As Boolean)
            If value Then
                Mirror.BorderStyle = BorderStyle.Fixed3D
            Else
                Mirror.BorderStyle = BorderStyle.None
            End If
            If IsPicking Then IsPicking = False
            If IsFilling Then IsFilling = False
            _IsMirroring = value
        End Set
        Get
            Return _IsMirroring
        End Get
    End Property

    Dim _BrushSize As Byte = 1
    <Description("The size of the brush"), Category("Appearance")>
    Property BrushSize As Byte
        Set(value As Byte)
            Select Case value
                Case 1
                    Size1.BorderStyle = BorderStyle.Fixed3D
                    Size2.BorderStyle = BorderStyle.None
                    Size3.BorderStyle = BorderStyle.None
                Case 2
                    Size1.BorderStyle = BorderStyle.None
                    Size2.BorderStyle = BorderStyle.Fixed3D
                    Size3.BorderStyle = BorderStyle.None
                Case 3
                    Size1.BorderStyle = BorderStyle.None
                    Size2.BorderStyle = BorderStyle.None
                    Size3.BorderStyle = BorderStyle.Fixed3D
                Case Else
                    Throw New Exception("Invalid value")
            End Select
            _BrushSize = value
        End Set
        Get
            Return _BrushSize
        End Get
    End Property

    Dim _IsCamoing As Boolean
    Property IsCamoing As Boolean
        Set(value As Boolean)
            If value Then
                Camo.BorderStyle = BorderStyle.Fixed3D
            Else
                Camo.BorderStyle = BorderStyle.None
            End If
            _IsCamoing = value
        End Set
        Get
            Return _IsCamoing
        End Get
    End Property

#Disable Warning BC40000

    Private Sub HSV_Paint(sender As Object, e As PaintEventArgs) Handles HSV.Paint
        If InDesignMode Then Exit Sub
        HSV.MakeCurrent()

        GL.ClearColor(BackColor)
        'First Clear Buffers
        GL.Clear(ClearBufferMask.ColorBufferBit)

        GL.Begin(BeginMode.Polygon)
        GL.Color3(HSVtoRGB(0, 0, RGBtoHSV(Color).Value))
        GL.Vertex2(0, 0)
        For I As Double = 0 To 2 * Math.PI Step Math.PI / 24
            GL.Color3(HSVtoRGB((I * 360) / (2 * Math.PI), 100, RGBtoHSV(Color).Value))
            GL.Vertex2(Math.Cos(I) * 0.965, Math.Sin(I) * 0.965)
        Next
        GL.End()

        If Not Color = Color.Transparent AndAlso Not IsPicking Then
            GL.LineWidth(2)
            GL.Begin(BeginMode.LineStrip)
            GL.Color3(Color.White)
            For I As Double = 0 To 2 * Math.PI Step Math.PI / 24
                GL.Vertex2(Math.Cos(I) * 0.05 + (Math.Cos((Color.GetHue * (2 * Math.PI)) / 360) * (RGBtoHSV(Color).Saturation / 100)) * 0.965, Math.Sin(I) * 0.05 + (Math.Sin((Color.GetHue * (2 * Math.PI)) / 360) * (RGBtoHSV(Color).Saturation / 100)) * 0.965)
            Next
            GL.End()
        End If

        GL.LineWidth(5)
        GL.Begin(BeginMode.LineLoop)
        GL.Color3(Color.Black)
        For I As Double = 0 To 2 * Math.PI Step Math.PI / 32
            GL.Vertex2(Math.Cos(I) * 0.98, Math.Sin(I) * 0.98)
        Next
        GL.End()
        HSV.SwapBuffers()

        HSV.Context.MakeCurrent(Nothing)
    End Sub


    Private Sub CurrentColor_Paint(sender As Object, e As PaintEventArgs) Handles CurrentColor.Paint
        If InDesignMode Then Exit Sub
        CurrentColor.MakeCurrent()
        GL.ClearColor(Color)
        GL.Clear(ClearBufferMask.ColorBufferBit)

        If Color = Color.Transparent Then
            GL.Enable(EnableCap.Texture2D)

            Dim texID As Integer = 1
            Dim tex As Bitmap = My.Resources.Transparent
            GL.BindTexture(TextureTarget.Texture2D, texID)
            Dim data As BitmapData = tex.LockBits(New Rectangle(0, 0, 80, 80), ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 80, 80, 0, Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
            tex.UnlockBits(data)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)

            GL.BindTexture(TextureTarget.ProxyTexture2D, texID)

            GL.Begin(BeginMode.Quads)

            GL.TexCoord2(0, 0)
            GL.Vertex2(-1, 1)
            GL.TexCoord2(1, 0)
            GL.Vertex2(1, 1)
            GL.TexCoord2(1, 5)
            GL.Vertex2(1, -1)
            GL.TexCoord2(0, 5)
            GL.Vertex2(-1, -1)

            GL.End()
        End If

        CurrentColor.SwapBuffers()

        CurrentColor.Context.MakeCurrent(Nothing)
    End Sub

    Private Sub Hue_Paint(sender As Object, e As PaintEventArgs) Handles Hue.Paint
        If InDesignMode Then Exit Sub
        Hue.MakeCurrent()
        GL.Clear(ClearBufferMask.ColorBufferBit)

        GL.Begin(BeginMode.QuadStrip)
        For I As Single = -1 To 1 Step 0.01
            GL.Color3(HSVtoRGB(((I + 1) * 360) / 2, 100, 100))
            GL.Vertex2(-1, -I)
            GL.Vertex2(1, -I)
        Next
        GL.End()

        If Not Color = Color.Transparent AndAlso Not IsPicking Then
            GL.LineWidth(3)
            GL.Begin(BeginMode.Lines)
            GL.Color3(Color.White)
            GL.Vertex2(-1, -((2.0F * (Hue.Height / 360) * Color.GetHue) / Hue.Height - 1.0F))
            GL.Vertex2(1, -((2.0F * (Hue.Height / 360) * Color.GetHue) / Hue.Height - 1.0F))
            GL.End()
        End If

        Hue.SwapBuffers()
        Hue.Context.MakeCurrent(Nothing)
    End Sub

    Private Sub Saturation_Paint(sender As Object, e As PaintEventArgs) Handles Saturation.Paint
        If InDesignMode Then Exit Sub
        Saturation.MakeCurrent()
        GL.Clear(ClearBufferMask.ColorBufferBit)

        GL.Begin(BeginMode.Quads)
        GL.Color3(HSVtoRGB(Color.GetHue, 100, RGBtoHSV(Color).Value))
        GL.Vertex2(-1, 1)
        GL.Vertex2(1, 1)
        GL.Color3(HSVtoRGB(Color.GetHue, 0, RGBtoHSV(Color).Value))
        GL.Vertex2(1, -1)
        GL.Vertex2(-1, -1)
        GL.End()

        If Not Color = Color.Transparent AndAlso Not IsPicking Then
            GL.LineWidth(3)
            GL.Begin(BeginMode.Lines)
            GL.Color3(Color.White)
            GL.Vertex2(-1, ((2.0F * (Saturation.Height / 100) * RGBtoHSV(Color).Saturation) / Saturation.Height - 1.0F))
            GL.Vertex2(1, ((2.0F * (Saturation.Height / 100) * RGBtoHSV(Color).Saturation) / Saturation.Height - 1.0F))
            GL.End()
        End If

        Saturation.SwapBuffers()
        Saturation.Context.MakeCurrent(Nothing)
    End Sub

    Private Sub Value_Paint(sender As Object, e As PaintEventArgs) Handles Value.Paint
        If InDesignMode Then Exit Sub
        Value.MakeCurrent()
        GL.Clear(ClearBufferMask.ColorBufferBit)

        GL.Begin(BeginMode.Quads)
        GL.Color3(HSVtoRGB(Color.GetHue, Color.GetSaturation * 100, 100))
        GL.Vertex2(-1, 1)
        GL.Vertex2(1, 1)
        GL.Color3(HSVtoRGB(Color.GetHue, Color.GetSaturation * 100, 0))
        GL.Vertex2(1, -1)
        GL.Vertex2(-1, -1)
        GL.End()

        If Not Color = Color.Transparent AndAlso Not IsPicking Then
            GL.LineWidth(3)
            GL.Begin(BeginMode.Lines)
            GL.Color3(Color.White)
            GL.Vertex2(-1, ((2.0F * ((Value.Height / 100) * RGBtoHSV(Color).Value)) / Value.Height - 1.0F))
            GL.Vertex2(1, ((2.0F * ((Value.Height / 100) * RGBtoHSV(Color).Value)) / Value.Height - 1.0F))
            GL.End()
        End If

        Value.SwapBuffers()
        Value.Context.MakeCurrent(Nothing)
    End Sub

    Dim ValueDown As Boolean
    Dim SaturationDown As Boolean
    Dim HueDown As Boolean
    Dim HSVDown As Boolean

    Private Sub Value_MouseDown(sender As Object, e As MouseEventArgs) Handles Value.MouseDown
        If e.Button = MouseButtons.Left Then
            ValueDown = True
        End If
    End Sub

    Private Sub Value_MouseUp(sender As Object, e As MouseEventArgs) Handles Value.MouseUp
        ValueDown = False
    End Sub

    Private Sub Saturation_MouseDown(sender As Object, e As MouseEventArgs) Handles Saturation.MouseDown
        If e.Button = MouseButtons.Left Then
            SaturationDown = True
        End If
    End Sub

    Private Sub Saturation_MouseUp(sender As Object, e As MouseEventArgs) Handles Saturation.MouseUp
        SaturationDown = False
    End Sub

    Private Sub Hue_MouseDown(sender As Object, e As MouseEventArgs) Handles Hue.MouseDown
        If e.Button = MouseButtons.Left Then
            HueDown = True
        End If
    End Sub

    Private Sub Hue_MouseUp(sender As Object, e As MouseEventArgs) Handles Hue.MouseUp
        HueDown = False
    End Sub

    Private Sub MouseDown_Tick(sender As Object, e As EventArgs) Handles timMouseDown.Tick
        If ValueDown Then
            Dim Point As Point = Value.PointToClient(New Point(Cursor.Position))
            If Point.X < 0 OrElse Point.X > Value.Width OrElse Point.Y < 0 OrElse Point.Y > Value.Height Then
                Exit Sub
            End If
            IsPicking = False
            Color = HSVtoRGB(Color.GetHue, RGBtoHSV(Color).Saturation, (100 / Value.Height) * Math.Abs(Point.Y - Value.Height))
            Refresh()
        ElseIf SaturationDown Then
            Dim Point As Point = Saturation.PointToClient(New Point(Cursor.Position))
            If Point.X < 0 OrElse Point.X > Saturation.Width OrElse Point.Y < 0 OrElse Point.Y > Saturation.Height Then
                Exit Sub
            End If
            IsPicking = False
            Color = HSVtoRGB(Color.GetHue, (100 / Saturation.Height) * Math.Abs(Point.Y - Saturation.Height), RGBtoHSV(Color).Value)
            Refresh()
        ElseIf HueDown Then
            Dim Point As Point = Hue.PointToClient(New Point(Cursor.Position))
            If Point.X < 0 OrElse Point.X > Hue.Width OrElse Point.Y < 0 OrElse Point.Y > Hue.Height Then
                Exit Sub
            End If
            IsPicking = False
            Color = HSVtoRGB((360 / Hue.Height) * Point.Y, RGBtoHSV(Color).Saturation, RGBtoHSV(Color).Value)
            Refresh()
        ElseIf HSVDown Then
            Dim Point As Point = HSV.PointToClient(New Point(Cursor.Position))
            If Point.X < 0 OrElse Point.X > HSV.Width OrElse Point.Y < 0 OrElse Point.Y > HSV.Height Then
                Exit Sub
            End If
            Dim HSVColor As New HSV(-Math.Atan2((Point.Y - HSV.Height / 2), (Point.X - HSV.Width / 2)) * 180 / Math.PI, ((((Point.X - HSV.Width / 2) ^ 2) + ((Point.Y - HSV.Height / 2) ^ 2)) ^ (1 / 2)) * 100 / (0.5 * (HSV.Width * 0.965)), RGBtoHSV(Color).Value)
            If HSVColor.Saturation > 100 Then HSVColor.Saturation = 100
            If HSVColor.Hue > 360 Then
                HSVColor.Hue -= 360
            ElseIf HSVColor.Hue < 0 Then
                HSVColor.Hue += 360
            End If
            IsPicking = False
            Color = HSVtoRGB(HSVColor.Hue, HSVColor.Saturation, HSVColor.Value)
            Refresh()
        End If
    End Sub

    Private Sub HSV_MouseDown(sender As Object, e As MouseEventArgs) Handles HSV.MouseDown
        If e.Button = MouseButtons.Left Then
            HSVDown = True
        End If
    End Sub

    Private Sub HSV_MouseUp(sender As Object, e As MouseEventArgs) Handles HSV.MouseUp
        HSVDown = False
    End Sub

    Private Sub RGBHex_Leave(sender As Object, e As EventArgs) Handles RGBHex.Leave
        Color = ColorTranslator.FromHtml("#" & RGBHex.Text)
        Refresh()
    End Sub

    Private Sub Transparent_Click(sender As Object, e As EventArgs) Handles Transparent.Click
        Color = Color.Transparent
        Refresh()
    End Sub

    Private Sub ColorPick_Click(sender As Object, e As EventArgs) Handles ColorPick.Click
        IsPicking = Not IsPicking
    End Sub

    Private Sub Size_Click(sender As Object, e As EventArgs) Handles Size1.Click, Size2.Click, Size3.Click
        BrushSize = sender.Tag
    End Sub

    Private Sub Fill_Click(sender As Object, e As EventArgs) Handles Fill.Click
        IsFilling = Not IsFilling
    End Sub

    Private Sub Mirror_Click(sender As Object, e As EventArgs) Handles Mirror.Click
        IsMirroring = Not IsMirroring
    End Sub

    Private Sub Camo_Click(sender As Object, e As EventArgs) Handles Camo.Click
        IsCamoing = Not IsCamoing
    End Sub
End Class
