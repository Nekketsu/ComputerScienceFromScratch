using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace NESEmulator;

public class NESEmulatorGame : Game
{
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D surface;

    private readonly Ppu ppu;
    private readonly Cpu cpu;
    private int ticks;

    private const int CpuFrequency = 1_789_773;
    private double cpuCycleAccumulator = 0;

    public NESEmulatorGame(Rom rom, string name)
    {
        graphics = new GraphicsDeviceManager(this);
        var (width, height) = GetMaxWindowSize(Ppu.NesWidth, Ppu.NesHeight);
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Window.Title = $"NES Emulator - {Path.GetFileNameWithoutExtension(name)}";

        ppu = new Ppu(rom);
        cpu = new Cpu(ppu, rom);
        ticks = 0;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        surface = new Texture2D(GraphicsDevice, Ppu.NesWidth, Ppu.NesHeight);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        cpuCycleAccumulator += gameTime.ElapsedGameTime.TotalSeconds * CpuFrequency;
        while (cpuCycleAccumulator > 0)
        {
            cpu.Step();
            var newTicks = cpu.CpuTicks - ticks;
            // 3 PPU cycles for every CPU tick
            for (var i = 0; i < newTicks * 3; i++)
            {
                ppu.Step();
                // Draw once per frame, everything onto the screen
                if ((ppu.Scanline == 240) && (ppu.Cycle == 257))
                {
                    var surfaceData = GetSurfaceData(ppu.DisplayBuffer);
                    surface.SetData(surfaceData);
                }
                if ((ppu.Scanline == 241) && (ppu.Cycle == 2) && ppu.GenerateNmi)
                {
                    cpu.TriggerNmi();
                }
            }
            ticks += newTicks;

            cpuCycleAccumulator -= newTicks;
        }

        cpu.Joypad1.Left = keyboardState.IsKeyDown(Keys.Left);
        cpu.Joypad1.Right = keyboardState.IsKeyDown(Keys.Right);
        cpu.Joypad1.Up = keyboardState.IsKeyDown(Keys.Up);
        cpu.Joypad1.Down = keyboardState.IsKeyDown(Keys.Down);
        cpu.Joypad1.A = keyboardState.IsKeyDown(Keys.X);
        cpu.Joypad1.B = keyboardState.IsKeyDown(Keys.Z);
        cpu.Joypad1.Start = keyboardState.IsKeyDown(Keys.S);
        cpu.Joypad1.Select = keyboardState.IsKeyDown(Keys.A);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(surface, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private static (int width, int height) GetMaxWindowSize(int preferredWidth, int preferredHeight)
    {
        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        float scaleX = (float)screenWidth / preferredWidth;
        float scaleY = (float)screenHeight / preferredHeight;

        float scale = MathF.Min(scaleX, scaleY);

        int width = (int)(preferredWidth * scale);
        int height = (int)(preferredHeight * scale);

        return (width, height);
    }

    private static Color[] GetSurfaceData(uint[,] displayBuffer)
    {
        var width = displayBuffer.GetLength(0);
        var height = displayBuffer.GetLength(1);

        var surfaceData = new Color[width * height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var color = new Color(displayBuffer[x, y]);
                surfaceData[y * width + x] = new Color(color.B, color.G, color.R);
            }
        }

        return surfaceData;
    }
}
