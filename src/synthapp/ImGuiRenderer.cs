﻿using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Graphics;
using Veldrid.Platform;
using System.Runtime.InteropServices;

using Key = Veldrid.Platform.Key;

namespace SynthApp
{
    public class ImGuiRenderer : RenderItem, IDisposable
    {
        private readonly DynamicDataProvider<Matrix4x4> _projectionMatrixProvider;
        private RawTextureDataArray<int> _fontTexture;
        private FontTextureData _textureData;

        // Context objects
        private Material _material;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private BlendState _blendState;
        private DepthStencilState _depthDisabledState;
        private RasterizerState _rasterizerState;
        private ShaderTextureBinding _fontTextureBinding;

        private int _fontAtlasID = 1;
        private RenderContext _rc;
        private bool _controlDown;
        private bool _shiftDown;
        private bool _altDown;

        public ImGuiRenderer(RenderContext rc, NativeWindow window)
        {
            _rc = rc;
            ImGui.GetIO().FontAtlas.AddDefaultFont();
            _projectionMatrixProvider = new DynamicDataProvider<Matrix4x4>();

            InitializeContextObjects(rc);
            SetOpenTKKeyMappings();

            SetPerFrameImGuiData(rc, 1f / 60f);

            ImGui.NewFrame();
        }

