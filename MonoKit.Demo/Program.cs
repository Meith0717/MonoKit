using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Core.IO;

namespace MonoKit.Demo;

/// <summary>
/// Minimal MonoGame entry point that exercises MonoKit types.
/// Extend this to demonstrate and test framework features.
/// </summary>
public class DemoGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    public DemoGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Example: use MonoKit's PathService to resolve a storage path
        var paths = new PathService<DemoPath>(System.AppContext.BaseDirectory);
        paths.RegisterPath(DemoPath.Saves, "saves");
        System.Console.WriteLine($"Demo save path: {paths.GetPath(DemoPath.Saves)}");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        base.Draw(gameTime);
    }
}

internal enum DemoPath
{
    Saves
}

internal static class Program
{
    [System.STAThread]
    private static void Main()
    {
        using var game = new DemoGame();
        game.Run();
    }
}
