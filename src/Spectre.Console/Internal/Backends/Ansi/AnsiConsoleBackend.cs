using System;
using System.Text;
using Spectre.Console.Rendering;

namespace Spectre.Console
{
    internal sealed class AnsiConsoleBackend : IAnsiConsoleBackend
    {
        private readonly AnsiBuilder _builder;
        private readonly IAnsiConsole _console;

        public IAnsiConsoleCursor Cursor { get; }

        public AnsiConsoleBackend(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _builder = new AnsiBuilder(_console.Profile);

            Cursor = new AnsiConsoleCursor(this);
        }

        public void Clear(bool home)
        {
            Write(new ControlSequence("\u001b[2J"));

            if (home)
            {
                Cursor.SetPosition(0, 0);
            }
        }

        public void Write(IRenderable renderable)
        {
            var builder = new StringBuilder();
            foreach (var segment in renderable.GetSegments(_console))
            {
                if (segment.IsControlCode)
                {
                    builder.Append(segment.Text);
                    continue;
                }

                var parts = segment.Text.NormalizeNewLines().Split(new[] { '\n' });
                foreach (var (_, _, last, part) in parts.Enumerate())
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        builder.Append(_builder.GetAnsi(part, segment.Style));
                    }

                    if (!last)
                    {
                        builder.Append(Environment.NewLine);
                    }
                }
            }

            if (builder.Length > 0)
            {
                _console.Profile.Out.Write(builder.ToString());
                _console.Profile.Out.Flush();
            }
        }
    }
}
