using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KNN.DigitRecognizer;

public class DigitRecognizer : Game
{
    private const int PixelWidth = 8;
    private const int PixelHeight = 8;
    private const double PixelToDigit = 16.0 / 255; // Pixel to digit scale factor
    private const double DigitToPixel = 255 / 16; // Digit to pixel scale factor
    const int k = 9;

    private Color[,] digitPixels;
    private KNN<Digit> digitsKnn;
    private Texture2D surface;

    private HashSet<Keys> keys = [];
    private Keys[] allowedKeys = [Keys.C, Keys.E, Keys.P];

    private Point mousePosition = Mouse.GetState().Position;
    private bool wasMousePressed = false;

    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    public DigitRecognizer()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Create a 2D array of pixels to represent the digit
        digitPixels = new Color[PixelHeight, PixelWidth];

        // Load the training data
        var digitsFile = Path.Combine("datasets", "digits", "digits.csv");
        digitsKnn = new KNN<Digit>(digitsFile, false);

        var (width, heigth) = GetMaxWindowSize(PixelWidth, PixelHeight);
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = heigth;

        Window.Title = "DigitRecognizer";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        surface = new Texture2D(GraphicsDevice, PixelWidth, PixelHeight);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        var surfaceData = GetSurfaceData(digitPixels);
        surface.SetData(surfaceData);

        var keys = new HashSet<Keys>();
        foreach (var key in allowedKeys)
        {
            if (keyboardState.IsKeyDown(key))
            {
                keys.Add(key);
            }
        }

        if (keys.Contains(Keys.C) && !this.keys.Contains(Keys.C)) // Classify digit
        {
            var pixels = surfaceData
                .Select(p => (int)(p.R * PixelToDigit))
                .ToArray();
            var classifiedDigit = digitsKnn.Classify(k, new Digit("", pixels));
            Window.Title = $"DigitRecognizer: {classifiedDigit}";
            Debug.WriteLine($"Classified as {classifiedDigit}");
        }
        else if (keys.Contains(Keys.E) && !this.keys.Contains(Keys.E)) // Erase the digit
        {
            for (var x = 0; x < PixelWidth; x++)
            {
                for (var y = 0; y < PixelHeight; y++)
                {
                    digitPixels[x, y] = Color.Black;
                }
            }
            Window.Title = "DigitRecognizer";
        }
        else if (keys.Contains(Keys.P) && !this.keys.Contains(Keys.P)) // Predit what the digit should look like
        {
            var pixels = surfaceData
                .Select(p => (int)(p.R * PixelToDigit))
                .ToArray();
            var predictedPixels = digitsKnn.PredictArray(k, new Digit("", pixels), nameof(Digit.Pixels));
            for (var x = 0; x < PixelWidth; x++)
            {
                for (var y = 0; y < PixelHeight; y++)
                {
                    var gray = (int)(predictedPixels[y * PixelWidth + x] * DigitToPixel);
                    digitPixels[x, y] = new Color(gray, gray, gray);
                }
            }
        }
        else if (mouseState.LeftButton == ButtonState.Pressed &&
            (!wasMousePressed || mousePosition != mouseState.Position))
        {
            if (mousePosition.X < graphics.PreferredBackBufferWidth &&
                mousePosition.Y < graphics.PreferredBackBufferHeight)
            {
                var pixelX = mousePosition.X * PixelWidth / graphics.PreferredBackBufferWidth;
                var pixelY = mousePosition.Y * PixelHeight / graphics.PreferredBackBufferHeight;
                digitPixels[pixelX, pixelY] = Color.White;
            }
        }

        this.keys = keys;
        mousePosition = mouseState.Position;
        wasMousePressed = mouseState.LeftButton == ButtonState.Pressed;

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

    private static Color[] GetSurfaceData(Color[,] displayBuffer)
    {
        var width = displayBuffer.GetLength(0);
        var height = displayBuffer.GetLength(1);

        var surfaceData = new Color[width * height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                surfaceData[y * width + x] = displayBuffer[x, y];
            }
        }

        return surfaceData;
    }
}