        private void InitializeContextObjects(RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vertexBuffer = factory.CreateVertexBuffer(500, false);
            _indexBuffer = factory.CreateIndexBuffer(100, false);
            _blendState = factory.CreateCustomBlendState(
                true,
                Blend.InverseSourceAlpha, Blend.Zero, BlendFunction.Add,
                Blend.SourceAlpha, Blend.InverseSourceAlpha, BlendFunction.Add);
            _depthDisabledState = factory.CreateDepthStencilState(false, DepthComparison.Always);
            _rasterizerState = factory.CreateRasterizerState(FaceCullingMode.None, TriangleFillMode.Solid, true, true);
            RecreateFontDeviceTexture(rc);
            _material = factory.CreateMaterial(
                rc,
                "imgui-vertex", "imgui-frag",
                new MaterialVertexInput(20, new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_texcoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Byte4)
                }),
                new MaterialInputs<MaterialGlobalInputElement>(new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, _projectionMatrixProvider)
                }),
                MaterialInputs<MaterialPerObjectInputElement>.Empty,
                new MaterialTextureInputs(new MaterialTextureInputElement[]
                {
                    new TextureDataInputElement("surfaceTexture", _fontTexture)
                }));

        }

        public unsafe void RecreateFontDeviceTexture(RenderContext rc)
        {
            var io = ImGui.GetIO();
            // Build
            _textureData = io.FontAtlas.GetTexDataAsRGBA32();
            int[] pixels = new int[_textureData.Width * _textureData.Height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ((int*)_textureData.Pixels)[i];
            }

            _fontTexture = new RawTextureDataArray<int>(pixels, _textureData.Width, _textureData.Height, _textureData.BytesPerPixel, PixelFormat.R8_G8_B8_A8);

            // Store our identifier
            io.FontAtlas.SetTexID(_fontAtlasID);

            var deviceTexture = rc.ResourceFactory.CreateTexture(_fontTexture.PixelData, _textureData.Width, _textureData.Height, _textureData.BytesPerPixel, PixelFormat.R8_G8_B8_A8);
            _fontTextureBinding = rc.ResourceFactory.CreateShaderTextureBinding(deviceTexture);

            io.FontAtlas.ClearTexData();
        }

        private string[] s_stages = { "Standard" };

        public IList<string> GetStagesParticipated()
        {
            return s_stages;
        }

        public unsafe void Render(RenderContext rc, string pipelineStage)
        {
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), rc);
        }

        public RenderOrderKey GetRenderOrderKey(System.Numerics.Vector3 position)
        {
            return new RenderOrderKey();
        }

        public void Update(float deltaSeconds)
        {
            SetPerFrameImGuiData(_rc, deltaSeconds);
        }

        public unsafe void SetPerFrameImGuiData(RenderContext rc, float deltaSeconds)
        {
            IO io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                rc.Window.Width / rc.Window.ScaleFactor.X,
                rc.Window.Height / rc.Window.ScaleFactor.Y);
            io.DisplayFramebufferScale = rc.Window.ScaleFactor;
            io.DeltaTime = deltaSeconds / 1000; // DeltaTime is in seconds.
        }

        public void OnInputUpdated(InputSnapshot snapshot)
        {
            UpdateImGuiInput((OpenTKWindow)_rc.Window, snapshot);
            ImGui.NewFrame();
        }

        private unsafe void UpdateImGuiInput(OpenTKWindow window, InputSnapshot snapshot)
        {
            IO io = ImGui.GetIO();
            MouseState cursorState = Mouse.GetCursorState();
            MouseState mouseState = Mouse.GetState();

            if (window.NativeWindow.Bounds.Contains(cursorState.X, cursorState.Y) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: This does not take into account viewport coordinates.
                if (window.Exists)
                {
                    Point windowPoint = window.NativeWindow.PointToClient(new Point(cursorState.X, cursorState.Y));
                    io.MousePosition = new System.Numerics.Vector2(
                        windowPoint.X / window.ScaleFactor.X,
                        windowPoint.Y / window.ScaleFactor.Y);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                io.MousePosition = new System.Numerics.Vector2(
                        cursorState.X,
                        cursorState.Y);
            }
            else
            {
                io.MousePosition = new System.Numerics.Vector2(-1f, -1f);
            }

            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            float delta = snapshot.WheelDelta;
            io.MouseWheel = delta;

            ImGui.GetIO().MouseWheel = delta;

            IReadOnlyList<char> keyCharPresses = snapshot.KeyCharPresses;
            for (int i = 0; i < keyCharPresses.Count; i++)
            {
                char c = keyCharPresses[i];
                ImGui.AddInputCharacter(c);
            }

            IReadOnlyList<KeyEvent> keyEvents = snapshot.KeyEvents;
            for (int i = 0; i < keyEvents.Count; i++)
            {
                KeyEvent keyEvent = keyEvents[i];
                io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
                if (keyEvent.Key == Key.ControlLeft)
                {
                    _controlDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.ShiftLeft)
                {
                    _shiftDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.AltLeft)
                {
                    _altDown = keyEvent.Down;
                }
            }

            io.CtrlPressed = _controlDown;
            io.AltPressed = _altDown;
            io.ShiftPressed = _shiftDown;
        }

        private static unsafe void SetOpenTKKeyMappings()
        {
            IO io = ImGui.GetIO();
            io.KeyMap[GuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[GuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[GuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[GuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[GuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[GuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[GuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[GuiKey.Home] = (int)Key.Home;
            io.KeyMap[GuiKey.End] = (int)Key.End;
            io.KeyMap[GuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[GuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[GuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[GuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[GuiKey.A] = (int)Key.A;
            io.KeyMap[GuiKey.C] = (int)Key.C;
            io.KeyMap[GuiKey.V] = (int)Key.V;
            io.KeyMap[GuiKey.X] = (int)Key.X;
            io.KeyMap[GuiKey.Y] = (int)Key.Y;
            io.KeyMap[GuiKey.Z] = (int)Key.Z;
        }

        private unsafe void RenderImDrawData(DrawData* draw_data, RenderContext rc)
        {
            VertexDescriptor descriptor = new VertexDescriptor((byte)sizeof(DrawVert), 3, 0, IntPtr.Zero);

            int vertexOffsetInVertices = 0;
            int indexOffsetInElements = 0;

            if (draw_data->CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < draw_data->CmdListsCount; i++)
            {
                NativeDrawList* cmd_list = draw_data->CmdLists[i];

                _vertexBuffer.SetVertexData(new IntPtr(cmd_list->VtxBuffer.Data), descriptor, cmd_list->VtxBuffer.Size, vertexOffsetInVertices);
                _indexBuffer.SetIndices(new IntPtr(cmd_list->IdxBuffer.Data), IndexFormat.UInt16, sizeof(ushort), cmd_list->IdxBuffer.Size, indexOffsetInElements);

                vertexOffsetInVertices += cmd_list->VtxBuffer.Size;
                indexOffsetInElements += cmd_list->IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                _projectionMatrixProvider.Data = mvp;
            }

            BlendState previousBlendState = rc.BlendState;
            rc.SetBlendState(_blendState);
            rc.SetDepthStencilState(_depthDisabledState);
            RasterizerState previousRasterizerState = rc.RasterizerState;
            rc.SetRasterizerState(_rasterizerState);
            rc.SetVertexBuffer(_vertexBuffer);
            rc.SetIndexBuffer(_indexBuffer);
            rc.SetMaterial(_material);

            ImGui.ScaleClipRects(draw_data, ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                NativeDrawList* cmd_list = draw_data->CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd->TextureId != IntPtr.Zero)
                        {
                            if (pcmd->TextureId == new IntPtr(_fontAtlasID))
                            {
                                _material.UseTexture(0, _fontTextureBinding);
                            }
                            else
                            {
                                ShaderTextureBinding binding = ImGuiImageHelper.GetShaderTextureBinding(pcmd->TextureId);
                                _material.UseTexture(0, binding);
                            }
                        }

                        // TODO: This doesn't take into account viewport coordinates.
                        rc.SetScissorRectangle(
                            (int)pcmd->ClipRect.X,
                            (int)pcmd->ClipRect.Y,
                            (int)pcmd->ClipRect.Z,
                            (int)pcmd->ClipRect.W);

                        rc.DrawIndexedPrimitives((int)pcmd->ElemCount, idx_offset, vtx_offset);
                    }

                    idx_offset += (int)pcmd->ElemCount;
                }
                vtx_offset += cmd_list->VtxBuffer.Size;
            }

            rc.ClearScissorRectangle();
            rc.SetBlendState(previousBlendState);
            rc.SetDepthStencilState(rc.DefaultDepthStencilState);
            rc.SetRasterizerState(previousRasterizerState);
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _material.Dispose();
            _depthDisabledState.Dispose();
            _blendState.Dispose();
            _fontTextureBinding.Dispose();
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }
    }
}
