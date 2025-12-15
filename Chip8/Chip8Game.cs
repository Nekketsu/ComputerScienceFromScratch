using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;

namespace Chip8;

public class Chip8Game : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D surface;

    private SoundEffectInstance beeSound;
    bool currentlyPlayingSound;

    private Chip8Vm vm;

    private TimeSpan timerAcumulator;

    public Chip8Game(byte[] programData, string name)
    {
        graphics = new GraphicsDeviceManager(this);
        var (width, heigth) = GetMaxWindowSize(Chip8Vm.ScreenWidth, Chip8Vm.ScreenHeight);
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = heigth;

        IsFixedTimeStep = true;
        TargetElapsedTime = Chip8Vm.FrameTimeExpected;

        Window.Title = $"Chip8 - {Path.GetFileNameWithoutExtension(name)}";
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        vm = new Chip8Vm(programData);
    }

    protected override void Initialize()
    {
        currentlyPlayingSound = false;
        timerAcumulator = TimeSpan.Zero;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        surface = new Texture2D(GraphicsDevice, Chip8Vm.ScreenWidth, Chip8Vm.ScreenHeight);

        var beeSound = Content.Load<SoundEffect>("Sounds/bee");
        this.beeSound = beeSound.CreateInstance();
        this.beeSound.IsLooped = true;
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        vm.Step();
        if (vm.NeedsRedraw)
        {
            var surfaceData = GetSurfaceData(vm.DisplayBuffer);
            surface.SetData(surfaceData);
        }

        var keys = new bool[16];
        foreach (var (index, key) in Chip8Vm.AllowedKeys.Index())
        {
            keys[index] = keyboardState.IsKeyDown(key);
        }
        vm.UpdateKeys(keys);

        if (vm.PlaySound)
        {
            if (!currentlyPlayingSound)
            {
                beeSound.Play();
                currentlyPlayingSound = true;
            }
        }
        else
        {
            currentlyPlayingSound = false;
            beeSound.Stop();
        }

        timerAcumulator += gameTime.ElapsedGameTime;
        if (timerAcumulator > Chip8Vm.TimerDelay)
        {
            vm.DecrementTimers();
            timerAcumulator = TimeSpan.Zero;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(surface, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private (int width, int height) GetMaxWindowSize(int preferredWidth, int preferredHeight)
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

    private Color[] GetSurfaceData(bool[,] displayBuffer)
    {
        var width = displayBuffer.GetLength(0);
        var height = displayBuffer.GetLength(1);

        var surfaceData = new Color[width * height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                surfaceData[y * width + x] = vm.DisplayBuffer[x, y] ? Color.White : Color.Black;
            }
        }

        return surfaceData;
    }
}
