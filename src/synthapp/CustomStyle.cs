using ImGuiNET;
using System.Numerics;

namespace SynthApp
{
    public static class CustomStyle
    {
        // Custom ImGui styles taken from this GitHub thread: https://github.com/ocornut/imgui/issues/539

        public static void ActivateStyle()
        {
            var io = ImGui.GetIO();
            Style style = ImGui.GetStyle();

            //io.FontAtlas.Clear();
            //io.Fonts.AddFontFromFileTTF(ofToDataPath("fonts/OpenSans-Light.ttf", true).c_str(), 16);
            //io.Fonts.AddFontFromFileTTF(ofToDataPath("fonts/OpenSans-Regular.ttf", true).c_str(), 16);
            //io.Fonts.AddFontFromFileTTF(ofToDataPath("fonts/OpenSans-Light.ttf", true).c_str(), 32);
            //io.Fonts.AddFontFromFileTTF(ofToDataPath("fonts/OpenSans-Regular.ttf", true).c_str(), 11);
            //io.Fonts.AddFontFromFileTTF(ofToDataPath("fonts/OpenSans-Bold.ttf", true).c_str(), 11);
            //io.Fonts.Build();


            style.WindowPadding = new Vector2(15, 15);
            style.WindowRounding = 5.0f;
            style.FramePadding = new Vector2(5, 5);
            style.FrameRounding = 4.0f;
            style.ItemSpacing = new Vector2(12, 8);
            style.ItemInnerSpacing = new Vector2(8, 6);
            style.IndentSpacing = 25.0f;
            style.ScrollbarSize = 15.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 5.0f;
            style.GrabRounding = 3.0f;

            style.SetColor(ColorTarget.Text, new Vector4(0.40f, 0.39f, 0.38f, 1.00f));
            style.SetColor(ColorTarget.TextDisabled, new Vector4(0.40f, 0.39f, 0.38f, 0.77f));
            style.SetColor(ColorTarget.WindowBg, new Vector4(0.92f, 0.91f, 0.88f, 0.70f));
            style.SetColor(ColorTarget.ChildWindowBg, new Vector4(1.00f, 0.98f, 0.95f, 0.58f));
            //style.SetColor(ColorTarget.PopupBg, new Vector4(0.92f, 0.91f, 0.88f, 0.92f));
            style.SetColor(ColorTarget.Border, new Vector4(0.84f, 0.83f, 0.80f, 0.65f));
            style.SetColor(ColorTarget.BorderShadow, new Vector4(0.92f, 0.91f, 0.88f, 0.00f));
            style.SetColor(ColorTarget.FrameBg, new Vector4(1.00f, 0.98f, 0.95f, 1.00f));
            style.SetColor(ColorTarget.FrameBgHovered, new Vector4(0.99f, 1.00f, 0.40f, 0.78f));
            style.SetColor(ColorTarget.FrameBgActive, new Vector4(0.26f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.TitleBg, new Vector4(1.00f, 0.98f, 0.95f, 1.00f));
            style.SetColor(ColorTarget.TitleBgCollapsed, new Vector4(1.00f, 0.98f, 0.95f, 0.75f));
            style.SetColor(ColorTarget.TitleBgActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.MenuBarBg, new Vector4(1.00f, 0.98f, 0.95f, 0.47f));
            style.SetColor(ColorTarget.ScrollbarBg, new Vector4(1.00f, 0.98f, 0.95f, 1.00f));
            style.SetColor(ColorTarget.ScrollbarGrab, new Vector4(0.00f, 0.00f, 0.00f, 0.21f));
            style.SetColor(ColorTarget.ScrollbarGrabHovered, new Vector4(0.90f, 0.91f, 0.00f, 0.78f));
            style.SetColor(ColorTarget.ScrollbarGrabActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.ComboBg, new Vector4(1.00f, 0.98f, 0.95f, 1.00f));
            style.SetColor(ColorTarget.CheckMark, new Vector4(0.25f, 1.00f, 0.00f, 0.80f));
            style.SetColor(ColorTarget.SliderGrab, new Vector4(0.00f, 0.00f, 0.00f, 0.14f));
            style.SetColor(ColorTarget.SliderGrabActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.Button, new Vector4(0.00f, 0.00f, 0.00f, 0.14f));
            style.SetColor(ColorTarget.ButtonHovered, new Vector4(0.99f, 1.00f, 0.22f, 0.86f));
            style.SetColor(ColorTarget.ButtonActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.Header, new Vector4(0.25f, 1.00f, 0.00f, 0.76f));
            style.SetColor(ColorTarget.HeaderHovered, new Vector4(0.25f, 1.00f, 0.00f, 0.86f));
            style.SetColor(ColorTarget.HeaderActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.Column, new Vector4(0.00f, 0.00f, 0.00f, 0.32f));
            style.SetColor(ColorTarget.ColumnHovered, new Vector4(0.25f, 1.00f, 0.00f, 0.78f));
            style.SetColor(ColorTarget.ColumnActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.ResizeGrip, new Vector4(0.00f, 0.00f, 0.00f, 0.04f));
            style.SetColor(ColorTarget.ResizeGripHovered, new Vector4(0.25f, 1.00f, 0.00f, 0.78f));
            style.SetColor(ColorTarget.ResizeGripActive, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.CloseButton, new Vector4(0.40f, 0.39f, 0.38f, 0.16f));
            style.SetColor(ColorTarget.CloseButtonHovered, new Vector4(0.40f, 0.39f, 0.38f, 0.39f));
            style.SetColor(ColorTarget.CloseButtonActive, new Vector4(0.40f, 0.39f, 0.38f, 1.00f));
            style.SetColor(ColorTarget.PlotLines, new Vector4(0.40f, 0.39f, 0.38f, 0.63f));
            style.SetColor(ColorTarget.PlotLinesHovered, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.PlotHistogram, new Vector4(0.40f, 0.39f, 0.38f, 0.63f));
            style.SetColor(ColorTarget.PlotHistogramHovered, new Vector4(0.25f, 1.00f, 0.00f, 1.00f));
            style.SetColor(ColorTarget.TextSelectedBg, new Vector4(0.25f, 1.00f, 0.00f, 0.43f));
            style.SetColor(ColorTarget.ModalWindowDarkening, new Vector4(1.00f, 0.98f, 0.95f, 0.73f));
        }

        public static unsafe void ActivateStyle2( bool bStyleDark_, float alpha_)
        {
            Style style = ImGui.GetStyle();

            // light style from Pacôme Danhiez (user itamago) https://github.com/ocornut/imgui/pull/511#issuecomment-175719267
            style.Alpha = 1.0f;
                style.FrameRounding = 3.0f;
                style.SetColor(ColorTarget.Text, new Vector4(0.00f, 0.00f, 0.00f, 1.00f));
                style.SetColor(ColorTarget.TextDisabled, new Vector4(0.60f, 0.60f, 0.60f, 1.00f));
                style.SetColor(ColorTarget.WindowBg, new Vector4(0.94f, 0.94f, 0.94f, 0.94f));
                style.SetColor(ColorTarget.ChildWindowBg, new Vector4(0.00f, 0.00f, 0.00f, 0.00f));
                //style.SetColor(ColorTarget.PopupBg, new Vector4(1.00f, 1.00f, 1.00f, 0.94f));
                style.SetColor(ColorTarget.Border, new Vector4(0.00f, 0.00f, 0.00f, 0.39f));
                style.SetColor(ColorTarget.BorderShadow, new Vector4(1.00f, 1.00f, 1.00f, 0.10f));
                style.SetColor(ColorTarget.FrameBg, new Vector4(1.00f, 1.00f, 1.00f, 0.94f));
                style.SetColor(ColorTarget.FrameBgHovered, new Vector4(0.26f, 0.59f, 0.98f, 0.40f));
                style.SetColor(ColorTarget.FrameBgActive, new Vector4(0.26f, 0.59f, 0.98f, 0.67f));
                style.SetColor(ColorTarget.TitleBg, new Vector4(0.96f, 0.96f, 0.96f, 1.00f));
                style.SetColor(ColorTarget.TitleBgCollapsed, new Vector4(1.00f, 1.00f, 1.00f, 0.51f));
                style.SetColor(ColorTarget.TitleBgActive, new Vector4(0.82f, 0.82f, 0.82f, 1.00f));
                style.SetColor(ColorTarget.MenuBarBg, new Vector4(0.86f, 0.86f, 0.86f, 1.00f));
                style.SetColor(ColorTarget.ScrollbarBg, new Vector4(0.98f, 0.98f, 0.98f, 0.53f));
                style.SetColor(ColorTarget.ScrollbarGrab, new Vector4(0.69f, 0.69f, 0.69f, 1.00f));
                style.SetColor(ColorTarget.ScrollbarGrabHovered, new Vector4(0.59f, 0.59f, 0.59f, 1.00f));
                style.SetColor(ColorTarget.ScrollbarGrabActive, new Vector4(0.49f, 0.49f, 0.49f, 1.00f));
                style.SetColor(ColorTarget.ComboBg, new Vector4(0.86f, 0.86f, 0.86f, 0.99f));
                style.SetColor(ColorTarget.CheckMark, new Vector4(0.26f, 0.59f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.SliderGrab, new Vector4(0.24f, 0.52f, 0.88f, 1.00f));
                style.SetColor(ColorTarget.SliderGrabActive, new Vector4(0.26f, 0.59f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.Button, new Vector4(0.26f, 0.59f, 0.98f, 0.40f));
                style.SetColor(ColorTarget.ButtonHovered, new Vector4(0.26f, 0.59f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.ButtonActive, new Vector4(0.06f, 0.53f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.Header, new Vector4(0.26f, 0.59f, 0.98f, 0.31f));
                style.SetColor(ColorTarget.HeaderHovered, new Vector4(0.26f, 0.59f, 0.98f, 0.80f));
                style.SetColor(ColorTarget.HeaderActive, new Vector4(0.26f, 0.59f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.Column, new Vector4(0.39f, 0.39f, 0.39f, 1.00f));
                style.SetColor(ColorTarget.ColumnHovered, new Vector4(0.26f, 0.59f, 0.98f, 0.78f));
                style.SetColor(ColorTarget.ColumnActive, new Vector4(0.26f, 0.59f, 0.98f, 1.00f));
                style.SetColor(ColorTarget.ResizeGrip, new Vector4(1.00f, 1.00f, 1.00f, 0.50f));
                style.SetColor(ColorTarget.ResizeGripHovered, new Vector4(0.26f, 0.59f, 0.98f, 0.67f));
                style.SetColor(ColorTarget.ResizeGripActive, new Vector4(0.26f, 0.59f, 0.98f, 0.95f));
                style.SetColor(ColorTarget.CloseButton, new Vector4(0.59f, 0.59f, 0.59f, 0.50f));
                style.SetColor(ColorTarget.CloseButtonHovered, new Vector4(0.98f, 0.39f, 0.36f, 1.00f));
                style.SetColor(ColorTarget.CloseButtonActive, new Vector4(0.98f, 0.39f, 0.36f, 1.00f));
                style.SetColor(ColorTarget.PlotLines, new Vector4(0.39f, 0.39f, 0.39f, 1.00f));
                style.SetColor(ColorTarget.PlotLinesHovered, new Vector4(1.00f, 0.43f, 0.35f, 1.00f));
                style.SetColor(ColorTarget.PlotHistogram, new Vector4(0.90f, 0.70f, 0.00f, 1.00f));
                style.SetColor(ColorTarget.PlotHistogramHovered, new Vector4(1.00f, 0.60f, 0.00f, 1.00f));
                style.SetColor(ColorTarget.TextSelectedBg, new Vector4(0.26f, 0.59f, 0.98f, 0.35f));
                style.SetColor(ColorTarget.ModalWindowDarkening, new Vector4(0.20f, 0.20f, 0.20f, 0.35f));

                if (bStyleDark_)
                {
                    for (int i = 0; i <= (int)ColorTarget.Count; i++)
                    {
                        Vector4 col = style.GetColor((ColorTarget)i);
                        float H, S, V;
                        ImGuiNative.igColorConvertRGBtoHSV(col.X, col.Y, col.Z, &H, &S, &V);

                        if (S < 0.1f)
                        {
                            V = 1.0f - V;
                        }
                        ImGuiNative.igColorConvertHSVtoRGB(H, S, V, &col.X, &col.Y, &col.Z);
                        if (col.W < 1.00f)
                        {
                            col.W *= alpha_;
                        }
                        style.SetColor((ColorTarget)i, col);
                    }
                }
                else
                {
                    for (int i = 0; i <= (int)ColorTarget.Count; i++)
                    {
                        Vector4 col = style.GetColor((ColorTarget)i);
                        if (col.W < 1.00f)
                        {
                            col.X *= alpha_;
                            col.Y *= alpha_;
                            col.Z *= alpha_;
                            col.W *= alpha_;
                        }
                        style.SetColor((ColorTarget)i, col);
                    }
                }
        }
    }
}
