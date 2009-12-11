using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.EditorTools;
using NetGore.Graphics;
using NetGore.Graphics.ParticleEngine;
using NetGore.IO;

namespace DemoGame.ParticleEffectEditor
{
    public partial class ScreenForm : Form, IGetTime
    {
        const string _defaultCategory = "Particle";
        readonly Camera2D _camera;

        readonly string _defaultTitle;
        readonly Stopwatch _watch = new Stopwatch();

        ContentManager _content;
        ParticleEmitter _emitter;
        string _lastEmitterName = string.Empty;
        IParticleRenderer _renderer;
        SpriteBatch _spriteBatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenForm"/> class.
        /// </summary>
        public ScreenForm()
        {
            InitializeComponent();

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            _defaultTitle = Text;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            _camera = new Camera2D(new Vector2(GameScreen.Width, GameScreen.Height));

            _watch.Start();
        }

        public ParticleEmitter Emitter
        {
            get { return _emitter; }
            set
            {
                if (_emitter == value)
                    return;

                _emitter = value;

                pgEffect.SelectedObject = Emitter;
            }
        }

        GraphicsDevice GraphicsDevice
        {
            get { return GameScreen.GraphicsDevice; }
        }

        void btnLoad_Click(object sender, EventArgs e)
        {
            string filePath;
            ParticleEmitter emitter;
            var wasSuccessful = FileDialogs.TryOpenParticleEffect(out filePath, out emitter);

            if (wasSuccessful)
                Emitter = emitter;
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            ParticleEmitterFactory.SaveEmitter(ContentPaths.Dev, Emitter);

            MessageBox.Show("Saved!", "Saved", MessageBoxButtons.OK);
        }

        void cmbEmitter_SelectedEmitterChanged(ParticleEmitterComboBox sender, ParticleEmitter emitter)
        {
            if (_emitter == null)
            {
                Emitter = CreateInitialEmitter();
                return;
            }

            if (_emitter.GetType() == emitter.GetType())
                return;

            _emitter.CopyValuesTo(emitter);
            Emitter = emitter;
        }

        /// <summary>
        /// Creates the initial <see cref="ParticleEmitter"/> to display.
        /// </summary>
        /// <returns>The initial <see cref="ParticleEmitter"/> to display.</returns>
        ParticleEmitter CreateInitialEmitter()
        {
            var ret = new PointEmitter
            {
                SpriteCategorization = new SpriteCategorization(_defaultCategory, "ball"),
                Origin = new Vector2(GameScreen.Width, GameScreen.Height) / 2f,
                ReleaseRate = 35
            };

            var colorModifier = new ParticleColorModifier
            { ReleaseColor = new Color(0, 255, 0, 255), UltimateColor = new Color(0, 0, 255, 175) };
            ret.Modifiers.Add(colorModifier);

            return ret;
        }

        /// <summary>
        /// Main entry point for all the screen drawing.
        /// </summary>
        public void DrawGame()
        {
            // Clear the background
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, Matrix.Identity);
            _renderer.Draw(_camera, new ParticleEmitter[] { Emitter });
            _spriteBatch.End();
        }

        void GameScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Emitter.Origin = new Vector2(e.X, e.Y);
        }

        void ScreenForm_Load(object sender, EventArgs e)
        {
            GameScreen.ScreenForm = this;

            _content = new ContentManager(GameScreen.Services, ContentPaths.Build.Root);
            GrhInfo.Load(ContentPaths.Build, _content);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderer = new SpriteBatchRenderer { SpriteBatch = _spriteBatch };
        }

        /// <summary>
        /// Main entry point for all the screen content updating.
        /// </summary>
        public void UpdateGame()
        {
            if (Emitter == null)
                return;

            Emitter.Update(GetTime());

            if (Emitter.Name != _lastEmitterName)
            {
                _lastEmitterName = Emitter.Name;
                Text = _defaultTitle + " - " + _lastEmitterName;
            }
        }

        #region IGetTime Members

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        public int GetTime()
        {
            return (int)_watch.ElapsedMilliseconds;
        }

        #endregion
    }
}